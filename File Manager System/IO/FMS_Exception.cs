using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_Manager_System
{
    public class FMS_Exception:Exception
    {
        public string Exception_Message;

        public FMS_Exception(string s)
        {
            Exception_Message = s;
        }
    }
}
