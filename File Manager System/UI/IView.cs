using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_Manager_System
{
    public interface IView
    {
        void Refresh(My_Folder F);
        void Arch_Refresh(My_Folder F);
        void Start_timer_1();
        void Progress_bar_work();
        void add_to_serans(string s, int i);
        event EventHandler<string[]> My_Rename;
        event EventHandler<string[]> My_Copy;
        event EventHandler<string[]> My_Translate;
        event EventHandler<string> My_Move;
        event EventHandler<string> My_Delete;
        event EventHandler<string> My_Archive_sync;
        event EventHandler<string> My_Archive_async;
        event EventHandler<string> My_Archive_await;
        event EventHandler<string> My_Archive_task;
        event EventHandler<string> My_Archive_tpl;
        event EventHandler<string[]> My_Dearchive;
        event EventHandler<string> My_Search_sync;
        event EventHandler<string> My_Search_async;
        event EventHandler<string> My_Search_await;
        event EventHandler<string> My_Search_task;
        event EventHandler<string> My_Search_tpl;
    }
}
