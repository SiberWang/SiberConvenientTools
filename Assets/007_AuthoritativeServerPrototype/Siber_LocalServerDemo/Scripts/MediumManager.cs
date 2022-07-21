using System;
using System.Collections.Generic;
using UnityEngine;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    /// <summary>
    /// 這邊連同 View端的東西一起處理
    /// </summary>
    public class MediumManager : SingletonMono<MediumManager>
    {
    #region ========== Private Variables ==========

        [SerializeField] private int             playerCount = 2;
        [SerializeField] private GameObject      ballPrefab;
        [SerializeField] private List<Transform> clientA_SpawnPoints;
        [SerializeField] private List<Transform> clientB_SpawnPoints;
        [SerializeField] private List<Transform> server_SpawnPoints;

        private List<Client> clientList = new List<Client>();
        private Server       server;

    #endregion

    #region ========== Unity Events ==========

        private void Start()
        {
            server = new Server();
            CreateClients();
        }

        private void Update()
        {
            for (int i = 0; i < clientList.Count; i++)
            {
                var client = clientList[i];
                client.Tick();
            }

            server.Tick();
        }

    #endregion

    #region ========== Public Methods ==========

        public void SendToServer(InputAction inputAction)
        {
            server.TakeInput(inputAction);
        }

    #endregion

    #region ========== Private Methods ==========

        private void CreateClients()
        {
            for (int i = 0; i < playerCount; i++)
            {
                // 建立 Client
                var clientID      = Guid.NewGuid().ToString();                 // 客戶端ID
                var controlSystem = new ControlSystem(i);                      // 控制器
                var client        = new Client(this, controlSystem, clientID); // 創立客戶端

                // 創立角色資料
                var ballID   = Guid.NewGuid().ToString(); // 角色ID
                var ballData = new BallData(ballID, server_SpawnPoints[i].position);

                // 清單紀錄
                clientList.Add(client);
                server.AddClient(clientID);

                // 變化角色顏色
                var ball = client.CreateBall(ballData, ballPrefab);
                if (ball.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                    spriteRenderer.color = i == 0 ? Color.blue : Color.red;
            }
        }

    #endregion
    }
}