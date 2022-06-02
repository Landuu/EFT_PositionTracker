using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFT_Posreader
{
    public class XY
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string DisplayString => $"({X}, {Y})";

        public bool IsPopulated => X > -1 && Y > -1;

        public XY()
        {
            X = -1;
            Y = -1;
        }

        public XY(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class ScreenRegion
    {
        public XY Start { get; set; }
        public XY End { get; set; }
        public int Width => Math.Abs(End.X - Start.X);
        public int Height => Math.Abs(End.Y - Start.Y);

        public int HasPopulated
        {
            get
            {
                if (Start.IsPopulated && End.IsPopulated) return 2;
                else if (Start.IsPopulated) return 1;
                else return 0;
            }
        }

        public ScreenRegion()
        {
            Start = new();
            End = new();
        }

        public ScreenRegion(string startX, string startY, string endX, string endY)
        {
            Start = new(Convert.ToInt32(startX), Convert.ToInt32(startY));
            End = new(Convert.ToInt32(endX), Convert.ToInt32(endY));
        }

        public void SetStart(int x, int y)
        {
            Start.X = x;
            Start.Y = y;
        }

        public void SetEnd(int x, int y)
        {
            End.X = x;
            End.Y = y;
        }

        public void Reset()
        {
            SetStart(-1, -1);
            SetEnd(-1, -1);
        }
    }
}
