using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace fss_client
{
    class Files
    {
        private string global_path;


        public Files(string path)
        {
            this.global_path = path;
            create_Files_dir(path);
        }

        private void create_Files_dir(string path)
        {
            if (!Directory.Exists(Path.Combine(path, ".fss")))
            {
                DirectoryInfo di = Directory.CreateDirectory(Path.Combine(path, ".fss"));
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
        }
    }
}
