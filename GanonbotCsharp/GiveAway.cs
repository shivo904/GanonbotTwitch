using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using GanonbotCsharp.Properties;
using System.Runtime.InteropServices;

namespace GanonbotCsharp
{
    public partial class GiveAway : Form
    {
        private bool isRaffleGoing = false;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        public GiveAway()
        {
            InitializeComponent();
        }

        private void GiveAway_Load(object sender, EventArgs e)
        {
            try
            {
                string dir = Directory.GetCurrentDirectory();
                float opacityvalue = (float).3;
                pictureBox1.Image = ChangeOpacity(Resources.Moderator, opacityvalue);
                pictureBox2.Image = ChangeOpacity(Resources.Subscriber, opacityvalue);
                pictureBox3.Image = ChangeOpacity(Resources.Follower, opacityvalue);
            }
            catch(Exception)
            {
            }
        }
        public static Bitmap ChangeOpacity(Image img, float opacityvalue)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height); // Determining Width and Height of Source Image
            Graphics graphics = Graphics.FromImage(bmp);
            ColorMatrix colormatrix = new ColorMatrix();
            colormatrix.Matrix33 = opacityvalue;
            ImageAttributes imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            graphics.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttribute);
            graphics.Dispose();   // Releasing all resource used by graphics 
            return bmp;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isRaffleGoing)
            {
                winner.Text = "WINNER";
                textBox1.ReadOnly = false;
                textBox1.Enabled = true;
                button2.Text = "Begin Giveaway";
                isRaffleGoing = false;
            }
            else
            {
                winner.Text = "Type " + textBox1.Text.ToString() + " to enter the giveaway!";
                textBox1.ReadOnly = true;
                textBox1.Enabled = false;
                button2.Text = "End Giveaway";
                isRaffleGoing = true;
            }
             
            while (winner.Width > System.Windows.Forms.TextRenderer.MeasureText(winner.Text,
            new Font(winner.Font.FontFamily, winner.Font.Size, winner.Font.Style)).Width)
            {
                winner.Font = new Font(winner.Font.FontFamily, winner.Font.Size + 0.5f, winner.Font.Style);
                if (winner.Font.Size > 55)
                    break;
            }
            while (winner.Width < System.Windows.Forms.TextRenderer.MeasureText(winner.Text,
            new Font(winner.Font.FontFamily, winner.Font.Size, winner.Font.Style)).Width)
            {
                winner.Font = new Font(winner.Font.FontFamily, winner.Font.Size - 0.5f, winner.Font.Style);
            }
           
        }


        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Form1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
