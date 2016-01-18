using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GanonbotCsharp
{
    public partial class WebAuthentication : Form
    {
        public WebAuthentication(string url)
        {
            InitializeComponent();
            
            webBrowser1.Navigate(url);
        }

        private void webAuthentication_Load(object sender, EventArgs e)
        {

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //{http://localhost/#access_token=jr20lumnvydmks9pydjbc8unvys4c8&scope=channel_read+channel_editor+channel_commercial+channel_subscriptions+channel_check_subscription}
            //https://api.twitch.tv/kraken/oauth2/login
            //https://api.twitch.tv/kraken/oauth2/authenticate?action=authorize&client_id=lmk1e6br71c8z8i9n248ed8t12bm719&redirect_uri=http%3A%2F%2Fasdfghjkl&response_type=token&scope=channel_read+channel_editor+channel_commercial+channel_subscriptions+channel_check_subscription
            label1.Hide();
            if (webBrowser1.Url.ToString().StartsWith("http://asdfghjkl") || webBrowser1.Url.ToString().StartsWith("https://asdfghjkl"))
            {
                Program.mainForm.SetUserToken(webBrowser1.Url.ToString());
                this.Hide();
            }
            if (webBrowser1.Url.ToString().StartsWith("https://api.twitch.tv/kraken/oauth2/login"))
            {
                Program.mainForm.resetWeb();
                this.Hide();
            }
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            try
            {
                label1.Show();
                if (webBrowser1.Url.ToString().StartsWith("http://asdfghjkl") || webBrowser1.Url.ToString().StartsWith("https://asdfghjkl"))
                {
                    Program.mainForm.SetUserToken(webBrowser1.Url.ToString());
                    this.Hide();
                }
            }
            catch(Exception)
            {
            }
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            try
            {
                if (webBrowser1.Url.ToString().StartsWith("http://asdfghjkl") || webBrowser1.Url.ToString().StartsWith("https://asdfghjkl"))
                {
                    Program.mainForm.SetUserToken(webBrowser1.Url.ToString());
                    this.Hide();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
