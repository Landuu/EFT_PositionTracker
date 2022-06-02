using System.Runtime.InteropServices;

namespace EFT_Posreader
{
    internal class MousePos
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        struct POINT
        {
            public int X;
            public int Y;
        }

        public static XY GetMousePosition()
        {
            POINT point;
            GetCursorPos(out point);
            return new XY() { X = point.X, Y = point.Y };
        }
    }
}
