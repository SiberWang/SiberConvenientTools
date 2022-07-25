using System.Collections.Generic;
using LocalServerDemo.Scripts;
using UnityEngine;

namespace LocalServerDemo.Repositorys
{
    /// <summary> 搜索 User 相關的資料庫 </summary>
    /// Server , Local 各有自己的資料庫 (用於區別自己區域的玩家畫面)
    public class UserRepository
    {
        private readonly Dictionary<string, GameObject> playerDict = new Dictionary<string, GameObject>();

        private readonly List<PlayerData> playerDataList = new List<PlayerData>();

        /// <summary> 新增玩家資料 </summary>
        /// <param name="playerData"> 玩家資料</param>
        /// <param name="gameObject"> 角色物件 </param>
        public void AddPlayer(PlayerData playerData, GameObject gameObject)
        {
            if (!playerDict.ContainsKey(playerData.ID))
                playerDict.Add(playerData.ID, gameObject);
            playerDataList.Add(playerData);
        }

        /// <summary> 取得角色物件 Component </summary>
        /// <param name="id">PlayerID</param>
        /// <returns> UnityComponent (Mono) </returns>
        public PlayerData GetUserData(string id)
        {
            return playerDataList.Find(d => d.ID.Equals(id));
        }

        /// <summary> 取得角色物件 Component </summary>
        /// <param name="id">PlayerID</param>
        /// <returns> UnityComponent (Mono) </returns>
        public UnityComponent GetComponent(string id)
        {
            if (playerDict.ContainsKey(id))
                if (playerDict[id].TryGetComponent<UnityComponent>(out var unityComponent))
                    return unityComponent;
            return null;
        }
    }
}