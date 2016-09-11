using File_Manager_System.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace File_Manager_System
{
    public class Model_Presenter
    {
        #region Variables
        IView Form_view;
        string path;
        string tmp = "D:\\Restart\\AF\\VS_Projects\\tmp";
        #endregion

        #region Presenter

        public Model_Presenter(IView view)
        {
            Form_view = view;
            Form_view.My_Move += new EventHandler<string>(On_Move);
            Form_view.My_Delete += new EventHandler<string>(On_Delete);
            Form_view.My_Archive_sync += new EventHandler<string>(On_Archive_sync);
            Form_view.My_Archive_async += new EventHandler<string>(On_Archive_async);
            Form_view.My_Archive_tpl += new EventHandler<string>(On_Archive_tpl);
            Form_view.My_Archive_task += new EventHandler<string>(On_Archive_task);
            Form_view.My_Archive_await += new EventHandler<string>(On_Archive_await);
            Form_view.My_Dearchive += new EventHandler<string[]>(On_Dearchive);
            Form_view.My_Search_sync += new EventHandler<string>(On_Search_sync);
            Form_view.My_Search_async += new EventHandler<string>(On_Search_async);
            Form_view.My_Search_tpl += new EventHandler<string>(On_Search_tpl);
            Form_view.My_Search_task += new EventHandler<string>(On_Search_task);
            Form_view.My_Search_await += new EventHandler<string>(On_Search_await);
            Form_view.My_Translate += new EventHandler<string[]>(On_Translate);
            Form_view.My_Copy += new EventHandler<string[]>(On_Copy);
            Form_view.My_Rename += new EventHandler<string[]>(On_Rename);
        }
        private void On_Move(object Sender, string e)
        {
            if (!Path.HasExtension(e))
            {
                path = e;
                if (!path.Contains(".zip"))
                    Form_view.Refresh(new My_Folder(path));
                else
                {
                    int i = Regex.Match(path,".zip").Index;
                    Form_view.Arch_Refresh(new My_ZipFolder(path, new My_ZipArchive(path.Substring(0,i+4))));
                }
            }
            else
            {
                if (Path.GetExtension(e)==".zip")
                {
                    Form_view.Arch_Refresh(new My_ZipArchive(e));
                }
                else
                {
                    Process.Start(e);
                }
            }
        }
        private void On_Delete(object Sender, string e)
        {
            Delete(e);
        }
        private void On_Archive_sync(object Sender, string e)
        {
            Start_Archive(e);
        }
        private void On_Archive_async(object Sender, string e)
        {
            Start_Archive_Async(e);
        }
        private void On_Archive_tpl(object Sender, string e)
        {
            Start_Archive_TPL(e);
        }
        private void On_Archive_task(object Sender, string e)
        {
            Start_Archive_Tasks(e);
        }
        private void On_Archive_await(object Sender, string e)
        {
            Start_Archive_Await(e);
        }
        private void On_Search_sync(object Sender, string e)
        {
            if (!e.Contains(".zip"))
                Start_Search(e);
            else
            {
                if (!(Path.GetExtension(e) == ".zip"))
                {
                    if (Path.HasExtension(e))
                    {
                        int i = Regex.Match(e, ".zip").Index;
                        My_ZipArchive zip2 = new My_ZipArchive(e.Substring(0, i + 4));
                        zip2.Dearchivefile(e.Substring(i + 5));
                        Start_Search(tmp +@"\"+ e.Substring(i + 5));
                    }
                }
                else
                {
                    My_ZipArchive zip = new My_ZipArchive(e);
                    zip.Dearchive(tmp);
                    Start_Search("D:\\Restart\\AF\\VS_Projects\\tmp");
                }
            }
        }
        private void On_Search_async(object Sender, string e)
        {
            Start_Async_Search(e);
        }
        private void On_Search_tpl(object Sender, string e)
        {
            Start_TPL_Search(e);
        }
        private void On_Search_task(object Sender, string e)
        {
            Start_Tasks_Search(e);
        }
        private void On_Search_await(object Sender, string e)
        {
            Start_Await_Search(e);
        }
        private void On_Copy(object Sender, string[] e)
        {
            string actname = e[0];

            string check = Path.Combine(e[1], Path.GetFileName(actname));
            My_File Used_file = Factory.Get_File(actname);
            if (Used_file.Exists())
            {
                    My_File Used_file1 = Factory.Get_File(check);
                    if (!Used_file1.Exists())
                    {
                        Used_file.Copy(check);
                    }
                
            }
            else
            {
                My_Folder Used_fold = Factory.Get_Folder(actname);
                My_Folder Used_fold1 = Factory.Get_Folder(check);
                if (!Used_fold1.Exists())
                {
                    Used_fold.CreateDirectory(check);
                    Used_fold.Copy(check);
                    //My_Folder di1 = Factory.Get_Folder(actname);
                    //My_Folder di2 = Factory.Get_Folder(Path.Combine(path, Path.GetFileName(actname)));
                    //CopyFolder(di1, di2);
                }
            }
        }
        private void On_Translate(object Sender, string[] e)
        {

            string actname = e[0];

            string check = Path.Combine(e[1], Path.GetFileName(actname));

            if (Path.HasExtension(actname))
            {
                My_File Used_fold = Factory.Get_File(check);
                if (!Used_fold.Exists())
                {
                    Translate(actname, check);
                }
            }
            else
            {
                My_Folder Used_fold = Factory.Get_Folder(check);
                if (!Used_fold.Exists())
                {
                    Translate(actname, check);
                }
            }

        }
        private void On_Rename(object Sender, string[] e)
        {
            Rename(e[0], e[1]);
        }
        private void On_Dearchive(object Sender, string[] e)
        {
            DeArchive(e[0], e[1]);
        }

        #endregion

        #region Model

        #region Manage files/folders
        public static void Rename(string Prev, string now)
        {
                My_Entry Used_file = Factory.Create_Entry(Prev);
                Used_file.Move(now);
        }

        public static void Delete(string path)
        {
            if (!path.Contains(".zip"))
            {
                {
                    if (Path.HasExtension(path))
                    {
                        My_File Used_file = Factory.Get_File(path);
                        Used_file.Delete();
                    }
                    else
                    {
                        My_Folder Used_fold = Factory.Get_Folder(path);
                        My_File[] dirs = Used_fold.GetFiles();
                        foreach (My_File dir in dirs)
                        {
                            dir.Delete();
                        }
                        My_Folder[] folds = Used_fold.GetDirectories();
                        foreach (My_Folder fold in folds)
                        {
                            fold.Delete();
                        }
                        Used_fold.Delete();
                    }
                }
            }
            else
            {
                if (Path.GetExtension(path)==".zip")
                {
                    My_File Used_file = Factory.Get_File(path);
                    Used_file.Delete();
                }
                int i = Regex.Match(path, ".zip").Index;
                My_ZipArchive zip2 = new My_ZipArchive(path.Substring(0, i + 4));
                zip2.Deletefile(path.Substring(i + 5));
            }
        }

        public static void CopyFile(string PathFrom, string PathTo)
        {
            string p = Path.Combine(PathTo, Path.GetFileName(PathFrom));
            My_Entry Used_file = Factory.Create_Entry(PathFrom);
            if (Used_file.Exists())
            {
                using (Stream inFile = Factory.Get_File(PathFrom).OpenStream(FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream outFile = Factory.Get_File(PathFrom).OpenStream(FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    Used_file.CopyToStream(inFile, outFile);
                }
            }
            else
            {
                throw new Exception("File do not exist");
            }
        }

        public static void CopyFolder(My_Folder source, My_Folder target)
        {
            My_Entry Used_file;
            foreach (My_Folder dir in source.GetDirectories())
                CopyFolder(dir, target.CreateSubdirectory(dir.Name));
            foreach (My_File file in source.GetFiles())
            {
                Used_file = Factory.Create_Entry(Path.Combine(target.FullName, file.Name));
                if (!Used_file.Exists())
                    file.Copy(Path.Combine(target.FullName, file.Name));
            }
        }

        public static void Translate(string PathFrom, string PathTo)
        {
            if (Path.HasExtension(PathFrom))
            {
                My_Entry Used_file = Factory.Create_Entry(PathFrom);
                Used_file.Move(PathTo);
            }
            else
            {
                My_Folder Used_file = new My_Folder(PathFrom);
                Used_file.Move(PathTo);
            }
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

        #endregion

        #region Archivation

        #region Archivation Sync
        public void Start_Archive(string path)
        {
            Archivator Arch_Sync = new Archivator_Sync();
            Arch_Sync.Start(path);
        }

        #endregion

        #region Archivation Async
        public void Start_Archive_Async(string path)
        {
            Archivator Arch_Async = new Archivator_Async();
            Arch_Async.Start(path);
        }
        #endregion

        #region Archivation TPL
        public void Start_Archive_TPL(string path)
        {

            Archivator Arch_TPL = new Archivator_TPL();
            Arch_TPL.Start(path);

        }
        #endregion

        #region Archivation Tasks
        public void Start_Archive_Tasks(string path)
        {

            Archivator Arch_TPL = new Archivator_Tasks();
            Arch_TPL.Start(path);

        }
        #endregion

        #region Archivation Await
        public void Start_Archive_Await(string path)
        {

            Archivator Arch_Await = new Archivator_Await();
            Arch_Await.Start(path);

        }

        #endregion
        #endregion

        #region Search

        #region Search Sync
        public void Start_Search(string path)
        {
            Searcher Srch_Sync = new Searcher_Sync(Form_view);
            Srch_Sync.Start(path);
        }

        #endregion

        #region Search Async

        public void Start_Async_Search(string path)
        {
            Searcher Srch_Async = new Searcher_Async(Form_view);
            Srch_Async.Start(path);
        }

        #endregion

        #region Search TPL
        public void Start_TPL_Search(string path)
        {
            Searcher Srch_TPL = new Searcher_TPL(Form_view);
            Srch_TPL.Start(path);
        }
        #endregion

        #region Search Tasks
        public void Start_Tasks_Search(string path)
        {
            Searcher Srch_Tsk = new Searcher_Tasks(Form_view);
            Srch_Tsk.Start(path);
        }
        #endregion

        #region Search Await
        public void Start_Await_Search(string path)
        {
            Searcher Srch_Awt = new Searcher_Await(Form_view);
            Srch_Awt.Start(path);
        }

        #endregion

        #region Search

        public static string Search(string Paths)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                My_File For_search = new My_File(Paths);
                string ans = string.Empty;
                string s;
                s = For_search.ReadAllText();
                Regex r = new Regex("7\\d{10}");
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
                
            }
            catch (Exception)
            { }

            string str = sb.ToString();
            return str;
        }
        #endregion
        #endregion

        #region Dearchivation
        public static void DeArchive(string PathFrom, string PathTo)
        {
            if (Path.HasExtension(PathFrom))
            {
                using (Stream inFile = Factory.Get_File(PathFrom).OpenStream(FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream outFile = Factory.Get_File(PathTo).OpenStream(FileMode.Create, FileAccess.Write, FileShare.None))
                using (Stream zipfile = new GZipStream(inFile, CompressionMode.Decompress))
                {
                    My_Entry Used_file = Factory.Get_File("");
                    Used_file.CopyToStream(zipfile, outFile);
                }
            }
            else
            {

            }
        }
        #endregion

        #endregion
    }
}
