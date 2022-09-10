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
        public enum EndState { win, lose, tie};

        [DataMember] public int Id { get; set; }
        [DataMember] public int Score { get; set; }
        [DataMember] public List<Card> Hand { get; set; }
        [DataMember] public bool isCurrentTurn { get; set; }
        [DataMember] public PlayState PlayingState { get; set; }
        [DataMember] public EndState GameResults { get; set; }

        public Player(int id)
        {
            Id = id;
            Hand = new List<Card>();
            isCurrentTurn = false;
            PlayingState = PlayState.none;
            Score = 0;
        }
        public void GoFish(Card card)
        {
            Hand.Add(card);
        }
        public void Guess()
        {
            isCurrentTurn = false;
        }

        public string HandToString()
        {
            string HandString = "";
            foreach (var card in Hand)
            {
                HandString += card + ":";
            }
            return HandString;
        }

        public override string ToString()
        {
            return $"Player {Id}";
        }
    }
}
