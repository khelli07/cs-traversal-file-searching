using System.Diagnostics;
using GViewer = Microsoft.Msagl.GraphViewerGdi.GViewer;
using Graph = Microsoft.Msagl.Drawing.Graph;
using Node = Microsoft.Msagl.Drawing.Node;
using Drawing = Microsoft.Msagl.Drawing;

namespace FileSearching
{
    public partial class Form1 : Form
    {
        FolderBrowserDialog folderDialog = new FolderBrowserDialog();
        private GViewer viewer = new GViewer();
        private Graph graph = new Graph("graph");

        // INITIALIZE GLOBAL VARIABLES
        //string[] trackAllOccurrences = { };
        List<string> foundFilePath = new List<string>();
        bool found = false;
        Stopwatch runTime = new Stopwatch();
        bool clearBtnClicked = false;

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
            linkLabel1.Text = "-";
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
            try
            {
                // Check if clear button has been clicked
                if (!clearBtnClicked)
                {
                    this.button3_Click(sender, new EventArgs());
                }
                // Change it to false for next use
                clearBtnClicked = false;
                // Save timestamp when search button is clicked
                runTime.Start();
                if (!string.IsNullOrWhiteSpace(folderDialog.SelectedPath) // STARTING DIR
                    && !string.IsNullOrWhiteSpace(textBox1.Text)) // SEARCHED FILE
                {
                    if (radioButton2.Checked)
                    {
                        found = false;
                        // DFS( root, destinationFile, isAllOccurrence )
                        bool fileIsFound = DFS(folderDialog.SelectedPath, textBox1.Text.Trim(), checkBox1.Checked);
                        // 
                    } else if (radioButton1.Checked)
                    {
                        BFS(folderDialog.SelectedPath, textBox1.Text.Trim(), checkBox1.Checked);
                    }
                }
                else
                {
                    throw new NoStartingPathException();
                }
            }
            catch (Exception ex) { MessageBox.Show($"{ex.Message}"); }

            // Stop stopwatch and writedown the elapsed time
            runTime.Stop();
            double elapsedTimeInSecond = runTime.ElapsedMilliseconds / 1000.0;
            label8.Text += elapsedTimeInSecond + "s";
            // Print hyperlink
            if (foundFilePath.Count > 0)
            {
                // Ubah style hyperlink dulu
                linkLabel1.Text = "";
                linkLabel1.ActiveLinkColor = System.Drawing.Color.Red;
                linkLabel1.LinkColor = System.Drawing.Color.Blue;
                linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.SystemDefault;
                int totalStrLength = 0;
                for (int i = 0; i < foundFilePath.Count; i++)
                {
                    linkLabel1.Text += "-  " + foundFilePath[i] + "\n";
                    linkLabel1.Links.Add(totalStrLength, foundFilePath[i].Length + 3, foundFilePath[i]);
                    totalStrLength += foundFilePath[i].Length + 4;
                }
            }
        }

        // Button Clear
        private void button3_Click(object sender, EventArgs e)
        {
            clearBtnClicked = true;
            viewer.Graph = null;
            graph = null;
            graph = new Graph("graph");
            foundFilePath = new List<string>();
            linkLabel1.Text = "-";
            linkLabel1.Links.Clear(); // Clear all the hyperlinks from before
            label8.Text = "Elapsed Time:";
            runTime = new Stopwatch();
        }

        // NON-COMPONENT METHODS
        private bool DFS(string currentNode, string searchedFile, bool isAllOccurrence)
        { 
            bool isInChild = false;
            int ctrFile = 0;
            int ctrFolder = 0;
            if (!found || isAllOccurrence)
            {
                string[] fileList = Directory.GetFiles(currentNode, "*.*", SearchOption.TopDirectoryOnly);
                string[] dirList = Directory.GetDirectories(currentNode, "*.*", SearchOption.TopDirectoryOnly);
                string currentName = currentNode.Split("\\").Last();
                foreach (string file in fileList)
                {
                    string fileName = file.Split("\\").Last();

                    if (fileName == searchedFile)
                    {
                        ctrFile++;
                        found = true; // Only useful when all occurences is not needed
                        foundFilePath.Add(currentNode); // Directory for file that has been found
                        
                        // COLORING THE MATCH NODE
                        graph.AddEdge(currentName, (fileName + foundFilePath.Count)).Attr.Color = Drawing.Color.Green;
                        graph.FindNode((fileName + foundFilePath.Count)).Attr.FillColor = Drawing.Color.LightGreen;

                        if (!isAllOccurrence) {
                            // COLOR THE PARENT NOW BECAUSE THE CONTROL WILL BE RETURNED
                            graph.FindNode(currentName).Attr.FillColor = Drawing.Color.Green;
                            wait(0.1);
                            return true;
                        };
                    }
                    else
                    {
                        // If it's not what we're looking for
                        graph.AddEdge(currentName, fileName).Attr.Color = Drawing.Color.Magenta;
                        graph.FindNode(fileName).Attr.FillColor = Drawing.Color.Magenta;
                        
                    }
                    wait(0.1);
                }

                if (!found || isAllOccurrence)
                {
                    foreach (string topDir in dirList)
                    {
                        string dirName = topDir.Split("\\").Last();
                        Drawing.Edge newEdge = graph.AddEdge(currentName, dirName);
                        wait(0.2);
                        // Recurrence
                        bool temp = DFS(topDir, searchedFile, isAllOccurrence);
                        if (temp) { 
                            ctrFolder++;
                            // color the edge to green if found
                            newEdge.Attr.Color = Drawing.Color.Green;
                            if (!isAllOccurrence)
                            {
                                graph.FindNode(currentName).Attr.FillColor= Drawing.Color.Green;
                                return true;
                            }
                        } else
                        {
                            // Edge color if not found
                            newEdge.Attr.Color= Drawing.Color.Magenta;
                        }
                    }
                }

                isInChild = (ctrFile > 0 || ctrFolder > 0);
                // COLORING THE FINISHED PROCESSING NODE
                Node tmp = graph.FindNode(currentName);
                if (isInChild) { 
                    // COLOR IF FOUND
                    graph.FindNode(currentName).Attr.FillColor = Drawing.Color.Green;
                } else { 
                    // COLOR IF NOT FOUND
                    graph.FindNode(currentName).Attr.FillColor = Drawing.Color.Magenta;
                }
                wait(0.1);
            }

            return isInChild;
        }

        private void BFS(string startingDir, string fileName, Boolean isAllOccurrence)
        {
            // TODO Ubah warna node jika gagal dkk, masukkan semua occurence ke foundfilepath, sambungin ke radio bfs
            Queue<string> findQue = new Queue<string>();
            List<string> doneCheck = new List<string>();
            Dictionary<string, Drawing.Edge> edgeMap = new Dictionary<string, Drawing.Edge>();

            findQue.Enqueue(startingDir);
            Boolean fileFound = false;
            while (findQue.Count != 0 && fileFound==false)
            {
                string checking = findQue.Dequeue();
                string checkingLast = checking.Split('\\').Last();
                if (doneCheck.Count == 0)
                {
                    graph.AddNode(checkingLast);
                }
                
                wait(0.2);

                string[] fileList = Directory.GetFiles(checking, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var file in fileList)
                {
                    string fileToken = file.Split('\\').Last();
                    if (fileToken == fileName)
                    {
                        foundFilePath.Add(checking);
                        // Color file leaf
                        edgeMap[file.Split('\\').Last() + foundFilePath.Count] = graph.AddEdge(checkingLast, file.Split('\\').Last() + foundFilePath.Count);
                        edgeMap[file.Split('\\').Last() + foundFilePath.Count].Attr.Color = Drawing.Color.Green;
                        graph.FindNode(file.Split('\\').Last() + foundFilePath.Count).Attr.FillColor = Drawing.Color.LightGreen;
                        
                        string[] startingDirSplit = startingDir.Split('\\')[..^1];
                        string[] checkingSplit = checking.Split('\\');
                        for (int i = 0; i < checkingSplit.Length; i++)
                        {
                            if (!startingDirSplit.Contains(checkingSplit[i]))
                            {
                                // Color the file dir node
                                Node thisNode = graph.FindNode(checkingSplit[i]);
                                thisNode.Attr.FillColor = Drawing.Color.Green;
                                if (edgeMap.ContainsKey(checkingSplit[i]))
                                {
                                    edgeMap[checkingSplit[i]].Attr.Color = Drawing.Color.Green; 
                                }
                            }
                        }
                        if (!isAllOccurrence)
                        {
                            fileFound = true;
                        }
                    }
                    else
                    {
                        edgeMap[file.Split('\\').Last()] = graph.AddEdge(checkingLast, file.Split('\\').Last());
                        if (fileFound)
                        {
                            graph.FindNode(file.Split('\\').Last()).Attr.FillColor = Drawing.Color.Gray;
                        }
                        else
                        {
                            edgeMap[file.Split('\\').Last()].Attr.Color = Drawing.Color.Magenta;
                            graph.FindNode(file.Split('\\').Last()).Attr.FillColor = Drawing.Color.Magenta;
                        }
                        
                    }
                    wait(0.1);
                }
                doneCheck.Add(checking);
                if (graph.FindNode(checkingLast.Split('\\').Last()).Attr.FillColor != Drawing.Color.Green)
                {
                    if (edgeMap.ContainsKey(checkingLast.Split('\\').Last()))
                    {
                        edgeMap[checkingLast.Split('\\').Last()].Attr.Color = Drawing.Color.Magenta;
                    }
                    graph.FindNode(checkingLast.Split('\\').Last()).Attr.FillColor = Drawing.Color.Magenta;
                }
                
                string[] dirList = Directory.GetDirectories(checking, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var dir in dirList)
                {
                    if (!doneCheck.Contains(dir))
                    {
                        findQue.Enqueue(dir);
                        edgeMap[dir.Split('\\').Last()] = graph.AddEdge(checking.Split('\\').Last(), dir.Split('\\').Last());
                        edgeMap[dir.Split('\\').Last()].Attr.Color = Drawing.Color.Gray;
                        graph.FindNode(dir.Split('\\').Last()).Attr.FillColor = Drawing.Color.Gray;
                    }
                }
                wait(0.1);
            }
        }

        private void wait(double seconds)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            viewer.Graph = graph;
            viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            while (stopwatch.ElapsedMilliseconds < seconds * 1000) ;
            Application.DoEvents();
            return;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try {
                string path = e.Link.LinkData as string;
                if (foundFilePath != null)
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = path
                    };
                    Process.Start(psi);
                }
                else { throw new NoLinkFound(); }
            } catch (Exception ex) { MessageBox.Show($"{ex.Message}"); }
        }
    }
}

// ADDITIONAL CLASS
public class NoStartingPathException : Exception
{
    public NoStartingPathException() : base("Please select a starting point and file that you want to search.") { }
}

public class NoLinkFound : Exception
{
    public NoLinkFound() : base("Your file is either not found or has not been specified.") { }
}