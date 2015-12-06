namespace Santase.AI.ProPlayer.States
{
	using System.Collections.Generic;

	using Logic.Players;
	using Logic.Cards;

	public abstract class State : IState
	{
		protected State(ProPlayer bot)
		{
			this.Bot = bot;
		}

		public ProPlayer Bot { get; private set; }

		public PlayerAction ChooseCard(PlayerTurnContext context)
		{
            var possibleCardsToPlay = this.Bot.PlayerActionValidator.GetPossibleCardsToPlay(context, this.Bot.CardMemorizer.GetMyHand());
			return this.ChooseCard(context, possibleCardsToPlay);
		}

		protected abstract PlayerAction  ChooseCard(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay);
	}
}