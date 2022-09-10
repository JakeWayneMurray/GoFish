using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GoFish.Data
{
    [DataContract]
    public class Card
    {
        //not important for go fish, but makes the text more interesting
        public enum SuitID
        {
            Clubs, Diamonds, Hearts, Spades
        };

        //ranks the cards out ace to king
        public enum RankID
        {
            Ace,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            Ten,
            Jack,
            Queen,
            King
        };
        [DataMember]
        public RankID Rank { get; set; }
        [DataMember]
        public SuitID Suit { get; set; }


        public Card(RankID rank, SuitID suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public bool Equals(Card card)
        {
            return this.Rank == card.Rank;
        }

        //checks if the cards are the same rank but different suits, useful for seraching for pairs in hand
        public bool Pairs(Card card)
        {
            return (this.Rank == card.Rank) && (this.Suit != card.Suit);
        }

        public override string ToString()
        {
            return Rank.ToString() + " of " + Suit.ToString();
        }

    }
}
