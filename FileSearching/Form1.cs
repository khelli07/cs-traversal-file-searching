using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

// TODO: RADIO BUTTON BUAT STATE COBA CARI NTAR ITU GIMANA NYIMPENNYA, ANIMASIIN GRAF, STATE STATE KEK CHECKBOX BUAT FONDASINYA

namespace FileSearching
{
    public partial class Form1 : Form
    {
        // create folder browser dialog
        FolderBrowserDialog folderDialog = new FolderBrowserDialog();

        // create viewer for graph
        Microsoft.Msagl.GraphViewerGdi.GViewer viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();

        //create graph object
        Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("graph");

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Tambah viewer ketika Form load
            groupBox1.SuspendLayout();
            viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox1.Controls.Add(viewer);
            groupBox1.ResumeLayout();

            // ubah linkLabel1 ketika load
            linkLabel1.Text = "---";
            linkLabel1.ActiveLinkColor = System.Drawing.Color.Black;
            linkLabel1.LinkColor = System.Drawing.Color.Black;
            linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
        }

        // button starting directory
        private void button1_Click(object sender, EventArgs e)
        {
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                label4.Text = "Starting directory: " + folderDialog.SelectedPath;
            }
        }

        // button search
        private void button2_Click(object sender, EventArgs e)
        {
            //linkLabel1.
            string startingDir = folderDialog.SelectedPath;
            string[] subDir = Directory.GetDirectories(startingDir);
            string[] files = Directory.GetFiles(startingDir);
            // bind the graph to the viewer
            foreach (string dir in subDir)
            {
                groupBox1.SuspendLayout();
                viewer.Graph = null;
                graph.AddEdge(startingDir, dir);
                viewer.Graph = graph;
                groupBox1.ResumeLayout();
            }
            graph.FindNode(startingDir).Attr.FillColor = Microsoft.Msagl.Drawing.Color.Green;
            viewer.Graph = graph;
            //testing linkLabel
            linkLabel1.ActiveLinkColor = System.Drawing.Color.Red;
            linkLabel1.LinkColor = System.Drawing.Color.Blue;
            linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.SystemDefault;
            linkLabel1.Text = files[0] + "\n" + files[1];
            linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            // kalo nambah link harus sesuai panjang stringnya
            linkLabel1.Links.Add(0, subDir[0].Length, subDir[0]);
            linkLabel1.Links.Add(subDir[0].Length + 1, subDir[0].Length+ subDir[1].Length, subDir[1]);

            // testing checkbox
            if (checkBox1.CheckState == System.Windows.Forms.CheckState.Checked)
            {
                groupBox1.Text = "Check state berhasil";
            }
        }

        // Handler linkLabel clicked
        private void linkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            string folder = e.Link.LinkData as string;
            if (folder != null)
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = folder
                };
                Process.Start(psi);
            }
        }
    }
}