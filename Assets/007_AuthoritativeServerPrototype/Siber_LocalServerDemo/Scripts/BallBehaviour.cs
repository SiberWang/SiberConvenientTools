using UnityEngine;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    /// <summary>
    /// 只記錄著 View 端要做的行為
    /// </summary>
    public class BallBehaviour : MonoBehaviour
    {
        public void Move(Vector2 pos)
        {
            var defaultX = transform.position.x;
            var defaultY = transform.position.y;
            transform.position = pos;
        }
    }
}