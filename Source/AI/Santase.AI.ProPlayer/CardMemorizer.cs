namespace Santase.AI.ProPlayer
{
	using System;
	using System.Text;
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

		private Tools.CardCollection undiscoveredCards;

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
			this.TrumpCardDrawn = false;

			this.undiscoveredCards = new Tools.CardCollection();
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
					this.undiscoveredCards.Add(card);

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

		public Card TrumpCard { get; private set; }

		public Card OldTrumpCard { get; private set; }

		public bool TrumpCardDrawn { get; private set; }

	    public int RemainingTrumpCardsCount { get; private set; }

	    public IReadOnlyCollection<Card> UndiscoveredCards
	    {
		    get
		    {
			    return this.undiscoveredCards;
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
				this.TrumpCardDrawn = true;
			}
			else
			{
				this.NewCardDicovered(card);
			}

			this.myHand.Add(card);
		}

	    public void LogPlayedCard(Card card)
	    {
			CardMemorizer.ValidateCardNotNull(card);

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

			bool isTrumpCard = card.Equals(this.TrumpCard);

			if (!isTrumpCard && !card.Equals(this.OldTrumpCard))
			{
				this.NewCardDicovered(card);
			}

			if (isTrumpCard)
			{
				this.TrumpCardDrawn = true;
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
			else if (this.undiscoveredCards.Contains(lowestTrump))
			{
				this.NewCardDicovered(lowestTrump);
			}
			else
			{
				throw new ArgumentException("The lowest trump card has alredy been played.");
			}

			this.OldTrumpCard = this.TrumpCard;
			this.TrumpCard = lowestTrump;
	    }

	    public void LogTrumpCardDrawn()
	    {
		    this.TrumpCardDrawn = true;
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
			bool removed = this.undiscoveredCards.Remove(card);

			if (!removed)
			{
				throw new ArgumentException($"Card Must be present in {nameof(this.UndiscoveredCards)}.");
			}

			if (card.Suit == this.TrumpCard.Suit)
			{
				this.RemainingTrumpCardsCount--;
			}
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			const string Separator = ": ";
			const string ListingSeparator = ", ";


			result.AppendLine(nameof(this.UndiscoveredCards) + Separator);
			result.AppendLine(string.Join(ListingSeparator, this.UndiscoveredCards));

			result.AppendLine(nameof(this.MyHand) + Separator);
			result.AppendLine(string.Join(ListingSeparator, this.MyHand));

			result.AppendLine(nameof(this.MyPlayedCards) + Separator);
			result.AppendLine(string.Join(ListingSeparator, this.MyPlayedCards));

			result.AppendLine(nameof(this.OpponentPlayedCards) + Separator);
			result.AppendLine(string.Join(ListingSeparator, this.OpponentPlayedCards));

			result.AppendLine(nameof(this.MyLastPlayedCard) + Separator + this.MyLastPlayedCard);

			result.AppendLine(nameof(this.TrumpCard) + Separator + this.TrumpCard);
			result.AppendLine(nameof(this.OldTrumpCard) + Separator + this.OldTrumpCard);
			result.AppendLine(nameof(this.TrumpCardDrawn) + Separator + this.TrumpCardDrawn);
			result.AppendLine(nameof(this.RemainingTrumpCardsCount) + Separator + this.RemainingTrumpCardsCount);

			return result.ToString();
		}
	}
}