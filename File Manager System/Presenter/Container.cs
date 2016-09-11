using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_Manager_System
{
    public interface Container
    {
        int Count { get; }
        bool IsEmpty { get; }
        void Purge();
        void Accept(C_Visitor visitor);

    }
}
