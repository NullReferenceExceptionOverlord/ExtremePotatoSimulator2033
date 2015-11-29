namespace Santase.AI.ProPlayer.Tools
{

	using System.Collections;
	using System.Collections.Generic;

	using Logic;
	using Logic.Cards;

	public class CardCollection : ICollection<Card>, IDeepCloneable<CardCollection>, IReadOnlyCollection<Card>
	{
		public CardCollection()
		{
			this.Collection = new Logic.Cards.CardCollection();
		}

		private Logic.Cards.CardCollection Collection { get; set; }

		public int Count => this.Collection.Count;

		public bool IsReadOnly => this.Collection.IsReadOnly;

		public void Add(Card card)
		{
			this.Collection.Add(card);
		}

		public bool Remove(Card card)
		{
			return this.Collection.Remove(card);
		}

		public bool Contains(Card card)
		{
			return this.Collection.Contains(card);
		}

		public void Clear()
		{
			this.Collection.Clear();
		}

		public CardCollection DeepClone()
		{
			CardCollection clone = new CardCollection
			{
				Collection = this.Collection.DeepClone()
			};

			return clone;
		}

		public void CopyTo(Card[] array, int arrayIndex)
		{
			this.Collection.CopyTo(array, arrayIndex);
		}

		public IEnumerator<Card> GetEnumerator()
		{
			return this.Collection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.Collection.GetEnumerator();
		}
	}
}