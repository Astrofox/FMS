using File_Manager_System.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace File_Manager_System
{
    public class My_Folder: My_Entry
    {
        public My_Folder(string path)
        {
            full_name = path;
            local_name = Path.GetFileName(full_name);
        }

        public override My_File[] GetFiles()
        {
            try
            {
                My_File[] files = new My_File[Directory.GetFiles(full_name).Length];
                string[] names = Directory.GetFiles(full_name);

                for (int i = 0; i < names.Length; i++)
                    files[i] = new My_File(names[i]);
                return files;
            }

            catch (UnauthorizedAccessException ex)
            {
                throw new My_UnauthorizedAccessException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new My_IOException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new My_Exception(ex.Message);
            }
        }

        public override My_Folder[] GetDirectories()
        {
            try
            {

                My_Folder[] folders = new My_Folder[Directory.GetDirectories(full_name).Length];
                string[] names = Directory.GetDirectories(full_name);

                for (int i = 0; i < names.Length; i++)
                    folders[i] = new My_Folder(names[i]);
                return folders;
            }

            catch (UnauthorizedAccessException ex)
            {
                throw new My_UnauthorizedAccessException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new My_IOException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new My_Exception(ex.Message);
            }
        }

        public override My_Folder CreateSubdirectory(string str)
        {
            try
            {
                DirectoryInfo DI = new DirectoryInfo(full_name);
                DI.CreateSubdirectory(str);
                return (new My_Folder(Path.Combine(full_name, str)));
            }

            catch (UnauthorizedAccessException ex)
            {
                throw new My_UnauthorizedAccessException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new My_IOException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new My_Exception(ex.Message);
            }
        }

        public void Clear()
        {
            string[] names = Directory.GetDirectories(full_name);

            foreach(string name in names)
            {
                My_Folder fold = new My_Folder(name);
                fold.Clear();
                Directory.Delete(name);
            }

            names = Directory.GetFiles(full_name);

            foreach (string name in names)
            {
                File.Delete(name);
            }
        }

        public override string Get_Rights()
        {
            try
            {

                DirectoryInfo di = new DirectoryInfo(full_name);
                string rights = string.Empty;
                DirectorySecurity DS = di.GetAccessControl();
                AuthorizationRuleCollection col = DS.GetAccessRules(true, true, typeof(NTAccount));
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

            catch (UnauthorizedAccessException ex)
            {
                throw new My_UnauthorizedAccessException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new My_IOException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new My_Exception(ex.Message);
            }
        }

        public override string Get_Extention()
        {
            throw new My_NotAFileException("Not a file");
        }

        public override void Copy(string To)
        {
            try
            {
                My_File Used_file;
                foreach (My_Folder dir in GetDirectories())
                {
                    dir.CreateSubdirectory(Path.Combine(To, dir.Name));
                    dir.Copy(Path.Combine(To, dir.Name));
                }
                foreach (My_File file in GetFiles())
                {
                    Used_file = new My_File(Path.Combine(To, file.Name));
                    if (!Used_file.Exists())
                        file.Copy(Path.Combine(To, file.Name));
                }
            }

            catch (UnauthorizedAccessException ex)
            {
                throw new My_UnauthorizedAccessException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new My_IOException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new My_Exception(ex.Message);
            }
        }

        public override void Move(string To)
        {
            try
            {
                Directory.Move(full_name, To);
            }

            catch (UnauthorizedAccessException ex)
            {
                throw new My_UnauthorizedAccessException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new My_IOException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new My_Exception(ex.Message);
            }
        }

        public override void Delete()
        {
            try
            {
                Directory.Delete(full_name);
            }

            catch (UnauthorizedAccessException ex)
            {
                throw new My_UnauthorizedAccessException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new My_IOException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new My_Exception(ex.Message);
            }
        }
        
        public override void Rename(string new_name)
        {
            try
            {
                Directory.Move(full_name, new_name);
                full_name = new_name;
                local_name = Path.GetFileName(full_name);
            }

            catch (UnauthorizedAccessException ex)
            {
                throw new My_UnauthorizedAccessException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new My_IOException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new My_Exception(ex.Message);
            }
        }

        public override bool Exists()
        {
            try
            {
                bool answer = false;

                if (Directory.Exists(full_name))
                    answer = true;

                return answer;
            }

            catch (UnauthorizedAccessException ex)
            {
                throw new My_UnauthorizedAccessException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new My_IOException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new My_Exception(ex.Message);
            }
        }

        public override DateTime GetLastAccessTime()
        {
            try
            {
                return Directory.GetLastAccessTime(full_name);
            }

            catch (UnauthorizedAccessException ex)
            {
                throw new My_UnauthorizedAccessException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new My_IOException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new My_Exception(ex.Message);
            }
        }

        public override void CreateDirectory(string str)
        {
            try
            {
                Directory.CreateDirectory(str);
            }

            catch (UnauthorizedAccessException ex)
            {
                throw new My_UnauthorizedAccessException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new My_IOException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new My_Exception(ex.Message);
            }
        }

        public override byte[] ReadAllBytes()
        {
                throw new My_NotAFileException("Not a file");
        }

        public override void WritellBytes(byte[] b)
        {
            throw new My_NotAFileException("Not a file");
        }

        public override FileStream OpenRead()
        {
            throw new My_NotAFileException("Not a file");
        }

        public override string ReadAllText()
        {
            throw new My_NotAFileException("Not a file");
        }

        public override string[] ReadAllLines()
        {
            throw new My_NotAFileException("Not a file");
        }

        public override void WriteAllText(string path, string text)
        {
            throw new My_NotAFileException("Not a file");
        }

        public override Encoding Get_Encoding()
        {
            throw new My_NotAFileException("Not a file");
        }

        public override string Name_Without_Ex()
        {
            throw new My_NotAFileException("Not a file");
        }

        public override void Create()
        {
            throw new My_NotAFileException("Not a file");
        }

        public override string Get_MD5()
        {
            throw new My_NotAFileException("Not a file");
        }

        public override Stream OpenStream(FileMode FM, FileAccess FA, FileShare FS)
        {
            throw new My_NotAFileException("Not a file");
        }

        public override void CopyToStream(Stream input, Stream output)
        {
            throw new My_NotAFileException("Not a file");
        }

        public override FileStream OpenStream(FileMode FM)
        {
            throw new My_NotAFileException("Not a file");
        }
    }
}