using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DominoServer
{
    [DataContract]
    class Player
    {
        [DataMember]
        public string Nickname { get; set; }
        [DataMember]
        public int Games { get; set; }
        [DataMember]
        public int Wins { get; set; }
        [DataMember]
        public int MinScore { get; set; }

        public Player(string nickname, int games, int wins, int score)
        {
            Nickname = nickname;
            Games = games;
            Wins = wins;
            MinScore = score;
        }
    }
}
