using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GoFish.Data
{
    [DataContract]
    public class Player
    {
        public enum PlayState { none, Play, Wait };

        [DataMember] public int Id { get; set; }
        [DataMember] public int Score { get; set; }
        [DataMember] public List<Card> Hand { get; set; }
        [DataMember] public PlayState PlayingState { get; set; }

        //initialize to the most basic state
        public Player(int id)
        {
            Id = id;
            Hand = new List<Card>();
            PlayingState = PlayState.none;
            Score = 0;
        }
        //just adds the card to their hand
        public void GoFish(Card card)
        {
            Hand.Add(card);
        }

        public override string ToString()
        {
            return $"Player {Id}";
        }
    }
}
