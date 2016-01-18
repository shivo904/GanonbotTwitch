﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GanonbotCsharp
{
    public partial class Commands : Form
    {
        SQLiteParser sql = new SQLiteParser();
        List<List<string>> readInList = new List<List<string>>();
        List<string> outputList = new List<string>();
        public Commands()
        {
            InitializeComponent();
        }

        private void Commands_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(this.pictureBox1, "Place any custom commands that you would like here. Note that commands can only be used by moderators.");
            readInList = sql.readSQLite("commands", "command", "response");
            int i = 0;
            dataGridView2.Rows.Add(readInList.Count);
            foreach (List<string> word in readInList)
            {
                dataGridView2[0, i].Value = word[0];
                dataGridView2[1, i].Value = word[1];
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
                    outputList.Add(dataGridView2[0, i].Value + "|" + dataGridView2[1, i].Value);
                }
            }
            sql.writeSQLite(outputList, "commands");
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
