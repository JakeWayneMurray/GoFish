using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GoFish.Data
{
    [DataContract]
    public class Card
    {
        public enum SuitID
        {
            Clubs, Diamonds, Hearts, Spades
        };

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

        public override string ToString()
        {
            return Rank.ToString() + " of " + Suit.ToString();
        }

    }
}
