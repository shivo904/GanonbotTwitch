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
    public partial class TimeoutWords : Form
    {
        SQLiteParser sql = new SQLiteParser();
        List<List<string>> readInList = new List<List<string>>();
        List<string> outputList = new List<string>();
        public TimeoutWords()
        {
            InitializeComponent();
        }

        private void TimeoutWords_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(this.pictureBox1, "Place any words or statements that you wish to remove from your chat." + Environment.NewLine + "The bot will timeout anyone who uses one of these statements for the diration specified." +Environment.NewLine+Environment.NewLine+"If you do not add a time, the bot will default to 10 minutes.");
            readInList = sql.readSQLite("timeoutWords", "name", "duration");
            int i = 0;
            dataGridView2.Rows.Add(readInList.Count);
            foreach (List<string> word in readInList)
            {
                dataGridView2[1, i].Value = word[0];
                dataGridView2[0, i].Value = word[1];
                i++;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            outputList.Clear();
            for (int i = 0; i < dataGridView2.RowCount; i++)
            {
                if ((string)dataGridView2[0, i].Value != "" && (string)dataGridView2[1, i].Value != "")
                {
                    outputList.Add(dataGridView2[1, i].Value + "|" + dataGridView2[0, i].Value);
                }
                // If the user left duration blank
                else if ((string)dataGridView2[1, i].Value != "")
                {
                    outputList.Add(dataGridView2[1, i].Value + "|600");
                }
            }
            sql.writeSQLite(outputList, "timeoutWords");
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
