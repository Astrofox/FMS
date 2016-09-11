using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace File_Manager_System
{
    static class Program
    {

        /// <summary>
        /// Главная точка входа приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Form1 Form_main = new Form1();
            Model_Presenter presenter = new Model_Presenter(Form_main);
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(Form_main);
            Environment.Exit(0);
        }
    }

    /*
    public class Refresher
    {
        public string destination;
        public static void Refresher(string Folder)
        {
            FileSystemWatcher wtchr = new FileSystemWatcher();
            wtchr.Path = Folder;
            wtchr.NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime;
            wtchr.Filter = "*.*";
            wtchr.Changed += wtchr_Change;
            wtchr.Created += wtchr_Change;
            wtchr.Deleted += wtchr_Change;
            wtchr.Renamed += new RenamedEventHandler(wtchr_Renamed);
            wtchr.EnableRaisingEvents = true;
            while (true)
            {

            }
        }
        public static void wtchr_Change(object sender, FileSystemEventArgs e)
        {
            Form1.Show_Folder1(destination, Form1.listView1);
        }
        public static void wtchr_Renamed(object sender, RenamedEventArgs e)
        {
            Form1.Show_Folder1(destination, Form1.listView1);
        }
    }
    */
}
