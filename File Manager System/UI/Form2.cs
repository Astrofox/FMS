using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace File_Manager_System
{
    public partial class Form2 : Form
    {
        public delegate void Syn_Delegate(string mes);
        public static Syn_Delegate Search_Delegate;

        public Form2()
        {
            InitializeComponent();
        }

        private void Change1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog BD = new FolderBrowserDialog();
            BD.ShowDialog();
            Path1.Text = BD.SelectedPath;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Path1.Text = Form1.Get_Current_Destination();
            Path2.Text = Form1.Get_Current_Destination();
            File_System_Watcher.SD += Change_Log;
            Search_Delegate += Write_Log;
        }

        private void Change2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog BD = new FolderBrowserDialog();
            BD.ShowDialog();
            Path2.Text = BD.SelectedPath;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1.Start_Sync(Path1.Text, Path2.Text);
        }

        public void Change_Log(string s)
        {
            object[] arg = new object[1];
            arg[0] = s;
            this.Invoke(Search_Delegate,arg);
        }

        public void Write_Log(string s)
        {
            this.Log_box.Text += (s+"\r\n");
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1.Stop_Sync();
        }
    }
}
