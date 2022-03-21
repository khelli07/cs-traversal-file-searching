using System;
using System.Collections.Generic;
using System.Numerics;
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
            // string[] subDir = Directory.GetDirectories(startingDir);
            
            BFS(startingDir, "test_2_2.txt", true);
            // bind the graph to the viewer
            // foreach (string dir in subDir)
            // {
            //     System.Threading.Thread.Sleep(25);
            //     groupBox1.SuspendLayout();
            //     viewer.Graph = null;
            //     graph.AddEdge(startingDir, dir);
            //     viewer.Graph = graph;
            //     groupBox1.ResumeLayout();
            // }
            // graph.FindNode(startingDir).Attr.FillColor = Microsoft.Msagl.Drawing.Color.Green;
            viewer.Graph = graph;
            
        }
        public void wait(int milliseconds)
        {
            var timer1 = new System.Windows.Forms.Timer();
            if (milliseconds == 0 || milliseconds < 0) return;

            // Console.WriteLine("start wait timer");
            timer1.Interval = milliseconds;
            timer1.Enabled  = true;
            timer1.Start();

            timer1.Tick += (s, e) =>
            {
                timer1.Enabled = false;
                timer1.Stop();
                // Console.WriteLine("stop wait timer");
            };

            while (timer1.Enabled)
            {
                Application.DoEvents();
            }
        }
        private void BFS(string startingDir, string fileName, bool allOccur)
        {
            Queue<string> findQue = new Queue<string>();
            List<string> doneCheck = new List<string>();
            List<string> result = new List<string>();

            findQue.Enqueue(startingDir);
            while (findQue.Count != 0)
            {
                string checking = findQue.Dequeue();
                if (doneCheck.Count > 0)
                {
                    graph.AddEdge(Directory.GetParent(checking).FullName, checking);
                    wait(1000);
                    viewer.Graph = graph;
                }
                string[] fileList = Directory.GetFiles(checking, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var file in fileList)
                {
                    string[] fileToken = file.Split('\\');
                    if (fileToken[fileToken.Length - 1] == fileName)
                    {
                        result.Add(file);
                        graph.FindNode(checking).Attr.FillColor = Microsoft.Msagl.Drawing.Color.Green;
                        if (!allOccur)
                        {
                            // return result;
                        }
                    }
                }
                doneCheck.Add(checking);
                string[] dirList = Directory.GetDirectories(checking, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var dir in dirList)
                {
                    if (!doneCheck.Contains(dir))
                    {
                        findQue.Enqueue(dir);
                    }
                }
            }

        }
    }
}