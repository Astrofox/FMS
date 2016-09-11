using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_Manager_System
{
    class List_Visitor:C_Visitor
    {
        public List<My_Entry> list = new List<My_Entry>();
        public void Visit(My_Entry E)
        {
            if (E is My_File)
            {
                list.Add(E as My_File);
            }
            else
            {
                My_Folder F = E as My_Folder;

                My_Folder[] LF= F.GetDirectories();
                foreach (My_Folder mf in LF)
                {
                    Visit(mf);
                }

                My_File[] KF = F.GetFiles();
                foreach (My_File mf in KF)
                {
                    Visit(mf);
                }
            }
        }

        public bool IsDone
        {
            get { return false; }
        }
    }
}
