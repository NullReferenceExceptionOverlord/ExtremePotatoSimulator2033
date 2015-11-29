namespace Santase.AI.ProPlayer
{
    using System;
    using System.Collections.Generic;
    using Logic.Cards;

    internal class CardMemoizer
    {
        private readonly IEnumerable<CardType> AllCardTypes = new List<CardType>
                                                                         {
                                                                             CardType.Nine,
                                                                             CardType.Ten,
                                                                             CardType.Jack,
                                                                             CardType.Queen,
                                                                             CardType.King,
                                                                             CardType.Ace
                                                                         };

        private readonly IEnumerable<CardSuit> AllCardSuits = new List<CardSuit>
                                                                         {
                                                                             CardSuit.Club,
                                                                             CardSuit.Diamond,
                                                                             CardSuit.Heart,
                                                                             CardSuit.Spade
                                                                         };

        private readonly IList<Card> listOfCards;

        public CardMemoizer()
        {
            this.AllCards = new List<Card>();
            this.ThrumpCardCount = 8;

            foreach (var cardSuit in AllCardSuits)
            {
                foreach (var cardType in AllCardTypes)
                {
                    AllCards.Add(new Card(cardSuit, cardType));
                }
            }
        }

        public IList<Card> AllCards { get; private set; }

        // We can make it even more rtarded by making Ace card count 10s count etc
        public int ThrumpCardCount { get; private set; }

        public void RemoveCard(Card cardToRemove, bool isThrumpCard)
        {
            if (isThrumpCard)
            {
                this.ThrumpCardCount--;
            }

            this.AllCards.Remove(cardToRemove);
        }
    }
}
