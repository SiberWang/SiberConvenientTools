using UnityEngine;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    /// <summary>
    /// 控制器 , 目前記錄移動相關
    /// 管理不同 Client 所使用的按鍵 (WASD , 上下左右..等等)
    /// </summary>
    public class ControlSystem
    {
    #region ========== Private Variables ==========

        private int index;

    #endregion

    #region ========== Constructor ==========

        public ControlSystem(int index)
        {
            this.index = index;
        }

        public int Index => index;

    #endregion

    #region ========== Public Methods ==========

        public bool MoveLeft()
        {
            return index switch
            {
                0 => Input.GetKey(KeyCode.LeftArrow),
                1 => Input.GetKey(KeyCode.A),
                _ => false
            };
        }

        public bool MoveRight()
        {
            return index switch
            {
                0 => Input.GetKey(KeyCode.RightArrow),
                1 => Input.GetKey(KeyCode.D),
                _ => false
            };
        }

    #endregion
    }
}