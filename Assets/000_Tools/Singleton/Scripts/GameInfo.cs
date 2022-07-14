using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NSubstitute;
using Random = UnityEngine.Random;

namespace SingletonExample
{
    public class GameInfo : Singleton<GameInfo>
    {
        public event Action LoadingDataAction;

        private List<UserData> userDataList = new List<UserData>();

        private CancellationTokenSource cancelToken = new CancellationTokenSource();

        public const int DataCount = 10;

        public async UniTask CreateUserDatas()
        {
            Clear();

            for (int i = 0; i < DataCount; i++)
            {
                var userData = new UserData
                {
                    ID     = Guid.NewGuid().ToString().Substring(0, 5), // 縮短至5個字
                    Level  = Random.Range(1, 101),
                    Health = Random.Range(1, 10) * 10 + Random.Range(1, 6) * 15
                };
                userDataList.Add(userData);
                CatchYouBug.DeShow($"Create UserData [{userData.ID}]");
                LoadingDataAction?.Invoke();
                await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: cancelToken.Token);
            }
        }

        public void Clear()
        {
            if (cancelToken != null)
            {
                cancelToken.Cancel();
                cancelToken.Dispose();
            }

            cancelToken = new CancellationTokenSource();
            ClearList();
        }

        public List<UserData> GetUserDatas()
        {
            return userDataList;
        }

        private void ClearList()
        {
            userDataList.Clear();
        }
    }

    public class UserData
    {
        public string ID;
        public int    Level;
        public int    Health;
    }
}