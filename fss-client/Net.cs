using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace fss_client
{


    class Net
    {
        private const int port = 3375;
        public const int MAX_PATH_LEN = 1024;
        public const int MAX_BUF_LEN = 4096;

        private Socket sock;
        private IPAddress[] addrs;

        public Net(string server)
        {
            IPHostEntry iphost = Dns.GetHostEntry(server);
            addrs = iphost.AddressList;


            //TODO:Attention: only connect first addr
            sock = new Socket(addrs[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //IPEndPoint remoteEP = new IPEndPoint(addrs[0], port);
            //sock.BeginConnect(remoteEP, new AsyncCallback(ConnectCallBack), sock);
            //connectDone.WaitOne();

            //Receive(sock);
        }
        public void connect()
        {
            IPEndPoint remoteEP = new IPEndPoint(addrs[0], port);
            try
            {
                sock.Connect(remoteEP);
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " Connected to " + addrs[0].ToString());

            }
            catch (Exception e)
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + e.ToString());
                throw e;
            }

        }


        public void disconnect()
        {
            if (sock.Connected)
                sock.Disconnect(false);
        }

        public string receive_line()
        {
            try
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In receive_line()");

                byte[] receiveBytes = new byte[MAX_PATH_LEN];
                int numBytes = sock.Receive(receiveBytes);

                if (numBytes == 0)
                {
                    Log.logon("Server crashed");
                    if (DialogResult.OK == 
                        MessageBox.Show("Server Down, click OK to quit.", "Warning",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button1))
                    {
                        Application.Exit();
                        Thread.CurrentThread.Abort();
                    }

                }

                // TODO: Attention: Code problem:
                string strTemp;
                strTemp = Encoding.UTF8.GetString(receiveBytes, 0, numBytes);
                byte[] byteTemp = Encoding.Default.GetBytes(strTemp);

                Log.logon("Received " + Encoding.Default.GetString(byteTemp, 0, byteTemp.Length));
                return Encoding.Default.GetString(byteTemp, 0, byteTemp.Length);
            }
            catch (Exception e)
            {
                e.ToString();
            }

            return string.Empty;
        }

        public void receive_file(string path, long size)
        {
            Log.logon("In receive_file() of Net.cs, path is " + path + "and size is " + size);
            byte[] buffer = new byte[MAX_BUF_LEN];
            long num = 0;
            int count = 0;
            //if (File.Exists(path))
            //    File.Delete(path);
            FileStream fs = File.Open(path, FileMode.Create, FileAccess.Write);

            // just touch it if receiving an empty file
            if (size == 0)
            {
                fs.Close();
                return;
            }

            while (num < size)
            {
                count = sock.Receive(buffer);
                fs.Write(buffer, 0, count);
                num += count;

            }
            fs.Flush();
            fs.Close();
            Log.logon("written to file");

        }

        public void send_file(string fullpath, long size)
        {
            Log.logon("In send_file, sending " + fullpath + " " + size);
            byte[] buffer = new byte[MAX_BUF_LEN];

            long num = 0;
            int count = 0;

            FileStream fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read);

            while (num < size)
            {
                count = fs.Read(buffer, 0, MAX_BUF_LEN);
                sock.Send(buffer, count, SocketFlags.None);
                num += count;
            }
            fs.Close();

        }



        //public string receive_line(IAsyncResult ar)
        //{ 
        //    StateObject state = (StateObject)ar.AsyncState;
        //    Socket client = state.workSocket;
        //    int numBytes = 0;

        //    try
        //    {
        //        numBytes = sock.EndReceive(ar);

        //        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
        //            new AsyncCallback(protocol.ReceiveCallBack), state);

        //    } catch (Exception e) {
        //        Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " " + e.ToString());
        //    }

        //   return Encoding.ASCII.GetString(state.buffer, 0, numBytes);

        //}

        //public bool receive_file(string path, ref long size, IAsyncResult ar)
        //{
        //    Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In receive_file()  of Net.cs");
        //    StateObject state = (StateObject)ar.AsyncState;
        //    Socket client = state.workSocket;
        //    FileStream fs = null;
        //    try
        //    {

        //        fs = File.Open(path, FileMode.Append, FileAccess.Write);

        //        int numBytes = sock.EndReceive(ar);
        //        size -= numBytes;

        //        if (numBytes > 0 && size > 0)
        //        {


        //            Log.logon( System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In receive_file(), received: --" + Encoding.ASCII.GetString(state.buffer, 0, numBytes) + "--");
        //            fs.Write(state.buffer, 0, numBytes);

        //            fs.Flush();
        //            fs.Close();

        //            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
        //                new AsyncCallback(protocol.ReceiveCallBack), state);

        //            return false;
        //        }
        //        else
        //        {
        //            fs.Flush();
        //            fs.Close();

        //            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
        //                new AsyncCallback(protocol.ReceiveCallBack), state);

        //            return true;
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " " + e.ToString());
        //    }

        //    return true;
        //}



        public void send_msg(string msg)
        {
            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " IN send_msg sending " + msg);
            //byte[] sendBytes = Encoding.ASCII.GetBytes(msg);
            byte[] sendBytes = Encoding.UTF8.GetBytes(msg);
            sock.Send(sendBytes);

        }

    }// end class Net
        
}// end namespace 
