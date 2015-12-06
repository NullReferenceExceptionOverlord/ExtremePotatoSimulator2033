namespace Santase.AI.ProPlayer.States
{
	using System.Linq;
	using System.Collections.Generic;

	using Logic.Cards;
	using Logic.Players;

	public class PlayingFirstWithRules : State
	{
		public PlayingFirstWithRules(ProPlayer bot) 
			: base(bot)
		{
			;
		}

		protected override PlayerAction ChooseCard(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
		{
			var myCards = this.Bot.CardMemorizer.MyHand.OrderBy(c => c.GetValue()); ;

			var winningCards = this.Bot.GetWinningCardsWhenFirst(possibleCardsToPlay);

			// Announce 40 or 20 if possible
			var anounce = this.Bot.GetPossibleBestAnounce(possibleCardsToPlay);

			if (anounce != null)
			{
				return this.Bot.PlayCard(anounce);
			}

			// Playing the cards that will win the hand
			if (winningCards.Any())
			{
				return this.Bot.PlayCard(winningCards.FirstOrDefault(this.Bot.IsTrumpCard) ?? winningCards.Reverse<Card>().First());
			}

			// Likely we will lose the hand so just give the lowest card
			Card result = myCards.FirstOrDefault(c => !this.Bot.IsTrumpCard(c)) ?? myCards.First();
			return this.Bot.PlayCard(result);
		}
	}
}