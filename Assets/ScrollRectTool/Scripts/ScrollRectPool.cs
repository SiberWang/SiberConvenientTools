using System.Collections.Generic;
using UnityEngine;

namespace CustomTool.UIScrollRect
{
    /// <summary>
    /// Stack ObjectPool - For ScrollRect
    /// 方便用於UI ScrollRect 依照指定順序呼叫及回收
    /// </summary>
    public class ScrollRectPool
    {
        #region Private Variables

        private readonly Stack<GameObject> stackPool = new Stack<GameObject>();
        private readonly GameObject spawnTarget;
        private readonly Transform contentTran;

        #endregion

        #region Constructor

        public ScrollRectPool(GameObject spawnTarget, Transform contentTran)
        {
            this.spawnTarget = spawnTarget;
            this.contentTran = contentTran;
        }

        #endregion

        #region Public Methods

        /// <summary> StackPool - 取出物件 </summary>
        public GameObject PopFromPool()
        {
            GameObject obj = TakeObject();
            obj.transform.SetParent(contentTran.transform);
            obj.transform.localScale = Vector3.one;
            obj.SetActive(true);
            return obj;
        }

        /// <summary> StackPool - 推送至物件池 </summary>
        public void PushToPool(GameObject cell)
        {
            if (cell == null) return;
            stackPool.Push(cell);
            cell.SetActive(false);
        }
        
        /// <summary> StackPool - 清除物件池 </summary>
        public void Clear()
        {
            foreach (GameObject obj in stackPool)
            {
                if (obj != null)
                {
                    Debug.Log($"obj name : {obj.name}");
                    GameObject.Destroy(obj.gameObject);
                }
            }
            stackPool.Clear();
        }

        #endregion

        #region Private Methods

        private GameObject TakeObject()
        {
            GameObject obj = null;
            if (stackPool.Count > 0) // 如果池中有，直接叫出
                obj = stackPool.Pop();
            if (obj == null) // 如果池中沒有物件或找不到，就生產及覆蓋
                obj = UnityEngine.GameObject.Instantiate(spawnTarget);
            return obj;
        }

        #endregion
    }
}