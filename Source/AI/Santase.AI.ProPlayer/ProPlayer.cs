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
            // Announce 40 or 20 if possible

            // If the player is close to the win => play trump card which will surely win the trick

            // Smallest non-trump card from the shortest opponent suit
            // Should never happen
    //        var cardToPlay =
				//possibleCardsToPlay.Where(x => x.Suit != context.TrumpCard.Suit)
				//	.OrderBy(x => x.GetValue())
				//	.FirstOrDefault();

            var cardToPlay = this.GetWeakestCard(possibleCardsToPlay);
            return this.PlayCard(cardToPlay);
        }

        private PlayerAction ChooseCardWhenPlayingFirstAndRulesApply(
            PlayerTurnContext context,
            ICollection<Card> possibleCardsToPlay)
        {
            // Find card that will surely win the trick

            // Announce 40 or 20 if possible

            // Smallest non-trump card
            var cardToPlay = this.GetWeakestCard(possibleCardsToPlay);

            // Smallest card
            return this.PlayCard(cardToPlay);
        }

        private PlayerAction ChooseCardWhenPlayingSecondAndRulesDoNotApply(PlayerTurnContext context,  ICollection<Card> possibleCardsToPlay)
        {
			int possibleRemainingTurns = this.CalcPossibleRemainingTurns(context.State.ShouldObserveRules);
			Card opponentCard = this.GetOpponentCard(context);

			var winningCards = this.GetWinningCards(possibleCardsToPlay, opponentCard);
			int winningCardsCount = winningCards.Count();

			if (winningCardsCount == 0)
			{
				return this.PlayCard(this.GetWeakestCard(possibleCardsToPlay));
			}

			Card card = winningCards.First();
	        if (possibleRemainingTurns < winningCardsCount)
	        {
				card = winningCards.Reverse()
					.ElementAt(possibleRemainingTurns - 1);
			}

            return this.PlayCard(card);
        }

        private PlayerAction ChooseCardWhenPlayingSecondAndRulesApply(
            PlayerTurnContext context,
            ICollection<Card> possibleCardsToPlay)
        {
            // If bigger card is available => play it

            // Play smallest trump card?

            // Smallest card
            var cardToPlay = possibleCardsToPlay.OrderBy(x => x.GetValue()).FirstOrDefault();
            return this.PlayCard(cardToPlay);
        }

        private PlayerAction TryToAnnounce20Or40(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
            // Choose card with announce 40 if possible          

            // Choose card with announce 20 if possible

            return null;
        }

	    protected IEnumerable<Card> GetWinningCards(IEnumerable<Card> cards, Card cardToBeat)
	    {
			return cards.Where(card => this.CardWinnerLogic.Winner(card, cardToBeat, this.CardMemorizer.TrumpCard.Suit) == PlayerPosition.FirstPlayer)
				.OrderBy(card => card.GetValue())
				.ThenBy(card =>
				{
					if (card.Suit == this.CardMemorizer.TrumpCard.Suit)
					{
						return 1;
					}

					return 0;
				});
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
				if (card.Suit != this.CardMemorizer.TrumpCard.Suit && (card.GetValue() < weakestCard.GetValue()))
				{
					weakestCard = card;
				}
			}

			return weakestCard;
	    }
    }
}