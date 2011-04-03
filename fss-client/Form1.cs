using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Xml;
using System.Net;

namespace fss_client
{
    public partial class Form_config : Form
    {
        private bool if_config_legal;
        
       
        private fss_client.Net net = null;
        private fss_client.Protocol protocol;

        

        public Form_config()
        {
            

            InitializeComponent();
            if (LoadConfig())
            {
                restart();
            }

        }

        private void restart()
        {
            string server = string.Empty;
            string path = string.Empty;

            ReadSettings(ref server, ref path);

            if (net != null)
                net.disconnect();

            net = null;
            net = new Net(server);
            try
            {
                net.connect();
            }
            catch (Exception)
            {
                this.Text = "FSS - Cannot connect to server \"" + server + "\" !";
                if_config_legal = false;
                this.appear();
                return;
            }

            protocol = null;
            protocol = new fss_client.Protocol(server, path, net);
            protocol.init();

        }

        


        private bool LoadConfig()
        {
            int flag = 0;
            if (!File.Exists("fss.conf"))
            {
                this.Text = "FSS - No configure file found";
                this.appear();
            }
            else
            {
                flag++;
            }

                string server, path;
                server = ""; path = "";

                ReadSettings(ref server, ref path);

                if (server == "" || path == "")
                {
                    this.Text = "FSS - Server or Path cannot be left blank !";
                    this.appear();

                }
                else
                {
                    flag++;
                }

                if (!Directory.Exists(path))
                {
                    this.Text = "FSS - Path \""+path +"\" dosen't exsit !";
                    this.appear();
                }
                else
                {
                    flag++;
                }

                try
                {
                    IPHostEntry iphost = Dns.GetHostEntry(server);
                    flag++;
                }
                catch (Exception )
                {
                    flag--;
                    this.Text = "FSS - Server address \"" + server +"\" is not available !";
                    this.appear();
                }

                if (flag == 4)
                {
                    this.if_config_legal = true;
                    return true;
                }
                else
                {
                    this.if_config_legal = false;
                    return false;
                }


        }

        private void SaveSettings(string server, string path)
        {
            XmlTextWriter writer = new XmlTextWriter("fss.conf", Encoding.UTF8);
            writer.WriteStartDocument();
            writer.WriteStartElement("Settings");

            writer.WriteStartElement("Server");
            writer.WriteValue(server);
            writer.WriteEndElement();// end "Server"

            writer.WriteStartElement("Path");
            writer.WriteValue(path);
            writer.WriteEndElement();//end "Path"

            writer.WriteEndElement();//end "Settings"

            writer.Flush();
            writer.Close();
        }

        private void ReadSettings(ref string server, ref string path)
        {
            if (File.Exists("fss.conf"))
            {
                XmlTextReader reader =
                    new XmlTextReader("fss.conf");
                while (reader.Read())
                {
                    switch (reader.Name)
                    {
                        case "Server":
                            server = reader.ReadString();
                            //this.textBox_server.Text = reader.ReadString();
                            break;
                        case "Path":
                            path = reader.ReadString();
                            //this.textBox_path.Text = reader.ReadString();
                            break;

                        default:
                            break;
                    }
                }
                reader.Close();
            }
        }

        private void disappear()
        {
            this.Hide();
            this.ShowInTaskbar = false;
        }

        private void appear()
        {
            string s0, s1;
            s0 = "";    s1 = "";
            this.ReadSettings(ref s0, ref s1);

            //this.ReadSettings(this.textBox_server.Text, this.textBox_path.Text);
            this.textBox_server.Text = s0;
            this.textBox_path.Text = s1;

            this.Show();
            this.ShowInTaskbar = true;
        }


        private void Form_config_Load(object sender, EventArgs e)
        {
            this.disappear();
        }

        private void contextMenuStrip_tray_Opening(object sender, CancelEventArgs e)
        {

        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Text = "FSS - configure";
            this.appear();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (this.if_config_legal)
            {
                this.disappear();
                e.Cancel = true;
            }
            else
                Application.Exit();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            this.SaveSettings(this.textBox_server.Text, this.textBox_path.Text);
            if (LoadConfig())
            {
                this.disappear();
                this.restart();
            }

            //if (!Directory.Exists(this.textBox_path.Text))
            //{
            //    MessageBox.Show(@"Path: " + this.textBox_path.Text +
            //        " Dosen't Exsit                                   ",
            //        "Path Invalid", MessageBoxButtons.OK);
            //}
            //else
            //{
            //    this.SaveSettings(this.textBox_server.Text, this.textBox_path.Text);
            //    this.disappear();
            //}
        }

        private void label_path_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowDialog();

            this.textBox_path.Text = folderBrowserDialog.SelectedPath;
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            if (this.if_config_legal)
                this.disappear();
            else
                Application.Exit();
        }

        private void notifyIcon_tray_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.appear();
        }

    }
}
