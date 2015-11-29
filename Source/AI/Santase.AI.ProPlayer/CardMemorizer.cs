namespace Santase.AI.ProPlayer
{
	using System;
	using System.Linq;
	using System.Collections.Generic;

	using Logic.Cards;

    public class CardMemorizer
    {
		public const int InitialHandValidSize = 6;

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

        public static readonly Tools.CardCollection AllCards = new Tools.CardCollection();

		private Tools.CardCollection remainingCards;

		private Tools.CardCollection myHand;

		private Tools.CardCollection myPlayedCards;

		private Tools.CardCollection opponentPlayedCards;

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

		public CardMemorizer(Card trumpCard, ICollection<Card> initialHand)
		{
			this.TrumpCard = trumpCard;
			this.RemainingTrumpCardsCount = 0;
			this.TrumpSuit = this.TrumpCard.Suit;

			this.remainingCards = new Tools.CardCollection();
			this.myHand = new Tools.CardCollection();
			this.myPlayedCards = new Tools.CardCollection();
			this.opponentPlayedCards = new Tools.CardCollection();

			foreach (var card in CardMemorizer.AllCards)
			{
				if (card.Equals(this.TrumpCard))
				{
					continue;
				}

				if (initialHand.Contains(card))
				{
					this.myHand.Add(card);
				}
				else
				{
					this.remainingCards.Add(card);

					if (card.Suit == this.TrumpCard.Suit)
					{
						this.RemainingTrumpCardsCount++;
					}
				}
			}

			if (this.myHand.Count != CardMemorizer.InitialHandValidSize)
			{
				throw new ArgumentException($"Initial hand size must be {CardMemorizer.InitialHandValidSize}.");
			}
		}

		public CardSuit TrumpSuit { get; private set; }

		public Card TrumpCard { get; private set; }

		public Card OldTrumpCard { get; private set; }

	    public int RemainingTrumpCardsCount { get; private set; }

	    public IReadOnlyCollection<Card> RemainingCards
	    {
		    get
		    {
			    return this.remainingCards;
		    }
	    }

		public IReadOnlyCollection<Card> MyHand
		{
			get
			{
				return this.myHand;
			}
		}

		public IReadOnlyCollection<Card> MyPlayedCards
	    {
		    get
		    {
			    return this.myPlayedCards;
		    }
	    }

	    public IReadOnlyCollection<Card> OpponentPlayedCards
	    {
		    get
		    {
			    return this.opponentPlayedCards;
		    }
	    }

		public Card MyLastPlayedCard { get; set; }

		public ICollection<Card> GetMyHand()
		{
			return this.myHand.DeepClone(); 
		}

		public void LogDrawnCard(Card card)
		{
			CardMemorizer.ValidateCardNotNull(card);

			if (card.Equals(this.TrumpCard))
			{
				this.TrumpCard = null;
			}
			else
			{
				this.NewCardDicovered(card);
			}

			this.myHand.Add(card);
		}

	    public void LogPlayedCard(Card card)
	    {
			bool removed = this.myHand.Remove(card);

			if (!removed)
			{
				throw new ArgumentException($"Card must be present in {nameof(this.MyHand)}");
			}

			this.MyLastPlayedCard = card;

		    this.myPlayedCards.Add(card);
	    }

	    public void LogOpponentPlayedCard(Card card)
	    {
			CardMemorizer.ValidateCardNotNull(card);

			bool isTrumpCard = this.TrumpCard != null && this.TrumpCard.Equals(card);

			if (!isTrumpCard && !card.Equals(this.OldTrumpCard))
			{
				this.NewCardDicovered(card);
			}

			if (isTrumpCard)
			{
				this.TrumpCard = null;
			}

			this.opponentPlayedCards.Add(card);
	    }

	    public void LogTrumpChange()
	    {
			Card lowestTrump = new Card(this.TrumpCard.Suit, CardType.Nine);

			if (this.myHand.Contains(lowestTrump))
			{
				this.myHand.Remove(lowestTrump);
				this.myHand.Add(this.TrumpCard);
			}
			else if (this.remainingCards.Contains(lowestTrump))
			{
				this.NewCardDicovered(lowestTrump);
			}
			else
			{
				throw new ArgumentException("The lowest trump card has alredy benn played.");
			}

			this.OldTrumpCard = this.TrumpCard;
			this.TrumpCard = lowestTrump;
	    }

		private static void ValidateCardNotNull(Card card)
		{
			if (card == null)
			{
				throw new ArgumentNullException("Can't log null card.");
			}
		}

		private void NewCardDicovered(Card card)
	    {
			bool removed = this.remainingCards.Remove(card);

			if (!removed)
			{
				throw new ArgumentException($"Card Must be present in {nameof(this.RemainingCards)}.");
			}

			if (card.Suit == this.TrumpSuit)
			{
				this.RemainingTrumpCardsCount--;
			}
		}
	}
}