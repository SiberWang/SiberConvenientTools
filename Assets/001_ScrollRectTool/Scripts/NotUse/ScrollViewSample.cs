using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomTool.UIScrollRect
{
    public class ScrollViewSample : MonoBehaviour
    {
        public enum style
        {
            Horizontal, //橫向分佈
            Vertical, //縱向分佈
        };

        public Image scrollView; //scroll view  
        public Transform grid; //grid
        public Scrollbar HorizontalScrollBar; //控制horizontal的scroll bar
        public Scrollbar VerticalScrollBar; //控制vertical的scroll bar
        public int number; //物品總數量
        public float middle; //物品間距

        public style manage_style; //管理物品方式

        //public int rowCount;                    //行數
        //public int columnCount;                 //列數
        public GameObject gameObj;

        private List<Item> itemList; //存放物品的列表
        private float item_width; //物品寬度
        private float item_height; //物品高度
        private float sv_width; //scroll view寬度
        private float sv_height; //scroll view高度   
        private float grid_width; //grid寬度
        private float grid_height; //grid高度
        private int row; //grid內可放物品行數
        private int column; //grid內可放物品列數
        private GameObject obj; //例項化物品物件
        private Vector3 gridOldPosition; //grid更新前的座標

        // Use this for initialization
        void Start()
        {
            grid.transform.localPosition = new Vector3(0, 0, 0); //設定grid座標
            item_width = gameObj.transform.GetComponent<RectTransform>().rect.size.x; //獲取Item寬高
            item_height = gameObj.transform.GetComponent<RectTransform>().rect.height;
            sv_width = scrollView.transform.GetComponent<RectTransform>().rect.size.x; //獲取scroll view 寬高
            sv_height = scrollView.transform.GetComponent<RectTransform>().rect.height;
            HorizontalScrollBar.transform.GetComponent<RectTransform>().sizeDelta =
                new Vector2(sv_width, 30); //設定scroll bar座標
            HorizontalScrollBar.transform.localPosition =
                new Vector3(0, -sv_height, 0) + scrollView.transform.localPosition;
            VerticalScrollBar.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(30, sv_height);
            VerticalScrollBar.transform.localPosition =
                new Vector3(sv_width, 0, 0) + scrollView.transform.localPosition;
            itemList = new List<Item>();

            switch (manage_style) //根據所選排列方式初始化建立Item
            {
                case style.Horizontal:
                    grid_width = number * (middle + item_width);
                    column = GetUpInt(sv_width, item_width + middle) + 1;
                    if (grid_width <= sv_width)
                    {
                        column = number;
                        grid_width = sv_width;
                    }

                    grid_height = sv_height;
                    row = 1;
                    grid.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(grid_width, grid_height);
                    HorizontalInitItem();
                    break;

                case style.Vertical:
                    grid_width = sv_width;
                    column = 1;
                    grid_height = number * (middle + item_height);
                    row = GetUpInt(sv_height, item_height + middle) + 1;
                    if (grid_height <= sv_height)
                    {
                        row = number;
                        grid_height = sv_height;
                    }

                    grid.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(grid_width, grid_height);
                    VerticalInitItem();
                    break;
            }
        }

        void HorizontalInitItem()
        {
            for (int i = 0; i < column; i++)
            {
                obj = Instantiate(gameObj);
                obj.transform.SetParent(grid.transform);
                obj.transform.localPosition = new Vector3(middle / 2 + item_width / 2 + (middle + item_width) * i,
                    -(sv_height / 2), 0);
                Item item = new Item();
                item.BindGameObject(obj);
                item.SetData(i + 1);
                itemList.Add(item);
            }
        }

        void VerticalInitItem()
        {
            for (int i = 0; i < row; i++)
            {
                obj = Instantiate(gameObj);
                obj.transform.SetParent(grid.transform);
                obj.transform.localPosition = new Vector3(sv_width / 2,
                    -(middle / 2 + item_height / 2 + (middle + item_height) * i), 0);
                Item item = new Item();
                item.BindGameObject(obj);
                item.SetData(i + 1);
                itemList.Add(item);
            }
        }

        // Update is called once per frame
        //根據grid的移動調整Item的位置
        void Update()
        {
            Vector3 gridNewPosition = grid.transform.localPosition;

            float h = gridNewPosition.y - gridOldPosition.y;

            float w = gridNewPosition.x - gridOldPosition.x;

            gridOldPosition = grid.transform.localPosition;

            if (h > 0.05f)
            {
                //當最後一個Item的值小於總量
                if (itemList[row - 1].GetData() < number)
                {
                    //當第一個Item的位置已經超出了一gird上一個Item的距離
                    while (itemList[0].GetGameObjectPosition().y + gridNewPosition.y > (item_height + middle) / 2)
                    {
                        Up(); //調整第一個Item位置
                    }
                }
            }
            else if (h < -0.05f)
            {
                if (itemList[0].GetData() > 1)
                {
                    while (itemList[row - 1].GetGameObjectPosition().y + gridNewPosition.y <
                           -(sv_height + (item_height + middle) / 2))
                    {
                        Down();
                    }
                }
            }

            if (w > 0.05f)
            {
                if (itemList[0].GetData() > 1)
                {
                    while (itemList[column - 1].GetGameObjectPosition().x + gridNewPosition.x >
                           (sv_width + (item_width + middle) / 2))
                    {
                        Right();
                    }
                }
            }
            else if (w < -0.05f)
            {
                if (itemList[column - 1].GetData() < number)
                {
                    while (itemList[0].GetGameObjectPosition().x + gridNewPosition.x < -((item_width + middle) / 2))
                    {
                        Left();
                    }
                }
            }
        }

        void Up()
        {
            //將第一個Item的位置放到最後一個Itemd的下方
            itemList[0].SetGameObjectPosition(itemList[row - 1].GetGameObjectPosition() +
                                              new Vector3(0, -(item_height + middle), 0));
            itemList[0].SetData(itemList[row - 1].GetData() + 1);
            itemList.Add(itemList[0]);
            itemList.RemoveAt(0);
        }

        void Down()
        {
            itemList[row - 1]
                .SetGameObjectPosition(itemList[0].GetGameObjectPosition() + new Vector3(0, item_height + middle, 0));
            itemList[row - 1].SetData(itemList[0].GetData() - 1);
            itemList.Insert(0, itemList[row - 1]);
            itemList.RemoveAt(row);
        }

        void Left()
        {
            itemList[0].SetGameObjectPosition(itemList[column - 1].GetGameObjectPosition() +
                                              new Vector3(item_width + middle, 0, 0));
            itemList[0].SetData(itemList[column - 1].GetData() + 1);
            itemList.Add(itemList[0]);
            itemList.RemoveAt(0);
        }

        void Right()
        {
            itemList[column - 1]
                .SetGameObjectPosition(itemList[0].GetGameObjectPosition() + new Vector3(-(item_width + middle), 0, 0));
            itemList[column - 1].SetData(itemList[0].GetData() - 1);
            itemList.Insert(0, itemList[column - 1]);
            itemList.RemoveAt(column);
        }

        int GetUpInt(float a, float b)
        {
            int i = 0;

            while (a > 0)
            {
                a -= b;
                i++;
            }

            return i;
        }
    }

    public class Item
    {
        private GameObject _gameObject;
        private int _dataID;

        public void BindGameObject(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public void SetData(int i)
        {
            _dataID = i;
        }

        public void SetGameObjectPosition(Vector3 newPos)
        {
            _gameObject.transform.position = newPos;
        }

        public Vector3 GetGameObjectPosition()
        {
            var pos = _gameObject.transform.position;
            return pos;
        }

        public int GetData()
        {
            return _dataID;
        }
    }
}