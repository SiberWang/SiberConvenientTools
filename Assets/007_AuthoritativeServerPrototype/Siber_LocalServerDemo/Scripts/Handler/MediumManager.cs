using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityTimer;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    /// <summary>
    /// 這邊連同 View端的東西一起處理
    /// </summary>
    public class MediumManager : SingletonMono<MediumManager>
    {
        private const string GroupMain   = "Main Setting";
        private const string GroupPlayer = "Player Setting";
        private const string GroupSpawn  = "SpawnPoints";

    #region ========== Private Variables ==========

        [BoxGroup(GroupMain)]
        [SerializeField] private int clientALag;

        [BoxGroup(GroupMain)]
        [SerializeField] private int clientBLag;

        [BoxGroup(GroupMain)]
        [SerializeField] private float serverUpdateTimes;

        [FoldoutGroup(GroupPlayer)]
        [SerializeField] private GameObject ballPrefab;

        [FoldoutGroup(GroupSpawn)]
        [SerializeField] private List<Transform> clientA_SpawnPoints;

        [FoldoutGroup(GroupSpawn)]
        [SerializeField] private List<Transform> clientB_SpawnPoints;

        [FoldoutGroup(GroupSpawn)]
        [SerializeField] private List<Transform> server_SpawnPoints;

        private List<Client> clientList = new List<Client>();
        private Server       server;
        private string       playerA_ID;
        private string       playerB_ID;

        private float tempUpdateTimes;
        private Timer serverTimer;

    #endregion

    #region ========== Unity Events ==========

        private void Start()
        {
            server = new Server();
            CreateClients(); // 創立 A , B 客戶端
            CreatePlayers(); // 創立 A , B 角色
        }

        private void Update()
        {
            if (tempUpdateTimes != serverUpdateTimes)
            {
                // Update N times per second
                if (serverTimer != null) serverTimer.Cancel();
                serverTimer = Timer.Register
                    (
                     duration: UpdateTimeBySec(),
                     onComplete: server.Tick,
                     isLooped: true,
                     useRealTime: false
                    );
                tempUpdateTimes = serverUpdateTimes;
                Debug.Log($"Update Timer : Update {serverUpdateTimes} times by sec");
            }

            // 處理 Client 的 Update
            for (var i = 0; i < clientList.Count; i++)
            {
                var client = clientList[i];
                if (i == 0) client.SetLag(clientALag);
                if (i == 1) client.SetLag(clientBLag);
                client.Tick();
            }
        }

    #endregion

    #region ========== Private Methods ==========

        private float UpdateTimeBySec()
        {
            // 每秒更新 N 次
            float updateTimes = 1f / serverUpdateTimes;
            return updateTimes;
        }

        private void CreateClients()
        {
            for (int i = 0; i < 2; i++)
            {
                // 建立 Client A , B , 並記錄到到 List
                var clientID      = i == 1 ? "1" : "2";                          // 客戶端ID
                var controlSystem = new ControlSystem(i);                        // 控制器
                var client        = new Client(server, controlSystem, clientID); // 創立客戶端
                clientList.Add(client);
                server.AddClient(client);
            }
        }

        //TODO: Client , Server 都要建立
        private void CreatePlayers()
        {
            // 創立角色資料 PlayerA,PlayerB
            playerA_ID = Guid.NewGuid().ToString();
            playerB_ID = Guid.NewGuid().ToString();
            CreatePlayerToWorld(clientA_SpawnPoints);
            CreatePlayerToWorld(clientB_SpawnPoints);
            CreatePlayerToWorld(server_SpawnPoints);
        }

        private void CreatePlayerToWorld(List<Transform> serverSpawnPoints)
        {
            for (int i = 0; i < serverSpawnPoints.Count; i++)
            {
                var id   = i == 0 ? playerA_ID : playerB_ID;
                var ball = CreatePlayer(id, clientList[i], serverSpawnPoints[i]);
                if (ball.TryGetComponent<SpriteRenderer>(out var spriteRenderer)) // 變化角色顏色
                    spriteRenderer.color = i == 0 ? Color.blue : Color.red;
            }
        }

        private GameObject CreatePlayer(string id, Client client, Transform serverSpawnPoint)
        {
            var playerData = new PlayerData(id, serverSpawnPoint.position);
            server.AddPlayer(playerData);
            client.AddPlayer(playerData);
            var ball = Instantiate(ballPrefab, playerData.Pos, Quaternion.identity);
            return ball;
        }

    #endregion

        public void UpdateWorld(string ID, List<PlayerData> playerList)
        {
            List<Transform> transforms;
            if (ID == "0") transforms = server_SpawnPoints;
            if (ID == "1") transforms = clientA_SpawnPoints;
            if (ID == "2") transforms = clientB_SpawnPoints;

            var client = clientList.Find(c => c.ClientID == ID);
            if (client != null)
            {
                for (int i = 0; i < playerList.Count; i++)
                {
                    PlayerData playerData    = playerList[i];
                    var        ballBehaviour = client.GetPlayerByID(playerData.ID);
                    ballBehaviour.Move(playerData.Pos.x);
                }
            }
        }
    }
}