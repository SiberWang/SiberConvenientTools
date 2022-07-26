using UnityEngine;

namespace LocalServerDemo.Scripts
{
    /// <summary>
    /// 只記錄著 View 端要做的行為
    /// </summary>
    public class UnityComponent : MonoBehaviour
    {
        public void MoveX(float x)
        {
            var defaultY = transform.position.y;
            var newX     = transform.position.x + x;
            transform.position = new Vector2(newX, defaultY);
        }

        public void Move(Vector2 pos)
        {
            transform.position = pos;
        }
    }
}