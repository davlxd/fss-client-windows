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

            // this is funny
            net.protocol = this;
            
            watcher = new FileSystemWatcher();
            InitializeMonitor(path);
            files = new Files(path, net);
        }


        private void InitializeMonitor(string path)
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


        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (LOCK)
                return;

            if (files.if_to_skip(e.FullPath))
                return;

            files.update_files();
            System.Windows.Forms.MessageBox.Show(@"FUCK " + "File: " + e.FullPath + " " + e.ChangeType ,
             "Path Invalid", System.Windows.Forms.MessageBoxButtons.OK);
        }

        public void ReceiveCallBack(IAsyncResult ar)
        {
            while (true)
            {
                switch (status)
                {

                    case WAIT_SHA1_FSS_INFO:
                        status_WAIT_SHA1_FSS_INFO(ar);
                        break;

                    case WAIT_SHA1_FSS:
                        status_WAIT_SHA1_FSS(ar);
                        break;

                    case WAIT_FILE:
                        status_WAIT_FILE(ar);
                        break;

                    case WAIT_MSG_SER_REQ_FILE:
                        status_WAIT_MSG_SER_REQ_FILE(ar);
                        break;

                    case WAIT_MSG_SER_RECEIVED:
                        status_WAIT_MSG_SER_RECEIVED(ar);
                        break;

                    case WAIT_MSG_SER_REQ_DEL_IDX:
                        status_WAIT_MSG_SER_REQ_DEL_IDX(ar);
                        break;

                } // end switch()



            }
        }

        public void init()
        {
            LOCK = true;
            status = WAIT_SHA1_FSS_INFO;

            //this.divination();

        }

        //public void divination()
        //{

        //    switch (status)
        //    {

        //        case WAIT_SHA1_FSS_INFO:
        //            status_WAIT_SHA1_FSS_INFO();
        //            break;

        //        case WAIT_SHA1_FSS:
        //            status_WAIT_SHA1_FSS();
        //            break;

        //        case WAIT_FILE:
        //            status_WAIT_FILE();
        //            break;

        //        case WAIT_MSG_SER_REQ_FILE:
        //            status_WAIT_MSG_SER_REQ_FILE();
        //            break;

        //        case WAIT_MSG_SER_RECEIVED:
        //            status_WAIT_MSG_SER_RECEIVED();
        //            break;

        //        case WAIT_MSG_SER_REQ_DEL_IDX:
        //            status_WAIT_MSG_SER_REQ_DEL_IDX();
        //            break;

        //    } // end switch()

        //}


        private void status_WAIT_SHA1_FSS_INFO(IAsyncResult ar)
        {
            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In status_WAIT_SHA1_FSS_INFO");
            string msg = net.receive_line(ar);
            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " Received line " + msg);

            if (msg.Substring(0, 1) != SHA1_FSS_INFO)
            {
                Log.logon("Current status WAIT_SHA1_FSS_INFO receive" + msg);
                return;
            }

            set_fileinfo(msg);
            net.send_msg(CLI_REQ_SHA1_FSS);
            status = WAIT_SHA1_FSS;

        }
        private void status_WAIT_SHA1_FSS(IAsyncResult ar)
        {
            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " IN status_WAIT_SHA1_FSS");
            if (files.receive_sha1_fss(ref req_sz, ar))
                ;
        }
        private void status_WAIT_ENTRY_INFO(IAsyncResult ar)
        {

        }
        private void status_WAIT_FILE(IAsyncResult ar)
        {

        }
        private void status_WAIT_MSG_SER_REQ_FILE(IAsyncResult ar)
        {
        }
        private void status_WAIT_MSG_SER_RECEIVED(IAsyncResult ar)
        {
        }
        private void status_WAIT_MSG_SER_REQ_DEL_IDX(IAsyncResult ar)
        {
        }

        public void set_fileinfo(string msg)
        {
            string[] words = msg.Split('\n');

            if (words[0] == ".fss/sha1.fss")
                this.rela_name = "remote.sha1.fss";
            else
                this.rela_name = words[0];
            this.mtime = Convert.ToInt64(words[1]);
            this.req_sz = Convert.ToInt64(words[2]);

            files.remove_specific_file(this.rela_name);

        }
        
    }
}
