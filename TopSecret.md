## Mega top secret algoritm
All infromation regarding the played/remaining cards is kept in CardMemorizer.cs.

Deck has cards logic

1. When our bot is first it makes a prediction of all the cards in his hand that can win - assumes all can win then compares them with all the cards in the deck & player and removes the once that can loose. Then it playes those winning cards (making check for announce) - when there are no matches it playes the weakest possible card. 
2. When our bot is second it does a check if any of the cards in the current hand can beat the other opponents card - if yes throws the highest if not throws the weakest.


Empty deck logic(same as closed game)

1. When the deck is closed we make the same possible winning cards check and we play them (here the announces are more importaint so even if we have to play a card - K or Q that will surely loose we play it so we can get the announce score) afterwards we contine with the logic of playing the "winner cards".
2. When our player is second follows the same logic as when deck has cards.
