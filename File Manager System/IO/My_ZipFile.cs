using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using Ionic.Zlib;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using File_Manager_System.IO;

namespace File_Manager_System
{
    class My_ZipFile : My_File
    {
        My_ZipArchive Archive;
        public My_ZipFile(string path, My_ZipArchive Arch)
        {
            full_name = path;
            local_name = Path.GetFileName(full_name);
            Archive = Arch;
        }

        public override My_File[] GetFiles()
        {
            throw new My_NotADirectoryException("Not a directory");
        }
        
        public override string Get_Extention()
        {
            return Path.GetExtension(full_name);
        }

        public override My_Folder[] GetDirectories()
        {
            throw new My_NotADirectoryException("Not a directory");
        }

        public override My_Folder CreateSubdirectory(string str)
        {
            throw new My_NotADirectoryException("Not a directory");
        }

        public override string Get_Rights()
        {
            return Archive.Get_Rights();
        }

        public override void Copy(string To)
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

        public override void Move(string To)
        {
            Directory.Move(full_name, To);
        }

        public override void Delete()
        {
            Directory.Delete(full_name);
        }

        public override void Rename(string new_name)
        {
            try
            {
                using (ZipFile zip1 = new ZipFile())
                {
                    zip1.AddFile(full_name).FileName = new_name;
                    zip1.Save(Archive.FullName);
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

        public override bool Exists()
        {
            try
            {
                My_File[] files = Archive.GetFiles();
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].FullName.Contains(FullName))
                    {
                        return true;
                    }
                }
                return false;
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
            return Archive.GetLastAccessTime();
        }

        public override void CreateDirectory(string str)
        {
            throw new My_NotADirectoryException("Not a directory");
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