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
            viewer.Graph = null;
            graph = null;
            graph = new Graph("graph");
            foundFilePath = new List<string>();
            linkLabel1.Text = "";
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
                    wait(0.2);

                    if (fileName == searchedFile)
                    {
                        graph.AddEdge(currentName, fileName);
                        // COLORING THE MATCH NODE
                        graph.FindNode(fileName).Attr.FillColor = Drawing.Color.MistyRose;
                        wait(0.1);

                        ctrFile++;
                        found = true; // Only useful when all occurences is not needed
                        foundFilePath.Add(currentNode); // Directory for file that has been found

                        if (!isAllOccurrence) {
                            // COLOR THE PARENT NOW BECAUSE THE CONTROL WILL BE RETURNED
                            graph.FindNode(currentName).Attr.FillColor = Drawing.Color.Green;
                            return true;
                        };
                    }
                }

                if (!found || isAllOccurrence)
                {
                    foreach (string topDir in dirList)
                    {
                        string dirName = topDir.Split("\\").Last();
                        graph.AddEdge(currentName, dirName);
                        wait(0.2);
                        // Recurrence
                        bool temp = DFS(topDir, searchedFile, isAllOccurrence);
                        if (temp) { 
                            ctrFolder++;
                            if (!isAllOccurrence)
                            {
                                graph.FindNode(currentName).Attr.FillColor= Drawing.Color.Green;
                                return true;
                            }
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
            List<string> result = new List<string>();

            findQue.Enqueue(startingDir);
            Boolean fileFound = false;
            while (findQue.Count != 0 && fileFound==false)
            {
                string checking = findQue.Dequeue();
                string checkingLast = checking.Split('\\').Last();
                if (doneCheck.Count > 0)
                {
                    graph.AddEdge(Directory.GetParent(checking).FullName.Split('\\').Last(), checkingLast);
                }
                else
                {
                    graph.AddNode(checkingLast);
                }
                wait(0.1);

                string[] fileList = Directory.GetFiles(checking, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var file in fileList)
                {
                    
                    string fileToken = file.Split('\\').Last();
                    if (fileToken == fileName)
                    {
                        result.Add(file);
                        graph.AddEdge(checkingLast, file.Split('\\').Last() + result.Count);
                        graph.FindNode(file.Split('\\').Last() + result.Count).Attr.FillColor = Drawing.Color.MistyRose;
                        string[] startingDirSplit = startingDir.Split('\\')[..^1];
                        string[] checkingSplit = checking.Split('\\');
                        foreach (var token in checkingSplit)
                        {
                            if (!startingDirSplit.Contains(token))
                            {
                                graph.FindNode(token).Attr.FillColor = Drawing.Color.Green;
                            }
                        }
                        if (!isAllOccurrence)
                        {
                            fileFound = true;
                        }
                        
                        foundFilePath.Add(checking);
                    }
                    else
                    {
                        graph.AddEdge(checkingLast, file.Split('\\').Last());
                        graph.FindNode(file.Split('\\').Last()).Attr.FillColor = Drawing.Color.Magenta;
                        
                    }
                    wait(0);
                }
                doneCheck.Add(checking);
                if (graph.FindNode(checkingLast.Split('\\').Last()).Attr.FillColor != Drawing.Color.Green)
                {
                    graph.FindNode(checkingLast.Split('\\').Last()).Attr.FillColor = Drawing.Color.Magenta;
                }
                
                string[] dirList = Directory.GetDirectories(checking, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var dir in dirList)
                {
                    if (!doneCheck.Contains(dir))
                    {
                        findQue.Enqueue(dir);
                    }
                }

                if (fileFound)
                {
                    foreach (var antrian in findQue)
                    {
                        graph.AddEdge(checking.Split('\\').Last(), antrian.Split('\\').Last());
                        graph.FindNode(antrian.Split('\\').Last()).Attr.FillColor = Drawing.Color.Gray;
                    }
                }
                
                wait(0);
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