namespace Santase.AI.ProPlayer
{
	using System;
    using System.Linq;
	using System.Collections.Generic;

    using Logic.Cards;

    internal class CardMemorizer
    {
        public static readonly CardType[] AllCardTypes =
        {
            CardType.Nine,
            CardType.Ten,
            CardType.Jack,
            CardType.Queen,
            CardType.King,
            CardType.Ace
        };

        public static readonly CardSuit[] AllCardSuits =
        {
            CardSuit.Club,
            CardSuit.Diamond,
            CardSuit.Heart,
            CardSuit.Spade
        };

        public static readonly CardCollection AllCards = new CardCollection();

		private Card trumpCard;

		private CardCollection remainingCards;

		private CardCollection myPlayedCards;

		private CardCollection opponentPlayedCards;

        static CardMemorizer()
        {
            foreach (var cardSuit in CardMemorizer.AllCardSuits)
            {
                foreach (var cardType in CardMemorizer.AllCardTypes)
                {
                    CardMemorizer.AllCards.Add(new Card(cardSuit, cardType));
                }
            }
        }

		public CardMemorizer(Card trumpCard, IEnumerable<Card> initialHand)
		{
			this.trumpCard = trumpCard;
			this.RemainingTrumpCardsCount = 0;

			this.remainingCards = new CardCollection();
			this.myPlayedCards = new CardCollection();
			this.opponentPlayedCards = new CardCollection();

			foreach (var card in CardMemorizer.AllCards)
			{
				if (initialHand.Contains(card))
				{
					continue;
				}

				this.remainingCards.Add(card);

				if (card.Suit == this.trumpCard.Suit)
				{
					this.RemainingTrumpCardsCount++;
				}
			}
		}

		public Card TrumpCard
	    {
		    get
		    {
			    return this.trumpCard;
		    }

		    set
		    {
				if (this.remainingCards.Contains(value))
				{
					this.NewCardDicovered(value);
				}

				this.trumpCard = value;
		    }
	    }

	    public int RemainingTrumpCardsCount { get; private set; }

	    public IEnumerable<Card> RemainingCards
	    {
		    get
		    {
			    return this.remainingCards;
		    }
	    }

	    public int RemainingCardsCount
	    {
		    get
		    {
			    return this.remainingCards.Count;
		    }
	    }

	    public IEnumerable<Card> MyPlayedCards
	    {
		    get
		    {
			    return this.myPlayedCards;
		    }
	    }

	    public int MyPlayedCardsCount
	    {
		    get
		    {
				return this.myPlayedCards.Count;
		    }
	    }

	    public IEnumerable<Card> OpponentPlayedCards
	    {
		    get
		    {
			    return this.opponentPlayedCards;
		    }
	    }

	    public int OpponentPlayedCardsCount
	    {
		    get
		    {
			    return this.opponentPlayedCards.Count;
		    }
	    }

	    //// We can make it even more rtarded by making Ace card count 10s count etc

		public void LogDrawnCard(Card card)
		{
			if (card == null)
			{
				throw new ArgumentNullException("Can't log null card.");
			}

			this.NewCardDicovered(card);
		}

	    public void LogPlayedCard(Card card)
	    {
		    this.myPlayedCards.Add(card);
	    }

	    public void LogOpponentPlayedCard(Card card)
	    {
			this.NewCardDicovered(card);

			this.opponentPlayedCards.Add(card);
	    }

	    private void NewCardDicovered(Card card)
	    {
			this.remainingCards.Remove(card);

			if (card.Suit == this.TrumpCard.Suit)
			{
				this.RemainingTrumpCardsCount--;
			}
		}
	}
}