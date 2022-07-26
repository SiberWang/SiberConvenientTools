using UnityEngine;

namespace LocalServerDemo.Scripts
{
    /// <summary>
    /// 這邊在舉例 Entity
    /// </summary>
    public class PlayerData
    {
        public string  ID;
        public Vector2 Pos;

        public PlayerData(string id , Vector2 pos)
        {
            ID  = id;
            Pos = pos;
        }

        public void MoveX(float x)
        {
            Pos.x += x;
        }
        
        public void Move(Vector2 pos)
        {
            Pos += pos;
        }
    }
}