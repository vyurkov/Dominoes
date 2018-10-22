using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace DominoServer
{
    [DataContract]
    struct TableValues
    {
        [DataMember]
        public int Left { get; set; }
        [DataMember]
        public int Right { get; set; }
    }

    class Game
    {
        public int CheckFirstMove(ref List<Bone>[] players, ref List<Bone> table, ref TableValues tv, int turn = -1)
        {
            Bone[] equalsBones =
            {
                new Bone(1, 1), new Bone(2, 2), new Bone(3, 3), new Bone(4, 4), new Bone(5, 5), new Bone(6, 6), new Bone(0, 0),
                new Bone(5, 6), new Bone(4, 6), new Bone(4, 5), new Bone(3, 6), new Bone(3, 5), new Bone(3, 4),
                new Bone(2, 6), new Bone(2, 5), new Bone(2, 4), new Bone(2, 3),
                new Bone(1, 6), new Bone(1, 5), new Bone(1, 4), new Bone(1, 3), new Bone(1, 2),
                new Bone(0, 6), new Bone(0, 5), new Bone(0, 4), new Bone(0, 3), new Bone(0, 2), new Bone(0, 1)
            };

            for (int i = 0; i < equalsBones.Length; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (turn != -1)
                    {
                        if (players[turn][j] == equalsBones[i])
                        {
                            players[turn][j].Coords = new Point(410, 270);
                            players[turn][j].Pos = Position.M;
                            players[turn][j].Angle = 270;
                            table.Add(players[turn][j]);
                            tv.Left = players[turn][j].FirstValue;
                            tv.Right = players[turn][j].SecondValue;
                            players[turn].RemoveAt(j);
                            return turn;
                        }
                    }
                    else {
                        for (int k = 0; k < players.Length; k++)
                        {
                            if (players[k][j] == equalsBones[i])
                            {
                                players[k][j].Coords = new Point(420, 270);
                                players[k][j].Pos = Position.M;
                                players[k][j].Angle = 270;
                                table.Add(players[k][j]);
                                tv.Left = players[k][j].FirstValue;
                                tv.Right = players[k][j].SecondValue;
                                players[k].RemoveAt(j);
                                return k;
                            }
                        }
                    }
                }
            }
            return turn;
        }

        public bool CheckRoundEnd(List<Bone>[] players, int deck, TableValues tv)
        {
            if (deck == 0)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    foreach (Bone b in players[i])
                    {
                        if (b.FirstValue == tv.Left || b.FirstValue == tv.Right ||
                            b.SecondValue == tv.Left || b.SecondValue == tv.Right) return false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool CheckGameOver(int[] playersScores, out int win)
        {
            for (int i = 0; i < playersScores.Length; i++)
            {
                if (playersScores[i] >= 100)
                {
                    win = i;
                    return true;
                }
            }

            win = -1;
            return false;
        }
    }
}
