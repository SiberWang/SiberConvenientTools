using UnityEngine;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    public class PlayerData
    {
        public string  ID;
        public Vector2 Pos;

        public PlayerData(string id , Vector2 pos)
        {
            ID  = id;
            Pos = pos;
        }
    }
}