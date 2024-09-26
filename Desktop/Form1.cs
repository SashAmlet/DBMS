using Shared.DTOs;
using Shared;
using System.Windows.Forms.Design;

namespace Desktop
{
    public partial class Form1 : Form
    {
        private TreeView treeView;
        private RichTextBox richTextBox;
        private MenuStrip menuStrip;
        private DataGridView dataGridView;
        private ContextMenuStrip dbContextMenu;
        private ContextMenuStrip tableContextMenu;
        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();

        }

        private void InitializeCustomComponents()
        {
            #region MENU STRIP
            menuStrip = new MenuStrip
            {
                Dock = DockStyle.Top,
            };
            var fileMenu = new ToolStripMenuItem("File");
            var onSelectDirectoryClick = new ToolStripMenuItem("Select directory", null, OnSelectDirectoryClick);
            var createDatabaseMenuItem = new ToolStripMenuItem("Create Database", null, OnCreateDatabaseClick);
            var exitMenuItem = new ToolStripMenuItem("Exit", null, OnExitClick);

            fileMenu.DropDownItems.Add(onSelectDirectoryClick);
            fileMenu.DropDownItems.Add(createDatabaseMenuItem);
            fileMenu.DropDownItems.Add(exitMenuItem);
            menuStrip.Items.Add(fileMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
            #endregion

            #region TABLE LAYOUT PANEL
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
            };


            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200)); 
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));


            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, menuStrip.Height));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));

            this.Controls.Add(tableLayoutPanel);
            #endregion

            #region TREE VIEW
            treeView = new TreeView
            {
                Dock = DockStyle.Fill,
            };
            tableLayoutPanel.Controls.Add(treeView, 0, 1);

            /*      db ContextMenuStrip       */
            dbContextMenu = new ContextMenuStrip();

            ToolStripMenuItem addTableMenuItem = new ToolStripMenuItem("Add Table", null, OnAddTableClick);
            ToolStripMenuItem deleteDatabaseMenuItem = new ToolStripMenuItem("Delete Database", null, OnDeleteDatabaseClick);

            dbContextMenu.Items.Add(addTableMenuItem);
            dbContextMenu.Items.Add(deleteDatabaseMenuItem);

            treeView.ContextMenuStrip = dbContextMenu;

            treeView.NodeMouseClick += OnTreeNodeRightClick;
            #endregion

            #region GRID VIEW
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
            };
            tableLayoutPanel.Controls.Add(dataGridView, 1, 1);
            #endregion

            #region RICH TEXT BOX
            richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
            };
            tableLayoutPanel.Controls.Add(richTextBox, 0, 2);
            tableLayoutPanel.SetColumnSpan(richTextBox, 2);
            #endregion

            #region OTHER SETTINGS
            this.Text = "Database Manager";
            this.WindowState = FormWindowState.Maximized;
            #endregion
        }



        /*      ACTIONS     */
        private async void OnCreateDatabaseClick(object sender, EventArgs e)
        {
            ApiService apiService = new ApiService();

            string dbName = string.Empty;
            /*    CREATE    */
            using (var dialog = new ActionDialog("Create"))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    dbName = dialog.DatabaseName;
                    CreateDatabaseDTO createDbDTO = new CreateDatabaseDTO()
                    {
                        DatabaseName = dbName,
                    };

                    try
                    {
                        await apiService.CreateDatabaseAsync(createDbDTO);

                        MessageBox.Show("The database has been created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error creating database: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            /*    OPEN    */
            OpenDatabase(dbName);
        }

        private void OnSelectDirectoryClick(object sender, EventArgs e)
        {
            try
            {
                SelectDirectory();
                var dbNames = GetJsonFileNames(Constants.BasePath);

                foreach (var dbName in dbNames)
                {
                    OpenDatabase(dbName);
                }
                MessageBox.Show("The directory has been opened successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening directory: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void OnTreeNodeRightClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView.SelectedNode = e.Node;

                dbContextMenu.Show(treeView, e.Location);
            }
        }
        private void OnAddTableClick(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView.SelectedNode;
            if (selectedNode != null)
            {
                MessageBox.Show($"Adding a table to the database: {selectedNode.Text}", "Add Table");
            }

        }
        private async void OnDeleteDatabaseClick(object sender, EventArgs e)
        {
            ApiService apiService = new ApiService();

            TreeNode selectedNode = treeView.SelectedNode;
            if (selectedNode != null)
            {
                DialogResult result = MessageBox.Show($"Delete database: {selectedNode.Text}?", "Delete Database", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        await apiService.DeleteDatabaseAsync(selectedNode.Text);
                        treeView.Nodes.Remove(selectedNode);
                        MessageBox.Show($"Database {selectedNode.Text} has been deleted.", "Delete Database");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting database: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }
 

        /*      HELPER      */
        private async void OpenDatabase(string dbName)
        {
            ApiService apiService = new ApiService();

            try
            {
                // Checking if a node with this name already exists
                foreach (TreeNode node in treeView.Nodes)
                {
                    if (node.Text == dbName)
                    {
                        return;
                    }
                }

                DatabaseDTO? db = await apiService.GetDatabaseAsync(dbName);
                if (db != null)
                {
                    TreeNode databaseNode = new TreeNode(db.Name)
                    {
                        Tag = db // You can save a database object in a Tag to use it later
                    };

                    // If the database contains tables, add them as subnodes
                    foreach (var table in db.Tables)
                    {
                        TreeNode tableNode = new TreeNode(table.Name)
                        {
                            Tag = table
                        };
                        databaseNode.Nodes.Add(tableNode);
                    }

                    treeView.Nodes.Add(databaseNode);
                }
                else
                {
                    throw new Exception("Database not found");
                }
                /*MessageBox.Show("The database has been opened successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);*/
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating database: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            
        }

        private void SelectDirectory()
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a directory to save the database to";

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    Constants.BasePath = $@"{folderDialog.SelectedPath}";
                }
            }
        }

        private List<string> GetJsonFileNames(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                string[] jsonFiles = Directory.GetFiles(directoryPath, "*.json");

                List<string> fileNames = jsonFiles.Select(file => Path.GetFileNameWithoutExtension(file)).ToList();

                return fileNames;
            }
            else
            {
                MessageBox.Show("The specified directory does not exist.");
                return new List<string>();
            }
        }





        public class ActionDialog : Form
        {
            private TextBox txtDatabaseName;
            private Button btnSave;
            private string action;
            public string DatabaseName => txtDatabaseName.Text;

            public ActionDialog(string action)
            {
                this.action = action;
                InitializeDialogComponents();
            }

            private void InitializeDialogComponents()
            {
                this.Text = $"{action} Database";
                this.Size = new System.Drawing.Size(300, 160);
                this.MinimumSize = new System.Drawing.Size(300, 160);
                this.MaximumSize = new System.Drawing.Size(300, 160);
                this.StartPosition = FormStartPosition.CenterParent;

                Label lblDatabaseName = new Label
                {
                    Text = "Enter Database Name:",
                    AutoSize = true,
                    Location = new System.Drawing.Point(10, 10),
                };

                txtDatabaseName = new TextBox
                {
                    Location = new System.Drawing.Point(10, 40),
                    Width = 260
                };

                btnSave = new Button
                {
                    Text = $"{action}",
                    DialogResult = DialogResult.OK,
                    Location = new System.Drawing.Point(200, 80),
                    Width = 70,
                };

                this.Controls.Add(lblDatabaseName);
                this.Controls.Add(txtDatabaseName);
                this.Controls.Add(btnSave);

                this.AcceptButton = btnSave;
            }
        }


    }
}