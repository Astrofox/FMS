using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using MyExtention;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Collections.Concurrent;

namespace File_Manager_System
{
    public partial class Form3 : Form
    {
        static CancellationTokenSource[] Cancel_Download = new CancellationTokenSource[4];
        public static bool[] Check = new bool[4];
        private static string[] downpath = new string[4];
        static string[] ways = new string[4];
        static WebClient[] WC = new WebClient[4];
        static long[] leng = new long[4];


        #region Download Sync
        public static void Download(int n, CancellationToken Canceling)
        {
                string downloaded;
                WC[n].DownloadStringCompleted += (s, eArgs) =>
                {
                    try
                    {
                        My_File Used_file = new My_File();
                        Canceling.ThrowIfCancellationRequested();
                        downloaded = eArgs.Result;
                        Used_file.WriteAllText(downpath[n], downloaded);
                        Process.Start(downpath[n]);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Download was canceled!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };
                WC[n].DownloadStringAsync(new Uri(ways[n]));
        }
        #endregion

        #region Download Async
        public static void Downloading_File(int n, CancellationToken Canceling, IProgress<long> PR)
        {
            string downloaded = string.Empty;
            Uri mU = new Uri(ways[n]);
            try
            {
                HttpWebRequest FWR = (HttpWebRequest)WebRequest.Create(mU);
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)FWR.GetResponse();
                
                Stream receiveStream = myHttpWebResponse.GetResponseStream();
                leng[n] = myHttpWebResponse.ContentLength;
                
                StreamReader readStream = new StreamReader(receiveStream);
                
                char[] readBuffer = new Char[1024];
                
                int count = readStream.Read(readBuffer, 0, 1024);
                int total = 0;
                
                while (count > 0)
                {
                    total += count;
                    PR.Report(total);
                    Canceling.ThrowIfCancellationRequested();
                    String str = new String(readBuffer, 0, count);
                    downloaded += str;
                    count = readStream.Read(readBuffer, 0, 1024);
                }
                

                readStream.Close();
                myHttpWebResponse.Close();

                PR.Report(leng[n]);

                My_File Used_file = new My_File();
                Used_file.WriteAllText(downpath[n], downloaded);
                Process.Start(downpath[n]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Download was canceled!", MessageBoxButtons.OK, MessageBoxIcon.Warning);                
            }            
        }

        public async void Download_Async(int n)
        {
            var progr = new Progress<long>(i => progressBar1.Value = (int)(i * 100));
            switch (n)
            {
                case (0):
                    progr = new Progress<long>(i => progressBar1.Value = (int)(i * 100/leng[0]));
                    break;
                case (1):
                    progr = new Progress<long>(i => progressBar2.Value = (int)(i * 100 / leng[1]));
                    break;
                case (2):
                    progr = new Progress<long>(i => progressBar3.Value = (int)(i * 100 / leng[2]));
                    break;
                case (3):
                    progr = new Progress<long>(i => progressBar4.Value = (int)(i * 100 / leng[3]));
                    break;

            };
            await Task.Run(() => Downloading_File(n, Cancel_Download[n].Token, progr));
        }
        #endregion

        #region Interface
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty)
            {
                ways[0] = textBox1.Text;
                switch (Form1.Download_Type)
                {
                    case 1:
                        Download(0, Cancel_Download[0].Token);
                        break;
                    case 2:
                        Download_Async(0);
                        break;
                }
            }
            else
                MessageBox.Show("Please, print the way to the file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            textBox1.Text = "https://drive.google.com/open?id=0ByVn0fhHBG7kZ0ZPbG1xOVVWRzA";
            textBox2.Text = "https://drive.google.com/open?id=0ByVn0fhHBG7kZXpnU0dEOENCSWM";
            textBox3.Text = "https://drive.google.com/open?id=0ByVn0fhHBG7kblpsdi1WMGQwU3QydEhDM0QybXJDcUpSalcw";
            textBox5.Text = "https://drive.google.com/open?id=0ByVn0fhHBG7kN1JId3pmdWJRMDA";

            for (int i = 0;i<4;i++)
            {
                WC[i] = new WebClient();
                ways[i] = String.Empty;
                Cancel_Download[i] = new CancellationTokenSource();
            }
            timer1.Interval = 100;
            timer1.Start();
            
            downpath[0] = @"D:\FMS\Interest\Down1.txt";
            downpath[1] = @"D:\FMS\Interest\Down2.txt";
            downpath[2] = @"D:\FMS\Interest\Down3.txt";
            downpath[3] = @"D:\FMS\Interest\Down4.txt";

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != string.Empty)
            {
                ways[1] = textBox2.Text;
                switch (Form1.Download_Type)
                {
                    case 1:
                        Download(1, Cancel_Download[1].Token);
                        break;
                    case 2:
                        Download_Async(1);
                        break;
                }
            }
            else
                MessageBox.Show("Please, print the way to the file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (textBox3.Text != string.Empty)
            {
                ways[2] = textBox3.Text;
                switch (Form1.Download_Type)
                {
                    case 1:
                        Download(2, Cancel_Download[2].Token);
                        break;
                    case 2:
                        Download_Async(2);
                        break;
                }
            }
            else
                MessageBox.Show("Please, print the way to the file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (textBox5.Text != string.Empty)
            {
                ways[3] = textBox5.Text;
                switch (Form1.Download_Type)
                {
                    case 1:
                        Download(3, Cancel_Download[3].Token);
                        break;
                    case 2:
                        Download_Async(3);
                        break;
                }
            }
            else
                MessageBox.Show("Please, print the way to the file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Check[0] = true;
            Cancel_Download[0].Cancel();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Check[1] = true;
            Cancel_Download[1].Cancel();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Check[2] = true;
            Cancel_Download[2].Cancel();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Check[3] = true;
            Cancel_Download[3].Cancel();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                if (Check[i])
                {
                   // WC[i].CancelAsync();
                }
            }
        }
        #endregion

    }
}
