namespace Santase.AI.ProPlayer.States
{
	using System.Linq;
	using System.Collections.Generic;

	using Logic.Players;
	using Logic.Cards;

	public class PlayingSecond : State
	{
		public PlayingSecond(ProPlayer bot)
			: base(bot)
		{
			;
		}

		protected override PlayerAction ChooseCard(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
		{
			Card opponentCard = this.Bot.GetOpponentCard(context);

			var winningCards = this.Bot.GetWinningCardsWhenSecond(possibleCardsToPlay, opponentCard);

			if (!winningCards.Any())
			{
				return this.Bot.PlayCard(this.Bot.GetWeakestCard(possibleCardsToPlay));
			}

			winningCards = winningCards.Reverse();

			Card card = winningCards.FirstOrDefault(c => !this.Bot.IsTrumpCard(c)) ?? winningCards.First();
			return this.Bot.PlayCard(card);
		}
	}
}