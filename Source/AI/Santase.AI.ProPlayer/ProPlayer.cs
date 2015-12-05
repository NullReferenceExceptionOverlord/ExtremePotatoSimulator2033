namespace Santase.AI.ProPlayer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
	using System.Linq;

    using Logic;
    using Logic.Cards;
    using Logic.Extensions;
    using Logic.PlayerActionValidate;
    using Logic.Players;
    using Logic.WinnerLogic;
    using Tools.Extensions;

    public class ProPlayer : IPlayer
    {
        public ProPlayer()
        {
            this.AnnounceValidator = new AnnounceValidator();
            this.PlayerActionValidator = new PlayerActionValidator();
			this.CardWinnerLogic = new CardWinnerLogic();
        }

        protected bool IsFirstPlayer { get; set; }

        protected int MyPoints { get; set; }

        protected int OpponentPoints { get; set; }

        protected CardMemorizer CardMemorizer { get; set; }

        protected IReadOnlyCollection<Card> MyHand
        {
            get
            {
                return this.CardMemorizer.MyHand;
            }
        }

        protected IAnnounceValidator AnnounceValidator { get; }

        protected IPlayerActionValidator PlayerActionValidator { get; }

		protected ICardWinnerLogic CardWinnerLogic { get; }

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
        }

        public virtual void EndGame(bool amIWinner)
        {
        }

        protected PlayerAction ChangeTrump()
        {
            this.CardMemorizer.LogTrumpChange();
            return PlayerAction.ChangeTrump();
        }

        protected PlayerAction PlayCard(Card card)
        {
            this.CardMemorizer.LogPlayedCard(card);
            return PlayerAction.PlayCard(card);
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

                return potentialPoints >= 60;
            }

            return false;
        }

        protected int CalcHandValue()
        {
            return this.MyHand.Sum(card => card.GetValue());
        }

	    protected Card GetOpponentCard(PlayerTurnContext context)
	    {
		    return this.IsFirstPlayer ? context.SecondPlayedCard : context.FirstPlayedCard;
	    }

		private PlayerAction ChooseCard(PlayerTurnContext context)
		{
			var possibleCardsToPlay = this.PlayerActionValidator.GetPossibleCardsToPlay(context, this.CardMemorizer.GetMyHand());
			return context.State.ShouldObserveRules
					   ? (this.IsFirstPlayer
							  ? this.ChooseCardWhenPlayingFirstAndRulesApply(context, possibleCardsToPlay)
							  : this.ChooseCardWhenPlayingSecondAndRulesApply(context, possibleCardsToPlay))
					   : (this.IsFirstPlayer
							  ? this.ChooseCardWhenPlayingFirstAndRulesDoNotApply(context, possibleCardsToPlay)
							  : this.ChooseCardWhenPlayingSecondAndRulesDoNotApply(context, possibleCardsToPlay));
		}

		private PlayerAction ChooseCardWhenPlayingFirstAndRulesDoNotApply(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
		{
			var anounce = this.TryToAnnounce20Or40(context, possibleCardsToPlay);

			if (anounce != null)
			{
				return anounce;
			}

			return this.PlayCard(this.GetWeakestCard(possibleCardsToPlay));
		}

        private PlayerAction ChooseCardWhenPlayingFirstAndRulesApply(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
            //Determine all the cards in our hand that will win (will word great for when the deck has ended/ will work very shittly when the game is closed and the deck has many cards!)
            var oponentCards = this.CardMemorizer.UndiscoveredCards;
            var myCards = this.CardMemorizer.MyHand;
            var myThrumpCardCount = myCards.Count(x => x.Suit == context.TrumpCard.Suit);

            var winningCards = myCards.ToList();
            
            // Take out our thrump cards out of the total remaining -> checks if the oponent has thrump cards
            var thrumpCardsRemain = this.CardMemorizer.RemainingTrumpCardsCount - myThrumpCardCount > 0;

            foreach (var myCard in myCards)
            {
                var oponentHasWeakerCard = false;
                foreach (var oponentCard in oponentCards)
                {
                    if(oponentCard.Suit == myCard.Suit)
                    {
                        if(oponentCard.GetValue() > myCard.GetValue())
                        {
                            winningCards.Remove(myCard);
                        }
                        else
                        {
                            oponentHasWeakerCard = true;
                        }

                    }
                }

                if (thrumpCardsRemain && winningCards.Contains(myCard) && !oponentHasWeakerCard)
                {
                    winningCards.Remove(myCard);
                }
            }


            // Announce 40 or 20 if possible
			var anounce = this.TryToAnnounce20Or40(context, possibleCardsToPlay);

			if (anounce != null)
			{
				return anounce;
			}

            // Playing the cards that will win the hand
            if(winningCards.Count != 0)
            {
                return this.PlayCard(winningCards.FirstOrDefault());
            }


            var thrumpCards = myCards.Where(x => x.Suit == context.TrumpCard.Suit).ToList();

            if(thrumpCards.Count != 0)
            {
                return this.PlayCard(thrumpCards.FirstOrDefault());
            }

            return this.PlayCard(myCards.FirstOrDefault());
            
        }

        private PlayerAction ChooseCardWhenPlayingSecondAndRulesDoNotApply(PlayerTurnContext context,  ICollection<Card> possibleCardsToPlay)
        {
			int possibleRemainingTurns = this.CalcPossibleRemainingTurns(context.State.ShouldObserveRules);
			Card opponentCard = this.GetOpponentCard(context);

			Card[] winningCards = this.GetWinningCards(possibleCardsToPlay, opponentCard)
				.ToArray();

			if (winningCards.Length == 0)
			{
				return this.PlayCard(this.GetWeakestCard(possibleCardsToPlay));
			}

			if (possibleRemainingTurns >= winningCards.Length)
			{
				Card card = winningCards.FirstOrDefault(c => !this.IsTrumpCard(c)) ?? winningCards.First();
				return this.PlayCard(card);
			}

			int startIndex = winningCards.Length - possibleRemainingTurns;
			for (int i = startIndex; i < winningCards.Length; i++)
			{
				if (!this.IsTrumpCard(winningCards[i]))
				{
					return this.PlayCard(winningCards[i]);
				}
			}

	        return this.PlayCard(winningCards[startIndex]);
        }

        private PlayerAction ChooseCardWhenPlayingSecondAndRulesApply(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
			return this.ChooseCardWhenPlayingSecondAndRulesDoNotApply(context, possibleCardsToPlay);
        }

        private PlayerAction TryToAnnounce20Or40(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
			foreach (var card in possibleCardsToPlay)
			{
				if (card.Type == CardType.Queen && this.AnnounceValidator.GetPossibleAnnounce(possibleCardsToPlay, card, this.CardMemorizer.TrumpCard) == Announce.Forty)
				{
					return this.PlayCard(card);
				}
			}

			foreach (var card in possibleCardsToPlay)
			{
				if (card.Type == CardType.Queen && this.AnnounceValidator.GetPossibleAnnounce(possibleCardsToPlay, card, this.CardMemorizer.TrumpCard) == Announce.Twenty)
				{
					return this.PlayCard(card);
				}
			}

            return null;
        }

	    protected IEnumerable<Card> GetWinningCards(IEnumerable<Card> cards, Card cardToBeat)
	    {
			return cards.Where(card => this.CardWinnerLogic.Winner(cardToBeat, card, this.CardMemorizer.TrumpCard.Suit) == PlayerPosition.SecondPlayer)
				.OrderBy(card => card.GetValue());
	    }

	    protected int CalcPossibleRemainingTurns(bool deckIsClosed)
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

	    protected Card GetWeakestCard(IEnumerable<Card> cards)
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

	    protected bool IsTrumpCard(Card card)
	    {
		    return card.Suit == this.CardMemorizer.TrumpCard.Suit;
	    }

	    protected int CountPotentioalWins(Card card)
	    {
			int count = 0;

			foreach (var opponentCard in this.CardMemorizer.UndiscoveredCards)
			{
				if (this.CardWinnerLogic.Winner(card, opponentCard, this.CardMemorizer.TrumpCard.Suit) == PlayerPosition.FirstPlayer)
				{
					count++;
				}
			}

			if (this.CardMemorizer.OldTrumpCard != null && this.CardWinnerLogic.Winner(card, this.CardMemorizer.OldTrumpCard, this.CardMemorizer.TrumpCard.Suit) == PlayerPosition.FirstPlayer)
			{
				count++;
			}

			return count;
	    }
    }
}