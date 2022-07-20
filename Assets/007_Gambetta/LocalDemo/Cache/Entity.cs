using System.Collections.Generic;

namespace Demo
{
    public class Entity
    {
        public int    ID;
        public float  X;
        public double Speed = 10d; // units/s

        public List<TimedPosition> PositionBuffer = new List<TimedPosition>();

        public void ApplyInput(Input input)
        {
            X += (float)(input.press_time * Speed);
        }
    }
}