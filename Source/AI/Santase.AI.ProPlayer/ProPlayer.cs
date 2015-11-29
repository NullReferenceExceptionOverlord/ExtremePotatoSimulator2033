namespace Santase.AI.ProPlayer
{
    using System;
    using System.Collections.Generic;
    using Logic.Cards;
    using Santase.Logic.Players;
    using Logic.Extensions;
    using System.Linq;

    public class ProPlayer : BasePlayer
    {
        public override string Name => "DonaldThrumpForPrez!";
        

        public override PlayerAction GetTurn(PlayerTurnContext context)
        {
            var possibleCardsToPlay = this.PlayerActionValidator.GetPossibleCardsToPlay(context, this.Cards);
            var shuffledCards = possibleCardsToPlay.Shuffle();
            var cardToPlay = shuffledCards.First();
            return this.PlayCard(cardToPlay);
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
