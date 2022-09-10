using GoFish.Data;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Linq;
using System.Threading;

namespace GoFish
{
    //callback interface
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void UpdateWindow(string message);
    }

    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IGoFishGame
    {
        [OperationContract]
        int GetPlayersCount();

        [OperationContract]
        Player GetPlayer(int playerId);

        [OperationContract(IsOneWay = true)]
        void PassCard(int playerfrom, int playerTo, Card card);

        [OperationContract]
        int JoinGame();

        [OperationContract(IsOneWay = true)]
        void LeaveGame(int playerId);

        [OperationContract(IsOneWay = true)]
        void Guess(int fromId, int toId, Card card);

        [OperationContract]
        int GetCurrentTurn();

        [OperationContract]
        int GetDeckSize();

        [OperationContract]
        List<int> GetPlayers();

        [OperationContract(IsOneWay = true)]
        void GoFish(int playerId); 

    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GoFishGame : IGoFishGame
    {

        private Shoe shoe;
        private int maxScore;
        int turn;
        public List<Tuple<Player, ICallback>> Players { get; set; }

        public GoFishGame()
        {
            Console.WriteLine("fishing started");
            shoe = new Shoe();
            Players = new List<Tuple<Player, ICallback>>();
            turn = -1;
            maxScore = 0;
        }

        private void Update(string message)
        {
            for(int i = 0; i < Players.Count; i++)
            {
                Players[i].Item2.UpdateWindow(message);
            }
        }

        public int GetCurrentTurn()
        {
            return turn;
        }

        public int GetPlayersCount()
        {
            return Players.Count;
        }

        public List<int> GetPlayers()
        {
            List<int> playerNumbers = new List<int>();
            for(int i = 0; i < Players.Count; i++)
            {
                playerNumbers.Add(Players[i].Item1.Id);
            }
            return playerNumbers;
        }

        public int GetDeckSize()
        {
            return shoe.Cards.Count;
        }

        public Player GetPlayer(int playerId)
        {
            return Players.Where(p => p.Item1.Id == playerId).First().Item1;
        }

        public int JoinGame()
        {
            ICallback callback = OperationContext.Current.GetCallbackChannel<ICallback>();
            Player player;
            if (Players.Count != 0)
                player = new Player(Players[Players.Count-1].Item1.Id + 1);
            else
                player = new Player(0);

            player.PlayingState = Player.PlayState.Wait;
            Players.Add(new Tuple<Player, ICallback>(player, callback));

            RefillHand(player.Id);

            Console.WriteLine($"{player} added to game");

            Update($"{player.Id} has joined the game");
            if (turn == -1)
            {
                turn = player.Id;
                player.PlayingState = Player.PlayState.Play;
            }

            getHandPairs(player.Id);

            return player.Id;
        }

        private void RefillHand(int playerId)
        {
            for (int i = 0; i < 6; i++)
            {
                GoFish(playerId);
            }
            getHandPairs(playerId);
        }

        private void NextTurn()
        {
            if (!CheckEnd())
            { 
                try
                {
                    int turnNumber = Players.IndexOf(Players.Where(p => p.Item1.PlayingState == Player.PlayState.Play).First());
                    Players[turnNumber].Item1.PlayingState = Player.PlayState.Wait;
                    turnNumber++;
                    if (turnNumber >= Players.Count)
                    {
                        turnNumber = 0;
                    }
                    Players[turnNumber].Item1.PlayingState = Player.PlayState.Play;
                    //at the end of the game when they can't select a card from their hand skip over them
                    if (Players[turnNumber].Item1.Hand.Count == 0)
                    {
                        NextTurn();
                    }
                }
                catch (Exception ex)
                {
                    Update(ex.Message);
                }
            }
            else
            {
                AnnounceEnd();
            }
        }

        public void LeaveGame(int playerId)
        {
            if(GetPlayer(playerId).Id == turn)
            {
                NextTurn();
            }

            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();

            Tuple<Player, ICallback> currentPlayer =
                    new Tuple<Player, ICallback>(GetPlayer(playerId), cb);

            List<Card> leftoverHand = new List<Card>();

            if(Players.Contains(currentPlayer))
            {
                leftoverHand = currentPlayer.Item1.Hand;
                Players.Remove(currentPlayer);
            }

            shoe.ReshuffleHand(leftoverHand);

            Update($"player {playerId} has left the game");
        }

        private void getHandPairs(int playerId)
        {
            int pairs = 0;
            Player player = GetPlayer(playerId);
            Card first, second;
            for(int i = 0; i < player.Hand.Count; i++)
            {
                first = player.Hand[i];
                for(int j = 1; j < player.Hand.Count; j++)
                {
                    second = player.Hand[j];
                    if (first.Pairs(second))
                    {
                        Thread.Sleep(500);
                        Update($"player {playerId} has made a pair of {first} and {second}");
                        pairs++;
                        player.Hand.Remove(first);
                        player.Hand.Remove(second);
                        i = 0;
                        j = 0;
                    }
                }
            }
            player.Score += pairs;

            if(player.Score > maxScore)
            {
                maxScore = player.Score;
            }

            if(player.Hand.Count == 0 && shoe.Cards.Count != 0)
            {
                RefillHand(playerId);
            }
            if(CheckEnd())
            {
                AnnounceEnd();
            }
        }

        public void Guess(int fromId, int toId, Card card)
        {
            if(findCard(card, fromId))
            {
                Card pair = GetPair(card, fromId);
                PassCard(fromId, toId, pair);
                NextTurn();
                Update($"player {fromId} gave card {pair} to player {toId}");
            }
            else
            {
                GoFish(toId);
                NextTurn();
                Update($"player {fromId} doesn't have a {card}. Go fish!");
            }
            getHandPairs(toId);

        }

        private bool findCard(Card findCard, int HandId)
        {
            foreach(var card in GetPlayer(HandId).Hand)
            {
                if (card.Pairs(findCard))
                    return true;
            }
            return false;
        }

        private Card GetPair(Card pair, int HandId)
        {
            foreach (var card in GetPlayer(HandId).Hand)
            {
                if (card.Pairs(pair))
                    return card;
            }
            return null;
        }

        public void GoFish(int playerId)
        {
            if(shoe.Cards.Count != 0)
                GetPlayer(playerId).GoFish(shoe.Cards.Pop());
        }

        public void PassCard(int playerfrom, int playerTo, Card card)
        {
            GetPlayer(playerfrom).Hand.Remove(card);
            GetPlayer(playerTo).Hand.Add(card);

            if(GetPlayer(playerfrom).Hand.Count == 0)
            {
                RefillHand(playerfrom);
            }
        }

        private bool CheckEnd()
        {
            int handCount = 0;
            for(int i = 0; i < Players.Count; i++)
            {
                handCount += Players[i].Item1.Hand.Count;
            }
            return (handCount == 0 && GetDeckSize() == 0);

        }

        //this is only spluit out so that formatting/timing can be preserved
        private void AnnounceEnd()
        {
            Update("The Game is finished!!!!");
            Update($"The most pairs for one person was {maxScore}");
            Update($"The player(s) with this score of {maxScore} was:");

            List<int> ids = new List<int>();
            for(int i = 0; i < Players.Count; i++)
            {
                if(Players[i].Item1.Score == maxScore)
                {
                    ids.Add(Players[i].Item1.Id);
                }
            }
            if(ids.Count != 0)
            {
                Update($"Players {string.Join<int>(", ", ids)} were winners!!!");
            }
            else
            {
                Update("A player that previously left the game had the high score");
            }
        }


    }
}
