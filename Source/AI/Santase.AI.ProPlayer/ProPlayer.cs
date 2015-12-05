namespace Santase.AI.ProPlayer
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Collections.Generic;

    using Logic.Cards;
    using Logic.Extensions;
    using Logic.Players;
    using Logic.PlayerActionValidate;
    using Tools.Extensions;

    public class ProPlayer : IPlayer
    {
		public ProPlayer()
        {
			this.AnnounceValidator = new AnnounceValidator();
			this.PlayerActionValidator = new PlayerActionValidator();
        }

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
			var myHand = this.CardMemorizer.GetMyHand();

            if (this.PlayerActionValidator.IsValid(PlayerAction.ChangeTrump(), context, myHand))
            {
                return this.ChangeTrump();
            }

            if (this.ShouldCloseGame(context))
            {
                return this.CloseGame();
            }

			if (context.CardsLeftInDeck == 0)
			{
				this.CardMemorizer.LogTrumpCardDrawn();
			}

            var possibleCardsToPlay = this.PlayerActionValidator.GetPossibleCardsToPlay(context, myHand);
            var cardToPlay = possibleCardsToPlay.First();
            foreach (var card in possibleCardsToPlay)
            {
                if(cardToPlay.GetValue() > card.GetValue())
                {
                    cardToPlay = card;
                }
            }


            return this.PlayCard(cardToPlay);
        }

		public virtual void EndTurn(PlayerTurnContext context)
        {
			if (!this.CardMemorizer.TrumpCardDrawn && !this.CardMemorizer.TrumpCard.Equals(context.TrumpCard))
			{
				this.CardMemorizer.LogTrumpChange();
			}
            this.Points = context.SecondPlayerRoundPoints;
            Card opponentCard = this.CardMemorizer.MyLastPlayedCard.Equals(context.FirstPlayedCard) ? context.SecondPlayedCard : context.FirstPlayedCard;

			if (opponentCard != null)
			{
				this.CardMemorizer.LogOpponentPlayedCard(opponentCard);
			}
		}

        public int Points { get; set; }

        public virtual void EndRound()
        {
           this.Points = CountCardPoints(this.CardMemorizer.MyPlayedCards);
        }

        private int CountCardPoints(IReadOnlyCollection<Card> myPlayedCards)
        {
            var cardPoints = 0;
            foreach (var card in myPlayedCards)
            {
                cardPoints += card.GetValue();
            }

            return cardPoints;
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

		private bool ShouldCloseGame(PlayerTurnContext context)
		{
            var myHand = this.CardMemorizer.GetMyHand();
            var shouldCloseGame = this.PlayerActionValidator.IsValid(PlayerAction.CloseGame(), context, myHand);
            if (shouldCloseGame)
            {
                var cardPoints = 0;
                foreach (var card in myHand)
                {
                    cardPoints += card.GetValue();
                }

                if (!(cardPoints + this.Points >= 60)) //TODO make it better
                {
                    return false;
                }

            }

            return shouldCloseGame;
		}

	}
}