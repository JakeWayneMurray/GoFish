using GoFish.Data;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace GoFishGame
{
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void Update();
    }

    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IGoFishGame
    {
        [OperationContract]
        int GetPlayersCount();

        [OperationContract]
        Player GetPlayer(int playerId);

        [OperationContract]
        void PassCard(int playerfrom, int playerTo, Card card);

        [OperationContract]
        int JoinGame();

        [OperationContract(IsOneWay = true)]
        void LeaveGame(int playerId);

        [OperationContract]
        void NextTurn();



        [OperationContract(IsOneWay = true)]
        void GoFish(int playerId);



    }

    class GoFishGame : IGoFishGame
    {

        private Shoe shoe;
        int turn;
        public List<Tuple<Player, ICallback>> Players { get; set; }

        public GoFishGame()
        {
            Console.WriteLine("fishing started");
            shoe = new Shoe();
            Players = new List<Tuple<Player, ICallback>>();
            turn = 0;
        }

        private void Update()
        {
            foreach (ICallback cb in Players)
            {
                cb.Update();
            }
        }

        public int GetPlayersCount()
        {
            return Players.Count;
        }

        public int GetDeckSize()
        {
            return shoe.Cards.Count;
        }

        public Player GetPlayer(int playerId)
        {
            return Players[playerId].Item1;
        }

        public int JoinGame()
        {
            ICallback callback = OperationContext.Current.GetCallbackChannel<ICallback>();

            Player player = new Player(Players.Count);
            player.PlayingState = Player.PlayState.Wait;
            Players.Add(new Tuple<Player, ICallback>(player, callback));

            RefillHand(player.Id);

            Console.WriteLine($"{player} added to game");

            return player.Id;
        }

        public void RefillHand(int playerId)
        {
            for (int i = 0; i < 6; i++)
            {
                GoFish(playerId);
            }
        }

        public void NextTurn()
        {
            Players[turn].Item1.PlayingState = Player.PlayState.Wait;
            turn++;
            if(turn > Players.Count)
            {
                turn = 0;
            }
            Players[turn].Item1.PlayingState = Player.PlayState.Play;
        }

        public void LeaveGame(int playerId)
        {
            if(playerId == turn)
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

            Update();
        }

        public void getHandPairs(int playerId)
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
                    if (first.Equals(second))
                    {
                        pairs++;
                        player.Hand.Remove(first);
                        player.Hand.Remove(second);
                        i = 0;
                        j = 0;
                    }
                }
            }
            player.Score += pairs;
        }

        public string Guess(int fromId, int toId, Card card)
        {
            if(findCard(card, fromId))
            {
                Card pair = GetPair(card, fromId);
                PassCard(fromId, toId, pair);
                return ($"player {fromId} gave card {pair} to player{toId}");
            }
            else
            {
                GoFish(toId);
                return ($"player {fromId} doesn't have a {card}. Go fish!");
            }
        }

        private bool findCard(Card findCard, int HandId)
        {
            foreach(var card in GetPlayer(HandId).Hand)
            {
                if (card.Equals(findCard))
                    return true;
            }
            return false;
        }

        private Card GetPair(Card pair, int HandId)
        {
            foreach (var card in GetPlayer(HandId).Hand)
            {
                if (card.Equals(pair))
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
        }




    }
}
