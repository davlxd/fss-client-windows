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

        public static int PREFIX0_SENT = 0;
        public static int PREFIX1_SENT = 1;
        public static int PREFIX2_SENT = 2;
        public static int PREFIX3_SENT = 3;


        private string global_root_path;
        private string fss_dir_path;
        private string finfo_fss_path;
        private string sha1_fss_path;
        private string temp_sha1_fss_path;
        private string remote_sha1_fss_path;
        private string diff_local_index_path;
        private string diff_remote_index_path;
        private string del_index_fss_path;

        private FileStream diff_remote_index;
        private FileStream diff_local_index;
        private int linenum_to_send;
        private long size_to_send;

        private fss_client.Net net;

        private Ftw ftw;

        public Files(string path, fss_client.Net net)
        {
            this.net = net;
            if(path[path.Length-1] == '\\')
                this.global_root_path =  path.Substring(0, path.Length-1);
            else
                this.global_root_path = path;

            this.fss_dir_path = Path.Combine(path, ".fss");
            this.finfo_fss_path = Path.Combine(this.fss_dir_path, "finfo.fss");
            this.sha1_fss_path = Path.Combine(this.fss_dir_path, "sha1.fss");
            this.remote_sha1_fss_path = Path.Combine(this.fss_dir_path, "remote.sha1.fss");
            this.temp_sha1_fss_path = Path.Combine(this.fss_dir_path, "temp.sha1.fss");
            this.diff_local_index_path = Path.Combine(this.fss_dir_path, "diff.local.index");
            this.diff_remote_index_path = Path.Combine(this.fss_dir_path, "diff.remote.index");
            this.del_index_fss_path = Path.Combine(this.fss_dir_path, "del.index.fss");

        }

        public void receive_sha1_fss( long size)
        {
            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In recieve_sha1_fss of Files.cs");
            this.receive_file(this.remote_sha1_fss_path, size);
        }
        public void receive_common_file(string relapath, long size)
        {
            Log.logon("In reciev_common_file(), relapth --" + relapath + "--");
            string fullpath = Path.Combine(this.global_root_path, relapath);

            if (!Directory.Exists(Path.GetDirectoryName(fullpath)))
                Directory.CreateDirectory(Path.GetDirectoryName(fullpath));

            net.receive_file(fullpath, size);
        }
        public void receive_file(string path, long size)
        {
            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In receive_file of Files.cs");
            net.receive_file(path, size);
        }

        public long sha1_fss_mtime()
        {
            return TimeSize.entry_mtime(this.sha1_fss_path);
        }


        public void update_files()
        {
            try
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

                // if dir is empty
                if (!File.Exists(this.temp_sha1_fss_path))
                {
                    FileStream fs = new FileStream(this.temp_sha1_fss_path, FileMode.Create, FileAccess.Write);
                    fs.Close();
                    //File.Create(this.temp_sha1_fss_path);
                }



                if (!File.Exists(this.sha1_fss_path))
                {
                    File.Move(this.temp_sha1_fss_path, this.sha1_fss_path);
                }
                else
                {
                    // if filesys not changed, we don't modify sha1.fss
                    if (Sha1.sha1_file_via_fname(this.sha1_fss_path) ==
                        Sha1.sha1_file_via_fname(this.temp_sha1_fss_path))
                        File.Delete(this.temp_sha1_fss_path);
                    else
                    {
                        File.Delete(this.sha1_fss_path);
                        File.Move(this.temp_sha1_fss_path, this.sha1_fss_path);
                    }
                }
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }
            
        }


        public void write_in(string fullpath)
        {
            if (if_to_skip(fullpath))
                return;
            File.AppendAllText(this.finfo_fss_path, fullpath, Encoding.Default);
            File.AppendAllText(this.finfo_fss_path, "\n");


            File.AppendAllText(this.temp_sha1_fss_path, Sha1.sha1_file_via_fname_fss(global_root_path, fullpath));
            File.AppendAllText(this.temp_sha1_fss_path, "\n");

        }

        public bool if_to_skip(string fullpath)
        {
            if (this.global_root_path == fullpath)
                return true;

            if (!File.Exists(fullpath) && !Directory.Exists(fullpath))
                return true;

            FileAttributes attr = File.GetAttributes(fullpath);
            if ((attr & FileAttributes.Hidden) == FileAttributes.Hidden)
                return true;

            if (string.Compare(fullpath, 0, this.fss_dir_path, 0, this.fss_dir_path.Length) == 0)
                return true;

            return false;
        }


        public void create_fss_dir()
        {
            if (!Directory.Exists(this.fss_dir_path))
            {
                DirectoryInfo di = Directory.CreateDirectory(this.fss_dir_path);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(this.fss_dir_path);

                if ((di.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    di.Attributes = FileAttributes.Hidden;

            }

        }



        // return 2: remote.sha1.fss is identical to sha1.fss
        // return 3: no unique records in remote.sha1.fss, only in sha1.fss
        // return 4: no unique records in sha1.fss, only in remote.sha1.fss
        // return 0: both have unique records

        public static int DIFF_BOTH_UNIQ = 0;
        public static int DIFF_IDENTICAL = 2;
        public static int DIFF_LOCAL_UNIQ = 3;
        public static int DIFF_REMOTE_UNIQ = 4;

        public int generate_diffs()
        {
            long size0 = 0;
            long size1 = 0;
            try
            {
                if (File.Exists(this.diff_remote_index_path))
                    File.Delete(this.diff_remote_index_path);
                if (File.Exists(this.diff_local_index_path))
                    File.Delete(this.diff_local_index_path);

                if (string.Compare(Sha1.sha1_file_via_fname(this.remote_sha1_fss_path),
                                    Sha1.sha1_file_via_fname(this.sha1_fss_path)) == 0)
                    return DIFF_IDENTICAL;

                Diff.diff(this.remote_sha1_fss_path, this.sha1_fss_path, this.diff_remote_index_path,
                    this.diff_local_index_path, string.Empty);

                FileInfo fi0 = new FileInfo(this.diff_remote_index_path);
                FileInfo fi1 = new FileInfo(this.diff_local_index_path);

                size0 = fi0.Length;
                size1 = fi1.Length;

            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }

            if (size0 == 0 && size1 == 0)
                return DIFF_IDENTICAL;

            if (size0 == 0)
                return DIFF_LOCAL_UNIQ;

            if (size1 == 0)
                 return DIFF_REMOTE_UNIQ;

            return DIFF_BOTH_UNIQ;
            
                

        }


        public int send_entryinfo_or_reqsha1info(bool ifinit, string prefix0,
            string prefix1, string prefix2)
        {
            if (ifinit)
                this.diff_local_index = new FileStream(this.diff_local_index_path, FileMode.Open, FileAccess.Read);

            if (this.diff_local_index.Position == this.diff_local_index.Length)
            {
                net.send_msg(prefix2);
                this.diff_local_index.Close();
                this.linenum_to_send = -1;
                return PREFIX2_SENT;
            }

            string record = Diff.get_line(this.diff_local_index);
            this.linenum_to_send = Convert.ToInt32(record);

            return send_entryinfo_via_linenum(linenum_to_send, prefix0, prefix1);
        }

        public int send_entryinfo_via_linenum(int linenum, string prefix0, string prefix1)
        {
            string record = Diff.get_line_via_linenum(this.finfo_fss_path, linenum);
            return send_entryinfo(record, prefix0, prefix1);

        }


        public void send_del_index_info(string prefix)
        {

            send_entryinfo(this.diff_remote_index_path, prefix, string.Empty);
        }

        public int send_entryinfo(string record, string prefix0, string prefix1)
        {
            int rv = 0;

                string msg = string.Empty;
                if (Directory.Exists(record))
                {
                    msg += prefix1;
                    rv = PREFIX1_SENT;
                }
                else
                {
                    msg += prefix0;
                    rv = PREFIX0_SENT;
                }

                msg += get_rela_path(record);
                msg += '\n';
                msg += TimeSize.entry_mtime(record);
                msg += '\n';
                this.size_to_send = TimeSize.entry_size(record);
                msg += this.size_to_send;

                net.send_msg(msg);


            return rv;

        }

        public int send_linenum_or_done(bool ifinit, string prefix0, string prefix1)
        {
            int rv = 0;
            string msg = string.Empty;
            
            try
            {
                if (ifinit)
                {
                    this.diff_remote_index = null;
                    this.diff_remote_index = new FileStream(this.diff_remote_index_path, FileMode.Open,
                        FileAccess.Read);
                }

                if (this.diff_remote_index.Position == this.diff_remote_index.Length)
                {
                    net.send_msg(prefix1);
                    this.diff_remote_index.Close();
                }
                else
                {
                    string record = Diff.get_line(this.diff_remote_index);
                    msg += prefix0;
                    msg += record;
                    net.send_msg(msg);
                }


            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }

            return rv;

        }

        public void send_file_via_linenum()
        {
            string record = Diff.get_line_via_linenum(this.finfo_fss_path, linenum_to_send);

            send_file(record);
        }

        public void send_del_index()
        {
            FileInfo fi = new FileInfo(this.diff_remote_index_path);
            net.send_file(this.diff_remote_index_path, fi.Length);

        }

        public void send_file(string fullpath)
        {
            net.send_file(fullpath, size_to_send);
        }


        public string get_rela_path(string fullpath)
        {
            Log.logon(" in get_relapath, fullpath is --" + fullpath + "--");
            Log.logon("root is --" + this.global_root_path + "-- " + this.global_root_path.Length);

            string relapath;
            relapath = fullpath.Substring(this.global_root_path.Length);

            Log.logon("relapath is --" + relapath + "--");

            if (relapath[0] == '\\')
                relapath = relapath.Substring(1);

            return relapath.Replace('\\', '/');
        }

        public void create_dir(string relapath)
        {
            string fullpath = Path.Combine(this.global_root_path, relapath);
            if (!Directory.Exists(fullpath))
            {
                Directory.CreateDirectory(fullpath);
            }

        }
        public void remove_files()
        {
            try
            {
                FileInfo fi = new FileInfo(this.diff_local_index_path);
                if (fi.Length == 0)
                    return;

                FileStream fs = new FileStream(this.diff_local_index_path, FileMode.Open, FileAccess.Read);
                while (fs.Position < fs.Length)
                {
                    string record = Diff.get_line(fs);
                    int linenum_to_delete = Convert.ToInt32(record);
                    record = Diff.get_line_via_linenum(this.finfo_fss_path, linenum_to_delete);
                    File.Delete(record);
                }

            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }

        }


    }
}
