using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
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
        private const string GroupUI     = "UISetting";

    #region ========== Private Variables ==========

        [BoxGroup(GroupMain)]
        [SerializeField] private int clientALag;
        [BoxGroup(GroupMain)]
        [SerializeField] private int clientBLag;
        [BoxGroup(GroupMain)]
        [SerializeField] private float serverUpdateTimes;

        [BoxGroup(GroupMain)]
        [SerializeField] private bool clientA_sidePrediction; //客戶端預測 Client-side prediction
        [BoxGroup(GroupMain)]
        [SerializeField] private bool clientB_sidePrediction;

        [FoldoutGroup(GroupPlayer)]
        [SerializeField] private GameObject ballPrefab;

        [FoldoutGroup(GroupSpawn)]
        [SerializeField] private List<Transform> clientA_SpawnPoints;
        [FoldoutGroup(GroupSpawn)]
        [SerializeField] private List<Transform> clientB_SpawnPoints;
        [FoldoutGroup(GroupSpawn)]
        [SerializeField] private List<Transform> server_SpawnPoints;

        [FoldoutGroup(GroupUI)]
        [SerializeField] private TMP_Text clientAInputText;
        [FoldoutGroup(GroupUI)]
        [SerializeField] private TMP_Text clientBInputText;
        [FoldoutGroup(GroupUI)]
        [SerializeField] private TMP_Text ServerInputText;

        private List<Client> clientList = new List<Client>();
        private Server       server;
        private string       playerA_ID;
        private string       playerB_ID;

        private float tempUpdateTimes;
        private int   tempClientALag;
        private int   tempClientBLag;
        private Timer serverTimer;
        private int   playerANumber;
        private int   playerBNumber;

    #endregion

    #region ========== Unity Events ==========

        private void Start()
        {
            server = new Server();
            CreateClients(); // 創立 A , B 客戶端
            CreatePlayers(); // 創立 A , B 角色
            ServerInputText.text = "Last acknowledged input: Player A: [#0] / Player B: [#0]";
        }

        private void Update()
        {
            // 處理 Server 的 Update , 依每秒更新幾次來進行
            if (tempUpdateTimes != serverUpdateTimes) // 數值改變了在重置更新事件
            {
                // Update N times per second
                if (serverTimer != null) serverTimer.Cancel();
                serverTimer = Timer.Register
                    (
                     duration: UpdateTimeBySec(),
                     onComplete: server.Tick,
                     onUpdate: null,
                     isLooped: true,
                     useRealTime: false
                    );
                tempUpdateTimes = serverUpdateTimes;
                Debug.Log($"Update Timer : Update {serverUpdateTimes} times by sec");
            }

            // 處理 Client 的 Update
            if (tempClientALag != clientALag)
            {
                clientList[0].SetLag(clientALag);
                tempClientALag = clientALag;
            }

            if (tempClientALag != clientBLag)
            {
                clientList[1].SetLag(clientBLag);
                tempClientALag = clientBLag;
            }

            clientList[0].Tick();
            clientList[1].Tick();
            clientList[0].SidePrediction = clientA_sidePrediction;
            clientList[1].SidePrediction = clientB_sidePrediction;
            clientAInputText.text        = $"Non-acknowledged Inputs:{clientList[0].InputList.Count}";
            clientBInputText.text        = $"Non-acknowledged Inputs:{clientList[1].InputList.Count}";
        }

        public void SetLastInputText(string client, int number)
        {
            if (client == "1") playerANumber = number;
            if (client == "2") playerBNumber = number;
            ServerInputText.text = $"Last acknowledged input: Player A: [#{playerANumber}] / Player B: [#{playerBNumber}]";
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
                var clientID      = $"{i + 1}";                                  // 客戶端ID
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
            playerA_ID = "A"; // 藍球
            playerB_ID = "B"; // 紅球
            CreatePlayerToClient(clientA_SpawnPoints, clientList[0]);
            CreatePlayerToClient(clientB_SpawnPoints, clientList[1]);
            CreatePlayerToServer(server_SpawnPoints);
        }

        private void CreatePlayerToServer(List<Transform> serverSpawnPoints)
        {
            for (int i = 0; i < serverSpawnPoints.Count; i++)
            {
                var playerID   = i == 0 ? playerA_ID : playerB_ID;
                var playerData = new PlayerData(playerID, serverSpawnPoints[i].position);
                var ball       = Instantiate(ballPrefab, playerData.Pos, Quaternion.identity);
                if (ball.TryGetComponent<SpriteRenderer>(out var spriteRenderer)) // 變化角色顏色
                    spriteRenderer.color = i == 0 ? Color.blue : Color.red;
                server.AddPlayer(playerData, ball);
            }
        }

        private void CreatePlayerToClient(List<Transform> serverSpawnPoints, Client client)
        {
            for (int i = 0; i < serverSpawnPoints.Count; i++)
            {
                var playerID   = i == 0 ? playerA_ID : playerB_ID;
                var playerData = new PlayerData(playerID, serverSpawnPoints[i].position);
                var ball       = Instantiate(ballPrefab, playerData.Pos, Quaternion.identity);
                if (ball.TryGetComponent<SpriteRenderer>(out var spriteRenderer)) // 變化角色顏色
                    spriteRenderer.color = i == 0 ? Color.blue : Color.red;
                client.AddPlayer(playerData, ball);
            }
        }

    #endregion
    }
}