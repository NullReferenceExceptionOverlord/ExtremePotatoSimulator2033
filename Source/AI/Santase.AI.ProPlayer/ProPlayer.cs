namespace Santase.AI.ProPlayer
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using Logic.Cards;
    using Logic.Extensions;
    using Logic.Players;
	using Logic.PlayerActionValidate;

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
            if (this.PlayerActionValidator.IsValid(PlayerAction.ChangeTrump(), context, this.CardMemorizer.GetMyHand()))
            {
                return this.ChangeTrump();
            }

            if (this.ShouldCloseGame(context))
            {
                return this.CloseGame();
            }

            var possibleCardsToPlay = this.PlayerActionValidator.GetPossibleCardsToPlay(context, this.CardMemorizer.GetMyHand());
            var shuffledCards = possibleCardsToPlay.Shuffle();
            var cardToPlay = shuffledCards.First();

            return this.PlayCard(cardToPlay);
        }

		public virtual void EndTurn(PlayerTurnContext context)
        {
			if (this.CardMemorizer.TrumpCard != null && !this.CardMemorizer.TrumpCard.Equals(context.TrumpCard))
			{
				this.CardMemorizer.LogTrumpChange();
			}

			Card opponentCard = this.CardMemorizer.MyLastPlayedCard.Equals(context.FirstPlayedCard) ? context.SecondPlayedCard : context.FirstPlayedCard;

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

		private bool ShouldCloseGame(PlayerTurnContext context)
		{
			var shouldCloseGame = this.PlayerActionValidator.IsValid(PlayerAction.CloseGame(), context, this.CardMemorizer.GetMyHand());

			return shouldCloseGame;
		}
	}
}