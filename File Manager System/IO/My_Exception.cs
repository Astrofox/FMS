using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_Manager_System.IO
{
    public class My_Exception : Exception
    {
        string My_message;

        public string My_Message
        {
            get { return My_message; }
        }

        public My_Exception(string s)
        {
            My_message = s;
        }
    }
    public class My_UnauthorizedAccessException : UnauthorizedAccessException
    {
        string My_message;

        public string My_Message
        {
            get { return My_message; }
        }

        public My_UnauthorizedAccessException(string s)
        {
            My_message = s;
        }
    }
    public class My_IOException: IOException
    {

        string My_message;

        public string My_Message
        {
            get { return My_message; }
        }

        public My_IOException(string s)
        {
            My_message = s;
        }
    }
    public class My_NotADirectoryException : Exception
    {

        string My_message;

        public string My_Message
        {
            get { return My_message; }
        }

        public My_NotADirectoryException(string s)
        {
            My_message = s;
        }
    }
    public class My_NotAFileException : Exception
    {

        string My_message;

        public string My_Message
        {
            get { return My_message; }
        }

        public My_NotAFileException(string s)
        {
            My_message = s;
        }
    }
}
