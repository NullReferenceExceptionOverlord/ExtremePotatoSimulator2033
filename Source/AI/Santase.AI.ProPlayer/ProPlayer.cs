namespace Santase.AI.ProPlayer
{
    using System;
    using System.Collections.Generic;
    using Logic.Cards;
    using Santase.Logic.Players;

    public class ProPlayer : BasePlayer
    {
        public override string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override PlayerAction GetTurn(PlayerTurnContext context)
        {
            throw new NotImplementedException();
        }

        public override void StartGame(string otherPlayerIdentifier)
        {
            base.StartGame(otherPlayerIdentifier);
        }

        public override void StartRound(ICollection<Card> cards, Card trumpCard, int myTotalPoints, int opponentTotalPoints)
        {
            base.StartRound(cards, trumpCard, myTotalPoints, opponentTotalPoints);
        }

        public override void AddCard(Card card)
        {
            base.AddCard(card);
        }

        public override void EndTurn(PlayerTurnContext context)
        {
            base.EndTurn(context);
        }

        public override void EndRound()
        {
            base.EndRound();
        }

        public override void EndGame(bool amIWinner)
        {
            base.EndGame(amIWinner);
        }
    }
}
