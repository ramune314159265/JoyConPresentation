using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JoyConPresentation
{
    internal class Cursor
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
            MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public static void Move(int screenIndex, int x, int y)
        {
            int current = 0;
            RECT target = new();

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (nint hMonitor, nint hdcMonitor, ref RECT rect, IntPtr data) =>
                {
                    if (current == screenIndex)
                    {
                        target = rect;
                    }
                    current++;
                    return true;
                },
            IntPtr.Zero);

            int absoluteX = target.Left + x;
            int absoluteY = target.Top + y;

            SetCursorPos(absoluteX, absoluteY);
        }
    }
}
