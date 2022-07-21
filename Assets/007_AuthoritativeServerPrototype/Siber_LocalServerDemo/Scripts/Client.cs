using System;
using System.Collections.Generic;
using UnityEngine;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    /// <summary>
    /// 處理 A , B 角色 的動作
    /// </summary>
    public class Client
    {
        private MediumManager mediumManager;
        private ControlSystem controlSystem;

        private Dictionary<string, BallBehaviour> ballDict = new Dictionary<string, BallBehaviour>();
        private string                            mainBallID;
        private string                            clientID;

        public Client(MediumManager mediumManager, ControlSystem controlSystem, string clientID)
        {
            this.clientID      = clientID;
            this.mediumManager = mediumManager;
            this.controlSystem = controlSystem;
        }

        public void Tick()
        {
            if (controlSystem.MoveLeft())
            {
                MoveAction(mainBallID, -1);
                // ballDict[cacheID].Move(-1 * Time.deltaTime * 10f);
            }

            if (controlSystem.MoveRight())
            {
                MoveAction(mainBallID, 1);
                // ballDict[cacheID].Move(1 * Time.deltaTime * 10f);
            }
        }

        public void MoveAction(string ID, float X)
        {
            var inputAction = new InputAction();
            inputAction.clientID = clientID;
            inputAction.BallID   = ID;
            inputAction.DateTime = DateTime.Now;
            inputAction.X        = X;
            mediumManager.SendToServer(inputAction);
        }

        public GameObject CreateBall(BallData ballData, GameObject ballPrefab)
        {
            mainBallID = ballData.ID;
            var ball          = GameObject.Instantiate(ballPrefab, ballData.Pos, Quaternion.identity);
            var ballBehaviour = ball.GetComponent<BallBehaviour>();
            ballDict.Add(ballData.ID, ballBehaviour);
            return ball;
        }
    }
}