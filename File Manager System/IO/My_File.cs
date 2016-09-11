using File_Manager_System.IO;
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

namespace File_Manager_System
{
    public class My_File : My_Entry
    {
        public long Length;

        public My_File(string path)
        {
            try
            {
                full_name = path;
                local_name = Path.GetFileName(full_name);
                if (File.Exists(full_name))
                {
                    Length = new FileInfo(full_name).Length;
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

        public My_File()
        {
            full_name = "";
            local_name = "";
            Length = 0;
        }

        public override My_File[] GetFiles()
        {
            throw new My_NotADirectoryException("Not a directory");
        }

        public override My_Folder[] GetDirectories()
        {
            throw new My_NotADirectoryException("Not a directory");
        }

        public override My_Folder CreateSubdirectory(string str)
        {
            throw new My_NotADirectoryException("Not a directory");
        }

        public override void CreateDirectory(string str)
        {
            throw new My_NotADirectoryException("Not a directory");
        }

        public override string Name_Without_Ex()
        {
            return Path.GetFileNameWithoutExtension(full_name);
        }

        public override void Create()
        {
            try
            {
                File.Create(full_name);
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

        public override string Get_MD5()
        {
            try
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
            return Path.GetExtension(full_name);
        }

        public override Stream OpenStream(FileMode FM, FileAccess FA, FileShare FS)
        {
            try
            {
                Stream new_file = null;
                try
                {
                    new_file = new FileStream(full_name, FM, FA, FS);
                }
                catch (Exception) { }

                return new_file;
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

        public override void CopyToStream(Stream input, Stream output)
        {
            const int buffersz = 16384;
            byte[] buffer = new byte[buffersz];
            int btscpd = 0;

            do
            {
                btscpd = input.Read(buffer, 0, buffersz);
                if (btscpd > 0)
                {
                    output.Write(buffer, 0, buffersz);
                }
            }
            while (btscpd > 0);
        }

        public override FileStream OpenStream(FileMode FM)
        {
            FileStream new_file = null;
            try
            {
                new_file = new FileStream(full_name, FM);
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
            return new_file;
        }    

        public override string Get_Rights()
        {
            try
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

        public override Encoding Get_Encoding()
        {
            try
            {
                byte[] data = new byte[4];
                using (FileStream fileStream = this.OpenStream(FileMode.Open))
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

                if (File.Exists(full_name))
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
                return File.GetLastAccessTime(full_name);
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

        public override FileStream OpenRead()
        {
            try
            {
                return File.OpenRead(full_name);
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

        public override string ReadAllText()
        {try
            {
                return File.ReadAllText(full_name);
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

        public override string[] ReadAllLines()
        {
            try
            {
                return File.ReadAllLines(full_name);
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

        public override void WriteAllText(string path, string text)
        {
            try
            {
                File.WriteAllText(path, text);
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

        public override void Copy(string To)
        {
            try
            {
                if (!To.Contains(".zip"))
                {
                    File.Copy(full_name, To);
                }
                else
                {
                    int i = Regex.Match(To, ".zip").Index;
                    string ass = To.Substring(0, i + 3);
                    string ass2 = To.Substring(i + 4, To.Length);
                    using (ZipFile zip = new ZipFile())
                    {
                        zip.AddFile(full_name,ass2);
                    }
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
                File.Move(full_name, To);
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
        {try
            {
                File.Delete(full_name);
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

        public override void Rename(string actual_name)
        {
            try
            {
                File.Move(full_name, actual_name);
                full_name = actual_name;
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

        public override byte[] ReadAllBytes()
        {
            try
            {
                return File.ReadAllBytes(full_name);
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

        public override void WritellBytes(byte[] b)
        {
            try
            {
                File.WriteAllBytes(full_name, b);
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
    }
}