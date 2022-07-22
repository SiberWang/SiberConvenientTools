﻿using UnityEngine;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
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
    }
}