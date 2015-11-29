namespace Santase.AI.ProPlayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Logic.Cards;
    using Logic.Extensions;
    using Santase.Logic.Players;

    public class ProPlayer : BasePlayer
    {
        private CardMemoizer cardMemo;

        public ProPlayer()
        {
            this.cardMemo = new CardMemoizer();
        }

        public override string Name => "DonaldThrumpForPrez!";

        public override PlayerAction GetTurn(PlayerTurnContext context)
        {
            if (this.PlayerActionValidator.IsValid(PlayerAction.ChangeTrump(), context, this.Cards))
            {
                return this.ChangeTrump(context.TrumpCard);
            }

            if (this.CloseGame(context))
            {
                return this.CloseGame();
            }

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
            var isFirstPlayerThrumpCard = context.FirstPlayedCard.Suit == context.TrumpCard.Suit;
            var isSecondPlayerThrumpCard = context.FirstPlayedCard.Suit == context.TrumpCard.Suit;

            this.cardMemo.RemoveCard(context.FirstPlayedCard, isFirstPlayerThrumpCard);
            this.cardMemo.RemoveCard(context.SecondPlayedCard, isSecondPlayerThrumpCard);
            base.EndTurn(context);
        }

        public override void EndRound()
        {
            this.cardMemo = new CardMemoizer();
        }

        public override void EndGame(bool amIWinner)
        {
            base.EndGame(amIWinner);
        }

        private bool CloseGame(PlayerTurnContext context)
        {
            var shouldCloseGame = this.PlayerActionValidator.IsValid(PlayerAction.CloseGame(), context, this.Cards);

            return shouldCloseGame;
        }
    }
}
