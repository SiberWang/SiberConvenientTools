using UnityEngine;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    public class BallData
    {
        public string  ID;
        public Vector2 Pos;

        public BallData(string id , Vector2 pos)
        {
            ID  = id;
            Pos = pos;
        }
    }
}