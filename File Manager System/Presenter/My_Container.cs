using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_Manager_System
{
    public class My_Container: Container
    {
        List<My_Entry> Entries;
        public My_Container(List<My_Entry> input)
        {
            Entries = input;
            List_Visitor CLV = new List_Visitor();
            Accept(CLV);
            Entries = CLV.list;
        }
        public int Count
        {
            get { return Entries.Count; }            
        }
        public bool IsEmpty
        {
            get
                {
                if (Entries.Count != 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public void Purge()
        {
            Entries.Clear();
        }
        public void Accept(C_Visitor visitor)
        {
            foreach (My_Entry E in Entries)
            {
                visitor.Visit(E);
            }
        }
    }
}
