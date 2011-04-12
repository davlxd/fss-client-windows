using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace fss_client
{
    class Protocol
    {
        private const string  CLI_REQ_SHA1_FSS = "A";
        private const string  CLI_REQ_FILE = "B";
        private const string  SER_REQ_FILE = "D";
        private const string  SER_REQ_DEL_IDX = "E";
        private const string  SER_RECEIVED = "F";
        private const string  DONE = "G";
        private const string  SHA1_FSS_INFO = "H";
        private const string  LINE_NUM = "I";
        private const string  FILE_INFO = "J";
        private const string  DEL_IDX_INFO = "K";
        private const string  FIN = "L";
        private const string  CLI_REQ_SHA1_FSS_INFO = "M";
        private const string  DIR_INFO = "N";


        private const int WAIT_SHA1_FSS_INFO = 1;
        private const int WAIT_SHA1_FSS = 3;
        private const int WAIT_ENTRY_INFO = 5;
        private const int WAIT_FILE = 7;
        private const int WAIT_MSG_SER_REQ_FILE = 9;
        private const int WAIT_MSG_SER_RECEIVED = 11;
        private const int WAIT_MSG_SER_REQ_DEL_IDX = 13;

        private int status;
        private string rela_name;

        //TODO: Attention: A portable problem here
        private long mtime;
        private long req_sz;

        // lock for filesystemwather
        private bool LOCK;

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
            if (files.if_to_skip(e.FullPath))
                return;

            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In Onchanged(), " + e.FullPath);
            Log.logon(files.sha1_fss_mtime().ToString());
            if (LOCK)
                return;

            files.update_files();
            //System.Windows.Forms.MessageBox.Show(@"FUCK " + "File: " + e.FullPath + " " + e.ChangeType,
             //"Path Invalid", System.Windows.Forms.MessageBoxButtons.OK);
        }


        public bool recieve_line_or_file(ref string relapath, ref long size)
        {
            switch (status)
            {

                case WAIT_SHA1_FSS_INFO:
                    return true;

                case WAIT_SHA1_FSS:

                    // TODO: Attention: capsulatoin is been corrupted here
                    relapath = "remote.sha1.fss";
                    size = this.req_sz;
                    return false;

                case WAIT_FILE:
                    relapath = this.rela_name;
                    size = this.req_sz;
                    return false;

                case WAIT_MSG_SER_REQ_FILE:
                    return true;

                case WAIT_MSG_SER_RECEIVED:
                    return true;

                case WAIT_MSG_SER_REQ_DEL_IDX:
                    return true;

            }

            return true;

        }


        

        //public void ReceiveCallBack(IAsyncResult ar)
        //{
        //    while (true)
        //    {
        //        switch (status)
        //        {

        //            case WAIT_SHA1_FSS_INFO:
        //                status_WAIT_SHA1_FSS_INFO(ar);
        //                break;

        //            case WAIT_SHA1_FSS:
        //                status_WAIT_SHA1_FSS(ar);
        //                break;

        //            case WAIT_FILE:
        //                status_WAIT_FILE(ar);
        //                break;

        //            case WAIT_MSG_SER_REQ_FILE:
        //                status_WAIT_MSG_SER_REQ_FILE(ar);
        //                break;

        //            case WAIT_MSG_SER_RECEIVED:
        //                status_WAIT_MSG_SER_RECEIVED(ar);
        //                break;

        //            case WAIT_MSG_SER_REQ_DEL_IDX:
        //                status_WAIT_MSG_SER_REQ_DEL_IDX(ar);
        //                break;

        //        } // end switch()

        //    }
        //}

        public void init()
        {
            LOCK = true;
            status = WAIT_SHA1_FSS_INFO;

        }

        public void divination()
        {

            while (true)
            {

                switch (status)
                {

                    case WAIT_SHA1_FSS_INFO:
                        status_WAIT_SHA1_FSS_INFO();
                        break;

                    case WAIT_SHA1_FSS:
                        status_WAIT_SHA1_FSS();
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


        private void status_WAIT_SHA1_FSS_INFO()
        {
            try
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In status_WAIT_SHA1_FSS_INFO");
                string msg = net.receive_line();
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " Received line " + msg);

                if (msg.Substring(0, SHA1_FSS_INFO.Length) != SHA1_FSS_INFO)
                {
                    Log.logon("Current status WAIT_SHA1_FSS_INFO receive" + msg);
                    return;
                }

                set_fileinfo(msg.Substring(SHA1_FSS_INFO.Length));
                net.send_msg(CLI_REQ_SHA1_FSS);

                status = WAIT_SHA1_FSS;
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }

        }

        private void status_WAIT_SHA1_FSS()
        {
            try
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " IN status_WAIT_SHA1_FSS");

                files.receive_sha1_fss(req_sz);
                files.update_files();

                analyse_sha1();
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }

        }

        private void analyse_sha1()
        {
            try
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In analyse_sha1()");
                int rv, rvv;

                rv = files.generate_diffs();

                Log.logon("mtime is: " + mtime);
                Log.logon("sha1_fss_mtime(): " + files.sha1_fss_mtime());
                if (mtime <= files.sha1_fss_mtime())
                {
                    if (rv == 0 || rv == 3)
                    {
                        rvv = files.send_entryinfo_or_reqsha1info(
                            true, FILE_INFO, DIR_INFO, CLI_REQ_SHA1_FSS_INFO);

                        if (rvv == 0)
                            status = WAIT_MSG_SER_REQ_FILE;
                        else if (rvv == 2)
                            status = WAIT_MSG_SER_RECEIVED;
                        else if (rvv == 3)
                            status = WAIT_SHA1_FSS_INFO;

                    }
                    else if (rv == 3)
                    {
                        if (mtime == 1)
                        {
                            net.send_msg(FIN);
                        }
                        else
                        {
                            net.send_msg(DONE);
                        }

                        status = WAIT_SHA1_FSS_INFO;
                        LOCK = false;

                    }
                    else if (rv == 4)
                    {
                        files.send_del_index_info(DEL_IDX_INFO);
                        status = WAIT_MSG_SER_REQ_DEL_IDX;

                    }

                }
                else // remote.sha1.fss is nower
                {
                    if (rv == 0 || rv == 4)
                    {
                        files.send_linenum_or_done(true, LINE_NUM, DONE);
                        status = WAIT_ENTRY_INFO;
                    }
                    else if (rv == 2)
                    {
                        net.send_msg(DONE);

                        LOCK = false;
                        status = WAIT_SHA1_FSS_INFO;

                    }
                    else if (rv == 3)
                    {
                        files.remove_files();
                        files.update_files();

                        net.send_msg(DONE);

                        LOCK = false;
                        status = WAIT_SHA1_FSS_INFO;

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
                    files.create_dir(words[0]);

                    download_sync();

                }
                else if (msg.Substring(0, FILE_INFO.Length) == FILE_INFO)
                {
                    set_fileinfo(msg.Substring(FILE_INFO.Length));

                    net.send_msg(CLI_REQ_FILE);

                    if (this.req_sz == 0)
                        status_WAIT_FILE();
                    else
                        status = WAIT_FILE;
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

                int rv = 0;
                int rvv = 0;
                int rvvv = 0;

                rv = files.send_linenum_or_done(false, LINE_NUM, DONE);

                if (rv == 0)
                    status = WAIT_ENTRY_INFO;
                else if (rv == 2)
                {
                    files.update_files();

                    rvv = files.generate_diffs();

                    if (rvv == 0 || rvv == 4)
                    {
                        rvvv = files.send_entryinfo_or_reqsha1info(true, FILE_INFO, DIR_INFO, CLI_REQ_SHA1_FSS_INFO);

                        if (rvvv == 0)
                            status = WAIT_MSG_SER_REQ_FILE;
                        else if (rvvv == 2)
                            status = WAIT_MSG_SER_RECEIVED;
                        else if (rvvv == 3)
                            status = WAIT_SHA1_FSS_INFO;
                    }
                    else if (rvv == 3 || rvv == 2)
                    {
                        if (rvv == 3)
                        {
                            files.remove_files();
                        }

                        LOCK = false;
                        status = WAIT_SHA1_FSS_INFO;


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

            try
            {
                string msg = net.receive_line();

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
            try
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In status_WAIT_MSG_SER_RECEVIED()");
                int rv = 0;
                string msg = net.receive_line();

                if (msg.Substring(0, SER_RECEIVED.Length) == SER_RECEIVED)
                {
                    rv = files.send_entryinfo_or_reqsha1info(false, FILE_INFO, DIR_INFO, CLI_REQ_SHA1_FSS_INFO);

                    if (rv == 0)
                        status = WAIT_MSG_SER_REQ_FILE;
                    else if (rv == 2)
                        status = WAIT_MSG_SER_RECEIVED;
                    else if (rv == 3)
                        status = WAIT_SHA1_FSS_INFO;
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
            try
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In status_WIAT_MSG_SER_REQ_DEL_IDX");

                string msg = net.receive_line();

                if (msg.Substring(0, SER_REQ_DEL_IDX.Length) == SER_REQ_DEL_IDX)
                {
                    files.send_del_index();

                    status = WAIT_SHA1_FSS;

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

                if (words[0] == ".fss/sha1.fss")
                    this.rela_name = "remote.sha1.fss";
                else
                    this.rela_name = words[0].Replace('/', '\\');
                this.mtime = Convert.ToInt64(words[1]);
                this.req_sz = Convert.ToInt64(words[2]);
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }

        }
        
    }
}
