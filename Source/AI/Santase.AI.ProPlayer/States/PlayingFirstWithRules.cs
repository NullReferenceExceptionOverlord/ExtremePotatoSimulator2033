using System.Collections.Generic;
using Santase.Logic.Cards;
using Santase.Logic.Players;

namespace Santase.AI.ProPlayer.States
{
	public class PlayingFirstWithRules : State
	{
		public PlayingFirstWithRules(ProPlayer bot) 
			: base(bot)
		{
			;
		}

		protected override PlayerAction ChooseCard(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
		{
			throw new System.NotImplementedException();
		}
	}
}