using File_Manager_System.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace File_Manager_System
{
    static class Factory
    {
        public static My_Entry Create_Entry(string path)
        {
            if (path.Contains(".zip"))
            {
                My_File File = new My_File(path);
                if (File.Get_Extention() == ".zip")
                {
                    My_ZipArchive Zip = new My_ZipArchive(path);
                    return Zip;
                }
                else
                {
                    if (Path.HasExtension(path))
                    {
                        int i = Regex.Match(path, ".zip").Index;
                        return new My_ZipFile(path, new My_ZipArchive(path.Substring(0, i + 4)));
                    }
                    else
                    {
                        int i = Regex.Match(path, ".zip").Index;
                        return new My_ZipFolder(path, new My_ZipArchive(path.Substring(0, i + 4)));
                    }
                }
            }
            else
            {
                My_Folder Folder = new My_Folder(path);
                if (Folder.Exists())
                {
                    return Folder;
                }
                else
                {
                    My_File File = new My_File(path);
                    if (File.Exists())
                    {
                        return File;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
        }

        public static My_Folder Get_Folder(string path)
        {
            My_Folder Folder = new My_Folder(path);
            return Folder;
        }

        public static My_File Get_File(string path)
        {
            My_File File = new My_File(path);
            return File;
        }

        public static My_Folder Get_ZipFolder(string path)
        {
            int i = Regex.Match(path, ".zip").Index;
            return new My_ZipFolder(path, new My_ZipArchive(path.Substring(0, i + 4)));
        }

        public static My_File Get_ZipFile(string path)
        {
            int i = Regex.Match(path, ".zip").Index;
            return new My_ZipFile(path, new My_ZipArchive(path.Substring(0, i + 4)));
        }
    }
}
