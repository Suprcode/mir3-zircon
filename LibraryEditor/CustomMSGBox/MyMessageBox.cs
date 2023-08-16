using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace ZirconMessageBox
{
    public partial class ZirconMessageBox : Form
    {

        public enum ZirconMessageBoxResult
        {
            Cancel = -1,
            Button1 = 1,
            Button2 = 2,
            Button3 = 3
        }

        static ZirconMessageBox newMessageBox;
        public Timer msgTimer;
        static ZirconMessageBoxResult Button_id;
        int disposeFormTimer;

        public ZirconMessageBox()
        {
            InitializeComponent();
        }

        public static ZirconMessageBoxResult ShowBox(string txtMessage, string txtTitle, string button1, string button2, string button3)
        {

            Button_id = ZirconMessageBoxResult.Cancel;//generic "cancel"

            newMessageBox = new ZirconMessageBox();
            newMessageBox.lblTitle.Text = txtTitle;
            newMessageBox.lblMessage.Text = txtMessage;

            if (string.IsNullOrEmpty(button3))
            {
                newMessageBox.btn3.Visible = false;
            }
            else
            {
                newMessageBox.btn3.Visible = true;
                newMessageBox.btn3.Text = button3;
            }

            if (string.IsNullOrEmpty(button2))
            {
                newMessageBox.btn2.Visible = false;
            }
            else
            {
                newMessageBox.btn2.Visible = true;
                newMessageBox.btn2.Text = button2;
            }

            if (string.IsNullOrEmpty(button1))
            {
                newMessageBox.btn1.Visible = false;
            }
            else
            {
                newMessageBox.btn1.Visible = true;
                newMessageBox.btn1.Text = button1;
            }

            newMessageBox.ShowDialog();
            return Button_id;
        }

        private void MyMessageBox_Load(object sender, EventArgs e)
        {
            disposeFormTimer = 30;
            newMessageBox.lblTimer.Text = disposeFormTimer.ToString();
            msgTimer = new Timer();
            msgTimer.Interval = 1000;
            msgTimer.Enabled = true;
            msgTimer.Start();
            msgTimer.Tick += new System.EventHandler(this.timer_tick);
        }

        private void MyMessageBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics mGraphics = e.Graphics;
            Pen p = new Pen(Color.FromArgb(96, 155, 173), 1);

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            LinearGradientBrush LG = new LinearGradientBrush(rect, Color.FromArgb(96, 155, 173), Color.FromArgb(245, 251, 251), LinearGradientMode.Vertical);
            mGraphics.FillRectangle(LG, rect);
            mGraphics.DrawRectangle(p, rect);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            newMessageBox.msgTimer.Stop();
            newMessageBox.msgTimer.Dispose();
            Button_id = ZirconMessageBoxResult.Button3;
            newMessageBox.Dispose();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            newMessageBox.msgTimer.Stop();
            newMessageBox.msgTimer.Dispose();
            Button_id = ZirconMessageBoxResult.Button2;
            newMessageBox.Dispose();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            newMessageBox.msgTimer.Stop();
            newMessageBox.msgTimer.Dispose();
            Button_id = ZirconMessageBoxResult.Button1;
            newMessageBox.Dispose();
        }

        private void timer_tick(object sender, EventArgs e)
        {
            disposeFormTimer--;

            if (disposeFormTimer >= 0)
            {
                newMessageBox.lblTimer.Text = disposeFormTimer.ToString();
            }
            else
            {
                newMessageBox.msgTimer.Stop();
                newMessageBox.msgTimer.Dispose();
                newMessageBox.Dispose();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            newMessageBox.Dispose();
        }
    }
}