using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using ExtensionMethods;
using System.Windows.Forms.DataVisualization.Charting;
namespace GanonbotCsharp
{
    public partial class MainMenu : Form
    {
        //http://paletton.com/#uid=33y0u0kaSlZ2fM66JtlcHe6leaU
        //http://blog.crazyegg.com/2012/07/11/website-color-palettes/
        static IRC bot;
        delegate void SetTextCallback(string text, Color color, bool isMod, string name);
        delegate void AddUserInteractionChartCallback(string name, int nameIndex, int count, bool exists);
        delegate void ClearChartCallback();
        delegate void ReloadBotConfigsCallback();
        delegate void AddInteractionChartCallback(int count);
        int currentInteractionCells = 0;
        public MainMenu()
        {
            InitializeComponent();
            InitIRC();
        }
        private void InitIRC()
        {
            bot = new IRC();
            if (!bot.IsAuthenticated())
            {
                WebAuthentication form = new WebAuthentication(bot.GetAuthenticationTokenUrl());
                form.Show();
            }
        }
        public void SetText(string text, Color color, bool isMod, string name = "")
        {
            if (this.chatTextBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text, color, isMod, name });
            }
            else
            {
                if(isMod)
                {
                    this.chatTextBox.AppendText("MOD ", Color.Red);
                }
                this.chatTextBox.AppendText(name, color);
                this.chatTextBox.AppendText(": ");
                this.chatTextBox.AppendText(text);
                this.chatTextBox.AppendText(Environment.NewLine);
                this.chatTextBox.ScrollToCaret();
            }
        }

        public void AddUserInteractionChart(string name, int nameIndex, int count, bool exists)
        {
            if (this.chatTextBox.InvokeRequired)
            {
                AddUserInteractionChartCallback d = new AddUserInteractionChartCallback(AddUserInteractionChart);
                this.Invoke(d, new object[] { name, nameIndex, count, exists });
            }
            else
            {
                if (nameIndex > -1)
                {
                    Series interaction = this.chart1.Series.FindByName("Interaction");
                    interaction.Points.AddXY(nameIndex, count);
                    interaction.Points[nameIndex].AxisLabel = name + ": " + count;
                }
            }
        }

        public void ClearChart()
        {
            if (this.chatTextBox.InvokeRequired)
            {
                ClearChartCallback d = new ClearChartCallback(ClearChart);
                this.Invoke(d, new object[] { });
            }
            else
            {
                Series topUserInteraction = this.chart1.Series.FindByName("Interaction");
                topUserInteraction.Points.Clear();
            }
        }

        public void AddInteractionChart(int count)
        {
           
            if (this.chatTextBox.InvokeRequired)
            {
                AddInteractionChartCallback d = new AddInteractionChartCallback(AddInteractionChart);
                this.Invoke(d, new object[] { count });
            }
            else
            { 
                chart2.ChartAreas[0].AxisX.Minimum = 0;
                chart2.ChartAreas[0].AxisX.Maximum = 20;
                Series interaction = this.chart2.Series.FindByName("UserInteraction");
                if (interaction.Points.Count < 21)
                {
                    interaction.Points.AddXY(currentInteractionCells, count);
                    currentInteractionCells++;
                }
                else
                {
                    for (int i = 0; i < interaction.Points.Count-1; i++)
                    {
                        interaction.Points[i].SetValueXY(interaction.Points[i].XValue,interaction.Points[i + 1].YValues[0]); 
                    }
                    interaction.Points[20].SetValueXY(20, count);
                    chart2.ChartAreas[0].RecalculateAxesScale();
                    chart2.Refresh();
                }
            }
        }

        public void SetUserToken(string url)
        {
            bot.SetAccountToken(url);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bot.SendMessage(textBox1.Text);
            textBox1.Text = "";
        }
        private void MainMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            bot.closeProgram();
        }

        private void bannedWordsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            BannedWords form = new BannedWords();
            form.Show();
        }

        private void bannedWordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TimeoutWords form = new TimeoutWords();
            form.Show();
        }

        public void ReloadBotConfigs()
        {
            if (this.chatTextBox.InvokeRequired)
            {
                ReloadBotConfigsCallback d = new ReloadBotConfigsCallback(ReloadBotConfigs);
                this.Invoke(d, new object[] {  });
            }
            bot.ReloadConfigs();
        }

        private void commandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Commands form = new Commands();
            form.Show();
        }

        private void resetStatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bot.ClearStats();
            Series interaction = this.chart2.Series.FindByName("UserInteraction");
            interaction.Points.Clear();
            Series interaction2 = this.chart1.Series.FindByName("Interaction");
            interaction2.Points.Clear();
            currentInteractionCells = 0;
        }

        public void resetWeb()
        {
            WebAuthentication form = new WebAuthentication(bot.GetAuthenticationTokenUrl());
            form.Show();
        }

        private void giveAwayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GiveAway form = new GiveAway();
            form.Show();
        }

        private void giveAwayWPFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //var wpfwindow = new WPFCustom.Giveaway();
            //ElementHost.EnableModelessKeyboardInterop(wpfwindow);
            //wpfwindow.Show();

        }
    }
}
