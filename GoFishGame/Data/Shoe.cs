using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoFish.Data
{
    public class Shoe
    {
        public Stack<Card> Cards { get; set; }

        public Shoe()
        {
            List<Card> cards = createDeck();
            Cards = new Stack<Card>(cards.OrderBy(c => Guid.NewGuid())); //shuffle
        }

        private List<Card> createDeck()
        {
            List<Card> deck = new List<Card>();
            //create 4 of each type of card
            foreach (var suit in Enum.GetValues(typeof(Card.SuitID)))
            {
                foreach (var rank in Enum.GetValues(typeof(Card.RankID)))
                {
                    deck.Add(new Card((Card.RankID)rank, (Card.SuitID)suit));
                }
            }

            return deck;
        }

        //shuffles someone's hand back into the deck when they leave
        public void ReshuffleHand(List<Card> cards)
        {
            while(Cards.Count != 0)
            {
                cards.Add(Cards.Pop());
            }
            Cards = new Stack<Card>(cards.OrderBy(c => Guid.NewGuid())); 
        }

    }
}
