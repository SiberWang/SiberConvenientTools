using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace SingletonExample
{
    /// <summary>
    /// 盡量不指定場景的特定物件，不然遺失了就爆了
    /// </summary>
    public class GameMgr : SingletonMono<GameMgr>
    {
    #region ========== Variables ==========

        private CancellationTokenSource cancelToken = new CancellationTokenSource();

        public event Action<string> UpdateUIAction;
        public event Action         FinishedAction;
        public event Action         StopAction;

        private string message;
        private int    loadAmount;

        private string pointsText;
        private int    pointIndex;

    #endregion

    #region ========== Unity Events ==========

        private void Start()
        {
            GameInfo.Instance.LoadingDataAction += () =>
            {
                loadAmount = GameInfo.Instance.GetUserDatas().Count;

                // ... 的跳動
                pointsText += "......";
                pointIndex++;
                if (pointIndex > 3)
                {
                    pointsText = "......";
                    pointIndex = 0;
                }

                var loadMessage = $"[{loadAmount}/{GameInfo.DataCount}]{pointsText}Loading";
                UpdateUIAction?.Invoke(loadMessage);
            };
        }

        private void OnDisable()
        {
            UpdateUIAction = null;
            message        = null;
            cancelToken.Cancel();
            cancelToken.Dispose();
        }

    #endregion

    #region ========== Public Methods ==========

        public async UniTask CallUserDatas()
        {
            await GameInfo.Instance.CreateUserDatas();
            CatchYouBug.DeShow("Start to set DataToList", "GameManager");
            await SetDataToList();
            CatchYouBug.DeShow("Finished all methods", "GameManager");
            FinishedAction?.Invoke();
            InitMessage();
        }

        public void StopCall()
        {
            GameInfo.Instance.Clear();
            if (cancelToken != null)
            {
                cancelToken.Cancel();
                cancelToken.Dispose();
            }

            cancelToken = new CancellationTokenSource();

            message = null;
            StopAction?.Invoke();
        }

        public void InitMessage()
        {
            message = null;
        }

    #endregion

    #region ========== Private Methods ==========

        private async UniTask SetDataToList()
        {
            var userDatas = GameInfo.Instance.GetUserDatas();
            for (int i = 0; i < userDatas.Count; i++)
            {
                var userData = userDatas[i];
                message += $"User:[{userData.ID}] , Level:[{userData.Level}] , HP:[{userData.Health}]\n";
                // message          += i < userDatas.Count - 1 ? "\n" : "";
                UpdateUIAction?.Invoke(message);
                await UniTask.Delay(TimeSpan.FromSeconds(0.15f), cancellationToken: cancelToken.Token);
            }

            message += $"\nDone!!";
            UpdateUIAction?.Invoke(message);
        }

    #endregion
    }

    /*
     #region ========== Editor ==========
    
    #if UNITY_EDITOR
    
        [CustomEditor(typeof(GameMgr))]
        public class GameMgrButtons : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var gameManager = (GameMgr)target;
                if (GUILayout.Button("Restart"))
                {
                    if (!Application.isPlaying)
                    {
                        CatchYouBug.Debug("It's only work for [PlayMode] , please start the game");
                    }
                    else
                    {
                        gameManager.CallUserDatas().Forget();
                        CatchYouBug.DeShow("Restart to create UserDatas", "GameManager");
                    }
                }
            }
        }
    
    #endif
    
    #endregion
    */
}