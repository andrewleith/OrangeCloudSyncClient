using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OrangeCloudClient;
using Newtonsoft.Json;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private static FileWatcher watcher;
        Repository LocalRepo = new Repository(@"C:\dsencloud");


        public Form1()
        {
            InitializeComponent();
            //  Server.Start();

        }

        private void button1_Click(object sender, EventArgs e)
        {

            // mimick
            // get changeset

            LocalRepo.SyncWithTheCloud();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

    

    }
}
