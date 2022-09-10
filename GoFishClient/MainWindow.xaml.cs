using GoFish;
using GoFish.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GoFishClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    //enable callbacks
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback
    {
        DuplexChannelFactory<IGoFishGame> channel;
        IGoFishGame game;
        int playerId;
        string prevMessage;

        public MainWindow()
        {
            channel = new DuplexChannelFactory<IGoFishGame>(this, "GoFishEndpoint");
            game = channel.CreateChannel();
            playerId = game.JoinGame();
            prevMessage = "";

            InitializeComponent();
            UpdateButton(game.GetPlayer(playerId).PlayingState == Player.PlayState.Play);
            UpdateServer("");
            UpdateControls();
            this.Title = ($"Player {playerId}'s game");
        }

        public delegate void WindowUpdateDelegate(string message);

        //whenever the window is updated, it will show the message from the server
            //update if the button is enabled
            //update the controls to their new text values
        public void UpdateWindow(string message)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    UpdateControls();
                    UpdateButton(game.GetPlayer(playerId).PlayingState == Player.PlayState.Play);
                    UpdateServer(message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new WindowUpdateDelegate(UpdateWindow), message);
            }
        }

        //sets all controls/labels to their relevant values from the server
        public void UpdateControls()
        {
            CardCombo.ItemsSource = game.GetPlayer(playerId).Hand;
            PlayerCombo.ItemsSource = game.GetPlayers().Where(i => i != playerId).ToList();

            PlayersLabel.Content = game.GetPlayersCount().ToString();
            DeckLabel.Content = game.GetDeckSize().ToString();
            PairLabel.Content = game.GetPlayer(playerId).Score.ToString();
            StatusLabel.Content = game.GetPlayer(playerId).PlayingState.ToString();

            string handList = "";
            foreach(var card in game.GetPlayer(playerId).Hand)
            {
                handList += card.ToString() + "\n";
            }

            HandLabel.Content = handList;
        }

        //changing the buttons ability to be pressed
        public void UpdateButton(bool button)
        {           
            AskButton.IsEnabled = button;
        }

        //updating the box with messages from the server
        public void UpdateServer(string message)
        {
            ServerLabel.Content = prevMessage + "\n" + message;
            prevMessage = message;
        }

        //will check if you made a selection or not for your button presses
        private void AskButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(PlayerCombo.Text != null && CardCombo.SelectedItem != null)
                    game.Guess(int.Parse(PlayerCombo.Text), playerId, (Card)CardCombo.SelectedItem);
                else
                    UpdateServer("please make a selection");
            }
            catch (Exception ex)
            {
                UpdateServer("please make a selection");
            }
        }

        //when it closes down, leave the game and shuffle extra cards into the deck
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            game.LeaveGame(playerId);
        }
    }
}
