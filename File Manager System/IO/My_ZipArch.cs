using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace File_Manager_System.IO
{
    public class My_ZipArchive: My_Folder
    {
        public My_ZipArchive(string path): base (path)
        {
            full_name = path;
            local_name = Path.GetFileName(full_name);
        }

        public override My_File[] GetFiles()
        {
            List<My_ZipFile> zfiles = new List<My_ZipFile>();
            using (ZipFile Arch = new ZipFile(full_name))
            {
                My_File[] files = new My_ZipFile[Arch.Count];
                string[] names = Arch.EntryFileNames.ToArray();

                for (int i = 0; i < names.Length; i++)
                {
                    files[i] = new My_ZipFile(names[i], this);
                    if (Path.GetDirectoryName(files[i].FullName) == "")
                        zfiles.Add(new My_ZipFile(files[i].FullName, this));
                }
                return zfiles.ToArray();
            }
        }

        public  My_File[] GetFileslist()
        {
            List<My_ZipFile> zfiles = new List<My_ZipFile>();
            using (ZipFile Arch = new ZipFile(full_name))
            {
                My_File[] files = new My_ZipFile[Arch.Count];
                string[] names = Arch.EntryFileNames.ToArray();

                for (int i = 0; i < names.Length; i++)
                    files[i] = new My_ZipFile(names[i], this);
                return files;
            }
        }

        public override string Get_Extention()
        {
            return Path.GetExtension(full_name);
        }

        public override My_Folder[] GetDirectories()
        {
            using (ZipFile Arch = new ZipFile(full_name))
            {
                List<string> top_dirs = new List<string>();

                string[] files = Arch.EntryFileNames.ToArray();
                string assist;
                Regex reg = new Regex(@"/");
                int index;

                for (int i = 0; i < files.Length; i++)
                {
                    index = reg.Match(files[i]).Index;
                    assist = files[i].Substring(0, index);
                    if ((!top_dirs.Contains(assist)) && (assist != ""))
                    {
                        top_dirs.Add(assist);
                    }
                }
                My_Folder[] folds = new My_ZipFolder[top_dirs.Count];
                for (int j = 0; j < folds.Length; j++)
                {
                    folds[j] = new My_ZipFolder(top_dirs[j],this);
                }

                return folds;
            }
        }

        public override My_Folder CreateSubdirectory(string str)
        {
            throw new Exception("Not a directory");
        }

        public override string Get_Rights()
        {
            FileInfo f = new FileInfo(full_name);
            string rights = string.Empty;
            FileSecurity FS = f.GetAccessControl();
            AuthorizationRuleCollection col = FS.GetAccessRules(true, true, typeof(NTAccount));
            foreach (FileSystemAccessRule r in col)
            {
                rights += " IdentityReference: ";
                rights += r.IdentityReference;
                rights += "\r\n Access control type: ";
                rights += r.AccessControlType;
                rights += "\r\n Rights: ";
                rights += r.FileSystemRights;
                rights += "\r\n Inherited: ";
                rights += r.IsInherited;
                rights += "\r\n\r\n";
            }
            return rights;
        }

        public override void Copy(string To)
        {
            File.Copy(full_name, To);
        }

        public override void Move(string To)
        {
            File.Move(full_name, To);
        }

        public override void Delete()
        {
            File.Delete(full_name);
        }

        public void Deletefile(string s)
        {
            using (ZipFile Arch = new ZipFile(full_name))
            {
                Arch.RemoveEntry(s);
                Arch.Save();
            }
        }

        public override void Rename(string new_name)
        {
            File.Move(full_name, new_name);
            full_name = new_name;
            local_name = Path.GetFileName(full_name);
        }

        public override bool Exists()
        {
            bool answer = false;

            if (File.Exists(full_name))
                answer = true;

            return answer;
        }

        public override DateTime GetLastAccessTime()
        {
            return File.GetLastAccessTime(full_name);
        }

        public override void CreateDirectory(string str)
        {

        }

        public override byte[] ReadAllBytes()
        {
            throw new Exception("Not a file");
        }

        public override void WritellBytes(byte[] b)
        {
            throw new Exception("Not a file");
        }

        public override FileStream OpenRead()
        {
            throw new Exception("Not a file");
        }

        public override string ReadAllText()
        {
            throw new Exception("Not a file");
        }

        public override string[] ReadAllLines()
        {
            throw new Exception("Not a file");
        }

        public override void WriteAllText(string path, string text)
        {
            throw new Exception("Not a file");
        }

        public override Encoding Get_Encoding()
        {
            throw new Exception("Not a file");
        }

        public override string Name_Without_Ex()
        {
            return Path.GetFileNameWithoutExtension(full_name);
        }

        public override void Create()
        {
            using (ZipFile zip = new ZipFile())
            {
                zip.Save(full_name);
            }
        }

        public override string Get_MD5()
        {

            FileInfo f = new FileInfo(full_name);
            string s = File.ReadAllText(f.FullName);
            byte[] bytes = Encoding.Unicode.GetBytes(s);

            MD5CryptoServiceProvider CSP = new MD5CryptoServiceProvider();

            byte[] bytehash = CSP.ComputeHash(bytes);
            //формируем одну цельную строку из массива  
            string hash = string.Empty;
            foreach (byte b in bytehash)
                hash += string.Format("{0:x2}", b);
            return hash;
        }

        public override Stream OpenStream(FileMode FM, FileAccess FA, FileShare FS)
        {
            throw new Exception("Not a file");
        }

        public override void CopyToStream(Stream input, Stream output)
        {
            throw new Exception("Not a file");
        }

        public override FileStream OpenStream(FileMode FM)
        {
            throw new Exception("Not a file");
        }

        public void Dearchive(string s)
        {
            using (ZipFile Arch = new ZipFile(full_name))
            {
                foreach (ZipEntry e in Arch)
                {
                    if (Path.HasExtension(e.FileName))
                    e.Extract("D:\\Restart\\AF\\VS_Projects\\tmp");
                }
            }
        }

        public void Dearchive()
        {
            using (ZipFile Arch = new ZipFile(full_name))
            {
                foreach (ZipEntry e in Arch)
                {
                    e.Extract(full_name);
                }
            }
        }

        public void Dearchivefile(string s)
        {
            using (ZipFile zip = ZipFile.Read(full_name))
            {
                ZipEntry e = zip[s];
                e.Extract("D:\\Restart\\AF\\VS_Projects\\tmp");
            }
        }

        public void AddFile(My_File F)
        {
            using (Ionic.Zip.ZipFile Arch = new Ionic.Zip.ZipFile())
            {
                Arch.AddFile(F.FullName);
                Arch.Save(full_name);
            }
        }

        public void AddFolder(My_File F)
        {

        }
    }
}
