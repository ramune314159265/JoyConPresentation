using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace JoyConPresentation
{
    internal class Pointer : Form
    {
        private int pointerSize = 20;

        public Pointer()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Lime;
            this.TransparencyKey = Color.Lime;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Size = new Size(pointerSize, pointerSize);
            this.Hide();

            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);

            this.StartPosition = FormStartPosition.Manual;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (SolidBrush brush = new(Color.FromArgb(255, 255, 0, 0)))
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                e.Graphics.FillEllipse(brush, 0, 0, pointerSize, pointerSize);
            }
        }

        public void MovePoint(int x, int y)
        {
            this.Location = new Point(x - pointerSize / 2, y - pointerSize / 2);
        }

        public void SetVisible(bool visible)
        {
            if (visible)
            {
                this.Show();
            }
            else
            {
                this.Hide();
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
