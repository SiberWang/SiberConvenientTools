namespace LocalServerDemo.Scripts
{
    public class InputEvent : EventArgs
    {
        public string    ClientID;
        public string    PlayerID;
        public float     X; 
        public int       inputNumber; // 這個就是 #1 #2 編號的概念
        public CacheTime CacheTime;   // 紀錄時間
    }
}