using System;
using System.Collections.Generic;
using LocalServerDemo.Repositorys;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityTimer;

namespace LocalServerDemo.Scripts
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

        public delegate void ViewUpdate();

        private ViewUpdate viewUpdateDelegate; // View 更新事件

        private UserRepository userRepository;
        private Server         server;

        private float            tempUpdateTimes;
        private int              tempClientALag;
        private int              tempClientBLag;
        private int              playerANumber;
        private int              playerBNumber;
        private ClientRepository clientRepository;
        private Client           clientA;
        private Client           clientB;

    #endregion

    #region ========== Unity Events ==========

        private void Awake()
        {
            server               = new Server();
            userRepository       = new UserRepository();
            clientRepository     = ClientRepository.Instance;
            ServerInputText.text = "Last acknowledged input: Player A: [#0] / Player B: [#0]";
        }

        private void Start()
        {
            CreateClients(); // 創立 A , B 客戶端 ,  A , B 角色
        }

        private void Update()
        {
            ValueSetting();
            viewUpdateDelegate?.Invoke();
        }

    #endregion

    #region ========== Public Methods ==========

        public void AddViewCallBack(ViewUpdate tick)
        {
            viewUpdateDelegate += tick;
        }

        public void SetLastInputText(string client, int number)
        {
            if (client == "ClientA") playerANumber = number;
            if (client == "ClientB") playerBNumber = number;
            ServerInputText.text = $"Last acknowledged input: Player A: [#{playerANumber}] / Player B: [#{playerBNumber}]";
        }

    #endregion

    #region ========== Private Methods ==========

        /// <summary> 創建客戶端玩家 </summary>
        private void CreateClients()
        {
            clientA = new Client("ClientA", server, new ControlSystem(0)); // 創立客戶端 A
            clientB = new Client("ClientB", server, new ControlSystem(1)); // 創立客戶端 B
            clientRepository.AddClient(clientA);
            clientRepository.AddClient(clientB);
            CreatePlayer(clientA_SpawnPoints, clientA.UserRepository);
            CreatePlayer(clientB_SpawnPoints, clientB.UserRepository);
            CreatePlayer(server_SpawnPoints, server.UserRepository);
            clientA.MainPlayerID = "PlayerA";
            clientB.MainPlayerID = "PlayerB";
        }

        /// <summary> 在指定位置創建玩家 </summary>
        /// <param name="serverSpawnPoints"> 位置 </param>
        /// <param name="repository"> 指定Client的玩家儲存庫 </param>
        private void CreatePlayer(List<Transform> serverSpawnPoints, UserRepository repository)
        {
            for (int i = 0; i < serverSpawnPoints.Count; i++)
            {
                var playerID   = i == 0 ? "PlayerA" : "PlayerB";
                var playerData = new PlayerData(playerID, serverSpawnPoints[i].position);
                var ball       = Instantiate(ballPrefab, playerData.Pos, Quaternion.identity);
                if (ball.TryGetComponent<SpriteRenderer>(out var spriteRenderer)) // 變化角色顏色
                    spriteRenderer.color = i == 0 ? Color.blue : Color.red;
                repository?.AddPlayer(playerData, ball);
            }
        }

        /// <summary> 手動更改 ms , 客戶端預測時使用 </summary>
        private void ValueSetting()
        {
            // 處理 Server 的 Update , 依每秒更新幾次來進行
            if (tempUpdateTimes != serverUpdateTimes) // 數值改變了在重置更新事件
            {
                server.SetUpdateTimes(serverUpdateTimes);
                tempUpdateTimes = serverUpdateTimes;
                CatchYouBug.DeShow($"Change Update Timer : Update [{serverUpdateTimes}] times by sec");
            }

            // 處理 Client 的 Update
            if (tempClientALag != clientALag)
            {
                clientA.SetLag(clientALag);
                tempClientALag = clientALag;
                CatchYouBug.DeShow($"Change ClientA Lag : [{clientALag}]");
            }

            if (tempClientBLag != clientBLag)
            {
                clientB.SetLag(clientBLag);
                tempClientBLag = clientBLag;
                CatchYouBug.DeShow($"Change ClientB Lag : [{clientBLag}]");
            }

            clientA.SidePrediction = clientA_sidePrediction;
            clientB.SidePrediction = clientB_sidePrediction;
            clientAInputText.text  = $"Non-acknowledged Inputs:{clientA.InputList.Count}";
            clientBInputText.text  = $"Non-acknowledged Inputs:{clientB.InputList.Count}";
        }

    #endregion
    }
}