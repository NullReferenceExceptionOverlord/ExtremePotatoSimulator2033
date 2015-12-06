namespace Santase.AI.ProPlayer
{
    using System.Collections.Generic;
    using System.Linq;

    using Logic;
    using Logic.Cards;
    using Logic.PlayerActionValidate;
    using Logic.Players;
	using Logic.WinnerLogic;

	using States;
	using Tools.Extensions;

	public class ProPlayer : IPlayer
	{
		public ProPlayer()
		{
			this.AnnounceValidator = new AnnounceValidator();
			this.PlayerActionValidator = new PlayerActionValidator();
			this.CardWinnerLogic = new CardWinnerLogic();
		}

		private IState State { get; set; }

		public bool IsFirstPlayer { get; set; }

		public int MyPoints { get; private set; }

		public int OpponentPoints { get; private set; }

		public CardMemorizer CardMemorizer { get; private set; }

		public IReadOnlyCollection<Card> MyHand
		{
			get
			{
				return this.CardMemorizer.MyHand;
			}
		}

		public IAnnounceValidator AnnounceValidator { get; }

		public IPlayerActionValidator PlayerActionValidator { get; }

		public ICardWinnerLogic CardWinnerLogic { get; }

		public string Name => "Potato!";

		public virtual void StartGame(string otherPlayerIdentifier)
		{
		}

		public virtual void StartRound(ICollection<Card> cards, Card trumpCard, int myTotalPoints, int opponentTotalPoints)
		{
			this.CardMemorizer = new CardMemorizer(trumpCard, cards);
		}

		public virtual void AddCard(Card card)
		{
			this.CardMemorizer.LogDrawnCard(card);
		}

		public virtual PlayerAction GetTurn(PlayerTurnContext context)
		{
			this.IsFirstPlayer = context.IsFirstPlayerTurn;

			if (context.CardsLeftInDeck == 0)
			{
				this.CardMemorizer.LogTrumpCardDrawn();
			}

			this.SetCorrectState(context);

			var myHand = this.CardMemorizer.GetMyHand();
			if (this.PlayerActionValidator.IsValid(PlayerAction.ChangeTrump(), context, myHand))
			{
				return this.ChangeTrump();
			}

			if (this.ShouldCloseGame(context))
			{
				return this.CloseGame();
			}

			return this.ChooseCard(context);
		}

		public virtual void EndTurn(PlayerTurnContext context)
		{
			if (!this.CardMemorizer.TrumpCardDrawn && !this.CardMemorizer.TrumpCard.Equals(context.TrumpCard))
			{
				this.CardMemorizer.LogTrumpChange();
			}

			if (this.IsFirstPlayer)
			{
				this.MyPoints = context.FirstPlayerRoundPoints;
				this.OpponentPoints = context.SecondPlayerRoundPoints;
			}
			else
			{
				this.MyPoints = context.SecondPlayerRoundPoints;
				this.OpponentPoints = context.FirstPlayerRoundPoints;
			}

			Card opponentCard = this.GetOpponentCard(context);

			if (opponentCard != null)
			{
				this.CardMemorizer.LogOpponentPlayedCard(opponentCard);
			}
		}

		public virtual void EndRound()
		{
			;
		}

		public virtual void EndGame(bool amIWinner)
		{
			;
		}

		public PlayerAction PlayCard(Card card)
		{
			this.CardMemorizer.LogPlayedCard(card);
			return PlayerAction.PlayCard(card);
		}

		public int CalcHandValue()
		{
			return this.MyHand.Sum(card => card.GetValue());
		}

		public Card GetOpponentCard(PlayerTurnContext context)
		{
			return this.IsFirstPlayer ? context.SecondPlayedCard : context.FirstPlayedCard;
		}

		public Card GetPossibleBestAnounce(ICollection<Card> possibleCardsToPlay)

		{
			foreach (var card in possibleCardsToPlay)
			{
				if (!this.IsTrumpCard(card))
				{
					continue;
				}

				if (card.Type == CardType.King && possibleCardsToPlay.Any(c => c.Suit == card.Suit && c.Type == CardType.Queen))
				{
					return card;
				}
			}

			foreach (var card in possibleCardsToPlay)
			{
				if (card.Type == CardType.King && possibleCardsToPlay.Any(c => c.Suit == card.Suit && c.Type == CardType.Queen))
				{
					return card;
				}
			}

			return null;
		}

		public IEnumerable<Card> GetWinningCardsWhenFirst(IEnumerable<Card> cards)
		{
			var opponentCards = this.GetPossibleOpponentCards();
			var winningCards = cards.ToList();
			foreach (var card in cards)
			{
				foreach (var oponentCard in opponentCards)
				{
					if (oponentCard.Suit == card.Suit)
					{
						if (oponentCard.GetValue() > card.GetValue())
						{
							winningCards.Remove(card);
						}
					}
				}
			}

			return winningCards;
		}

		public IEnumerable<Card> GetWinningCardsWhenSecond(IEnumerable<Card> cards, Card cardToBeat)
		{
			return cards
				.Where(card => this.CardWinnerLogic.Winner(cardToBeat, card, this.CardMemorizer.TrumpCard.Suit) == PlayerPosition.SecondPlayer)
				.OrderBy(card => card.GetValue());
		}

		public int CalcPossibleRemainingTurns(bool deckIsClosed)
		{
			int cardsToBePlayedCount = this.CardMemorizer.MyHand.Count;

			if (deckIsClosed)
			{
				return cardsToBePlayedCount;
			}

			cardsToBePlayedCount += this.CardMemorizer.UndiscoveredCards.Count;

			if (!this.CardMemorizer.TrumpCardDrawn)
			{
				cardsToBePlayedCount++;
			}

			return cardsToBePlayedCount / 2;
		}

		public Card GetWeakestCard(IEnumerable<Card> cards)
		{
			Card weakestCard = cards.First();

			foreach (var card in cards)
			{
				if ((this.IsTrumpCard(weakestCard) || !this.IsTrumpCard(card)) && (card.GetValue() < weakestCard.GetValue()))
				{
					weakestCard = card;
				}
			}

			return weakestCard;
		}

		public bool IsTrumpCard(Card card)
		{
			return card.Suit == this.CardMemorizer.TrumpCard.Suit;
		}

		public int CountPotentialWins(Card card)
		{
			int count = 0;

			var opponentCards = this.GetPossibleOpponentCards();

			foreach (var opponentCard in opponentCards)
			{
				if (this.CardWinnerLogic.Winner(card, opponentCard, this.CardMemorizer.TrumpCard.Suit) == PlayerPosition.FirstPlayer)
				{
					count++;
				}
			}

			return count;
		}

		public IReadOnlyCollection<Card> GetPossibleOpponentCards()
		{
			if (this.CardMemorizer.OldTrumpCard == null)
			{
				return this.CardMemorizer.UndiscoveredCards;
			}

			var result = new Tools.CardCollection();

			this.CardMemorizer.UndiscoveredCards.ForEach(result.Add);

			result.Add(this.CardMemorizer.OldTrumpCard);

			return result;
		}

		protected PlayerAction ChooseCard(PlayerTurnContext context)
		{
			return this.State.ChooseCard(context);
		}

		protected PlayerAction ChangeTrump()
		{
			this.CardMemorizer.LogTrumpChange();
			return PlayerAction.ChangeTrump();
		}

		protected PlayerAction CloseGame()
		{
			return PlayerAction.CloseGame();
		}

		protected virtual bool ShouldCloseGame(PlayerTurnContext context)
		{
			bool canCloseGame = this.PlayerActionValidator.IsValid(PlayerAction.CloseGame(), context, this.CardMemorizer.GetMyHand());

			if (canCloseGame)
			{
				int potentialPoints = this.MyPoints + this.CalcHandValue();
				var myThrumpCardCount = this.CardMemorizer.GetMyHand().Count(x => x.Suit == context.TrumpCard.Suit);
				return potentialPoints >= 60 && myThrumpCardCount >= 3;
			}

			return false;
		}

		protected virtual void SetCorrectState(PlayerTurnContext context)
		{
			if (!this.IsFirstPlayer)
			{
				this.State = new PlayingSecond(this);
			}
			else if (context.State.ShouldObserveRules)
			{
				this.State = new PlayingFirstWithRules(this);
			}
			else
			{
				this.State = new PlayingFirst(this);
			}
		}
	}
}