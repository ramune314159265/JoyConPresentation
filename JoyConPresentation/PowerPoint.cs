using Microsoft.Office.Interop.PowerPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JoyConPresentation
{
    internal class PowerPoint
    {
        [DllImport("oleaut32.dll", PreserveSig = false)]
        private static extern void GetActiveObject(
            ref Guid rclsid,
            IntPtr pvReserved,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);

        public static SlideShowWindow? GetSlideShowWindow()
        {
            try
            {
                Guid clsid = Type.GetTypeFromProgID("PowerPoint.Application").GUID;
                GetActiveObject(ref clsid, IntPtr.Zero, out object obj);
                var powerpoint = (Application)obj;
                SlideShowWindow slideShow = powerpoint.SlideShowWindows[1];
                return slideShow;
            }
            catch
            {
                return null;
            }
        }
    }
}
