using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DominoServer
{
    interface IDominoesCallback
    {
        [OperationContract(IsOneWay = true)]
        void SetPlayerNumber(int pNumber);

        [OperationContract(IsOneWay = true)]
        void UpdateGameInfo(int currTurn, List<Bone>[] players, List<Bone> table, int deck, int game, int[] scores,
            TableValues tv, bool changeMove);

        [OperationContract(IsOneWay = true)]
        void OpponentExit();

        [OperationContract(IsOneWay = true)]
        void GameOver(int pNumber, int[] scores);

    }
}
