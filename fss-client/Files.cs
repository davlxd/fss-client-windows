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

        public static int PREFIX0_SENT = 1 << 1;
        public static int PREFIX1_SENT = 1 << 2;
        public static int PREFIX2_SENT = 1 << 3;
        public static int PREFIX3_SENT = 1 << 4;
        public static int SIZE0_SENT = 1 << 5;


        private string global_root_path;
        private string fss_dir_path;
        private string finfo_fss_path;
        private string sha1_fss_path;
        private string hash_fss_path;
        private string temp_hash_fss_path;
        private string remote_hash_fss_path;
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
            this.hash_fss_path = Path.Combine(this.fss_dir_path, "hash.fss");
            this.remote_hash_fss_path = Path.Combine(this.fss_dir_path, "remote.hash.fss");
            this.temp_hash_fss_path = Path.Combine(this.fss_dir_path, "temp.hash.fss");
            this.diff_local_index_path = Path.Combine(this.fss_dir_path, "diff.local.index");
            this.diff_remote_index_path = Path.Combine(this.fss_dir_path, "diff.remote.index");
            this.del_index_fss_path = Path.Combine(this.fss_dir_path, "del.index.fss");

        }

        public void receive_hash_fss( long size)
        {
            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In recieve_hash_fss of Files.cs");
            this.receive_file(this.remote_hash_fss_path, size);
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

        public long hash_fss_mtime()
        {
            return TimeSize.entry_mtime(this.hash_fss_path);
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

                if (File.Exists(this.temp_hash_fss_path))
                    File.Delete(this.temp_hash_fss_path);

                if (File.Exists(this.sha1_fss_path))
                    File.Delete(this.sha1_fss_path);


                Fn fn = new Fn(this.write_in); // Instantiate delagate, register function
                ftw = new Ftw(fn, this.global_root_path);  // Instaiate traverse functio

                // if dir is empty
                if (!File.Exists(this.temp_hash_fss_path))
                {
                    FileStream fs = new FileStream(this.temp_hash_fss_path, FileMode.Create, FileAccess.Write);
                    fs.Close();
                    //File.Create(this.temp_hash_fss_path);
                }



                if (!File.Exists(this.hash_fss_path))
                {
                    File.Move(this.temp_hash_fss_path, this.hash_fss_path);
                }
                else
                {
                    // if filesys not changed, we don't modify hash.fss
                    if (Sha1.sha1_file_via_fname(this.hash_fss_path) ==
                        Sha1.sha1_file_via_fname(this.temp_hash_fss_path))
                        File.Delete(this.temp_hash_fss_path);
                    else
                    {
                        File.Delete(this.hash_fss_path);
                        File.Move(this.temp_hash_fss_path, this.hash_fss_path);
                    }
                }
            }
            catch (Exception e)
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " " + e.ToString());
            }
            
        }


        public void write_in(string fullpath)
        {
            if (if_to_skip(fullpath))
                return;

            string sha1 = string.Empty;
            string hash = string.Empty;

            Sha1.compute_hash(fullpath, global_root_path, ref sha1, ref hash);

            File.AppendAllText(this.sha1_fss_path, sha1);
            File.AppendAllText(this.sha1_fss_path, "\n");

            File.AppendAllText(this.finfo_fss_path, fullpath, Encoding.Default);
            File.AppendAllText(this.finfo_fss_path, "\n");


            File.AppendAllText(this.temp_hash_fss_path, hash);
            File.AppendAllText(this.temp_hash_fss_path, "\n");

        }

        public bool if_to_skip_for_monitor(string fullpath)
        {
            if (this.global_root_path == fullpath)
                return true;

            if (string.Compare(fullpath, 0, this.fss_dir_path, 0, this.fss_dir_path.Length) == 0)
                return true;


            return false;
        }

        public bool if_to_skip(string fullpath)
        {

            if (!File.Exists(fullpath) && !Directory.Exists(fullpath))
                return true;

            return if_to_skip_for_monitor(fullpath);

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



        // return 2: remote.hash.fss is identical to hash.fss
        // return 3: no unique records in remote.hash.fss, only in hash.fss
        // return 4: no unique records in sha1.fss, only in remote.hash.fss
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
                //if (File.Exists(this.diff_remote_index_path))
                //    File.Delete(this.diff_remote_index_path);
                //if (File.Exists(this.diff_local_index_path))
                //    File.Delete(this.diff_local_index_path);

                //if (string.Compare(Sha1.sha1_file_via_fname(this.remote_hash_fss_path),
                //                    Sha1.sha1_file_via_fname(this.hash_fss_path)) == 0)
                //    return DIFF_IDENTICAL;

                Diff.diff(this.remote_hash_fss_path, this.hash_fss_path, this.diff_remote_index_path,
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


        public void send_entryinfo_or_reqhashinfo(bool ifinit, string prefix0,
            string prefix1, string prefix2, ref int flag)
        {
            flag = 0;

            if (ifinit)
                this.diff_local_index = new FileStream(this.diff_local_index_path, FileMode.Open, FileAccess.Read);

            if (this.diff_local_index.Position == this.diff_local_index.Length)
            {
                net.send_msg(prefix2);
                this.diff_local_index.Close();
                this.linenum_to_send = -1;
                flag |= PREFIX2_SENT;
                return;
            }

            string record = Diff.get_line(this.diff_local_index);
            this.linenum_to_send = Convert.ToInt32(record);

            send_entryinfo_via_linenum(linenum_to_send, prefix0, prefix1, ref flag);
        }

        public void send_entryinfo_via_linenum(int linenum, string prefix0, string prefix1, ref int flag)
        {
            string record = Diff.get_line_via_linenum(this.finfo_fss_path, linenum);
            send_entryinfo(record, prefix0, prefix1, ref flag);

        }


        public void send_del_index_info(string prefix, ref int flag)
        {

            send_entryinfo(this.diff_remote_index_path, prefix, string.Empty, ref flag);
        }

        public void send_entryinfo(string record, string prefix0, string prefix1, ref int flag)
        {
            flag = 0;
            string sha1 = string.Empty;
            string hash = string.Empty;

            string msg = string.Empty;
            if (Directory.Exists(record))
            {
                msg += prefix1;
                flag |= PREFIX1_SENT;
            }
            else
            {
                msg += prefix0;
                flag |= PREFIX0_SENT;
            }

            Sha1.compute_hash(record, global_root_path, ref sha1, ref hash);

            msg += sha1;
            msg += '\n';
            msg += get_rela_path(record);
            msg += '\n';
            msg += TimeSize.entry_mtime(record);
            msg += '\n';
            this.size_to_send = TimeSize.entry_size(record);
            if (this.size_to_send == 0)
                flag |= SIZE0_SENT;
            msg += this.size_to_send;
            
            net.send_msg(msg);


        }

        public bool reuse_file(string sha1_str, string rela_fname)
        {
            string target_file_path = Path.Combine(this.global_root_path, rela_fname);
            string source_file_path = string.Empty;
            long source_file_linenum = Diff.search_line(this.sha1_fss_path, sha1_str);

            if (source_file_linenum > 0)
            {
                source_file_path = Diff.get_line_via_linenum(this.finfo_fss_path, source_file_linenum);
                try
                {
                    File.Copy(source_file_path, target_file_path, true);
                    return true;
                }
                catch (Exception e)
                {
                    Log.logon("@reuse_file, " + e.ToString());
                }
            }

            return false;

        }


        public void send_linenum_or_done(bool ifinit, string prefix0, string prefix1, ref int flag)
        {
            flag = 0;
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
                    flag |= PREFIX1_SENT;
                }
                else
                {
                    string record = Diff.get_line(this.diff_remote_index);
                    msg += prefix0;
                    msg += record;
                    net.send_msg(msg);
                    flag |= PREFIX0_SENT;
                }


            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }


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

                    if (Directory.Exists(record))
                        Directory.Delete(record, true);
                    if (File.Exists(record))
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
