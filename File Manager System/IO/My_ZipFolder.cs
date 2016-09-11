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
using System.Security.Cryptography;
using File_Manager_System.IO;
using System.Text.RegularExpressions;

namespace File_Manager_System
{
    public class My_ZipFolder: My_Folder
    {
        My_ZipArchive Archive;
        public My_ZipFolder(string path, My_ZipArchive Arch) :base(path)
        {
            full_name = path;
            local_name = Path.GetFileName(full_name);
            Archive = Arch;
        }

        public override My_File[] GetFiles()
        {
            List<My_ZipFile> zfiles = new List<My_ZipFile>();
            try
            {
                My_File[] files = Archive.GetFileslist();
                for (int i = 0; i < files.Length; i++)
                {
                    if ((Path.HasExtension(files[i].FullName)) && (files[i].FullName.Contains(Name))&&(Path.GetFileName(Path.GetDirectoryName(files[i].FullName))==local_name))
                    {
                        zfiles.Add(new My_ZipFile(files[i].FullName, Archive));
                    }
                }
                return zfiles.ToArray();

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
            return "";
        }

        public override My_Folder[] GetDirectories()
        {
            using (ZipFile Arch = new ZipFile(Archive.FullName))
            {
                List<string> top_dirs = new List<string>();

                string[] files = Arch.EntryFileNames.ToArray();
                string assist;
                Regex reg = new Regex(@"/");
                int index;

                for (int i = 0; i < files.Length; i++)
                {
                    if (Regex.IsMatch(files[i], local_name))
                    {
                        index = Regex.Match(files[i], local_name).Index;
                        files[i] = files[i].Substring(index + local_name.Length + 1, files[i].Length - (index + local_name.Length + 1));
                        index = reg.Match(files[i]).Index;
                        assist = files[i].Substring(0, index);
                        if ((!top_dirs.Contains(assist)) && (assist != ""))
                        {
                            top_dirs.Add(assist);
                        }
                    }

                }
                My_Folder[] folds = new My_ZipFolder[top_dirs.Count];
                for (int j = 0; j < folds.Length; j++)
                {
                    folds[j] = new My_ZipFolder(top_dirs[j], Archive);
                }

                return folds;
            }
        }

        public override My_Folder CreateSubdirectory(string str)
        {
            throw new My_NotADirectoryException("Not a simple directory");
        }

        public override string Get_Rights()
        {
            return Archive.Get_Rights();
        }

        public override void Copy(string To)
        {
            throw new My_NotADirectoryException("Not a simple directory"); ;
        }

        public override void Move(string To)
        {
            throw new My_NotADirectoryException("Not a simple directory");
        }

        public override void Delete()
        {
            try
            {
                My_File[] files = Archive.GetFiles();
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].FullName.Contains(FullName))
                    {
                        files[i].Delete();
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

        public override void Rename(string new_name)
        {
            throw new My_NotADirectoryException("Not a simple directory");
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
            throw new My_NotADirectoryException("Not a simple folder");
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
            return Path.GetFileNameWithoutExtension(full_name);
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