using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace fss_client
{
    public class StateObject
    {
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 4096;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();

    }

    class Net
    {
        private const int port = 3375;
        private const int MAX_PATH_LEN = 1024;
        private const int MAX_BUF_LEN = 4096;
        private static ManualResetEvent connectDone =
        new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);


        private Socket sock;
        private IPAddress[] addrs;

        public fss_client.Protocol protocol;

        public Net(string server)
        {
            IPHostEntry iphost = Dns.GetHostEntry(server);
            addrs = iphost.AddressList;


            //TODO:Attention: only connect first addr
            sock = new Socket(addrs[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint remoteEP = new IPEndPoint(addrs[0], port);

            //sock.BeginConnect(remoteEP, new AsyncCallback(ConnectCallBack), sock);
            //connectDone.WaitOne();

            //Receive(sock);
        }
        public void connect()
        {
            IPEndPoint remoteEP = new IPEndPoint(addrs[0], port);
            try
            {
                protocol.init();
                sock.Connect(remoteEP);
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " Connected to " + addrs[0].ToString());
                
            } catch (Exception e)
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() +" " +  e.ToString());
                throw e;
            }

            try
            {
                StateObject state = new StateObject();
                state.workSocket = sock;
                sock.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, 
                    new AsyncCallback(protocol.ReceiveCallBack), state);

            }
            catch (Exception e)
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() +" " + e.ToString());
            }
        }

        //public void ReceiveCallBack(IAsyncResult ar)
        //{
        //    try {

        //        string relapath = string.Empty;
        //        long size = 0;
        //        StateObject state = (StateObject)ar.AsyncState;
        //        Socket client = state.workSocket;

        //        if (protocol.recieve_line_or_file(ref relapath, ref size))
        //        {
        //            int numBytes = sock.EndReceive(ar);
        //            protocol.set_fileinfo(Encoding.ASCII.GetString(state.buffer, 0, numBytes));
        //            Log.logon("In recievCallBack, receved " + Encoding.ASCII.GetString(state.buffer, 0, numBytes));
        //        }
        //        //Log.logon("IN receiveCallBack(), calling protocol.divination()");
        //        //protocol.divination();

        //    }catch (Exception e)
        //    {
        //        Log.logon(e.ToString());
        //    }
        //}


        public void disconnect()
        {
            if (sock.Connected)
                sock.Disconnect(false);
        }

        //public string receive_line()
        //{
        //    Log.logon("IN receive_line()");

        //    byte[] receiveBytes = new byte[MAX_PATH_LEN];
        //    int numBytes = sock.Receive(receiveBytes);

        //    Log.logon("Received " + Encoding.ASCII.GetString(receiveBytes, 0, numBytes));
        //    return Encoding.ASCII.GetString(receiveBytes, 0, numBytes);
        //}

        //public void receive_file(string path, long size, IAsyncResult ar)
        //{
        //    byte[] buffer = new byte[MAX_BUF_LEN];
        //    long num = 0;
        //    int count = 0;
        //    if (File.Exists(path))
        //        File.Delete(path);
        //    FileStream fs = File.Open(path, FileMode.Create, FileAccess.Write);

        //    while (num < size)
        //    {
        //        count = sock.Receive(buffer);
        //        fs.Write(buffer, 0, count);
        //        num += count;

        //    }
        //    fs.Flush();
        //    fs.Close();

        //}

        public string receive_line(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;
            int numBytes = 0;

            try
            {
                numBytes = sock.EndReceive(ar);

                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(protocol.ReceiveCallBack), state);

            } catch (Exception e) {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " " + e.ToString());
            }

            

           return Encoding.ASCII.GetString(state.buffer, 0, numBytes);

        }

        public bool receive_file(string path, ref long size, IAsyncResult ar)
        {
            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In receive_file()  of Net.cs");
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;
            FileStream fs = null;
            try
            {

                fs = File.Open(path, FileMode.Append, FileAccess.Write);

                int numBytes = sock.EndReceive(ar);
                size -= numBytes;

                if (numBytes > 0 && size > 0)
                {


                    Log.logon( System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " In receive_file(), received: --" + Encoding.ASCII.GetString(state.buffer, 0, numBytes) + "--");
                    fs.Write(state.buffer, 0, numBytes);

                    fs.Flush();
                    fs.Close();

                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(protocol.ReceiveCallBack), state);

                    return false;
                }
                else
                {
                    fs.Flush();
                    fs.Close();

                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(protocol.ReceiveCallBack), state);

                    return true;
                }

            }
            catch (Exception e)
            {
                Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " " + e.ToString());
            }

            return true;
        }

        public void send_msg(string msg)
        {
            Log.logon(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " IN send_msg sending " + msg);
            byte[] sendBytes = Encoding.ASCII.GetBytes(msg);
            sock.Send(sendBytes);

        }


    }// end class Net
        
}// end namespace 
