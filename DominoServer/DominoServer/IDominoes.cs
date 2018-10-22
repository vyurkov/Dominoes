using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;

namespace DominoServer
{
    [ServiceContract(CallbackContract = typeof(IDominoesCallback))]
    interface IDominoes
    {
        #region AccountAccess

        [OperationContract]
        int Registration(string user, string password, string reminderText);

        [OperationContract]
        int Login(string user, string password, out Player p);

        [OperationContract (IsOneWay = true)]
        void Logout(string nickname, bool isInGame);

        [OperationContract]
        string GetReminderText(string user);

        [OperationContract(IsOneWay = true)]
        void UpdatePlayerInfo(Player player, int score, bool isWin);

        #endregion

        [OperationContract(IsOneWay = true)]
        void CanJoinGame();

        #region Game

        [OperationContract(IsOneWay = true)]
        void GetBone(int pNumber);

        [OperationContract(IsOneWay = true)]
        void MakeMove(int index, int pNumber, Point p, double angle, Position pos, TableValues tv);

        [OperationContract(IsOneWay = true)]
        void SkipMove(int pNumber);

        #endregion
    }
}