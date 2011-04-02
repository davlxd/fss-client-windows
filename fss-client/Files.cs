using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace fss_client
{
    //delegate write_in() which do actual stuff
    delegate void Fn(string fullname);

    class Files
    {
        private string global_root_path;
        private string fss_dir_path;
        private string finfo_fss_path;
        private string sha1_fss_path;
        private string temp_sha1_fss_path;

        private Sha1 sha1;

        private Ftw ftw;

        public Files(string path)
        {
            sha1 = new Sha1();

            this.global_root_path = path;
            this.fss_dir_path = Path.Combine(path, ".fss");
            this.finfo_fss_path = Path.Combine(this.fss_dir_path, "finfo.fss");
            this.sha1_fss_path = Path.Combine(this.fss_dir_path, "sha1.fss");
            this.temp_sha1_fss_path = Path.Combine(this.fss_dir_path, "temp.sha1.fss");
            this.update_files();

        }

        public bool if_to_skip(string fullpath)
        {
            if (!File.Exists(fullpath) && !Directory.Exists(fullpath))
                return true;
            FileAttributes attr = File.GetAttributes(fullpath);
            if ((attr & FileAttributes.Hidden) == FileAttributes.Hidden)
                return true;

            if (string.Compare(fullpath, 0, this.fss_dir_path, 0, this.fss_dir_path.Length) == 0)
                return true;

            return false;
        }
        public void update_files()
        {
            if (!Directory.Exists(this.fss_dir_path))
            {
                DirectoryInfo di = Directory.CreateDirectory(Path.Combine(this.global_root_path, ".fss"));
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            if (File.Exists(this.finfo_fss_path))
                File.Delete(this.finfo_fss_path);

            if (File.Exists(this.temp_sha1_fss_path))
                File.Delete(this.temp_sha1_fss_path);
                

            Fn fn = new Fn(this.write_in); // Instantiate delagate, register function
            ftw = new Ftw(fn, this.global_root_path);  // Instaiate traverse functio

            if (!File.Exists(this.sha1_fss_path))
                File.Move(this.temp_sha1_fss_path, this.sha1_fss_path);
            else
            {
                if (sha1.sha1_file_via_fname(this.sha1_fss_path) ==
                    sha1.sha1_file_via_fname(this.temp_sha1_fss_path))
                    File.Delete(this.temp_sha1_fss_path);
                else
                {
                    File.Delete(this.sha1_fss_path);
                    File.Move(this.temp_sha1_fss_path, this.sha1_fss_path);
                }
            }
            
        }

        public void write_in(string fullpath)
        {
            if (if_to_skip(fullpath))
                return;

            File.AppendAllText(this.finfo_fss_path, fullpath);
            File.AppendAllText(this.finfo_fss_path, "\n");


            File.AppendAllText(this.temp_sha1_fss_path, sha1.sha1_file_via_fname_fss(global_root_path, fullpath));
            File.AppendAllText(this.temp_sha1_fss_path, "\n");

        }
    }
}
