using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace File_Manager_System
{
    public abstract class My_Entry
    {
        protected string local_name;
        protected string full_name;

        public string FullName
        {
            get { return full_name; }
        }

        public string Name
        {
            get { return local_name; }
        }

        public abstract string Get_Rights();

        public abstract string Get_Extention();

        public abstract void Rename(string actual_name);

        public abstract void Delete();

        public abstract My_File[] GetFiles();

        public abstract My_Folder[] GetDirectories();

        public abstract My_Folder CreateSubdirectory(string str);

        public abstract void Copy(string To);

        public abstract void Move(string To);
        
        public abstract bool Exists();

        public abstract DateTime GetLastAccessTime();

        public abstract void CreateDirectory(string str);

        public abstract string Name_Without_Ex();

        public abstract void Create();

        public abstract string Get_MD5();

        public abstract Stream OpenStream(FileMode FM, FileAccess FA, FileShare FS);

        public abstract void CopyToStream(Stream input, Stream output);

        public abstract FileStream OpenStream(FileMode FM);

        public abstract Encoding Get_Encoding();

        public abstract FileStream OpenRead();

        public abstract string ReadAllText();

        public abstract string[] ReadAllLines();

        public abstract void WriteAllText(string path, string text);

        public abstract byte[] ReadAllBytes();

        public abstract void WritellBytes(byte[] b);
    }
}