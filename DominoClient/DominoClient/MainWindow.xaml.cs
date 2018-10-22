using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DominoClient.DominoesService;


namespace DominoClient
{
    public delegate void LoginDelegate(Player player);
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDominoesCallback
    {
        internal DominoesClient client;

        private System.Timers.Timer clock;
        private TimeSpan timeForMove;

        private Player player;
        private int playerNumber;
        private bool isLogedIn = false;
        private bool inGame = false;

        public TableValues tv;
        private int deck;
        private List<BoneButton> playerBones;
        private List<BoneButton> opponentBones;
        //private List<BoneButton> gameTable;
        private BoneButton middleGameTable;
        private List<BoneButton> leftGameTable;
        private List<BoneButton> rightGameTable;
        

        public MainWindow()
        {
            client = new DominoesClient(new InstanceContext(this));
            player = new Player();
            playerBones = new List<BoneButton>();
            opponentBones = new List<BoneButton>();
            //gameTable = new List<BoneButton>();
            middleGameTable = new BoneButton(new Bone(), BoneButton_Click, true);
            leftGameTable = new List<BoneButton>();
            rightGameTable = new List<BoneButton>();
            SetTimer();
            InitializeComponent();

        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow lw = new LoginWindow(new LoginDelegate(PlayerInit));
            lw.Owner = this;
            lw.ShowDialog();
        }

        private void PlayerInit(Player player)
        {
            this.player = player;
            JoinBtn.IsEnabled = StatisticBtn.IsEnabled = true;
            LoginBtn.IsEnabled = false;
            isLogedIn = true;
        }

        private void SetTimer()
        {
            clock = new System.Timers.Timer(1000);
            clock.Elapsed += OnTimedEvent;
            clock.AutoReset = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate()
            {
                if (timeForMove == TimeSpan.Zero) client.SkipMove(playerNumber);
                else
                {
                    timeForMove = timeForMove.Subtract(TimeSpan.FromSeconds(1));
                    TimeLabel.Content = String.Format("{0}:{1}", timeForMove.Minutes, timeForMove.Seconds);
                }
            }));
        }

        private void JoinBtn_Click(object sender, RoutedEventArgs e)
        {
            client.CanJoinGame();
        }

        private void DeckBtn_Click(object sender, RoutedEventArgs e)
        {
            if(deck == 0) MessageBox.Show("There are no bones in deck!");
            else
            {
                bool inHand = false;
                foreach (BoneButton b in playerBones)
                {
                    if (b._Bone.FirstValue == tv.Left || b._Bone.FirstValue == tv.Right ||
                        b._Bone.SecondValue == tv.Left || b._Bone.SecondValue == tv.Right) inHand = true;
                }
                if (inHand) MessageBox.Show("You already have bone in your hand to make move!");
                else
                {
                    client.GetBone(playerNumber);
                    if (CheckSkipMove()) client.SkipMove(playerNumber);
                }
            }
        }

        private void StatisticBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(String.Format("Nickname: {0}\nTotal Games: {1}\nWins: {2}\nMinimal score: {3}",
                player.Nickname, player.Games, player.Wins, player.MinScore));
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (isLogedIn)
            {
                if (inGame) client.Logout(player.Nickname, true);
                else client.Logout(player.Nickname, false);
            }
            clock.Close();
            clock.Dispose();
        }

        #region GameMethods

        private bool CheckSkipMove()
        {
            if (deck == 0)
            {
                foreach (BoneButton b in playerBones)
                {
                    if (b._Bone.FirstValue == tv.Left || b._Bone.FirstValue == tv.Right ||
                        b._Bone.SecondValue == tv.Left || b._Bone.SecondValue == tv.Right) return false;
                }
                return true;
            }
            return false;
        }

        private void BoneButton_Click(object sender, RoutedEventArgs e)
        {
            BoneButton tmp = sender as BoneButton;
            int index = playerBones.IndexOf((sender as BoneButton));
            BoneButton b;
            Point coords = new Point();
            Position pos;
            double angle = 0;
            
            if (tmp._Bone.FirstValue == tv.Left || tmp._Bone.SecondValue == tv.Left)
            {
                bool isFirstValue = tmp._Bone.FirstValue == tv.Left;
                switch (leftGameTable.Count)
                {
                    case 0:
                        coords.X = isFirstValue ? middleGameTable._Bone.Coords.X : middleGameTable._Bone.Coords.X - 80;
                        coords.Y = isFirstValue ? middleGameTable._Bone.Coords.Y - 40 : middleGameTable._Bone.Coords.Y;
                        angle = isFirstValue ? 90 : 270;
                        break;
                    case 1: case 2: case 3:
                        b = leftGameTable.Last();
                        coords.X = isFirstValue ? 
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.X - 80 : b._Bone.Coords.X :
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.X - 160 : b._Bone.Coords.X - 80;
                        coords.Y = isFirstValue ?
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.Y : b._Bone.Coords.Y - 40 :
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.Y + 40 : b._Bone.Coords.Y;
                        angle = isFirstValue ? 90 : 270;
                        break;
                    case 4:
                        b = leftGameTable.Last();
                        coords.X = isFirstValue ?
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.X - 80 : b._Bone.Coords.X :
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.X - 120 : b._Bone.Coords.X - 40;
                        coords.Y = isFirstValue ?
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.Y + 40 : b._Bone.Coords.Y :
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.Y - 40 : b._Bone.Coords.Y - 80;
                        angle = isFirstValue ? 180 : 0;
                        break;
                    case 5: case 6:
                        b = leftGameTable.Last();
                        coords.X = isFirstValue ?
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.X + 40 : b._Bone.Coords.X :
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.X : b._Bone.Coords.X - 40;
                        coords.Y = isFirstValue ?
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.Y : b._Bone.Coords.Y - 80:
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.Y - 80 : b._Bone.Coords.Y - 160;
                        angle = isFirstValue ? 180 : 0;
                        break;
                    case 7:
                        b = leftGameTable.Last();
                        coords.X = isFirstValue ?
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.X + 40 : b._Bone.Coords.X :
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.X + 120: b._Bone.Coords.X + 80;
                        coords.Y = isFirstValue ?
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.Y + 40: b._Bone.Coords.Y - 40 :
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.Y : b._Bone.Coords.Y - 80;
                        angle = isFirstValue ? 270 : 90;
                        break;
                    case 8: case 9: case 10: case 11: case 12: case 13: case 14: case 15:
                        b = leftGameTable.Last();
                        coords.X = isFirstValue ?
                            (int)b._Bone.Angle == 270 ? b._Bone.Coords.X + 80 : b._Bone.Coords.X :
                            (int)b._Bone.Angle == 270 ? b._Bone.Coords.X + 160 : b._Bone.Coords.X + 80;
                        coords.Y = isFirstValue ?
                            (int)b._Bone.Angle == 270 ? b._Bone.Coords.Y : b._Bone.Coords.Y + 40 :
                            (int)b._Bone.Angle == 270 ? b._Bone.Coords.Y - 40 : b._Bone.Coords.Y;
                        angle = isFirstValue ? 270 : 90;
                        break;
                    case 16:
                    case 17:
                        break;
                }
                tv.Left = isFirstValue ? tmp._Bone.SecondValue : tmp._Bone.FirstValue;
                pos = Position.L;
                client.MakeMove(index, playerNumber, coords, angle, pos, tv);
                return;
            }
            if (tmp._Bone.FirstValue == tv.Right || tmp._Bone.SecondValue == tv.Right)
            {
                bool isFirstValue = tmp._Bone.FirstValue == tv.Right;
                switch (rightGameTable.Count)
                {
                    case 0:
                        coords.X = isFirstValue ? middleGameTable._Bone.Coords.X + 80 : middleGameTable._Bone.Coords.X + 160;
                        coords.Y = isFirstValue ? middleGameTable._Bone.Coords.Y : middleGameTable._Bone.Coords.Y - 40;
                        angle = isFirstValue ? 270 : 90;
                        break;
                    case 1: case 2: case 3:
                        b = rightGameTable.Last();
                        coords.X = isFirstValue ?
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.X : b._Bone.Coords.X + 80:
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.X + 80 : b._Bone.Coords.X + 160;
                        coords.Y = isFirstValue ?
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.Y + 40: b._Bone.Coords.Y :
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.Y : b._Bone.Coords.Y - 40;
                        angle = isFirstValue ? 270 : 90;
                        break;
                    case 4:
                        b = rightGameTable.Last();
                        coords.X = isFirstValue ?
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.X : b._Bone.Coords.X + 80:
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.X + 40 : b._Bone.Coords.X + 120;
                        coords.Y = isFirstValue ?
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.Y : b._Bone.Coords.Y - 40:
                            (int)b._Bone.Angle == 90 ? b._Bone.Coords.Y + 80 : b._Bone.Coords.Y + 40;
                        angle = isFirstValue ? 0 : 180;
                        break;
                    case 5: case 6:
                        b = rightGameTable.Last();
                        coords.X = isFirstValue ?
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.X : b._Bone.Coords.X - 40 :
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.X + 40 : b._Bone.Coords.X;
                        coords.Y = isFirstValue ?
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.Y + 80 : b._Bone.Coords.Y :
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.Y + 160 : b._Bone.Coords.Y + 80;
                        angle = isFirstValue ? 0 : 180;
                        break;
                    case 7:
                        b = rightGameTable.Last();
                        coords.X = isFirstValue ?
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.X : b._Bone.Coords.X - 40:
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.X - 80 : b._Bone.Coords.X -120;
                        coords.Y = isFirstValue ?
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.Y + 40 : b._Bone.Coords.Y - 40 :
                            (int)b._Bone.Angle == 0 ? b._Bone.Coords.Y + 80 : b._Bone.Coords.Y;
                        angle = isFirstValue ? 90 : 270;
                        break;
                    case 8: case 9: case 10: case 11: case 12: case 13: case 14: case 15:
                        b = rightGameTable.Last();
                        coords.X = isFirstValue ?
                            (int)b._Bone.Angle == 270 ? b._Bone.Coords.X : b._Bone.Coords.X - 80 :
                            (int)b._Bone.Angle == 270 ? b._Bone.Coords.X - 80 : b._Bone.Coords.X - 160;
                        coords.Y = isFirstValue ?
                            (int)b._Bone.Angle == 270 ? b._Bone.Coords.Y - 40 : b._Bone.Coords.Y :
                            (int)b._Bone.Angle == 270 ? b._Bone.Coords.Y : b._Bone.Coords.Y + 40;
                        angle = isFirstValue ? 90 : 270;
                        break;
                    case 16:
                    case 17:
                        break;
                }
                tv.Right = isFirstValue ? tmp._Bone.SecondValue : tmp._Bone.FirstValue;
                pos = Position.R;
                client.MakeMove(index, playerNumber, coords, angle, pos, tv);
                return;
            }
        }

        #endregion

        #region CallBackMethods

        public void SetPlayerNumber(int pNumber)
        {
            if (pNumber == -1)
            {
                MessageBox.Show("A game is already in progress, please try again later");
            }
            else
            {
                NicknameLabel.Content = player.Nickname;
                playerNumber = pNumber;
                JoinBtn.IsEnabled = false;
                ActionLabel.Content = "Waiting for opponents!";
                inGame = true;
            }
        }

        public void UpdateGameInfo(int currTurn, Bone[][] players, Bone[] table, int deck, int game, int[] scores, TableValues tableValues, bool changeMove)
        {
            playerBones.Clear();
            opponentBones.Clear();
            //gameTable.Clear();
            middleGameTable = null;
            leftGameTable.Clear();
            rightGameTable.Clear();
            PlayerField.Children.Clear();
            FirstOpponentField.Children.Clear();
            GameField.Children.Clear();

            for (int i = 0; i < players.Length; i++)
            {
                for (int j = 0; j < players[i].Length; j++)
                {
                    if (i == playerNumber) playerBones.Add(new BoneButton(players[i][j], BoneButton_Click));
                    else
                        opponentBones.Add(new BoneButton(new Bone() { FirstValue = 0, SecondValue = 0, Coords = new Point() }, BoneButton_Click, true));
                }
            }

            for (int i = 0; i < playerBones.Count; i++) PlayerField.Children.Add(playerBones[i]);

            for (int i = 0; i < opponentBones.Count; i++) FirstOpponentField.Children.Add(opponentBones[i]);

            for (int i = 0; i < table.Length; i++)
            {
                //gameTable.Add(new BoneButton(table[i], BoneButton_Click, true));
                switch (table[i].Pos)
                {
                    case Position.M: middleGameTable = new BoneButton(table[i], BoneButton_Click, true);
                        break;
                    case Position.L: leftGameTable.Add(new BoneButton(table[i], BoneButton_Click, true));
                        break;
                    case Position.R: rightGameTable.Add(new BoneButton(table[i], BoneButton_Click, true));
                        break;
                }
            }

            //for (int i = 0; i < gameTable.Count; i++)
            //{
            //    gameTable[i].RenderTransform = new RotateTransform(gameTable[i]._Bone.Angle);
            //    Canvas.SetTop(gameTable[i], gameTable[i]._Bone.Coords.Y);
            //    Canvas.SetLeft(gameTable[i], gameTable[i]._Bone.Coords.X);
            //    GameField.Children.Add(gameTable[i]);
            //}

            middleGameTable.RenderTransform = new RotateTransform(middleGameTable._Bone.Angle);
            Canvas.SetTop(middleGameTable, middleGameTable._Bone.Coords.Y);
            Canvas.SetLeft(middleGameTable, middleGameTable._Bone.Coords.X);
            GameField.Children.Add(middleGameTable);
            

            for (int i = 0; i < leftGameTable.Count; i++)
            {
                leftGameTable[i].RenderTransform = new RotateTransform(leftGameTable[i]._Bone.Angle);
                Canvas.SetTop(leftGameTable[i], leftGameTable[i]._Bone.Coords.Y);
                Canvas.SetLeft(leftGameTable[i], leftGameTable[i]._Bone.Coords.X);
                GameField.Children.Add(leftGameTable[i]);
            }

            for (int i = 0; i < rightGameTable.Count; i++)
            {
                rightGameTable[i].RenderTransform = new RotateTransform(rightGameTable[i]._Bone.Angle);
                Canvas.SetTop(rightGameTable[i], rightGameTable[i]._Bone.Coords.Y);
                Canvas.SetLeft(rightGameTable[i], rightGameTable[i]._Bone.Coords.X);
                GameField.Children.Add(rightGameTable[i]);
            }

            this.deck = deck;
            DeckLabel.Content = deck.ToString();
            GameCountLabel.Content = game.ToString();
            ScoreCountLabel.Content = scores[playerNumber].ToString();
            tv.Left = tableValues.Left;
            tv.Right = tableValues.Right;

            if (playerNumber == currTurn)
            {
                if (CheckSkipMove()) client.SkipMove(playerNumber);
                ActionLabel.Content = "Your move!";
                PlayerField.IsEnabled = true;
                DeckBtn.IsEnabled = true;
                if (changeMove)
                {
                    timeForMove = new TimeSpan(0, 2, 0);
                    clock.Start();
                }
            }
            else
            {
                ActionLabel.Content = "Opponents move!";
                PlayerField.IsEnabled = false;
                DeckBtn.IsEnabled = false;
                clock.Stop();
            }
        }

        public void OpponentExit()
        {
            playerNumber = -1;
            tv.Left = -1;
            tv.Right = -1;
            clock.Stop();

            JoinBtn.IsEnabled = true;
            DeckBtn.IsEnabled = false;
            ActionLabel.Content = "";
            DeckLabel.Content = NicknameLabel.Content = GameCountLabel.Content = ScoreCountLabel.Content = "";
            inGame = false;

            playerBones.Clear();
            opponentBones.Clear();
            //gameTable.Clear();
            middleGameTable = null;
            leftGameTable.Clear();
            rightGameTable.Clear();

            PlayerField.Children.Clear();
            FirstOpponentField.Children.Clear();
            GameField.Children.Clear();
        }

        public void GameOver(int pNumber, int[] scores)
        {
            player.Games += 1;
            if (playerNumber != pNumber)
            {
                int score = (scores[playerNumber] < player.MinScore) ? scores[playerNumber] : 0;
                player.Wins += 1;
                client.UpdatePlayerInfo(player, score, true);
                MessageBox.Show("The game is over! You won this game! Congratulation!");
            }
            else
            {
                MessageBox.Show("The game is over! You lose this game! Lucky next time!");
                client.UpdatePlayerInfo(player, 0, false);
            }
            OpponentExit();
        }

        #endregion
    }
}
