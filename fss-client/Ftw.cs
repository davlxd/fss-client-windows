using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace fss_client
{
    // this function implement a simple nftw()(POSIX) 
    class Ftw
    {
        Fn the_fn;
        // Fn fn is delegate
        public Ftw(Fn fn, string rootpath)
        {
            the_fn = fn;
            this.traverse(rootpath);            
        }

        private void traverse(string filename)
        {
            FileAttributes attr = File.GetAttributes(filename);

            the_fn(filename);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                DirectoryInfo di = new DirectoryInfo(filename);

                FileInfo[] fis = di.GetFiles();
                foreach (FileInfo finfo in fis)
                    traverse(finfo.FullName);

                DirectoryInfo[] dis = di.GetDirectories();
                foreach (DirectoryInfo dinfo in dis)
                    traverse(dinfo.FullName);

            }
            else
                return;

        } // end traverse()

    }
}
