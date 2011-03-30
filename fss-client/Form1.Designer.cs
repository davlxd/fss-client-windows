namespace fss_client
{
    partial class Form_config
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_config));
            this.label_sever = new System.Windows.Forms.Label();
            this.label_path = new System.Windows.Forms.Label();
            this.textBox_server = new System.Windows.Forms.TextBox();
            this.textBox_path = new System.Windows.Forms.TextBox();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.notifyIcon_tray = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip_tray = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip_tray.SuspendLayout();
            this.SuspendLayout();
            // 
            // label_sever
            // 
            this.label_sever.AutoSize = true;
            this.label_sever.Location = new System.Drawing.Point(76, 25);
            this.label_sever.Name = "label_sever";
            this.label_sever.Size = new System.Drawing.Size(41, 12);
            this.label_sever.TabIndex = 0;
            this.label_sever.Text = "Server";
            // 
            // label_path
            // 
            this.label_path.AutoSize = true;
            this.label_path.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.label_path.Location = new System.Drawing.Point(28, 64);
            this.label_path.Name = "label_path";
            this.label_path.Size = new System.Drawing.Size(89, 12);
            this.label_path.TabIndex = 1;
            this.label_path.Text = "Monitored Path";
            this.label_path.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label_path.Click += new System.EventHandler(this.label_path_Click);
            // 
            // textBox_server
            // 
            this.textBox_server.Location = new System.Drawing.Point(153, 22);
            this.textBox_server.Name = "textBox_server";
            this.textBox_server.Size = new System.Drawing.Size(216, 21);
            this.textBox_server.TabIndex = 2;
            // 
            // textBox_path
            // 
            this.textBox_path.Location = new System.Drawing.Point(153, 61);
            this.textBox_path.Name = "textBox_path";
            this.textBox_path.Size = new System.Drawing.Size(216, 21);
            this.textBox_path.TabIndex = 3;
            // 
            // button_ok
            // 
            this.button_ok.Location = new System.Drawing.Point(229, 99);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(75, 23);
            this.button_ok.TabIndex = 4;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Location = new System.Drawing.Point(323, 99);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(75, 23);
            this.button_cancel.TabIndex = 5;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // notifyIcon_tray
            // 
            this.notifyIcon_tray.ContextMenuStrip = this.contextMenuStrip_tray;
            this.notifyIcon_tray.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon_tray.Icon")));
            this.notifyIcon_tray.Text = "FSS";
            this.notifyIcon_tray.Visible = true;
            this.notifyIcon_tray.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_tray_MouseDoubleClick);
            // 
            // contextMenuStrip_tray
            // 
            this.contextMenuStrip_tray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.preferencesToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.contextMenuStrip_tray.Name = "contextMenuStrip_tray";
            this.contextMenuStrip_tray.Size = new System.Drawing.Size(145, 54);
            this.contextMenuStrip_tray.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_tray_Opening);
            // 
            // preferencesToolStripMenuItem
            // 
            this.preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
            this.preferencesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.preferencesToolStripMenuItem.Text = "Preferences";
            this.preferencesToolStripMenuItem.Click += new System.EventHandler(this.preferencesToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // Form_config
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 134);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.textBox_path);
            this.Controls.Add(this.textBox_server);
            this.Controls.Add(this.label_path);
            this.Controls.Add(this.label_sever);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form_config";
            this.Text = "FSS";
            this.Load += new System.EventHandler(this.Form_config_Load);
            this.contextMenuStrip_tray.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_sever;
        private System.Windows.Forms.Label label_path;
        private System.Windows.Forms.TextBox textBox_server;
        private System.Windows.Forms.TextBox textBox_path;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.NotifyIcon notifyIcon_tray;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_tray;
        private System.Windows.Forms.ToolStripMenuItem preferencesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}

