using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DominoServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant, IncludeExceptionDetailInFaults = true)]
    class Dominoes:IDominoes
    {
        public static ReaderWriterLockSlim UserThreadLock = new ReaderWriterLockSlim();
        
        private static string newpath = Environment.CurrentDirectory;
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB; AttachDbFilename=\"" + newpath + "\\DominoesDB.mdf\"; Initial Catalog=Accounts; Integrated Security=True";

        private List<Player> users = new List<Player>();
        private const int MAX_ACTIVE_PLAYERS = 2;
        private static int activePlayers = 0;

        #region GameFields

        private Random rnd = new Random();
        private Game game = new Game();

        private static List<IDominoesCallback> subscribers = new List<IDominoesCallback>();

        private List<Bone>[] players;
        private List<Bone> deck;
        private List<Bone> table;
        private TableValues tableValues;
        private int[] playersScores;

        private int gamesPlayed = 0;
        private int currentPlayerTurn = -1;

        #endregion

        public Dominoes()
        {
            players = new List<Bone>[MAX_ACTIVE_PLAYERS];
            deck = new List<Bone>();
            table = new List<Bone>();
            playersScores = new int[MAX_ACTIVE_PLAYERS];

            for (int i = 0; i < 7; i++)
            {
                for (int j = i; j < 7; j++)
                {
                    deck.Add(new Bone(i, j));
                }
            }
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = new List<Bone>();
                playersScores[i] = 0;
            }
        }

        #region AccountAccessMethods

        public int Login(string user, string password, out Player p)
        {
            //0 success
            //1 incorrect password
            //2 already online 
            //3 attempt

            string[] Words = { "--", "\"", "'", ";--", ";", "/*", "*/", "@@", "@", "char", "nchar", "varchar", "nvarchar", "alter", "begin", "cast", "create", "cursor", "declare", "delete", "drop", "end", "exec", "execute", "fetch", "insert", "kill", "select", "sys", "sysobjects", "syscolumns", "table", "update" };

            UserThreadLock.EnterReadLock();
            Player gamer = users.Find(u => u.Nickname == user);
            UserThreadLock.ExitReadLock();

            if (gamer != null) { p = null; return 2; }

            //Check for sqlInjection
            if (Words.Any(word => user.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0 ||
                password.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)) { p = null; return 3; }

            try
            {
                string cmdText = "SELECT * FROM Accounts WHERE Login='" + user + "'";
                using (SqlConnection dbConnection = new SqlConnection(connectionString))
                {
                    dbConnection.Open();
                    SqlCommand cmd = new SqlCommand(cmdText, dbConnection);
                    SqlDataReader res = cmd.ExecuteReader();

                    if (res.Read())
                    {
                        string pass = res.GetString(2);
                        int games = res.GetInt32(4);
                        int wins = res.GetInt32(5);
                        int minscore = res.GetInt32(6);

                        res.Close();
                        dbConnection.Close();

                        if (password == pass)
                        {
                            p = new Player(user, games, wins, minscore);
                            UserThreadLock.EnterWriteLock();
                            users.Add(p);
                            UserThreadLock.ExitWriteLock();
                            return 0;
                        }

                        p = null;
                        return 1;
                    }

                    res.Close();
                    dbConnection.Close();
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("ERROR: " + exc.Message + "\r\n" + exc.StackTrace);
            }

            p = null;
            return 4;
        }
        
        public void Logout(string nickname, bool isInGame)
        {
            int index = users.IndexOf(users.Find(u => u.Nickname == nickname));

            if (isInGame)
            {
                try
                {
                    string cmdText = "UPDATE Accounts SET Games=" + ++users[index].Games + " WHERE Login='" +
                                     users[index].Nickname + "'";
                    using (SqlConnection dbConnection = new SqlConnection(connectionString))
                    {
                        dbConnection.Open();
                        SqlCommand cmd = new SqlCommand(cmdText, dbConnection);
                        cmd.ExecuteNonQuery();
                        dbConnection.Close();
                    }

                    IDominoesCallback callback = OperationContext.Current.GetCallbackChannel<IDominoesCallback>();
                    if (subscribers.Contains(callback)) subscribers.Remove(callback);
                    SetUpGame();
                    for (int i = 0; i < subscribers.Count; i++)
                    {
                        subscribers[i].OpponentExit();
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine("ERROR: " + exc.Message + "\r\n" + exc.StackTrace);
                }
                activePlayers--;
            }
            users.RemoveAt(index);
        }

        public int Registration(string user, string password, string reminderText)
        {
            //0 success 
            //1 fail
            //2 attempt

            string[] Words = { "--", "\"", "'", ";--", ";", "/*", "*/", "@@", "@", "char", "nchar", "varchar", "nvarchar", "alter", "begin", "cast", "create", "cursor", "declare", "delete", "drop", "end", "exec", "execute", "fetch", "insert", "kill", "select", "sys", "sysobjects", "syscolumns", "table", "update" };

            //Check for sqlInjection
            if (Words.Any(word => user.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                  password.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)) return 2;

            try
            {
                string cmdText = "SELECT Login FROM Accounts WHERE Login='" + user + "'";
                using (SqlConnection dbConnection = new SqlConnection(connectionString))
                {
                    dbConnection.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = dbConnection;
                    cmd.CommandText = cmdText;
                    SqlDataReader sdr = cmd.ExecuteReader();
                    if (sdr.Read()) { sdr.Close(); dbConnection.Close(); return 1; }
                    sdr.Close();
                    cmdText = "INSERT INTO Accounts VALUES('" + user + "', '" + password + "', '" + reminderText +
                              "', '" + 0 + "', '" + 0 + "', '" + 0 + "')";
                    cmd.CommandText = cmdText;
                    cmd.ExecuteNonQuery();
                    dbConnection.Close();
                }
                return 0;
            }
            catch (Exception exc)
            {
                Console.WriteLine("ERROR: " + exc.Message + "\r\n" + exc.StackTrace);
            }
            return 3;
        }

        public string GetReminderText(string user)
        {
            string[] Words = { "--", "\"", "'", ";--", ";", "/*", "*/", "@@", "@", "char", "nchar", "varchar", "nvarchar", "alter", "begin", "cast", "create", "cursor", "declare", "delete", "drop", "end", "exec", "execute", "fetch", "insert", "kill", "select", "sys", "sysobjects", "syscolumns", "table", "update" };

            //Check for sqlInjection
            if (Words.Any(word => user.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)) return "Error: You tried to use forbidden characters!";

            try
            {
                string cmdText = "SELECT Reminder FROM Accounts WHERE Login='" + user + "'";
                using (SqlConnection dbConnection = new SqlConnection(connectionString))
                {
                    dbConnection.Open();

                    SqlCommand cmd = new SqlCommand(cmdText, dbConnection);
                    SqlDataReader res = cmd.ExecuteReader();

                    if (res.Read())
                    {
                        string remind = res.GetString(0);
                        res.Close();
                        dbConnection.Close();
                        return remind;
                    }

                    res.Close();
                    dbConnection.Close();
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("ERROR: " + exc.Message + "\r\n" + exc.StackTrace);
            }
            return "Error: Something going wrong!";
        }

        public void UpdatePlayerInfo(Player player, int score, bool isWin)
        {
            try
            {
                string setStatistic;
                if (isWin)
                    setStatistic = (score > 0)
                        ? "Games=" + player.Games + ", Wins=" + player.Wins + ", MinScore=" + score
                        : "Games=" + player.Games + ", Wins=" + player.Wins;
                else setStatistic = "Games=" + player.Games;
                string cmdText = "UPDATE Accounts SET " + setStatistic + "WHERE Login='" + player.Nickname + "'";
                using (SqlConnection dbConnection = new SqlConnection(connectionString))
                {
                    dbConnection.Open();

                    SqlCommand cmd = new SqlCommand(cmdText, dbConnection);
                    cmd.ExecuteNonQuery();
                    dbConnection.Close();
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("ERROR: " + exc.Message + "\r\n" + exc.StackTrace);
            }
        }

        #endregion

        public void CanJoinGame()
        {
            try
            {
                IDominoesCallback callback = OperationContext.Current.GetCallbackChannel<IDominoesCallback>();
                if (!subscribers.Contains(callback)) subscribers.Add(callback);

                //If game in progress
                if (activePlayers >= MAX_ACTIVE_PLAYERS)
                {
                    callback.SetPlayerNumber(-1);
                    subscribers.Remove(callback);
                    return;
                }

                for (int i = 0; i < 7; i++) SetBone(activePlayers);

                callback.SetPlayerNumber(activePlayers);
                activePlayers++;

                if (activePlayers == MAX_ACTIVE_PLAYERS)
                {
                    int move = game.CheckFirstMove(ref players, ref table, ref tableValues);
                    currentPlayerTurn = (move == -1) ? 0 : (move == (players.Length - 1)) ? move - move : ++move;
                    gamesPlayed++;
                    foreach (IDominoesCallback sub in subscribers)
                        sub.UpdateGameInfo(currentPlayerTurn, players, table, deck.Count, gamesPlayed, playersScores, tableValues, true);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("ERROR: " + exc.Message + "\r\n" + exc.StackTrace);
            }
        }

        private void SetUpGame(bool roundEnd = false)
        {
            for (int i = 0; i < players.Length; i++)
            {
                players[i].Clear();
            }

            table.Clear();
            deck.Clear();

            for (int i = 0; i < 7; i++)
            {
                for (int j = i; j < 7; j++)
                {
                    deck.Add(new Bone(i, j));
                }
            }

            tableValues.Left = -1;
            tableValues.Right = -1;
            if (roundEnd)
            {
                gamesPlayed++;
                for (int i = 0; i < players.Length; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        SetBone(i);
                    }
                }
                int move = game.CheckFirstMove(ref players, ref table, ref tableValues, currentPlayerTurn);
                currentPlayerTurn = (move == -1) ? 0 : (move == (players.Length - 1)) ? move - move : ++move;
            }
            else
            {
                gamesPlayed = 0;
                currentPlayerTurn = -1;
                for (int i = 0; i < players.Length; i++)
                {
                    playersScores[i] = 0;
                }
            }
        }

        #region GameMethods

        public void SetBone(int pNumber)
        {
            if (deck.Count == 0) return;
            int t = rnd.Next(deck.Count);
            Bone tmp = deck[t];
            players[pNumber].Add(tmp);
            deck.RemoveAt(t);
        }

        public void GetBone(int pNumber)
        {
            SetBone(pNumber);
            foreach (IDominoesCallback sub in subscribers)
                sub.UpdateGameInfo(currentPlayerTurn, players, table, deck.Count, gamesPlayed, playersScores, tableValues, false);
        }

        public void MakeMove(int index, int pNumber, Point p, double angle, Position pos, TableValues tv)
        {
            Bone tmp = players[pNumber][index];
            tmp.Coords = new Point(p.X, p.Y);
            tmp.Angle = angle;
            tmp.Pos = pos;
            tableValues.Left = tv.Left;
            tableValues.Right = tv.Right;
            players[pNumber].RemoveAt(index);
            table.Add(tmp);
            SkipMove(pNumber);

            if (players[pNumber].Count == 0 || game.CheckRoundEnd(players, deck.Count, tableValues))
            {
                for (int i = 0; i < players.Length; i++)
                {
                    if(players[i].Count == 0) continue;
                    for (int j = 0; j < players[i].Count; j++)
                    {
                        if (players[i].Count == 1 && players[i][j].FirstValue == 0 && players[i][j].SecondValue == 0)
                            playersScores[i] += 10;
                        else playersScores[i] += (players[i][j].FirstValue + players[i][j].SecondValue);
                    }
                }

                int win;
                if (game.CheckGameOver(playersScores, out win))
                {
                    foreach (IDominoesCallback sub in subscribers)
                        sub.GameOver(win, playersScores);
                    SetUpGame();
                    return;
                }

                SetUpGame(true);
               
                foreach (IDominoesCallback sub in subscribers)
                    sub.UpdateGameInfo(currentPlayerTurn, players, table, deck.Count, gamesPlayed, playersScores, tableValues, true);
            }
        }

        public void SkipMove(int pNumber)
        {
            currentPlayerTurn = (pNumber == (players.Length - 1)) ? pNumber - pNumber : ++pNumber;
            foreach (IDominoesCallback sub in subscribers)
                sub.UpdateGameInfo(currentPlayerTurn, players, table, deck.Count, gamesPlayed, playersScores, tableValues, true);
        }

        #endregion
    }
}
