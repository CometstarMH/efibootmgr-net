using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfiBootMgr
{
    class ErrorHandling
    {
        public static int Verbosity { get; set; } = 0;

        public static void PrintWarning(string msg, int win32err = 0)
        {
            string x = $"{(win32err != 0 ? $": error {win32err}" : "")}";
            Console.WriteLine($"{msg}{x}");
        }

        public static void PrintWarning(int verbosity, string msg, int win32err = 0)
        {
            if (Verbosity >= verbosity)
            {
                PrintWarning(msg, win32err);
            }
        }

        public static void HandleWarning(Action func, string msg, int verbosity)
        {
            try
            {
                func();
            }
            catch (Exception ex)
            {
                if (ex is Win32Exception)
                {
                    PrintWarning(verbosity, msg, ((Win32Exception)ex).NativeErrorCode);
                }
                else
                {
                    PrintWarning(verbosity, msg); 
                }

            }
        }
    }
}
