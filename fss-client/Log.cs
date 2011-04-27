using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace fss_client
{
    class Log
    {
        public static void logon(string msg)
        {
            lock (typeof(Log))
            {
                File.AppendAllText("fss.log", DateTime.Now.ToString());
                File.AppendAllText("fss.log", "   ");
                File.AppendAllText("fss.log", msg);
                File.AppendAllText("fss.log", "\r\n");
            }
        }

    }
}
