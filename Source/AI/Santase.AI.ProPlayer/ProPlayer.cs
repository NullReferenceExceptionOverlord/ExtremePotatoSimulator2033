﻿namespace Santase.AI.ProPlayer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Logic;
    using Logic.Cards;
    using Logic.Extensions;
    using Logic.PlayerActionValidate;
    using Logic.Players;
    using Logic.WinnerLogic;
    using Tools.Extensions;

    public class ProPlayer : IPlayer
    {
        public ProPlayer()
        {
            this.AnnounceValidator = new AnnounceValidator();
            this.PlayerActionValidator = new PlayerActionValidator();
            this.CardWinnerLogic = new CardWinnerLogic();
        }

		public bool IsFirstPlayer { get; set; }

		public int MyPoints { get; set; }

		public int OpponentPoints { get; set; }

		public CardMemorizer CardMemorizer { get; set; }

		public IReadOnlyCollection<Card> MyHand
        {
            get
            {
                return this.CardMemorizer.MyHand;
            }
        }

		public IAnnounceValidator AnnounceValidator { get; }

		public IPlayerActionValidator PlayerActionValidator { get; }

        public ICardWinnerLogic CardWinnerLogic { get; }

        public string Name => "Potato!";

        public virtual void StartGame(string otherPlayerIdentifier)
        {
        }

        public virtual void StartRound(ICollection<Card> cards, Card trumpCard, int myTotalPoints, int opponentTotalPoints)
        {
            this.CardMemorizer = new CardMemorizer(trumpCard, cards);
        }

        public virtual void AddCard(Card card)
        {
            this.CardMemorizer.LogDrawnCard(card);
        }

        public virtual PlayerAction GetTurn(PlayerTurnContext context)
        {
            this.IsFirstPlayer = context.IsFirstPlayerTurn;

            if (context.CardsLeftInDeck == 0)
            {
                this.CardMemorizer.LogTrumpCardDrawn();
            }

            var myHand = this.CardMemorizer.GetMyHand();
            if (this.PlayerActionValidator.IsValid(PlayerAction.ChangeTrump(), context, myHand))
            {
                return this.ChangeTrump();
            }

            if (this.ShouldCloseGame(context))
            {
                return this.CloseGame();
            }

            return this.ChooseCard(context);
        }

        public virtual void EndTurn(PlayerTurnContext context)
        {
            if (!this.CardMemorizer.TrumpCardDrawn && !this.CardMemorizer.TrumpCard.Equals(context.TrumpCard))
            {
                this.CardMemorizer.LogTrumpChange();
            }

            if (this.IsFirstPlayer)
            {
                this.MyPoints = context.FirstPlayerRoundPoints;
                this.OpponentPoints = context.SecondPlayerRoundPoints;
            }
            else
            {
                this.MyPoints = context.SecondPlayerRoundPoints;
                this.OpponentPoints = context.FirstPlayerRoundPoints;
            }

            Card opponentCard = this.GetOpponentCard(context);

            if (opponentCard != null)
            {
                this.CardMemorizer.LogOpponentPlayedCard(opponentCard);
            }
        }

        public virtual void EndRound()
        {
        }

        public virtual void EndGame(bool amIWinner)
        {
        }

        protected PlayerAction ChangeTrump()
        {
            this.CardMemorizer.LogTrumpChange();
            return PlayerAction.ChangeTrump();
        }

        public PlayerAction PlayCard(Card card)
        {
            this.CardMemorizer.LogPlayedCard(card);
            return PlayerAction.PlayCard(card);
        }

        protected PlayerAction CloseGame()
        {
            return PlayerAction.CloseGame();
        }

        protected virtual bool ShouldCloseGame(PlayerTurnContext context)
        {
            bool canCloseGame = this.PlayerActionValidator.IsValid(PlayerAction.CloseGame(), context, this.CardMemorizer.GetMyHand());

            if (canCloseGame)
            {
                int potentialPoints = this.MyPoints + this.CalcHandValue();
                var myThrumpCardCount = this.CardMemorizer.GetMyHand().Count(x => x.Suit == context.TrumpCard.Suit);
                return potentialPoints >= 60 && myThrumpCardCount >= 3;
            }

            return false;
        }

        public int CalcHandValue()
        {
            return this.MyHand.Sum(card => card.GetValue());
        }

        private PlayerAction ChooseCard(PlayerTurnContext context)
        {
            var possibleCardsToPlay = this.PlayerActionValidator.GetPossibleCardsToPlay(context, this.CardMemorizer.GetMyHand());
            return context.State.ShouldObserveRules
                       ? (this.IsFirstPlayer
                              ? this.ChooseCardWhenPlayingFirstAndRulesApply(context, possibleCardsToPlay)
                              : this.ChooseCardWhenPlayingSecondAndRulesApply(context, possibleCardsToPlay))
                       : (this.IsFirstPlayer
                              ? this.ChooseCardWhenPlayingFirstAndRulesDoNotApply(context, possibleCardsToPlay)
                              : this.ChooseCardWhenPlayingSecondAndRulesDoNotApply(context, possibleCardsToPlay));
        }

	    public Card GetOpponentCard(PlayerTurnContext context)
	    {
		    return this.IsFirstPlayer ? context.SecondPlayedCard : context.FirstPlayedCard;
	    }

		private PlayerAction ChooseCardWhenPlayingFirstAndRulesDoNotApply(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
		{
			//Determine all the cards in our hand that will win (will word great for when the deck has ended/ will work very shittly when the game is closed and the deck has many cards!)
			var opponentCards = this.GetPossibleOpponentCards();
			var myCards = this.CardMemorizer.MyHand.OrderBy(c => c.GetValue());
			var possibleWins = new Dictionary<Card, int>();

			foreach (var myCard in myCards)
			{
				possibleWins[myCard] = this.CountPotentialWins(myCard);
			}

			var winningCards = myCards.ToList();
            foreach (var myCard in myCards)
            {
                foreach (var oponentCard in opponentCards)
                {
                    if (oponentCard.Suit == myCard.Suit)
                    {
                        if (oponentCard.GetValue() > myCard.GetValue())
                        {
                            winningCards.Remove(myCard);
                        }
                    }
                }
            }

			var anounce = this.GetPossibleBestAnounce(possibleCardsToPlay);

			if (winningCards.Any())
			{
				if (anounce != null && winningCards.Contains(anounce))
				{
					return this.PlayCard(anounce);
				}

				Card card = winningCards.FirstOrDefault(this.IsTrumpCard) ?? winningCards.First();
				return this.PlayCard(card);
			}

			if (anounce != null)
			{
				return this.PlayCard(anounce);
			}
			// Playing the cards that will win the hand


			// Likely we will lose the hand so just give the lowest card
			Card result = myCards.FirstOrDefault(c => !this.IsTrumpCard(c)) ?? myCards.First();
            return this.PlayCard(result);
        }

        private PlayerAction ChooseCardWhenPlayingFirstAndRulesApply(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
            var oponentCards = this.CardMemorizer.UndiscoveredCards;
            var myCards = this.CardMemorizer.MyHand.OrderBy(c => c.GetValue()); ;
            var myThrumpCardCount = myCards.Count(x => this.IsTrumpCard(x));

            var winningCards = myCards.ToList();

            foreach (var myCard in myCards)
            {
                foreach (var oponentCard in oponentCards)
                {
                    if (oponentCard.Suit == myCard.Suit)
                    {
                        if (oponentCard.GetValue() > myCard.GetValue())
                        {
                            winningCards.Remove(myCard);
                        }

                    }
                }

            }

            // Announce 40 or 20 if possible
			var anounce = this.GetPossibleBestAnounce(possibleCardsToPlay);

			if (anounce != null)
			{
				return this.PlayCard(anounce);
			}

            // Playing the cards that will win the hand
            if (winningCards.Count != 0)
            {
                return this.PlayCard(winningCards.FirstOrDefault(this.IsTrumpCard) ?? winningCards.Reverse<Card>().First());
            }

            // Likely we will lose the hand so just give the lowest card
            Card result = myCards.FirstOrDefault(c => !this.IsTrumpCard(c)) ?? myCards.First();
            return this.PlayCard(result);
        }

        private PlayerAction ChooseCardWhenPlayingSecondAndRulesDoNotApply(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
            Card opponentCard = this.GetOpponentCard(context);

            var winningCards = this.GetWinningCards(possibleCardsToPlay, opponentCard);

			if (!winningCards.Any())
			{
				return this.PlayCard(this.GetWeakestCard(possibleCardsToPlay));
			}

            int possibleRemainingTurns = this.CalcPossibleRemainingTurns(context.State.ShouldObserveRules);

			int skip = winningCards.Count() - possibleRemainingTurns;

			//skip = skip > 0 ? skip : 0;

			winningCards = winningCards
				.Reverse()
				//.Skip(skip)
				//.Take(possibleRemainingTurns)
				;

			//Card firstCard = winningCards.First();
			//if (firstCard.GetValue() + opponentCard.GetValue() + this.MyPoints >= 66)
			//{
			//	return this.PlayCard(firstCard);
			//}


			Card card = winningCards.FirstOrDefault(c => !this.IsTrumpCard(c)) ?? winningCards.First();
			return this.PlayCard(card);
		}

        private PlayerAction ChooseCardWhenPlayingSecondAndRulesApply(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
			return ChooseCardWhenPlayingSecondAndRulesDoNotApply(context, possibleCardsToPlay);
			//var winningCards = this.GetWinningCards(possibleCardsToPlay, context.FirstPlayedCard);

			//var shouldPlayNormalCardsBeforeThrumps = winningCards.Count(x => !this.IsTrumpCard(x)) > 0;
			//if (winningCards.Any())
			//{
			//    if (shouldPlayNormalCardsBeforeThrumps)
			//    {
			//        return this.PlayCard(winningCards.OrderByDescending(x => x.GetValue()).FirstOrDefault());
			//    }
			//    else
			//    {
			//        return this.PlayCard(winningCards.OrderBy(x => x.GetValue()).FirstOrDefault());
			//    }

			//}
			//else
			//{
			//    return this.PlayCard(possibleCardsToPlay.OrderBy(x => x.GetValue()).FirstOrDefault());
			//}        



		}

        public Card GetPossibleBestAnounce(ICollection<Card> possibleCardsToPlay)
        {
			foreach (var card in possibleCardsToPlay)
			{
				if (!this.IsTrumpCard(card))
				{
					continue;
				}

				if (card.Type == CardType.King && possibleCardsToPlay.Any(c => c.Suit == card.Suit && c.Type == CardType.Queen))
				{
					return card;
				}
			}

			foreach (var card in possibleCardsToPlay)
			{
				if (card.Type == CardType.King && possibleCardsToPlay.Any(c => c.Suit == card.Suit && c.Type == CardType.Queen))
				{
					return card;
				}
			}

            return null;
        }

        public IEnumerable<Card> GetWinningCards(IEnumerable<Card> cards, Card cardToBeat)
        {
            return cards
                .Where(card => this.CardWinnerLogic.Winner(cardToBeat, card, this.CardMemorizer.TrumpCard.Suit) == PlayerPosition.SecondPlayer)
                .OrderBy(card => card.GetValue());
        }

        public int CalcPossibleRemainingTurns(bool deckIsClosed)
        {
            int cardsToBePlayedCount = this.CardMemorizer.MyHand.Count;

            if (deckIsClosed)
            {
                return cardsToBePlayedCount;
            }

            cardsToBePlayedCount += this.CardMemorizer.UndiscoveredCards.Count;

            if (!this.CardMemorizer.TrumpCardDrawn)
            {
                cardsToBePlayedCount++;
            }

            return cardsToBePlayedCount / 2;
        }

        public Card GetWeakestCard(IEnumerable<Card> cards)
        {
            Card weakestCard = cards.First();

            foreach (var card in cards)
            {
                if ((this.IsTrumpCard(weakestCard) || !this.IsTrumpCard(card)) && (card.GetValue() < weakestCard.GetValue()))
                {
                    weakestCard = card;
                }
            }

            return weakestCard;
        }

        public bool IsTrumpCard(Card card)
        {
            return card.Suit == this.CardMemorizer.TrumpCard.Suit;
        }

	    public int CountPotentialWins(Card card)
	    {
			int count = 0;

			var opponentCards = this.GetPossibleOpponentCards();

			foreach (var opponentCard in opponentCards)
			{
				if (this.CardWinnerLogic.Winner(card, opponentCard, this.CardMemorizer.TrumpCard.Suit) == PlayerPosition.FirstPlayer)
				{
					count++;
				}
			}

			return count;
	    }

	    public IReadOnlyCollection<Card> GetPossibleOpponentCards()
	    {
			if (this.CardMemorizer.OldTrumpCard == null)
			{
				return this.CardMemorizer.UndiscoveredCards;
			}

			var result = new Tools.CardCollection();

			this.CardMemorizer.UndiscoveredCards.ForEach(result.Add);

			result.Add(this.CardMemorizer.OldTrumpCard);

			return result;
	    }
    }
}