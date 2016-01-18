using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GanonbotCsharp
{
    public partial class BannedWords : Form
    {
        SQLiteParser sql = new SQLiteParser();
        List<List<string>> readInList = new List<List<string>>();
        List<string> outputList = new List<string>();
        public BannedWords()
        {
            InitializeComponent();
        }

        private void BannedWords_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(this.pictureBox1, "Place any words or statements that you wish to ban from your chat." + Environment.NewLine + "The bot will permaban anyone who uses one of these statements." + Environment.NewLine + "Place each statement on a new line.");
            readInList = sql.readSQLite("bannedWords", "name");
            foreach (List<string> word in readInList)
            {
                if (word.Count == 1)
                    this.richTextBox1.AppendText(word[0]);
                else
                    this.richTextBox1.AppendText(word[0] + "|" + word[1]);
                this.richTextBox1.AppendText(Environment.NewLine);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            outputList.Clear();
            for (int i = 0; i < richTextBox1.Lines.Length; i++)
            {
                if(richTextBox1.Lines[i] != "")
                {
                    outputList.Add(richTextBox1.Lines[i]);
                }
            }
            sql.writeSQLite(outputList, "bannedWords");
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
