using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging;

namespace WPFCustom
{
    /// <summary>
    /// Interaction logic for Giveaway.xaml
    /// </summary>
    public partial class Giveaway : Window
    {
        private bool isRaffleGoing = false;
        public Giveaway()
        {
            InitializeComponent();
        }

        

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (isRaffleGoing)
            {
                winner.Text = "label2";
                button1.Content = "Start Giveaway";
                isRaffleGoing = false;
                textBox1.IsEnabled = true;
                textBox1.Focus();
                textBox1.IsReadOnly = false;
                
            }
            else
            {
                winner.Text = "Type " + textBox1.Text.ToString() + " to enter the giveaway!";
                textBox1.IsReadOnly = true;
                button1.Content = "End Giveaway";
                isRaffleGoing = true;
                textBox1.Focus();
                textBox1.IsEnabled = false;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        private void Giveaway_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        public static Bitmap ChangeOpacity(System.Drawing.Image img, float opacityvalue)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height); // Determining Width and Height of Source Image
            Graphics graphics = Graphics.FromImage(bmp);
            ColorMatrix colormatrix = new ColorMatrix();
            colormatrix.Matrix33 = opacityvalue;
            ImageAttributes imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            graphics.DrawImage(img, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttribute);
            graphics.Dispose();   // Releasing all resource used by graphics 
            return bmp;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Height = 555;
            this.Width = 471;
        }
    }
}
