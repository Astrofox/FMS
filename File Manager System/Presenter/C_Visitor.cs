using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_Manager_System
{
    public interface C_Visitor
    {
        void Visit(My_Entry E);

        bool IsDone { get; }
    }
}
