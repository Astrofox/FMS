using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using File_Manager_System;
using Ionic.Zip;
using File_Manager_System.IO;

namespace File_Manager_System
{
    public partial class Form1 : Form, IView
    {
        Settings sets;
        
        // Variables.
        #region SearchOutputFile
        private static string infopath = @"D:\Data.txt";
        
        public static ConcurrentDictionary<string,bool> dic = new ConcurrentDictionary<string, bool>();
        #endregion

        #region General_Variables
        private static string archi;
        public static int processrunning = 0;
        public static string path;
        private string actname;
        private int type = 0;
        private string search_fold;
        public static string[] serans = new string[Environment.ProcessorCount-1];
        private delegate void Sear(object o);
        DateTime Search_start;
        public static int Countfiles = 0;
        public static int Donefiles = 0;
        private static List<string> Folders_Path = new List<string>();
        public static int Download_Type;
        private static CancellationTokenSource Canceler = new CancellationTokenSource();
        public static Form2 F2;
        public Progress<int> Progr;

        #endregion

        #region Syncronisation_and_Watcher
        static File_System_Watcher Watch_Dog;
        public delegate void S_Delegate(My_Folder F, ListView b);
        public delegate void Up_Delegate(My_Folder F, ListView b);
        public static Up_Delegate up_Delegate;
        public static S_Delegate Search_Delegate = new S_Delegate(Show_Folder1);
        #endregion

        #region Events
        public event EventHandler<string[]> My_Rename;
        public event EventHandler<string[]> My_Copy;
        public event EventHandler<string> My_Move;
        public event EventHandler<string[]> My_Translate;
        public event EventHandler<string> My_Delete;
        public event EventHandler<string[]> My_Dearchive;
        public event EventHandler<string> My_Archive_sync;
        public event EventHandler<string> My_Archive_async;
        public event EventHandler<string> My_Archive_await;
        public event EventHandler<string> My_Archive_task;
        public event EventHandler<string> My_Search_sync;
        public event EventHandler<string> My_Search_async;
        public event EventHandler<string> My_Search_await;
        public event EventHandler<string> My_Search_task;
        public event EventHandler<string> My_Archive_tpl;
        public event EventHandler<string> My_Search_tpl;
        #endregion

        #region Table update
        public static void Show_Folder1(My_Folder F, ListView b)
        {
            try {
                b.Items.Clear();               
                IEnumerable<My_Folder> dirInfo = F.GetDirectories();
                foreach (My_Folder info in dirInfo)
                {
                    ListViewItem item1 = new ListViewItem(info.Name, 0);
                    item1.SubItems.Add("Folder");
                    item1.SubItems.Add(info.FullName);
                    item1.SubItems.Add(info.GetLastAccessTime().ToShortTimeString());
                    item1.SubItems.Add("-");
                    b.Items.AddRange(new ListViewItem[] { item1 });
                }
                My_File[] dirs = F.GetFiles();
                foreach (My_File dir in dirs)
                {
                    long sz = dir.Length;
                    ListViewItem item1 = new ListViewItem(dir.Name, 0);
                    item1.SubItems.Add("File");
                    item1.SubItems.Add(dir.FullName);
                    item1.SubItems.Add(dir.GetLastAccessTime().ToShortTimeString());
                    item1.SubItems.Add(sz.ToString());
                    b.Items.AddRange(new ListViewItem[] { item1 });
                }
            }
            catch (Exception) { }
        }        

        public void Show_Folder_with_Mask(My_Folder F, ListView b, string Mask)
        {
            try
            {
                string name_of_file;
                My_File[] dirs = F.GetFiles();
                foreach (My_File dir in dirs)
                {
                    if (Regex.IsMatch(dir.Name, Mask))
                    {
                        name_of_file = dir.FullName;
                        if (name_of_file.Contains(path))
                        name_of_file = name_of_file.Remove(0, path.Length + 1);
                        long sz = dir.Length;
                        ListViewItem item1 = new ListViewItem(name_of_file, 0);
                        item1.SubItems.Add("File");
                        item1.SubItems.Add(dir.FullName);
                        item1.SubItems.Add(dir.GetLastAccessTime().ToShortTimeString());
                        item1.SubItems.Add(sz.ToString());
                        b.Items.AddRange(new ListViewItem[] { item1 });
                    }
                }
                IEnumerable<My_Folder> dirInfo = F.GetDirectories();

                string name_of_fold;
                foreach (My_Folder info in dirInfo)
                {
                    name_of_fold = info.FullName;
                    if (Regex.IsMatch(info.Name, Mask))
                    {
                        if (name_of_fold.Contains(path))
                            name_of_fold = name_of_fold.Remove(0, path.Length+1);
                        ListViewItem item1 = new ListViewItem(name_of_fold, 0);
                        item1.SubItems.Add("Folder");
                        item1.SubItems.Add(info.FullName);
                        item1.SubItems.Add(info.GetLastAccessTime().ToShortTimeString());
                        item1.SubItems.Add("-");
                        b.Items.AddRange(new ListViewItem[] { item1 });
                    }
                    Show_Folder_with_Mask(new My_Folder(info.FullName), b, Mask);
                }
            }
            catch (Exception)
            { }
        }

        public static string Get_Current_Destination()
        {
            return path;
        }

        public static string Get_Mask(string text)
        {
            StringBuilder mask = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '*')
                {
                    mask.Append("(\\w*\\s*)*");
                }
                else
                {
                    if (text[i] == '?')
                    {
                        mask.Append("[\\w\\s]");
                    }
                    else
                    {
                        if (text[i] == '.')
                        {
                            mask.Append("\\.");
                        }
                        else
                        {
                            if (text[i] == '-')
                            {
                                mask.Append("\\-");
                            }
                            else
                            {
                                if (text[i] == '(')
                                {
                                    mask.Append("\\(");
                                }
                                else
                                {
                                    if (text[i] == ')')
                                    {
                                        mask.Append("\\)");
                                    }
                                    else
                                    {
                                        if (text[i] == ',')
                                        {
                                            mask.Append("\\,");
                                        }
                                        else
                                        {
                                            if (text[i] == '!')
                                            {
                                                mask.Append("\\!");
                                            }
                                            else
                                            {
                                                mask.Append(text[i]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return mask.ToString();
        }

        public void Refresh(My_Folder F)
        {
            path = F.FullName;
            Show_Folder1(F, listView1);
            File_System_Watcher.Change_Destination(F.FullName);
        }

        public void Arch_Refresh(My_Folder F)
        {
            path = F.FullName;
            Show_Folder1(F, listView1);
        }
        #endregion

        #region Syncronization

        public void Show_Watcher(string s)
        {
            object[] arg = new object[2];
            arg[0] = s;
            arg[1] = listView1;
            this.Invoke(Search_Delegate,arg);
        }

        public static void Start_Sync(string path1, string path2)
        {
            try
            {
                if ((path1 != string.Empty) & (path2 != string.Empty))
                {
                    #region Copingfiles
                    
                    string name1 = string.Empty;
                    string name2 = string.Empty;
                    My_Folder new_dir;
                    My_Folder di;
                    di = new My_Folder(path1);
                    IEnumerable<My_Folder> dirInfo = di.GetDirectories();
                    foreach (My_Folder info in dirInfo)
                    {
                        new_dir = new My_Folder(info.FullName);
                        new_dir.CreateDirectory(new_dir.FullName);
                        new_dir.Copy(Path.Combine(path2, info.Name));
                    }
                    My_File Used_file = new My_File(path1);
                    My_File[] dirs = Used_file.GetFiles();
                    foreach (My_File dir in dirs)
                    {
                        Used_file = new My_File(name2);
                        name2 = Path.Combine(path2, dir.Name);
                        if (!Used_file.Exists())
                            dir.Copy(name2);
                    }
                    di = new My_Folder(path2);
                    dirInfo = di.GetDirectories();
                    foreach (My_Folder info in dirInfo)
                    {
                        info.Copy(Path.Combine(path1, info.Name));
                        info.CreateDirectory(Path.Combine(path1, info.Name));
                    }
                    dirs = di.GetFiles();
                    foreach (My_File dir in dirs)
                    {
                        Used_file = new My_File(name2);
                        name2 = Path.Combine(path1, dir.Name);
                        if (!Used_file.Exists())
                            dir.Copy(name2);
                    }
                    #endregion

                    File_System_Watcher.Watching_Sync_Start(path1, path2);
                    Watch_Dog.Enable_Sync();
                }
                else
                {
                    MessageBox.Show("Plese, select two folders.", "Attention!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Attention!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static void Stop_Sync()
        {
            Watch_Dog.Disable_Sync();
        }
        #endregion

        // Interface.

        #region Form
        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            path = @"D:\";
            comboBox1.Text = @"D:\"; 
            DriveInfo[] drivs = DriveInfo.GetDrives();
            foreach (DriveInfo info in drivs)
            {
                comboBox1.Items.Add(info.Name);
            }
            // Set the view to show details.
            listView1.View = View.Details;
            // Allow the user to edit item text.
            listView1.LabelEdit = true;
            // Allow the user to rearrange columns.
            listView1.AllowColumnReorder = true;
            // Select the item and subitems when selection is made.
            listView1.FullRowSelect = true;
            // Display grid lines.
            listView1.GridLines = true;
            // Sort the items in the list in ascending order.
            listView1.Sorting = SortOrder.Ascending;

            // Create columns for the items and subitems.
            // Width of -2 indicates auto-size.
            listView1.Columns.Add("Name", 130, HorizontalAlignment.Left);
            listView1.Columns.Add("Type", 70, HorizontalAlignment.Left);
            listView1.Columns.Add("Full name", 170, HorizontalAlignment.Left);
            listView1.Columns.Add("Last accessed", 160, HorizontalAlignment.Left);
            listView1.Columns.Add("Size", 100, HorizontalAlignment.Left);
            //Загрузка директории.
            Show_Folder1(new My_Folder(path), listView1);

            My_File Used_file = new My_File("Sets.dat");
            //Загрузка сериализованных настроек из файла.
            sets = new Settings();
            Stream fStream = Used_file.OpenRead();
            BinaryFormatter binform = new BinaryFormatter();
            sets = (Settings)binform.Deserialize(fStream);
            fStream.Close();
            listView1.Font = sets.Fnt;
            groupBox1.BackColor = sets.Clr;
            groupBox2.BackColor = sets.Clr;
            groupBox3.BackColor = sets.Clr;
            groupBox4.BackColor = sets.Clr;
            this.BackColor = sets.Clr;
            groupBox2.Visible = false;


            //Настройка таймера
            timer1.Interval = 1000;
            timer2.Interval = 1000;
            timer3.Interval = 100;
            timer2.Start();

            Watch_Dog = new File_System_Watcher();
            File_System_Watcher.MD +=Update_tab; 
            Watch_Dog.Enable();

            Search_Delegate += Show_Folder1;

        }

        public void Update_tab(string s)
        {
            object[] arg = new object[2];
            arg[0] = new My_Folder(s);
            arg[1] = listView1;
            this.Invoke(Search_Delegate, arg);
        }

        public void Progress_bar_work()
        {
            timer3.Start();
        }

        public void Start_timer_1()
        {
            timer1.Start();
        }

        public void Stop_timer_3()
        {
            timer3.Stop();
        }
        #endregion

        #region Interface
        
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems!=null)
            {
                try
                {
                    My_Move(this,Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString()));                    
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("You do not have an access", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (IndexOutOfRangeException)
                {
                    MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (ArgumentOutOfRangeException)
                {
                    MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception)
                {
                    MessageBox.Show("Womething went wrong", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                My_Move(this, Path.GetDirectoryName(path));
            }
            catch(Exception ex)
            {
                Show_Folder1(new My_Folder(path), listView1);
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems != null)
            {
                try
                {
                    type = 1;
                    actname = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                    listView1.SelectedItems[0].BackColor = sets.Clr;
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("You do not have an access", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (IndexOutOfRangeException)
                {
                    MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (ArgumentOutOfRangeException)
                {
                    MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception)
                {
                    MessageBox.Show("Womething went wrong", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems != null)
            {
                try
                {
                    type = 2;
                    actname = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                    listView1.SelectedItems[0].BackColor = sets.Clr;
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("You do not have an access", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (IndexOutOfRangeException)
                {
                    MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (ArgumentOutOfRangeException)
                {
                    MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception)
                {
                    MessageBox.Show("Womething went wrong", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string[] ass = new string[2];
                string t = path;
                if (t.Contains(actname))
                {
                    MessageBox.Show("You can not copy a folde into itself.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                else
                {
                    switch (type)
                    {
                        case 0:
                            MessageBox.Show("You did not select anything", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            break;
                        case 1:
                            ass[0] = actname;
                            ass[1] = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                            My_Copy(this, ass);                            
                            break;
                        case 2:
                            ass[0] = actname;
                            ass[1] = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                            My_Translate(this, ass);
                            break;
                        default:
                            MessageBox.Show("Something went wrong.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            break;
                    }
                }
                type = 0;
                Show_Folder1(new My_Folder(path), listView1);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have an access", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Womething went wrong", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems != null)
                {
                    string p = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                    My_Delete(this,p);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have an access", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Womething went wrong", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void changeNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems != null)
                {
                    textBox1.Text = listView1.SelectedItems[0].SubItems[0].Text.ToString();
                    groupBox1.Visible = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have an access", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Womething went wrong", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dearchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems != null)
                {

                    string p = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                    if (Path.GetExtension(p) == ".zip")
                    {
                        string a = Path.Combine(path, Path.GetFileNameWithoutExtension(p));
                        string[] assis = { p, a };
                        My_Dearchive(this, assis);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have an access", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Womething went wrong", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
            if (textBox1.Text != null)
            {
                groupBox1.Visible = false;
                string p1 = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                string p2 = Path.Combine(path, textBox1.Text);
                    string[] p = { p1, p2 };
                    My_Rename(this, p);
            }
            }
            catch (IOException)
            {
                MessageBox.Show("You do not have an access", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show("Something wen wrong", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                DriveInfo info = new DriveInfo(comboBox1.SelectedItem.ToString());
                if (info.IsReady)
                {
                    My_Move(this, comboBox1.SelectedItem.ToString());
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Device is unaveliable", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
                try
                {

                string act = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                My_Move(this, act);
                    //path = act;
                    //Show_Folder1(path, listView1);
                    //File_System_Watcher.Change_Destination(path);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("You do not have an access", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    path = Path.GetDirectoryName(path);
                    Show_Folder1(new My_Folder(path), listView1);
                }
                catch (Exception)
            {
                path = Path.GetDirectoryName(path);
                Show_Folder1(new My_Folder(path), listView1);
                MessageBox.Show("Something wen wrong", "Warning");
                }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (sets.Authorize(textBox3.Text, textBox2.Text))
            {
                MessageBox.Show("Now, you have an access.", "Authorization is completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                groupBox2.Visible = false;
                listView1.Visible = true;
            }
            else
            {
                MessageBox.Show("Please, try again.", "Authorization is currupted", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            BinaryFormatter binform = new BinaryFormatter();
            Stream fStream = new FileStream("Sets.dat", FileMode.Create, FileAccess.Write, FileShare.None);
            binform.Serialize(fStream, sets);
            fStream.Close();
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowDialog();
            sets.Fnt = fontDialog1.Font;
            listView1.Font = sets.Fnt;
        }

        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            sets.Clr = colorDialog1.Color;
            groupBox1.BackColor = sets.Clr;
            groupBox2.BackColor = sets.Clr;
            this.BackColor = sets.Clr;
        }
        
        private void getMD5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems != null)
                {
                    My_File fi = new My_File(Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString()));
                    MessageBox.Show(fi.Get_MD5());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void getEncodingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems != null)
                {
                    My_File fi = new My_File(Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString()));
                    MessageBox.Show(fi.Get_Encoding().ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void getToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems != null)
                {
                    My_File fi = new My_File(Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString()));
                    MessageBox.Show(fi.Get_Rights(), "File rights", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int n = 0;
            for (int i = 0; i < processrunning; i++)
            {
                if (Form1.serans[i] != string.Empty)
                    n++;
            }
            if ((n == processrunning))
            {
                StreamWriter sw = new StreamWriter(Form1.infopath);
                for (int q = 0; q < serans.Length; q++)
                {
                    sw.WriteLine(serans[q]);
                    serans[q] = string.Empty;
                }
                sw.Close();

                processrunning = 0;
                if (Donefiles == Countfiles)
                {
                    this.timer1.Stop();
                    Process.Start(infopath);
                    MessageBox.Show((DateTime.Now - Search_start).ToString(), "Time", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    My_Folder tmp = new My_Folder("D:\\Restart\\AF\\VS_Projects\\tmp");
                    tmp.Clear();
                }
            }
                    
        }

        private void syncronizationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            F2 = new Form2();
            F2.Show();
        }

        private void simpleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Search_start = DateTime.Now;
            try
            {
                search_fold = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                My_Search_sync(this,search_fold);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("You do not have an access" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException ex)
            {
                MessageBox.Show("Select an item" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("Select an item" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Womething went wrong" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void syncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime time = DateTime.Now;
            try
            {
                if (listView1.SelectedItems != null)
                {
                    string p = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                    if (Path.GetExtension(p) != ".zip")
                    {
                        My_Archive_sync(this, p);
                    }
                    else
                    {
                        MessageBox.Show("Archive is already exists", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    MessageBox.Show((DateTime.Now - time).ToString(), "Time", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have an access", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Womething went wrong", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void asyncToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DateTime time = DateTime.Now;
            try
            {
                if (listView1.SelectedItems != null)
                {
                    string p = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                    if (Path.GetExtension(p) != ".zip")
                    {
                        My_Archive_async(this, p);
                    }
                    else
                    {
                        MessageBox.Show("Archive is already exists", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    MessageBox.Show((DateTime.Now - time).ToString(), "Time", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have an access", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Womething went wrong", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }        

    }
        
        private void asyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Search_start = DateTime.Now;
            try
            {
                search_fold = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                My_Search_async(this, search_fold);
                timer1.Start();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("You do not have an access" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException ex)
            {
                MessageBox.Show("Select an item" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("Select an item" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Womething went wrong" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void tPLToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Search_start = DateTime.Now;
            try
            {
                search_fold = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                My_Search_tpl(this, search_fold);
                //timer1.Start();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("You do not have an access" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException ex)
            {
                MessageBox.Show("Select an item" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("Select an item" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Womething went wrong" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void tasksToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Search_start = DateTime.Now;
            try
            {
                search_fold = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                My_Search_task(this, search_fold);
                timer1.Start();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("You do not have an access" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException ex)
            {
                MessageBox.Show("Select an item" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("Select an item" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Womething went wrong" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void tPLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime time = DateTime.Now;
            try
            {
                if (listView1.SelectedItems != null)
                {
                    string p = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                    if (Path.GetExtension(p) != ".zip")
                    {
                        My_Archive_tpl(this, p);
                    }
                    else
                    {
                        MessageBox.Show("Archive is already exists", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    MessageBox.Show((DateTime.Now - time).ToString(), "Time", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have an access", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Womething went wrong", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void tasksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime time = DateTime.Now;
            try
            {
                if (listView1.SelectedItems != null)
                {
                    string p = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                    if (Path.GetExtension(p) != ".zip")
                    {
                        My_Archive_task(this, p);
                    }
                    else
                    {
                        MessageBox.Show("Archive is already exists", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    MessageBox.Show((DateTime.Now - time).ToString(), "Time", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have an access", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Womething went wrong", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (Countfiles > 0)
                progressBar1.Value = 100 * Donefiles / Countfiles;
        }

        private void timer_sync_Tick(object sender, EventArgs e)
        {
            if(!File_System_Watcher.Check_Files_Sync())
            { Show_Folder1(new My_Folder(path), listView1); }
        }

        private void syncToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Download_Type = 1;
            Form3 F3 = new Form3();
            F3.Show();
        }

        private void asyncToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Download_Type = 2;
            Form3 F3 = new Form3();
            F3.Show();
        }

        private void awaitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DateTime time = DateTime.Now;
            try
            {
                Progr = new Progress<int>(i => progressBar1.Value = (int)(i * 100 / Countfiles));
                if (listView1.SelectedItems != null)
                {
                    string p = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());

                   // Start_Archive_Await(p, Canceler.Token, Progr);
                    if (Path.GetExtension(p) != ".zip")
                    {
                        My_Archive_await(this, p);
                    }
                    else
                    {
                        MessageBox.Show("Archive is already exists", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    MessageBox.Show((DateTime.Now - time).ToString(), "Time", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have an access", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Select an item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Womething went wrong", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void awaitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Search_start = DateTime.Now;
            try
            {
                var Progr = new Progress<int>(i => progressBar1.Value = (int)(i * 100 / Countfiles));
                search_fold = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
                My_Search_await(this, search_fold);
                //Start_Await_Search(search_fold, Canceler.Token, Progr);
                timer1.Start();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("You do not have an access" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException ex)
            {
                MessageBox.Show("Select an item" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("Select an item" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Womething went wrong" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Canceler.Cancel();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            string Mask = Get_Mask(textBox4.Text);

            if (Mask != string.Empty)
            {
                listView1.Items.Clear();
                if (!path.Contains(".zip"))
                    Show_Folder_with_Mask(new My_Folder(path), listView1, Mask);
                else
                {
                    if (!(Path.GetExtension(path) == ".zip"))
                    {
                        int i = Regex.Match(path, ".zip").Index;
                        Show_Folder_with_Mask(new My_ZipFolder(path, new My_ZipArchive(path.Substring(0, i + 4))), listView1, Mask);
                    }
                    else
                    {
                        Show_Folder_with_Mask(new My_ZipArchive(path), listView1, Mask);
                    }
                }
            }
            else
            {
                if (!path.Contains(".zip"))
                    Show_Folder1(new My_Folder(path), listView1);
                else
                {
                    if (!(Path.GetExtension(path) == ".zip"))
                    {
                        int i = Regex.Match(path, ".zip").Index;
                        Show_Folder1(new My_ZipFolder(path, new My_ZipArchive(path.Substring(0, i + 4))), listView1);
                    }
                    else
                    {
                        Show_Folder1(new My_ZipArchive(path), listView1);
                    }
                }
            }
        }

        private void searchInBookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string book = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
            Form4 Srch_bk = new Form4();
            Srch_bk.Show();
            Srch_bk.Search_in_book(book);
        }

        private void encryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string c_path = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
            List<My_Entry> Cr_List = new List<My_Entry>();
            Cr_List.Add(new My_File(c_path));
            Form5 F5 = new Form5();
            F5.Encrypt = true;
            F5.Objects = Cr_List;
            F5.Show();
        }

        private void decryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string c_path = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
            List<My_Entry> Cr_List = new List<My_Entry>();
            Cr_List.Add(new My_File(c_path));
            Form5 F5 = new Form5();
            F5.Encrypt = false;
            F5.Objects = Cr_List;
            F5.Show();
        }
        
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            groupBox3.Visible = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            My_ZipArchive zip = new My_ZipArchive(path+"\\"+textBox5.Text);
            zip.Create();
            textBox5.Text = "";
            groupBox3.Visible = false;
        }

        private void dearchiveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string act = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
            My_ZipArchive zip = new My_ZipArchive(act);
            zip.Dearchive();
        }

        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            archi = Path.Combine(path, listView1.SelectedItems[0].SubItems[0].Text.ToString());
            groupBox4.Visible = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            My_ZipArchive zip = new My_ZipArchive(archi);
            zip.AddFile(new My_File(path+"\\"+textBox6.Text));
            textBox6.Text = "";
            groupBox4.Visible = false;
            archi = "";
        }

        public void add_to_serans(string s,int i)
        {
            serans[i] = s;
        }

        #endregion
    }

    [Serializable]
    public class Settings
    {
        public Color Clr;
        public Font Fnt;
        public string[] logins;
        public string[] passes;

        public bool Authorize(string l, string p)
        {
            bool ans = false;
            for (int i = 0; i < logins.Length; i++)
            {
                if ((l == logins[i]) & (p == passes[i]))
                {
                    ans = true;
                    break;
                }
            }
            return ans;
        }

        [OnSerializing]
        internal void Encrypt(StreamingContext context)
        {
            string sup;
            for (int i = 0; i < logins.Length; i++)
            {
                sup = "";
                for (int q = 0; q < logins[i].Length; q++)
                {
                    sup += (char)(logins[i][q] + 4);
                }
                logins[i] = sup;
                sup = "";
                for (int q = 0; q < passes[i].Length; q++)
                {
                    sup += (char)(passes[i][q] + 4);
                }
                passes[i] = sup;
            }
        }

        [OnDeserialized]
        internal void Decrypt(StreamingContext context)
        {
            string sup;
            for (int i = 0; i < logins.Length; i++)
            {
                sup = "";
                for (int q = 0; q < logins[i].Length; q++)
                {
                    sup += (char)(logins[i][q] - 4);
                }
                logins[i] = sup;
                sup = "";
                for (int q = 0; q < passes[i].Length; q++)
                {
                    sup += (char)(passes[i][q] - 4);
                }
                passes[i] = sup;
            }
        }
    }

    public struct Search_Info
    {
        public int n;
        public Queue<string> str;
        public Search_Info(int i, Queue<string> q)
        {
            n = i;
            str = q;
        }
    }

    public class File_System_Watcher
    {
        #region Watch
        Thread Watch_Thread = new Thread(Wtch_zaglushka);
        public delegate void My_Delegate(string fold);
        public static My_Delegate MD;
        static string dest;
        public static Dictionary<string, File_Inf> List_of_Files = new Dictionary<string, File_Inf>();
        #endregion

        #region Sync
        Thread Sync_Thread = new Thread(Syncing);
        static string sync1 = string.Empty;
        static string sync2 = string.Empty;
        public static Dictionary<string, File_Inf> List_of_Files1 = new Dictionary<string, File_Inf>();
        public static Dictionary<string, File_Inf> List_of_Files2 = new Dictionary<string, File_Inf>();
        public delegate void Sync_Delegate(string s);
        public static Sync_Delegate SD;
        public static string reason;
        #endregion
        
        public File_System_Watcher()
        {
            dest = Form1.Get_Current_Destination(); 
        }
        public void Enable()
        {
            Watch_Thread.Start();
        }
        public void Disable()
        {
            Watch_Thread.Abort();
        }
        public void Enable_Sync()
        {
            Sync_Thread.Start();
        }
        public void Disable_Sync()
        {
            Sync_Thread.Abort();
        }
        public static bool Watching(string dest)
        {
            Dictionary<string, File_Inf> New_List = new Dictionary<string, File_Inf>();
            Add_To_List(dest, New_List);
            return (Check_Files(List_of_Files, New_List));
        }
        public static void Add_To_List(string dest, Dictionary<string, File_Inf> List)
        { try
            {
                /*
                My_File[] dirs = My_Folder.GetFiles(dest);
                foreach (My_File dir in dirs)
                {
                    File_Inf FI = new File_Inf(dir.FullName);
                    List.Add(dir.FullName, FI);
                }
                My_Folder[] dirs2 = My_Folder.GetDirectories(dest);
                foreach (My_Folder dir in dirs2)
                {
                    Add_To_List(dir.FullName, List);
                }
                */
                string[] dirs = Directory.GetFiles(dest);
                foreach (string dir in dirs)
                {
                    File_Inf FI = new File_Inf(dir);
                    List.Add(dir, FI);
                }
                string[] dirs2 = Directory.GetDirectories(dest);
                foreach (string dir in dirs2)
                {
                    Add_To_List(dir, List);
                }

            }
            catch(Exception)
            { }
        }            
        public static void Watching_Start(string dest)
        {
            Add_To_List(dest, List_of_Files);
        }
        public static void Watching_Sync_Start(string dest1, string dest2)
        {
            sync1 = dest1;
            sync2 = dest2;
            Add_To_List(sync1, List_of_Files1);
            Add_To_List(sync2, List_of_Files2);
        }
        public  static void Syncing()
        {
            while (true)
            {
                if (!Check_Files_Sync()) 
                {
                    List_of_Files1.Clear();
                    List_of_Files2.Clear();
                    Watching_Sync_Start(sync1,sync2);                    
                    SD(reason);
                    reason = string.Empty;
                }
                Thread.Sleep(500);
            }
        }
        public static void Change_Destination(string s)
        {
            dest = s;
            List_of_Files.Clear();
            Watching_Start(dest);
        }
        public static bool Check_Files(Dictionary<string, File_Inf> q1, Dictionary<string, File_Inf> q2)
        {try
            {
                File_Inf test;
                foreach (var v in q1)
                {
                    if (q2.ContainsKey(v.Key))
                    {
                        q2.TryGetValue(v.Key, out test);
                        if (v.Value.sz != test.sz)
                        {
                            return false;
                        }
                        if (v.Value.tm != test.tm)
                        {                     
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                foreach (var v in q2)
                {
                    if (q1.ContainsKey(v.Key))
                    {
                        q1.TryGetValue(v.Key, out test);
                        if (v.Value.sz != test.sz)
                        {
                            return false;
                        }
                        if (v.Value.tm != test.tm)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            { }
            return true;
        }
        public static bool Check_Files_Sync()
        {
            My_File Used_file;
            string changed_file;
            bool Flag = true;
            File_Inf test;
            Dictionary<string, File_Inf> Chek1 = new Dictionary<string, File_Inf>();
            Dictionary<string, File_Inf> Chek2 = new Dictionary<string, File_Inf>();
            
            Add_To_List(sync1, Chek1);
            Add_To_List(sync2, Chek2);
            
            foreach (var v in Chek1)
            {
                if (List_of_Files1.ContainsKey(v.Key))
                {
                    List_of_Files1.TryGetValue(v.Key, out test);
                    if (v.Value.sz != test.sz)
                    {
                        changed_file = v.Key.Remove(0, sync1.Length + 1);
                        changed_file = Path.Combine(sync2, changed_file);
                        Used_file = new My_File(changed_file);
                        if (Used_file.Exists())
                            Used_file.Delete();
                        Used_file = new My_File(v.Key);
                        Used_file.Copy(changed_file);
                        reason = "File \r\n" + v.Key + " \r\n was changed \r\n";
                        return false;
                    }
                    if (v.Value.tm != test.tm)
                    {
                        reason = "File \r\n" + v.Key + " \r\n was accessed \r\n";
                        return false;
                    }
                }
                else
                {
                    changed_file = v.Key.Remove(0, sync1.Length + 1);
                    changed_file = Path.Combine(sync2, changed_file);
                    Used_file = new My_File(changed_file);
                    if (Used_file.Exists())
                        Used_file.Delete();
                    Used_file = new My_File(v.Key);
                    Used_file.Copy(changed_file);
                    reason = "File \r\n" + v.Key + " \r\n was created \r\n";
                    return false;
                }
            }
            foreach (var v in List_of_Files1)
            {
                if (!Chek1.ContainsKey(v.Key))
                {
                    changed_file = v.Key.Remove(0, sync1.Length+1);
                    changed_file = Path.Combine(sync2, changed_file);
                    Used_file = new My_File(changed_file);
                    if (Used_file.Exists())
                        Used_file.Delete();
                    reason = "File \r\n" + v.Key + " \r\n was deleted \r\n";
                    return false;
                }
            }
            foreach (var v in Chek2)
            {
                if (List_of_Files2.ContainsKey(v.Key))
                {
                    List_of_Files2.TryGetValue(v.Key, out test);
                    if (v.Value.sz != test.sz)
                    {
                        changed_file = v.Key.Remove(0, sync2.Length + 1);
                        changed_file = Path.Combine(sync1, changed_file);
                        Used_file = new My_File(changed_file);
                        if (Used_file.Exists())
                            Used_file.Delete();
                        Used_file = new My_File(v.Key);
                        Used_file.Copy(changed_file);
                        reason = "File \r\n" + v.Key + " \r\n was changed \r\n";
                        return false;
                    }
                    if (v.Value.tm != test.tm)
                    {
                        reason = "File \r\n" + v.Key + " \r\n was accessed \r\n";
                        return false;
                    }
                }
                else
                {
                    changed_file = v.Key.Remove(0, sync2.Length+1);
                    changed_file = Path.Combine(sync1, changed_file);
                    Used_file = new My_File(changed_file);
                    if (Used_file.Exists())
                        Used_file.Delete();
                    Used_file = new My_File(v.Key);
                    Used_file.Copy(changed_file);
                    reason = "File \r\n" + v.Key + " \r\n was created \r\n";
                    return false;
                }
            }
            foreach (var v in List_of_Files2)
            {
                if (!Chek2.ContainsKey(v.Key))
                {
                    changed_file = v.Key.Remove(0, sync2.Length + 1);
                    changed_file = Path.Combine(sync1, changed_file);
                    Used_file = new My_File(changed_file);
                    if (Used_file.Exists())
                        Used_file.Delete();
                    reason = "File \r\n" + v.Key + " \r\n was deleted \r\n";
                    return false;
                }
            }
            return Flag;
        }
        public static void Wtch_zaglushka()
        {
            while (true)
            {
                if ((!Watching(dest))&&(!Form1.path.Contains(".zip")))// Troubles with archives
                {
                    List_of_Files.Clear();
                    Watching_Start(dest);
                    MD(dest);
                }
                Thread.Sleep(100);
            }
        }
    }

    public class File_Inf
    {
        public string path;
        public My_File FI;
        public long sz;
        public DateTime tm;
        public File_Inf(string t)
        {
            path = t;
            FI = new My_File(path);
            sz = FI.Length;
            tm = FI.GetLastAccessTime();
        }
    }


    #region Archivators
    public abstract class Archivator
    {
        //protected static Form1 Main_Form;
        protected static int processrunning = 0;
        protected string actname;
        protected int type = 0;
        protected string search_fold;
        protected static string[] serans = new string[Environment.ProcessorCount - 1];
        protected delegate void Sear(object o);
        protected static List<string> Folders_Path = new List<string>();
        protected static CancellationTokenSource Canceler = new CancellationTokenSource();

        public abstract void Start(string Path);

        protected void Add_Arch(string PathFrom, int pc, ref int n, ref Queue<string>[] q)
        {
            My_File Used_file = new My_File(PathFrom);
            if (!Used_file.Exists())
            {
                My_Folder Used_fold = new My_Folder(PathFrom);
                Folders_Path.Add(PathFrom);
                My_Folder[] folds = Used_fold.GetDirectories();
                foreach (My_Folder fold in folds)
                {
                    Folders_Path.Add(fold.FullName);
                    Add_Arch(fold.FullName, pc, ref n, ref q);
                }
            }
            else
            {
                q[n].Enqueue(PathFrom);
                n = (n + 1) % pc;
                Form1.Countfiles++;
            }
        }

        protected void Arching_files(string PathFrom, int pc, ref int n, ref Queue<string>[] q)
        {
            My_Folder Used_Fold = new My_Folder(PathFrom);
            My_File[] names = Used_Fold.GetFiles();
            foreach (My_File name in names)
            {
                q[n].Enqueue(name.FullName);
                n = (n + 1) % pc;
                object o = new object();
                lock (o)
                {
                    Form1.Countfiles++;
                }
                if (processrunning < pc)
                    processrunning++;
            }
        }

        protected void Archive(string PathFrom)
        {
            string PathTo = Path.Combine(PathFrom + ".zip");

            if ((Path.HasExtension(PathFrom)) & (Path.GetExtension(PathFrom) != ".zip"))
            {
                const int buffersz = 16384;
                byte[] buffer = new byte[buffersz];
                int btscpd = 0;
                using (Stream inFile = new My_File(PathFrom).OpenStream(FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream outFile = new My_File(PathTo).OpenStream(FileMode.Create, FileAccess.Write, FileShare.None))
                using (GZipStream zipfile = new GZipStream(outFile, CompressionMode.Compress))
                {
                    do
                    {
                        btscpd = inFile.Read(buffer, 0, buffersz);
                        zipfile.Write(buffer, 0, buffersz);
                    } while (btscpd > 0);
                }
            }
            object o = new object();
            lock (o)
            {
                Form1.Donefiles++;
            }
        }

        protected void Archive_Queue(object o)
        {
            Queue<string> q = (Queue<string>)o;
            string s;
            while (q.Count > 0)
            {
                s = q.Dequeue();
                Archive(s);
            }
        }
        
    }

    public class Archivator_Sync: Archivator
    {
        public Archivator_Sync()
        {
           // Main_Form = Form;
        }

        public override void Start(string path)
        {
            Folders_Path.Clear();
            int pc = Environment.ProcessorCount - 1;
            int n = 0;
            Queue<string>[] QueArc = new Queue<string>[pc];
            for (int q = 0; q < pc; q++)
            {
                QueArc[q] = new Queue<string>();
            }
            Add_Arch(path, pc, ref n, ref QueArc);
            Parallel.ForEach(Folders_Path, fold => Arching_files(fold, pc, ref n, ref QueArc));
            //Main_Form.Progress_bar_work();
            for (int u = 0; u < pc; u++)
            {
                new Thread(Archive_Queue).Start(QueArc[u]);
            }
        }
    }
    public class Archivator_Async : Archivator
    {
        public Archivator_Async()
        {
            //Main_Form = Form;
        }

        public override void Start(string path)
        {

            Folders_Path.Clear();
            int pc = Environment.ProcessorCount - 1;
            int n = 0;
            Queue<string>[] QueArc = new Queue<string>[pc];
            for (int q = 0; q < pc; q++)
            {
                QueArc[q] = new Queue<string>();
            }
            Add_Arch(path, pc, ref n, ref QueArc);

            Parallel.ForEach(Folders_Path, fold => Arching_files(fold, pc, ref n, ref QueArc));
            Sear[] a = new Sear[pc];
            IAsyncResult[] aAR = new IAsyncResult[pc];
            for (int u = 0; u < pc; u++)
            {
                a[u] = new Sear(Archive_Queue);
                aAR[u] = a[u].BeginInvoke(QueArc[u], null, null);
                a[u].EndInvoke(aAR[u]);
            }
        }
    }
    public class Archivator_TPL : Archivator
    {
        public Archivator_TPL()
        {
            //Main_Form = Form;
        }

        public override void Start(string Path)
        {
            Folders_Path.Clear();
            int pc = Environment.ProcessorCount - 1;
            int n = 0;
            Queue<string>[] QueArc = new Queue<string>[pc];
            for (int q = 0; q < pc; q++)
            {
                QueArc[q] = new Queue<string>();
            }
            Add_Arch(Path, pc, ref n, ref QueArc);

            Parallel.ForEach(Folders_Path, fold => Arching_files(fold, pc, ref n, ref QueArc));
            //Main_Form.Progress_bar_work();
            Sear[] a = new Sear[pc];
            for (int u = 0; u < pc; u++)
            {
                a[u] = new Sear(Archive_Queue);
                IAsyncResult aAR = a[u].BeginInvoke(QueArc[u], null, null);
                a[u].EndInvoke(aAR);
            }
            Parallel.ForEach(QueArc, Que =>
                        Archive_Queue(Que));
        }
    }
    public class Archivator_Tasks : Archivator
    {
        public Archivator_Tasks()
        {
            //Main_Form = Form;
        }

        public override void Start(string path)
        {
            Folders_Path.Clear();
            int pc = Environment.ProcessorCount - 1;
            int n = 0;
            Queue<string>[] QueArc = new Queue<string>[pc];
            for (int q = 0; q < pc; q++)
            {
                QueArc[q] = new Queue<string>();
            }
            Add_Arch(path, pc, ref n, ref QueArc);

            Parallel.ForEach(Folders_Path, fold => Arching_files(fold, pc, ref n, ref QueArc));
            //Main_Form.Progress_bar_work();
            Action<Queue<string>> action = (Queue<string> obj) =>
            {
                Archive_Queue(obj);
            };
            for (int u = 0; u < pc; u++)
            {
                int y = u;
                Task Search = Task.Run
                     (() =>
                     {
                         Archive_Queue(QueArc[y]);
                     });
            }
        }
    }
    public class Archivator_Await : Archivator
    {
        public Archivator_Await()
        {
            //Main_Form = Form;
        }

        public override void Start(string Path)
        {
            Start_Archive_Await(Path);
        }
        public async void Start_Archive_Await(string path)
        {
            try
            {
                Folders_Path.Clear();
                int pc = Environment.ProcessorCount - 1;
                int n = 0;
                Queue<string>[] QueArc = new Queue<string>[pc];
                for (int q = 0; q < pc; q++)
                {
                    QueArc[q] = new Queue<string>();
                }
                Add_Arch(path, pc, ref n, ref QueArc);

                Parallel.ForEach(Folders_Path, fold => Arching_files(fold, pc, ref n, ref QueArc));
                //Main_Form.Progress_bar_work();
                Thread.Sleep(2000);
                await Arch_Work(pc, QueArc);
                //progr.Report(Form1.Donefiles);
                //Canc.ThrowIfCancellationRequested();
                //Main_Form.Stop_timer_3();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Process was canceled!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        private Task Arch_Work(int pc, Queue<string>[] QueArc)
        {
            return Task.Run(() =>
            {
                Sear[] a = new Sear[pc];
                IAsyncResult[] aAR = new IAsyncResult[pc];
                for (int u = 0; u < pc; u++)
                {
                    a[u] = new Sear(Archive_Queue);
                    aAR[u] = a[u].BeginInvoke(QueArc[u], null, null);
                    a[u].EndInvoke(aAR[u]);
                }
            });
        }
    }
    #endregion


    #region Serchers
    public interface Searcher
    {
        void Start(string path);
    }

    public class Basic_Searcher
    {
        protected static IView Main_Form;
        //protected static Form1 Main_Form;
        protected string actname;
        protected int type = 0;
        protected string search_fold;
        protected static string[] serans = new string[Environment.ProcessorCount - 1];
        protected delegate void Sear(object o);
        protected static List<string> Folders_Path = new List<string>();
        protected static CancellationTokenSource Canceler = new CancellationTokenSource();

        public Basic_Searcher(IView Form)
        {
            Main_Form = Form;
        }

        public static void Search_and_Save(string paths, int pc, ref int n, ref Queue<string>[] q)
        {
            My_File Used_file = new My_File(paths);
            if (Used_file.Exists())
            {
                try
                {
                    q[0].Enqueue(paths);
                    Form1.processrunning++;
                    Form1.Countfiles++;
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("You do not have an access.", "Warning");
                }
                catch (Exception)
                {
                    MessageBox.Show("Something wen wrong.", "Warning");
                }
            }
            else
            {
                Folders_Path.Add(paths);
                My_Folder Used_fold = new My_Folder(paths);
                My_Folder[] folds = Used_fold.GetDirectories();
                foreach (My_Folder fold in folds)
                {
                    try
                    {
                        Folders_Path.Add(fold.FullName);
                        Search_and_Save(fold.FullName, pc, ref n, ref q);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("You do not have an access.", "Warning");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Something wen wrong.", "Warning");
                    }
                }
            }
        }

        public static void Add_Files(string paths, int pc, ref int n, ref Queue<string>[] q)
        {
            My_Folder Used_fold = new My_Folder(paths);
            My_File[] dirs = Used_fold.GetFiles();
            foreach (My_File dir in dirs)
            {
                try
                {
                    q[n].Enqueue(dir.FullName);
                    n = (n + 1) % pc;
                    object o = new object();
                    lock (o)
                    {
                        Form1.Countfiles++;
                    }
                    if (Form1.processrunning < pc)
                        Form1.processrunning++;

                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("You do not have an access.", "Warning");
                }
                catch (Exception)
                {
                    MessageBox.Show("Something wen wrong.", "Warning");
                }
            }
        }

        protected static void Search_in_Queue(object o)
        {
            Search_Info q = (Search_Info)o;
            string s;
            while (q.str.Count > 0)
            {
                s = q.str.Dequeue();
                Search(s, q.n);
            }
            serans[q.n] += " ";
        }

        public static void Search(string Paths, int num)
        {
            try
            {
                My_File For_search = new My_File(Paths);
                string ans = string.Empty;
                string s;
                s = For_search.ReadAllText();
                Regex r = new Regex("7\\d{10}");
                StringBuilder sb = new StringBuilder();
                foreach (Match m in r.Matches(s))
                {
                    {
                        sb.Append("Telephone number: +");
                        sb.Append(m.ToString());
                        sb.Append("\r\n");
                        Form1.dic[m.ToString()] = true;
                    }
                }
                r = new Regex("7\\s\\d{3}\\s\\d{3}\\s\\d{4}");
                foreach (Match m in r.Matches(s))
                {                 
                    {
                        sb.Append("Telephone number: +");
                        sb.Append(m.ToString());
                        sb.Append("\r\n");
                        Form1.dic[m.ToString()] = true;
                    }
                }
                r = new Regex("\\sINN: \\d{11}\\s");
                foreach (Match m in r.Matches(s))
                {
                    {
                        sb.Append(m.ToString());
                        sb.Append("\r\n");
                        Form1.dic[m.ToString()] = true;
                    }
                }
                r = new Regex("(\\w+\\.)*\\w+@(\\w+[\\._\\-])*\\w*\\.\\w{2,}");
                foreach (Match m in r.Matches(s))
                {
                    {
                        sb.Append("Mail: ");
                        sb.Append(m.ToString());
                        sb.Append("\r\n");
                        Form1.dic[m.ToString()] = true;
                    }
                }
                r = new Regex("\\s\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\s");
                foreach (Match m in r.Matches(s))
                {
                    {
                        sb.Append("IP: ");
                        sb.Append(m.ToString());
                        sb.Append("\r\n");
                        Form1.dic[m.ToString()] = true;
                    }
                }
                r = new Regex("https://\\w*(\\.\\w*)*(/\\w*)*\\w*\\.\\w*");
                foreach (Match m in r.Matches(s))
                {
                    {
                        sb.Append("Link: ");
                        sb.Append(m.ToString());
                        sb.Append("\r\n");
                        Form1.dic[m.ToString()] = true;
                    }
                }
                sb.Append("\r\n");
                Form1.serans[num] += sb.ToString();
                Form1.Donefiles = Interlocked.Increment(ref Form1.Donefiles);
            }
            catch (Exception)
            { }
        }
    }
    public class Searcher_Sync : Basic_Searcher, Searcher
    {
        public Searcher_Sync(IView Form):base(Form)
        {
            Main_Form = Form;
        }

        public void Start(string path)
        {
            Folders_Path.Clear();
            int pc = Environment.ProcessorCount - 1;

            for (int o = 0; o < pc; o++)
            {
                Form1.serans[o] = string.Empty;
            }

            int n = 0;
            Form1.processrunning = 0;
            Queue<string>[] QueSer = new Queue<string>[pc];
            for (int q = 0; q < pc; q++)
            {
                QueSer[q] = new Queue<string>();
            }
            Search_and_Save(path, pc, ref n, ref QueSer);
            Parallel.ForEach(Folders_Path, fold => Add_Files(fold, pc, ref n, ref QueSer));
            Main_Form.Progress_bar_work();
            Search_Info[] si = new Search_Info[Form1.processrunning];
            for (int z = 0; z < Form1.processrunning; z++)
            {
                si[z] = new Search_Info(z, QueSer[z]);
            }
            Main_Form.Start_timer_1();
            Thread[] thred = new Thread[Form1.processrunning];
            for (int u = 0; u < Form1.processrunning; u++)
            {
                thred[u] = new Thread(Search_in_Queue);
                thred[u].Start(si[u]);
            }
        }
    }
    public class Searcher_Async : Basic_Searcher, Searcher
    {
        public Searcher_Async(IView Form) : base(Form)
        {
            Main_Form = Form;
        }

        public void Start(string path)
        {
            Folders_Path.Clear();
            int pc = Environment.ProcessorCount - 1;

            for (int o = 0; o < pc; o++)
            {
                Form1.serans[o] = string.Empty;
            }

            int n = 0;
            Form1.processrunning = 0;
            Queue<string>[] QueSer = new Queue<string>[pc];
            for (int q = 0; q < pc; q++)
            {
                QueSer[q] = new Queue<string>();
            }
            Search_and_Save(path, pc, ref n, ref QueSer);
            Parallel.ForEach(Folders_Path, fold => Add_Files(fold, pc, ref n, ref QueSer));
            Main_Form.Progress_bar_work();
            Search_Info[] si = new Search_Info[Form1.processrunning];
            for (int z = 0; z < Form1.processrunning; z++)
            {
                si[z] = new Search_Info(z, QueSer[z]);
            }
            Main_Form.Start_timer_1();
            Sear[] a = new Sear[Form1.processrunning];
            IAsyncResult[] aAR = new IAsyncResult[Form1.processrunning];
            for (int u = 0; u < Form1.processrunning; u++)
            {
                a[u] = new Sear(Search_in_Queue);
                aAR[u] = a[u].BeginInvoke(si[u], null, null);
                a[u].EndInvoke(aAR[u]);
            }
        }
    }
    public class Searcher_Tasks : Basic_Searcher, Searcher
    {
        public Searcher_Tasks(IView Form) : base(Form)
        {
            Main_Form = Form;
        }
        public void Start(string path)
        {
            Folders_Path.Clear();
            int pc = Environment.ProcessorCount - 1;

            for (int o = 0; o < pc; o++)
            {
                Form1.serans[o] = string.Empty;
            }

            int n = 0;
            Form1.processrunning = 0;
            Queue<string>[] QueSer = new Queue<string>[pc];
            for (int q = 0; q < pc; q++)
            {
                QueSer[q] = new Queue<string>();
            }
            Search_and_Save(path, pc, ref n, ref QueSer);
            Parallel.ForEach(Folders_Path, fold => Add_Files(fold, pc, ref n, ref QueSer));
            Main_Form.Progress_bar_work();
            Search_Info[] si = new Search_Info[Form1.processrunning];
            for (int z = 0; z < Form1.processrunning; z++)
            {
                si[z] = new Search_Info(z, QueSer[z]);
            }
            Main_Form.Start_timer_1();
            for (int u = 0; u < Form1.processrunning; u++)
            {
                int y = u;
                Task Search = Task.Run
                     (() =>
                     {
                         Search_in_Queue(si[y]);
                     });

            }
        }

    }
    public class Searcher_TPL : Basic_Searcher, Searcher
    {
        public Searcher_TPL(IView Form) : base(Form)
        {
            Main_Form = Form;
        }
        public void Start(string path)
        {
            Folders_Path.Clear();
            int pc = Environment.ProcessorCount - 1;

            for (int o = 0; o < pc; o++)
            {
                Form1.serans[o] = string.Empty;
            }

            int n = 0;
            Form1.processrunning = 0;
            Queue<string>[] QueSer = new Queue<string>[pc];
            for (int q = 0; q < pc; q++)
            {
                QueSer[q] = new Queue<string>();
            }
            Search_and_Save(path, pc, ref n, ref QueSer);
            Parallel.ForEach(Folders_Path, fold => Add_Files(fold, pc, ref n, ref QueSer));
            Main_Form.Progress_bar_work();
            Search_Info[] si = new Search_Info[Form1.processrunning];
            for (int z = 0; z < Form1.processrunning; z++)
            {
                si[z] = new Search_Info(z, QueSer[z]);
            }
            Main_Form.Start_timer_1();
            Sear[] a = new Sear[Form1.processrunning];
            Parallel.ForEach(si, queue =>
            Search_in_Queue(queue)
            );
        }

    }
    public class Searcher_Await : Basic_Searcher, Searcher
    {
        public Searcher_Await(IView Form) : base(Form)
        {
            Main_Form = Form;
        }

        public void Start(string Path)
        {
            Start_Await_Search(Path);
        }

        public async void Start_Await_Search(string path)
        {
            try
            {
                Folders_Path.Clear();
                int pc = Environment.ProcessorCount - 1;

                for (int o = 0; o < pc; o++)
                {
                    Form1.serans[o] = string.Empty;
                }

                int n = 0;
                Form1.processrunning = 0;
                Queue<string>[] QueSer = new Queue<string>[pc];
                for (int q = 0; q < pc; q++)
                {
                    QueSer[q] = new Queue<string>();
                }
                Search_and_Save(path, pc, ref n, ref QueSer);
                Parallel.ForEach(Folders_Path, fold => Add_Files(fold, pc, ref n, ref QueSer));
                Main_Form.Progress_bar_work();
                Search_Info[] si = new Search_Info[Form1.processrunning];
                for (int z = 0; z < Form1.processrunning; z++)
                {
                    si[z] = new Search_Info(z, QueSer[z]);
                }
                Main_Form.Start_timer_1();
                Thread[] thred = new Thread[Form1.processrunning];
                await Search_Work(si, thred);
                //Canc.ThrowIfCancellationRequested();
            }
            catch (Exception ex)
            {
                //Main_Form.Stop_timer_3();
                MessageBox.Show(ex.Message, "Process was canceled!", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private Task Search_Work(Search_Info[] si, Thread[] thred)
        {
            return Task.Run(() =>
            {
                for (int u = 0; u < Form1.processrunning; u++)
                {
                    thred[u] = new Thread(Search_in_Queue);
                    thred[u].Start(si[u]);
                }
            });
        }

    }
    #endregion
}

namespace MyExtention
{
    
    public static class MyExtentions
    {
        public static Encoding Get_Encoding(this FileInfo f)
        {
            byte[] data = new byte[4];
            using (FileStream fileStream = f.Open(FileMode.Open))
            {
                fileStream.Read(data, 0, 4);
            }

            if (data[0] == 0xef && data[1] == 0xbb && data[2] == 0xbf)
            {
                return Encoding.UTF8;
            }
            if (data[0] == 0x2b && data[1] == 0x2f && data[2] == 0x76)
            {
                return Encoding.UTF7;
            }
            if (data[0] == 0xff && data[1] == 0xfe)
            {
                return Encoding.Unicode;
            }
            if (data[0] == 0xfe && data[1] == 0xff)
            {
                return Encoding.BigEndianUnicode;
            }
            if (data[0] == 0 && data[1] == 0 && data[2] == 0xfe && data[2] == 0xff)
            {
                return Encoding.UTF32;
            }
            return Encoding.ASCII;



        }
        public static string Get_MD5_Hash(this FileInfo f)
        {
            My_File Used_file = new My_File(f.FullName);
            string s = Used_file.ReadAllText();
            byte[] bytes = Encoding.Unicode.GetBytes(s);

            MD5CryptoServiceProvider CSP = new MD5CryptoServiceProvider();

            byte[] bytehash = CSP.ComputeHash(bytes);
            //формируем одну цельную строку из массива  
            string hash = string.Empty;
            foreach (byte b in bytehash)
                hash += string.Format("{0:x2}", b);
            return hash;
        }
        public static string Get_Rights(this FileInfo f)
        {
            string s = string.Empty;
            FileSecurity FS = f.GetAccessControl();
            AuthorizationRuleCollection col = FS.GetAccessRules(true, true, typeof(NTAccount));
            foreach (FileSystemAccessRule r in col)
            {
                s += " IdentityReference: ";
                s += r.IdentityReference;
                s += "\r\n Access control type: ";
                s += r.AccessControlType;
                s += "\r\n Rights: ";
                s += r.FileSystemRights;
                s += "\r\n Inherited: ";
                s += r.IsInherited;
                s += "\r\n\r\n";
            }
            return s;
        }
    }


    
}