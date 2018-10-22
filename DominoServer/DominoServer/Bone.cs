using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DominoServer
{
    enum Position
    {
        L,
        R,
        M
    };
    

    [DataContract]
    class Bone
    {
        [DataMember] public int FirstValue { get; set; }

        [DataMember] public int SecondValue { get; set; }

        [DataMember] public Point Coords { get; set; }

        [DataMember] public Position Pos { get; set; }

        [DataMember] public double Angle { get; set; }

        public Bone(int l, int r)
        {
            FirstValue = l;
            SecondValue = r;
            Coords = new Point();
        }

        public static bool operator ==(Bone b1, Bone b2)
        {
            return ((b1.FirstValue == b2.FirstValue) && (b1.SecondValue == b2.SecondValue));
        }

        public static bool operator !=(Bone b1, Bone b2)
        {
            return ((b1.FirstValue != b2.FirstValue) || (b1.SecondValue != b2.SecondValue));
        }
    }
}
