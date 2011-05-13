using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Threading;

namespace fss_client
{
    class Protocol
    {
        private const string  CLI_REQ_HASH_FSS = "A";
        private const string  CLI_REQ_FILE = "B";
        private const string  SER_REQ_FILE = "D";
        private const string  SER_REQ_DEL_IDX = "E";
        private const string  SER_RECEIVED = "F";
        private const string  DONE = "G";
        private const string  HASH_FSS_INFO = "H";
        private const string  LINE_NUM = "I";
        private const string  FILE_INFO = "J";
        private const string  DEL_IDX_INFO = "K";
        private const string  FIN = "L";
        private const string  CLI_REQ_HASH_FSS_INFO = "M";
        private const string  DIR_INFO = "N";


        private const int WAIT_XXX = 1;
        private const int WAIT_HASH_FSS_INFO = 3;
        private const int WAIT_HASH_FSS = 5;
        private const int WAIT_ENTRY_INFO = 7;
        private const int WAIT_FILE = 9;
        private const int WAIT_MSG_SER_REQ_FILE = 11;
        private const int WAIT_MSG_SER_RECEIVED = 13;
        private const int WAIT_MSG_SER_REQ_DEL_IDX = 15;

        volatile private int status;

        private string sha1_str;
        private string rela_name;
        //TODO: Attention: A portable problem here
        private long mtime;
        private long req_sz;

        // lock for filesystemwather
        volatile private bool LOCK;

        private FileSystemWatcher watcher;
        private fss_client.Files files;
        private fss_client.Net net;

        private string server = string.Empty;
        private string path = string.Empty;

        public Protocol(string server, string path, fss_client.Net net)
        {
            this.server = server;
            this.path = path;
            this.net = net;
            
            watcher = new FileSystemWatcher();
            files = new Files(path, net);
            files.create_fss_dir();

            files.update_files();
        }


        public void InitializeMonitor()
        {
            
            watcher.Path = path;

            // BUG fixed
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnChanged);

            watcher.EnableRaisingEvents = true;

        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            int rv = 0;
            int flag0 = 0;

            if (files.if_to_skip_for_monitor(e.FullPath))
                return;

            if (LOCK)
                return;

            Log.logon(Thread.CurrentThread.GetHashCode().ToString() + " " + e.FullPath + " " + e.ChangeType + " detected and passed.");

            files.update_files();
            rv = files.generate_diffs();

            Log.logon("updated and genearted");

            if(rv == Files.DIFF_BOTH_UNIQ || rv == Files.DIFF_LOCAL_UNIQ)
            {
                LOCK = true;
                files.send_entryinfo_or_reqhashinfo(true, FILE_INFO, DIR_INFO,
                    CLI_REQ_HASH_FSS_INFO, ref flag0 );

                //if (rvv == Files.PREFIX0_SENT)
                //    status = WAIT_MSG_SER_REQ_FILE;

                //else if (rvv == Files.PREFIX1_SENT)
                //    status = WAIT_MSG_SER_RECEIVED;

                //else if (rvv == Files.PREFIX2_SENT)
                //    status = WAIT_SHA1_FSS_INFO;

            }
            else if (rv == Files.DIFF_REMOTE_UNIQ)
            {
                LOCK = true;

                files.send_del_index_info(DEL_IDX_INFO, ref flag0);
                //status = WAIT_MSG_SER_REQ_DEL_IDX;

            }

        }



        public void init()
        {
            LOCK = true;
            status = WAIT_HASH_FSS_INFO;

        }

        public void divination()
        {

            while (true)
            {

                switch (status)
                {
                    case WAIT_XXX:
                        status_WAIT_XXX();
                        break;

                    case WAIT_HASH_FSS_INFO:
                        status_WAIT_HASH_FSS_INFO();
                        break;

                    case WAIT_HASH_FSS:
                        status_WAIT_HASH_FSS();
                        break;

                    case WAIT_FILE:
                        status_WAIT_FILE();
                        break;

                    case WAIT_ENTRY_INFO:
                        this.status_WAIT_ENTRY_INFO();
                        break;

                    case WAIT_MSG_SER_REQ_FILE:
                        status_WAIT_MSG_SER_REQ_FILE();
                        break;

                    case WAIT_MSG_SER_RECEIVED:
                        status_WAIT_MSG_SER_RECEIVED();
                        break;

                    case WAIT_MSG_SER_REQ_DEL_IDX:
                        status_WAIT_MSG_SER_REQ_DEL_IDX();
                        break;

                } // end switch()

            }// end while(true)

        }


        private void status_WAIT_XXX()
        {

            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In status_XXX");
            string msg = net.receive_line();
            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " Received line " + msg);

            LOCK = true;

            if(msg.Substring(0, HASH_FSS_INFO.Length) == HASH_FSS_INFO)
            {
                wait_hash_fss_info(msg);
            }
            else if(msg.Substring(0, SER_REQ_FILE.Length) == SER_REQ_FILE)
            {
                wait_msg_ser_req_file(msg);
            }
            else if (msg.Substring(0, SER_RECEIVED.Length) == SER_RECEIVED)
            {
                wait_msg_ser_received(msg);
            }
            else if (msg.Substring(0, SER_REQ_DEL_IDX.Length) == SER_REQ_DEL_IDX)
            {
                wait_msg_ser_req_del_idx(msg);
            }
            else
            {
                Log.logon("Current status WAIT_HASH_FSS_INFO receive" + msg);
                return;
            }


        }


        private void status_WAIT_HASH_FSS_INFO()
        {

            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In status_WAIT_HASH_FSS_INFO");
            string msg = net.receive_line();
            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " Received line " + msg);

            if (msg.Substring(0, HASH_FSS_INFO.Length) == HASH_FSS_INFO)
            {
                wait_hash_fss_info(msg);
            }
            else
            {
                Log.logon("Current status WAIT_HASH_FSS_INFO receive" + msg);
                return;
            }


        }


        private void wait_hash_fss_info(string msg)
        {
            try
            {
                set_fileinfo(msg.Substring(HASH_FSS_INFO.Length));

                if (this.req_sz == 0)
                {
                    this.status_WAIT_HASH_FSS();
                }
                else
                {
                    net.send_msg(CLI_REQ_HASH_FSS);
                    status = WAIT_HASH_FSS;

                }
                
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }
            

        }

        private void status_WAIT_HASH_FSS()
        {
            try
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " IN status_WAIT_HASH_FSS");

                files.receive_hash_fss(req_sz);
                files.update_files();

                analyse_hash();
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " analyse_hash()-ed");
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }

        }

        private void analyse_hash()
        {
            try
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In analyse_hash()");
                int rv;
                int flag0 = 0;
                int flag1 = 0;
                int flag2 = 0;

                rv = files.generate_diffs();

                Log.logon("mtime is: " + mtime);
                Log.logon("hash_fss_mtime(): " + files.hash_fss_mtime());
                if (mtime <= files.hash_fss_mtime())
                {
                    if (rv == Files.DIFF_BOTH_UNIQ || rv == Files.DIFF_LOCAL_UNIQ)
                    {
                        files.send_entryinfo_or_reqhashinfo(
                            true, FILE_INFO, DIR_INFO, CLI_REQ_HASH_FSS_INFO, ref flag0);

                        Log.logon(">>>> flag0=" + flag0);

                        if (((flag0 & Files.PREFIX1_SENT) != 0) || ((flag0 & Files.SIZE0_SENT) != 0))
                            status = WAIT_MSG_SER_RECEIVED;

                        else if ((flag0 &  Files.PREFIX0_SENT) != 0)
                            status = WAIT_MSG_SER_REQ_FILE;

                        else if ((flag0 & Files.PREFIX2_SENT) != 0)
                            status = WAIT_HASH_FSS_INFO;

                    }
                    else if (rv == Files.DIFF_IDENTICAL)
                    {
                        if (mtime == 1)
                        {
                            net.send_msg(FIN);
                        }
                        else
                        {
                            net.send_msg(DONE);
                        }

                        status = WAIT_XXX;
                        LOCK = false;

                    }
                    else if (rv == Files.DIFF_REMOTE_UNIQ)
                    {
                        files.send_del_index_info(DEL_IDX_INFO, ref flag1);

                        Log.logon(">>>> flag1=" + flag1);

                        if ((flag1 & Files.SIZE0_SENT) != 0)
                            status = WAIT_HASH_FSS_INFO;
                        else 
                            status = WAIT_MSG_SER_REQ_DEL_IDX;

                    }

                }
                else // remote.hash.fss is nower
                {
                    if (rv == Files.DIFF_BOTH_UNIQ || rv == Files.DIFF_REMOTE_UNIQ)
                    {
                        files.send_linenum_or_done(true, LINE_NUM, DONE, ref flag2);
                        status = WAIT_ENTRY_INFO;
                    }
                    else if (rv == Files.DIFF_IDENTICAL)
                    {
                        net.send_msg(DONE);

                        LOCK = false;
                        status = WAIT_XXX;
                    }
                    else if (rv == Files.DIFF_LOCAL_UNIQ)
                    {
                        files.remove_files();
                        files.update_files();

                        net.send_msg(DONE);

                        LOCK = false;
                        status = WAIT_XXX;

                    }

                }
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }

        }

        private void status_WAIT_ENTRY_INFO()
        {
            try
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In status_WAIT_ENTRY_INFO");

                string msg = net.receive_line();

                if (msg.Substring(0, DIR_INFO.Length) == DIR_INFO)
                {
                    string[] words = msg.Substring(DIR_INFO.Length).Split('\n');
                    files.create_dir(words[1]);

                    download_sync();

                }
                else if (msg.Substring(0, FILE_INFO.Length) == FILE_INFO)
                {
                    set_fileinfo(msg.Substring(FILE_INFO.Length));

                    if (this.req_sz == 0)
                        status_WAIT_FILE();
                    else
                    {
                        net.send_msg(CLI_REQ_FILE);
                        status = WAIT_FILE;
                    }
                }
                else
                {
                    Log.logon("Current status WAIT_ENTRY_INFO received unkonw msg: " + msg);
                    return;
                }
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }

        }


        private void status_WAIT_FILE()
        {
            try
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In status_WAIT_FILE()");
                files.receive_common_file(rela_name, req_sz);
                download_sync();
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }

        }

        private void download_sync()
        {
            try
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In download_sync()");

                int rvv = 0;
                int flag0 = 0;
                int flag1 = 0;

                files.send_linenum_or_done(false, LINE_NUM, DONE, ref flag1);

                Log.logon(">>>> flag1=" + flag1);

                if ((flag1 & Files.PREFIX0_SENT)!=0)
                    status = WAIT_ENTRY_INFO;

                else if ((flag1 & Files.PREFIX1_SENT) != 0)
                {
                    files.update_files();

                    rvv = files.generate_diffs();

                    if (rvv == Files.DIFF_BOTH_UNIQ || rvv == Files.DIFF_REMOTE_UNIQ)
                    {
                        files.send_entryinfo_or_reqhashinfo(true, FILE_INFO, DIR_INFO, CLI_REQ_HASH_FSS_INFO, ref flag0);

                        Log.logon(">>>> flag0=" + flag0);

                        if ( ((flag0 & Files.PREFIX1_SENT) != 0) || ((flag0 & Files.SIZE0_SENT) !=0 ) )
                            status = WAIT_MSG_SER_RECEIVED;

                        else if ((flag0 & Files.PREFIX0_SENT) != 0)
                            status = WAIT_MSG_SER_REQ_FILE;

                        else if ( (flag0 & Files.PREFIX2_SENT) != 0)
                            status = WAIT_HASH_FSS_INFO;
                    }
                    else if (rvv == Files.DIFF_LOCAL_UNIQ || rvv == Files.DIFF_IDENTICAL)
                    {
                        if (rvv == 3)
                        {
                            files.remove_files();
                        }

                        LOCK = false;
                        Log.logon("lock unset");
                        status = WAIT_XXX;

                    }

                }
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }


        }


        private void status_WAIT_MSG_SER_REQ_FILE()
        {
            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In status_WAIT_MSG_SER_REQ_FILE()");
            string msg = net.receive_line();
            wait_msg_ser_req_file(msg);
        }

        private void wait_msg_ser_req_file(string msg)
        {
            try
            {
                if (msg.Substring(0, SER_REQ_FILE.Length) == SER_REQ_FILE)
                {
                    files.send_file_via_linenum();
                    status = WAIT_MSG_SER_RECEIVED;
                }
                else
                {
                    Log.logon("Current status WAIT_MSG_SER_REQ_FILE, receive unkonw msg: " + msg);
                }
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }
        }


        private void status_WAIT_MSG_SER_RECEIVED()
        {
            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In status_WAIT_MSG_SER_RECEVIED()");
            string msg = net.receive_line();
            wait_msg_ser_received(msg);
        }

        private void wait_msg_ser_received(string msg)
        {
            int flag0 = 0;
            try
            {
                if (msg.Substring(0, SER_RECEIVED.Length) == SER_RECEIVED)
                {
                    files.send_entryinfo_or_reqhashinfo(false, FILE_INFO, DIR_INFO, CLI_REQ_HASH_FSS_INFO, ref flag0);


                    Log.logon(">>>> flag0=" + flag0);

                    if (((flag0 & Files.PREFIX1_SENT) !=0 ) || ((flag0 & Files.SIZE0_SENT) != 0))
                        status = WAIT_MSG_SER_RECEIVED;

                    else if ((flag0 & Files.PREFIX0_SENT) != 0)
                        status = WAIT_MSG_SER_REQ_FILE;

                    else if ((flag0 & Files.PREFIX2_SENT) != 0)
                        status = WAIT_HASH_FSS_INFO;
                }
                else
                {
                    Log.logon("Current status WAIT_MSG_SER_RECEIVED, received unkonw msg: " + msg);
                }
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }

        }


        private void status_WAIT_MSG_SER_REQ_DEL_IDX()
        {
            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In status_WIAT_MSG_SER_REQ_DEL_IDX");
            string msg = net.receive_line();
            wait_msg_ser_req_del_idx(msg);
        }

        private void wait_msg_ser_req_del_idx(string msg)
        {
            try
            {
                if (msg.Substring(0, SER_REQ_DEL_IDX.Length) == SER_REQ_DEL_IDX)
                {
                    files.send_del_index();

                    status = WAIT_HASH_FSS_INFO;

                }
                else
                {
                    Log.logon("Current status WAIT_MSG_SER_REQ_DEL_IDX, receive unkonw msg: " + msg);

                }
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }
        }



        public void set_fileinfo(string msg)
        {
            try
            {
                string[] words = msg.Split('\n');
                this.sha1_str = words[0];
                

                if (words[1] == ".fss/hash.fss")
                    this.rela_name = "remote.hash.fss";
                else
                    this.rela_name = words[1].Replace('/', '\\');

                this.mtime = Convert.ToInt64(words[2]);
                this.req_sz = Convert.ToInt64(words[3]);
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }

        }
        
    }
}
