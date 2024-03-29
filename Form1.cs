﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Renci.SshNet;
using System.Net.Sockets;

namespace WindowsFormsApplication1
{

    //I'm sorry for this really sloppy code, but had to finish it all in one day.
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        delegate void SetTextCallback(string text);

        public void setSyslog(String text)
        {
            if (textBox3.InvokeRequired)
                textBox3.Invoke(new SetTextCallback(setSyslog),new object[]{text});
            else
                textBox3.Text = text.Replace("\n","\r\n");
        }

        public void setNetstat(String text)
        {
            if(textBox4.InvokeRequired)
                textBox4.Invoke(new SetTextCallback(setNetstat),new object[]{text});
            else
                textBox4.Text = text.Replace("\n", "\r\n");
        }

        public void setPS(String text)
        {
            if(textBox5.InvokeRequired)
                textBox5.Invoke(new SetTextCallback(setPS), new object[] { text });
            else
                textBox5.Text = text.Replace("\n", "\r\n");
        }

        public void setTerminalPathText(String text)
        {
            if (text != "")
            {
                if(textBox2.InvokeRequired)
                    textBox2.Invoke(new SetTextCallback(setTerminalPathText), new object[] { text });
                else
                    textBox2.Text = text + "#";
            }
        }

        public void setTerminalText(String text)
        {
            if (richTextBox1.InvokeRequired)
                richTextBox1.Invoke(new SetTextCallback(setTerminalText), new object[] { text });
            else
            {
                String txt = richTextBox1.Text;
                if (text != "") ;
                {
                    txt = txt + "\n" + text;
                    richTextBox1.Text = txt;
                }
            }
        }


        public void setCPUPercentage(double perc)
        {
            //progressBar1.Value = (int)perc;
        }

        public void setRAMPercentage(double perc)
        {
           // progressBar2.Value = (int)perc;
        }

        public void setHDDPercentage(double perc)
        {
            //progressBar3.Value = (int)perc;
        }

        public void addServerFolderPath(String folder)
        {
            listBox3.Items.Add(folder);
        }

        public void addServerFolderFile(String file)
        {
            listBox4.Items.Add(file);
        }

        public void clearServers()
        {
            listBox3.Items.Clear();
            listBox4.Items.Clear();
        }

        public void setReomtePath(String path)
        {
            comboBox2.Text = path;
        }

        public void updateDirectory()
        {
            try
            {
                String[] allfiles = System.IO.Directory.GetFiles(Globals.directory, "*.*");
                String[] alldirs = System.IO.Directory.GetDirectories(Globals.directory, "*.*");
                listBox2.Items.Clear();
                foreach (var dirs in alldirs)
                {
                    int li = dirs.LastIndexOf("\\") + 1;
                    String dir = dirs.Substring(li);
                    listBox2.Items.Add(dir);

                }
                listBox1.Items.Clear();
                listBox1.Items.Add(".");
                listBox1.Items.Add("..");
                foreach (var files in allfiles)
                {
                    int li = files.LastIndexOf("\\") + 1;
                    String file = files.Substring(li);
                    listBox1.Items.Add(file);
                }
                comboBox1.Text = Globals.directory;
            }
            catch (Exception e) { Globals.directory = Globals.directory.Substring(0, Globals.directory.LastIndexOf("\\")); }

        }

        private void richTextBox1_Click_1(object sender, EventArgs e)
        {
            textBox1.Focus();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Globals.mainForm = this;
            updateDirectory();
            Globals.load_data();
            String pass="";            
            {
                pass = Microsoft.VisualBasic.Interaction.InputBox("Please enter the master password. Leave empty to cancel.", "Master Password", " ");
                if (pass == "")
                    Application.Exit();
                else
                    if (SessionData.StringCipher.Decrypt(Globals.Settings.Token, pass) != "B33F")
                    {
                        if (Globals.Settings.Token != "")
                        {
                            MessageBox.Show("Wrong password. Application will exit.");
                            Application.Exit();
                        }
                    }
            }
            Globals.masterpass = pass;


            foreach (String key in Globals.Settings.Sessions.Keys)
            {
                listView1.Items.Add(((SessionData)Globals.Settings.Sessions[key]).SessionName);
            }            

            foreach (String key in Globals.Settings.Macros.Keys)
                listBox5.Items.Add(key);

            foreach (String key in Globals.Settings.Monitors.Keys)
            {
                treeView1.Nodes.Add(((MonitoringData)Globals.Settings.Monitors[key]).name);
            }

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo d in drives)
            {
                comboBox3.Items.Add(d.Name);
            }
            comboBox3.SelectedIndex = 0;

            backgroundWorker2.RunWorkerAsync();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox5.SelectedIndex > -1)
            {
                String macro = (String)Globals.Settings.Macros[listBox5.SelectedItem.ToString()];
                foreach (var myString in macro.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    try
                    {
                        Globals.sshconnection.sendCommand(myString);
                    }
                    catch (Exception e4) { setTerminalText("<<<SCRIPT ERROR>>>"); }
                }
            }            
            
        }
     
        private void button6_Click(object sender, EventArgs e)
        {
            Globals.sshconnection.sendCommand(textBox1.Text);
            textBox1.Text = "";
        }     

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == 13)
                {
                    Globals.mainForm.setTerminalText(textBox2.Text + " " + textBox1.Text);
                    Globals.sshconnection.sendCommand(textBox1.Text);
                    textBox1.Text = "";
                    e.Handled = true;
                }
            }
            catch (Exception ex) { }
        }

        private void listBox4_DoubleClick(object sender, EventArgs e)
        {
            if (listBox4.SelectedItem.ToString().Equals(".."))
            
                    Globals.sshconnection.upOneDir();
        }

        private void listBox3_DoubleClick(object sender, EventArgs e)
        {
                Globals.sshconnection.chdir(listBox3.SelectedItem.ToString());
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
          
                String token = "";
                if (Globals.directory.LastIndexOf("\\") != Globals.directory.Length - 1)
                    token = "\\";
                Globals.directory += token + listBox2.SelectedItem.ToString();
                updateDirectory();
            
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (Globals.directory.Length > 3 && listBox1.SelectedItem.ToString().Equals(".."))
            {
                if (Globals.directory.Substring(Globals.directory.Length - 1, 1) == "\\")
                {
                    Globals.directory = Globals.directory.Substring(0, Globals.directory.Length - 1);
                }
                Globals.directory = Globals.directory.Substring(0, Globals.directory.LastIndexOf("\\") + 1);
                if (Globals.directory.Substring(Globals.directory.Length - 1, 1) != "\\")
                {
                    
                    Globals.directory += "\\";
                }
                    
            }
            updateDirectory();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FormMacros fmacros = new FormMacros();
            fmacros.ShowDialog();
            if (fmacros.DialogResult == DialogResult.OK)
            {
                Globals.Settings.Macros.Add(fmacros.macroname, fmacros.macro);
            }
            listBox5.Items.Clear();
            foreach (String s in Globals.Settings.Macros.Keys)
                listBox5.Items.Add(s);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if(listBox5.SelectedIndex>-1)
            {                
                FormMacroEdit fmacros = new FormMacroEdit();
                fmacros.macrotext = listBox5.SelectedItem.ToString();
                fmacros.macro = (String)Globals.Settings.Macros[fmacros.macrotext];
                fmacros.ShowDialog();

                if (fmacros.DialogResult == DialogResult.OK)
                {
                    if (!fmacros.macrodelete)
                        Globals.Settings.Macros[fmacros.macrotext] = fmacros.macro;
                    else
                        Globals.Settings.Macros.Remove(fmacros.macrotext);
                }
            }
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            WindowsFormsApplication1.SSHConnection con = new WindowsFormsApplication1.SSHConnection("84.200.113.177", 22, "root", "Bl4f4s3L4711$", "test");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FileInfo f = new FileInfo(Globals.directory+"\\"+listBox1.SelectedItem.ToString());
            string uploadfile = f.FullName;
            var client = new SftpClient("84.200.113.177", 22, "root", "Bl4f4s3L4711$");
            client.Connect();
            if(client.IsConnected)
            {
                var fileStream = new FileStream(uploadfile, FileMode.Open);
                if (fileStream != null)
                {
                    client.UploadFile(fileStream, comboBox2.Text+"/"+ f.Name, null);
                    client.Disconnect();
                    client.Dispose();
                    MessageBox.Show("File upload finished.");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (listBox4.SelectedItem != null)
            {
                string host = @"84.200.113.177";
                string username = "root";
                string password = "Bl4f4s3L4711$";

                string remoteDirectory = comboBox2.Text;
                string localDirectory = Globals.directory;

                using (var sftp = new SftpClient(host, username, password))
                {
                    sftp.Connect();
                    String remoteFileName = Globals.directory + "/" + listBox4.SelectedItem.ToString();


                    if ((!listBox4.SelectedItem.ToString().StartsWith(".")))
                        using (Stream file1 = File.OpenWrite(comboBox1.Text + "\\" + remoteFileName.Substring(remoteFileName.LastIndexOf("/") + 1)))
                        {
                            sftp.DownloadFile(remoteDirectory + "/" + listBox4.SelectedItem.ToString(), file1);
                            MessageBox.Show("Download finished.");
                        }
                }

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Globals.sshconnection != null)
            {
                Globals.sshconnection.getSyslog();
                Globals.sshconnection.getNetstat();
                Globals.sshconnection.getPS();
                
                Globals.sshconnection.getCPUPercentage();
                Globals.sshconnection.getRAMPercentage();
               // Globals.sshconnection.getHDDPercentage();
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            textBox3.SelectionStart = textBox3.Text.Length;
            textBox3.ScrollToCaret();
        }

        private void button8_Click(object sender, EventArgs e)
        {

            AddMonitoringForm frm = new AddMonitoringForm();
            frm.ShowDialog();
            if (frm.DialogResult == DialogResult.OK)
            {
                MonitoringData d = new MonitoringData();
                d.bannertext = frm.bannertext;
                d.ignorebanner = frm.ignorebanner;
                d.ip = frm.ip;
                d.name = frm.name;
                d.port = frm.port;
                Globals.Settings.Monitors.Add(d.name, d);
                treeView1.Nodes.Add(d.name, d.name, 1, 1);
                TreeNode[] n = treeView1.Nodes.Find(d.name,true);
                
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                EditMonitoringForm frm = new EditMonitoringForm();
                String name = treeView1.SelectedNode.Text;
                MonitoringData dat = (MonitoringData)Globals.Settings.Monitors[name];
                frm.name = dat.name;
                frm.bannertext = dat.bannertext;
                frm.ignorebanner = dat.ignorebanner;
                frm.ip = dat.ip;
                frm.port = dat.port;

                frm.ShowDialog();
                if (frm.DialogResult == DialogResult.OK)
                {
                    MonitoringData d = new MonitoringData();
                    d.bannertext = frm.bannertext;
                    d.ignorebanner = frm.ignorebanner;
                    d.ip = frm.ip;
                    d.name = frm.name;
                    d.port = frm.port;
                    Globals.Settings.Monitors.Add(d.name, d);
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                String name = treeView1.SelectedNode.Text;
                Globals.Settings.Monitors.Remove(name);
                treeView1.Nodes.Clear();
                foreach (String nodename in Globals.Settings.Monitors.Keys)
                    treeView1.Nodes.Add(nodename);
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            foreach (TreeNode n in treeView1.Nodes)
            {
                MonitoringData m = (MonitoringData)Globals.Settings.Monitors[n.Text];
                if(m!=null)
                using (TcpClient tcpClient = new TcpClient())
                {
                    bool portisopen=false;
                    try
                    {
                        tcpClient.Connect(m.ip, m.port);
                        portisopen = true;
                    }
                    catch (Exception)
                    {
                        portisopen = false;                     
                    }

                    if (portisopen)
                    {
                        n.ImageIndex = 0;
                        n.SelectedImageIndex = 0;
                    }
                    else
                    {
                        n.ImageIndex = 1;
                        n.SelectedImageIndex = 1;
                    }                    
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
      

            AddSessionForm f = new AddSessionForm();
            f.ShowDialog();
            if (f.DialogResult == DialogResult.OK)
            {
                SessionData s = new SessionData(f.name, f.ip, f.port, f.username,f.password);
                Globals.Settings.Sessions.Add(f.name,s);
                listView1.Items.Add(f.name);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                SessionData s = (SessionData)Globals.Settings.Sessions[listView1.SelectedItems[0].Text];

                EditSessionForm f = new EditSessionForm();
                f.name = s.SessionName;
                f.ip = s.Host;
                f.port = s.Port;
                f.username = s.Username;
                f.password = SessionData.StringCipher.Decrypt(s.password, Globals.masterpass);
                f.ShowDialog();
                if (f.DialogResult == DialogResult.OK)
                {
                    s.SessionName = f.name;
                    s.Host = f.ip;
                    s.Port = f.port;
                    s.Username = f.username;
                    s.password = SessionData.StringCipher.Encrypt(f.password,Globals.masterpass);
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count>0)
                Globals.Settings.Sessions.Remove(listView1.SelectedItems[0].Text);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            SessionData s = (SessionData)Globals.Settings.Sessions[listView1.SelectedItems[0].Text];
            Globals.sshconnection = new SSHConnection(s.Host, s.Port, s.Username, SessionData.StringCipher.Decrypt(s.password,Globals.masterpass), s.SessionName);

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Globals.save_data();
        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.Text = comboBox3.SelectedItem.ToString();
            Globals.directory = comboBox1.Text;
            updateDirectory();
        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (Globals.sshconnection != null)
                {

                    Globals.sshconnection.getSyslog();
                    Globals.sshconnection.getNetstat();
                    Globals.sshconnection.getPS();
                    System.Threading.Thread.Sleep(5000);
                    
                }
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            textBox4.SelectionStart = textBox4.Text.Length;
            textBox4.ScrollToCaret();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            textBox5.SelectionStart = textBox5.Text.Length;
            textBox5.ScrollToCaret();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_Click(object sender, EventArgs e)
        {
            textBox1.Focus();
        }
    }
}
