using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace fss_client
{
    class TimeSize
    {
        public static long entry_mtime(string fullpath)
        {

            FileInfo fi = new FileInfo(fullpath);
            return UNIX_ticks(fi.LastWriteTime);
        }

        public static long entry_size(string fullpath)
        {
            if (Directory.Exists(fullpath))
                return 2046;

            FileInfo fi = new FileInfo(fullpath);
            return fi.Length;
        }

        public static long UNIX_ticks(DateTime dt)
        {
            DateTime dt1 = dt.ToUniversalTime();
            TimeSpan ts = new TimeSpan(dt1.Ticks - (new DateTime(1970, 1, 1, 0, 0, 0)).Ticks);
            return (long)ts.TotalSeconds;
        }
    }
}
