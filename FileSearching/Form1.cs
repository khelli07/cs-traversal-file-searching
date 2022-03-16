using System;
using System.Collections.Generic;
using System.Windows.Forms;

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
            groupBox1.SuspendLayout();
            viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox1.Controls.Add(viewer);
            groupBox1.ResumeLayout();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                label4.Text = "Starting directory: " + folderDialog.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string startingDir = folderDialog.SelectedPath;
            string[] subDir = Directory.GetDirectories(startingDir);
            // bind the graph to the viewer
            foreach (string dir in subDir)
            {
                System.Threading.Thread.Sleep(25);
                groupBox1.SuspendLayout();
                viewer.Graph = null;
                graph.AddEdge(startingDir, dir);
                viewer.Graph = graph;
                groupBox1.ResumeLayout();
            }
            graph.FindNode(startingDir).Attr.FillColor = Microsoft.Msagl.Drawing.Color.Green;
            viewer.Graph = graph;
            
        }
    }
}