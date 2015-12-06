namespace Santase.AI.ProPlayer.States
{
	using System.Linq;
	using System.Collections.Generic;

	using Logic.Players;
	using Logic.Cards;


	public class PlayingFirst : State
	{
		public PlayingFirst(ProPlayer bot)
			: base(bot)
		{
			;
		}

		protected override PlayerAction ChooseCard(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
		{
			var myCards = this.Bot.CardMemorizer.MyHand.OrderBy(c => c.GetValue());
			//var possibleWins = new System.Collections.Generic.Dictionary<Card, int>();

			//foreach (var myCard in myCards)
			//{
			//	possibleWins[myCard] = this.Bot.CountPotentialWins(myCard);
			//}

			var winningCards = this.Bot.GetWinningCardsWhenFirst(possibleCardsToPlay);

			var anounce = this.Bot.GetPossibleBestAnounce(possibleCardsToPlay);

			if (winningCards.Any())
			{
				if (anounce != null && winningCards.Contains(anounce))
				{
					return this.Bot.PlayCard(anounce);
				}

				Card card = winningCards.FirstOrDefault(this.Bot.IsTrumpCard) ?? winningCards.First();
				return this.Bot.PlayCard(card);
			}

			if (anounce != null)
			{
				return this.Bot.PlayCard(anounce);
			}

			// Likely we will lose the hand so just give the lowest card
			Card result = myCards.FirstOrDefault(c => !this.Bot.IsTrumpCard(c)) ?? myCards.First();
			return this.Bot.PlayCard(result);
		}
	}
}