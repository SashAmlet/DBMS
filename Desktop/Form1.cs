using Shared.DTOs;
using Shared;
using System.Windows.Forms.Design;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Linq;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using System.Runtime.CompilerServices;
using System.Globalization;

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
        /*is used purely to create or fill in the table when mouse focus leaves the TreeView*/
        private TreeNode activeTreeNode;
        private Mode mode = Mode.None;
        private string formName = "Database Manager";
        /*Recognize three saves - save tables when creating, updating and adding.*/
        private enum Mode
        {
            None,
            CreateTable,
            UpdateTable,
            FillInTable
        }
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
            var clearTextBox = new ToolStripMenuItem("Clear TextBox", null, OnClearTextBoxClick);
            var exitMenuItem = new ToolStripMenuItem("Exit", null, OnExitClick);

            fileMenu.DropDownItems.Add(onSelectDirectoryClick);
            fileMenu.DropDownItems.Add(createDatabaseMenuItem);
            fileMenu.DropDownItems.Add(clearTextBox);
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

            // Initializing db ContextMenuStrip (for Database type nodes)
            dbContextMenu = new ContextMenuStrip();
            ToolStripMenuItem addTableMenuItem = new ToolStripMenuItem("Add Table", null, OnAddTableClick);
            ToolStripMenuItem deleteDatabaseMenuItem = new ToolStripMenuItem("Delete Database", null, OnDeleteDatabaseClick);
            dbContextMenu.Items.Add(addTableMenuItem);
            dbContextMenu.Items.Add(deleteDatabaseMenuItem);

            // Initializing table ContextMenuStrip (for Table type nodes)
            tableContextMenu = new ContextMenuStrip();
            //ToolStripMenuItem renameTableMenuItem = new ToolStripMenuItem("Rename Table", null, OnRenameTableClick);
            ToolStripMenuItem updateTableMenuItem = new ToolStripMenuItem("Update Table", null, OnUpdateTableClick);
            ToolStripMenuItem removeDuplicateRowsMenuItem = new ToolStripMenuItem("Remove Duplicate Rows", null, OnRemoveDuplicateRows);
            ToolStripMenuItem deleteTableMenuItem = new ToolStripMenuItem("Delete Table", null, OnDeleteTableClick);
            //tableContextMenu.Items.Add(renameTableMenuItem);
            tableContextMenu.Items.Add(updateTableMenuItem);
            tableContextMenu.Items.Add(removeDuplicateRowsMenuItem);
            tableContextMenu.Items.Add(deleteTableMenuItem);

            // 
            treeView.NodeMouseClick += OnTreeNodeRightClick;
            treeView.NodeMouseDoubleClick += OnTreeNodeDoubleClick;

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
            richTextBox.ReadOnly = true;
            #endregion

            #region OTHER SETTINGS
            this.Text = formName;
            this.WindowState = FormWindowState.Maximized;
            // Allow processing of key press events
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(OnKeyDown);
            #endregion
        }



        /*      ACTIONS     */
        private async void OnSelectDirectoryClick(object sender, EventArgs e)
        {
            try
            {
                await SelectDirectory();
                var dbNames = GetJsonFileNames(Constants.BasePath);

                foreach (var dbName in dbNames)
                {
                    await OpenDatabase(dbName);
                }
                AppendMessageToRichTextBox("The directory has been opened successfully.");
            }
            catch (Exception ex)
            {
                AppendMessageToRichTextBox($"Error opening directory: {ex.Message}");
            }
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            Application.Exit();
        }
        
        #region DB ACTIONS
        private async void OnCreateDatabaseClick(object sender, EventArgs e)
        {
            ApiService apiService = new ApiService();

            string dbName = string.Empty;
            /*    CREATE    */
            using (var dialog = new ActionDialog("Create", "Database"))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    dbName = dialog.Name;
                    CreateDatabaseDTO createDbDTO = new CreateDatabaseDTO()
                    {
                        DatabaseName = dbName,
                    };

                    try
                    {
                        if (Constants.BasePath == string.Empty)
                        {
                            throw new Exception("Specify the path to the database storage directory.");
                        }

                        await apiService.CreateDatabaseAsync(createDbDTO);

                        AppendMessageToRichTextBox("The database has been created successfully.");
                    }
                    catch (Exception ex)
                    {
                        AppendMessageToRichTextBox($"Error creating database: {ex.Message}");
                        return;
                    }
                }
            }

            /*    OPEN    */
            OpenDatabase(dbName);
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
                        AppendMessageToRichTextBox($"Database {selectedNode.Text} has been deleted.");
                    }
                    catch (Exception ex)
                    {
                        AppendMessageToRichTextBox($"Error deleting database: {ex.Message}");
                    }
                }
            }

        }
        #endregion

        #region TABLE ACTIONS

        private void OnTreeNodeRightClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView.SelectedNode = e.Node;

                if (e.Node.Tag is DatabaseDTO)
                {
                    dbContextMenu.Show(treeView, e.Location);
                }
                else if (e.Node.Tag is TableDTO)
                {
                    tableContextMenu.Show(treeView, e.Location);
                }
            }
        }
        private void OnTreeNodeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Check that the node is a table
            if (e.Node.Tag is TableDTO table)
            {
                // Remember which note I work in and set the appropriate mode
                activeTreeNode = e.Node;
                mode = Mode.FillInTable;

                ClearAndUnlockDataGridView();
                this.Text = formName + $" - {table.Name}";

                // Add columns to DataGridView from table structure
                foreach (var column in table.Columns.OrderBy(c => c.DisplayIndex))
                {
                    dataGridView.Columns.Add(new DataGridViewTextBoxColumn
                    {
                        Name = column.Name,
                        HeaderText = column.Name
                    });
                }

                // Group cells by RowNum
                var groupedRows = (table.Cells ?? Enumerable.Empty<CellDTO>())
                    .GroupBy(cell => cell.RowNum)
                    .OrderBy(g => g.Key);


                // Adding Rows to DataGridView
                foreach (var rowGroup in groupedRows)
                {
                    var rowData = new object[table.Columns.Count()];

                    foreach (var cell in rowGroup)
                    {

                        int columnIndex = table.Columns.First(col => col.Id == cell.ColumnId).DisplayIndex;
                        if (columnIndex >= 0)
                        {
                            rowData[columnIndex] = cell.Value;
                        }
                    }

                    dataGridView.Rows.Add(rowData);
                }
            }
        }

        private async void OnRemoveDuplicateRows(object sender, EventArgs e)
        {
            ClearAndLockDataGridView();
            ApiService apiService = new ApiService();

            TreeNode selectedNode = treeView.SelectedNode;
            TreeNode parentNode = selectedNode.Parent;
            if (selectedNode != null && parentNode != null && selectedNode.Tag is TableDTO table && parentNode.Tag is DatabaseDTO db)
            {
                DialogResult result = MessageBox.Show($"Remove duplicate rows in the {selectedNode.Text}?", "Remove duplicate rows", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        TableDTO? new_table = await apiService.RemoveDuplicateRowsAsync(db.Name, table.Id);


                        // add node to TreeView

                        if (new_table is null)
                        {
                            throw new Exception("Null returned instead of table");
                        }

                        selectedNode.Tag = new_table;
                        AppendMessageToRichTextBox($"Duplicate lines in {table.Name} have been successfully removed.");
                    }
                    catch (Exception ex)
                    {
                        AppendMessageToRichTextBox($"Error removing duplicate rows in table: {ex.Message}");
                    }
                }
            }

        }

        #region CREATE
        private void InitializeCreateTableForm()
        {
            mode = Mode.CreateTable;
            ClearAndUnlockDataGridView();
            this.Text = formName + " - Creating a table";

            DataGridViewTextBoxColumn columnNameColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "Column Name",
                Name = "ColumnName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            dataGridView.Columns.Add(columnNameColumn);

            DataGridViewComboBoxColumn dataTypeColumn = new DataGridViewComboBoxColumn
            {
                HeaderText = "Data Type",
                Name = "DataType",
                DataSource = Enum.GetValues(typeof(Constants.DataType)),
                ValueType = typeof(Constants.DataType),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            dataGridView.Columns.Add(dataTypeColumn);

            DataGridViewCheckBoxColumn allowNullsColumn = new DataGridViewCheckBoxColumn
            {
                HeaderText = "Allow Nulls",
                Name = "AllowNulls",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            dataGridView.Columns.Add(allowNullsColumn);

            dataGridView.AllowUserToAddRows = true;

            dataGridView.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridView.RowsAdded += DataGridView_RowsAdded;
            /*dataGridView.CellEndEdit += DataGridView_CellEndEdit;*/
        }
        private void OnAddTableClick(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView.SelectedNode;
            if (selectedNode != null)
            {
                // Remember which note I work in and set the appropriate mode
                activeTreeNode = selectedNode;
                mode = Mode.CreateTable;

                InitializeCreateTableForm();
                // Initializing datatype in the first line
                DataGridView_RowsAdded(sender, new DataGridViewRowsAddedEventArgs(0, 0));

            }

        }
        private async void CTRL_S_OnAddTableClick(object sender, EventArgs e)
        {
            List<ColumnDTO> columns = new List<ColumnDTO>();
            ApiService apiService = new ApiService();

            // First, we go through the rows of the form and form columns.
            try
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (row.IsNewRow) continue;

                    string columnName = row.Cells["ColumnName"].Value.ToString();
                    var aaa = row.Cells["DataType"].Value;
                    Constants.DataType dataType = (Constants.DataType)row.Cells["DataType"].Value;
                    bool allowNulls = (bool)(row.Cells["AllowNulls"].Value ?? false);

                    columns.Add(new ColumnDTO
                    {
                        Id = Guid.NewGuid(),
                        Name = columnName,
                        Type = dataType,
                        IsNullable = allowNulls,
                        DisplayIndex = row.Index
                    });
                }

            }
            catch (Exception ex)
            {
                AppendMessageToRichTextBox($"All fields must be filled in");
            }

            // If everything is fine, we form a DTO and call the service
            using (var dialog = new ActionDialog("Create", "Table"))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var tableName = dialog.Name;
                    CreateTableDTO createTableDTO = new CreateTableDTO()
                    {
                        TableId = Guid.NewGuid(),
                        TableName = tableName,
                        Columns = columns
                    };

                    try
                    {
                        string dbName = activeTreeNode.Text;
                        await apiService.CreateTableAsync(dbName, createTableDTO);

                        // add node to TreeView
                        TableDTO tableDTO = new TableDTO()
                        {
                            Id = createTableDTO.TableId,
                            Name = createTableDTO.TableName,
                            Columns = createTableDTO.Columns,
                            Cells = null,
                        };
                        TreeNode tableNode = new TreeNode(createTableDTO.TableName)
                        {
                            Tag = tableDTO
                        };
                        activeTreeNode.Nodes.Add(tableNode);

                        ClearAndLockDataGridView();

                        AppendMessageToRichTextBox($"The table has been created successfully.");
                    }
                    catch (Exception ex)
                    {
                        AppendMessageToRichTextBox($"Error creating table: {ex.Message}");
                    }
                }
            }

        }
        #endregion

        #region UPDATE
        private void OnUpdateTableClick(object sender, EventArgs e)
        {

        }
        private async void CTRL_S_OnUpdateTableClick(object sender, EventArgs e)
        {

        }
        #endregion

        #region FILL IN
        private async void CTRL_S_OnFillInTableClick(object sender, EventArgs e)
        {
            List<CellDTO> cells = new List<CellDTO>();
            ApiService apiService = new ApiService();

            if (activeTreeNode.Tag is TableDTO table)
            {
                var columns = table.Columns.OrderBy(c => c.DisplayIndex).ToList();
                // First, we go through the rows of the form and form cells.
                try
                {
                    foreach (DataGridViewRow row in dataGridView.Rows)
                    {
                        if (row.IsNewRow) continue;

                        foreach (var column in columns)
                        {
                            string errorMessage;
                            object cellValue = dataGridView.Rows[row.Index].Cells[column.DisplayIndex].Value;

                            bool isValid = ValidateCell(cellValue, column.Type, column.IsNullable, column.DisplayIndex, row.Index, out errorMessage);

                            if (!isValid)
                            {
                                throw new Exception(errorMessage);
                            }

                            cells.Add(new CellDTO
                            {
                                Value = cellValue,
                                ColumnId = column.Id,
                                RowNum = row.Index
                            });
                        }
                    }

                }
                catch (Exception ex)
                {
                    AppendMessageToRichTextBox($"{ex.Message}");
                    return;
                }

                // If everything is fine, we form a DTO and call the service
                try
                {
                    if (activeTreeNode.Parent.Tag is DatabaseDTO db)
                    {
                        string dbName = db.Name;

                        var updateTableDTO = new UpdateTableDTO()
                        {
                            TableName = table.Name,
                            Columns = columns,
                            Cells = cells,
                        };

                        await apiService.UpdateTableAsync(dbName, table.Id, updateTableDTO);

                        // add node to TreeView
                        TableDTO new_table = table;
                        new_table.Cells = cells;
                        activeTreeNode.Tag = new_table;
                    }
                }
                catch (Exception ex)
                {
                    AppendMessageToRichTextBox($"Error filling in table: {ex.Message}");
                }
            }                
            


        }
        public static bool ValidateCell(object value, Constants.DataType dataType, bool isNullable, int column, int row, out string errorMessage)
        {
            errorMessage = string.Empty;

            // 1. Check for nullability
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                if (!isNullable)
                {
                    errorMessage = $"Cell ({column}, {row}) - Value cannot be null.";
                    return false;
                }
                return true;
            }

            // 2. Checking the data type
            switch (dataType)
            {
                case Constants.DataType.Integer:
                    if (!int.TryParse(value.ToString(), out _))
                    {
                        errorMessage = $"Cell ({column}, {row}) - Invalid integer value.";
                        return false;
                    }
                    break;

                case Constants.DataType.Real:
                    if (!double.TryParse(value.ToString(), out _))
                    {
                        errorMessage = $"Cell ({column}, {row}) - Invalid real (floating-point) value.";
                        return false;
                    }
                    break;

                case Constants.DataType.Char:
                    if (value.ToString().Length != 1)
                    {
                        errorMessage = $"Cell ({column}, {row}) - Invalid char value. It should be a single character.";
                        return false;
                    }
                    break;

                case Constants.DataType.String:
                    if (value.ToString().Length > 255)
                    {
                        errorMessage = $"Cell ({column}, {row}) - String length exceeds the maximum allowed length of 255 characters.";
                        return false;
                    }
                    break;

                case Constants.DataType.Time:
                    if (!TimeSpan.TryParseExact(value.ToString(), "hh\\:mm\\:ss", CultureInfo.InvariantCulture, out TimeSpan timeValue))
                    {
                        throw new Exception($"Cell ({column}, {row}) - Invalid time format. Expected format is HH:MM:SS.");
                    }
                    break;

                case Constants.DataType.TimeLvl:
                    // Split a string by the "-" character
                    var timeRangeParts = value.ToString().Split('-');
                    if (timeRangeParts.Length != 2)
                    {
                        throw new Exception($"Cell ({column}, {row}) - Invalid time range format. Expected format is HH:MM:SS - HH:MM:SS.");
                    }

                    // Trying to parse the start and end of a time interval
                    if (!TimeSpan.TryParseExact(timeRangeParts[0], "hh\\:mm\\:ss", CultureInfo.InvariantCulture, out TimeSpan startTime) ||
                        !TimeSpan.TryParseExact(timeRangeParts[1], "hh\\:mm\\:ss", CultureInfo.InvariantCulture, out TimeSpan endTime))
                    {
                        throw new Exception($"Cell ({column}, {row}) - Invalid time range format. Both start and end times must be in HH:MM:SS format.");
                    }

                    // Checking if the start is less than the end
                    if (startTime >= endTime)
                    {
                        throw new Exception($"Cell ({column}, {row}) - Invalid time range in row. Start time must be earlier than end time.");
                    }
                    break;

                default:
                    errorMessage = "Unsupported data type.";
                    return false;
            }

            // 3. All checks passed
            return true;
        }
        #endregion

        private async void OnDeleteTableClick(object sender, EventArgs e)
        {
            ApiService apiService = new ApiService();

            TreeNode selectedNode = treeView.SelectedNode;
            TreeNode parentNode = selectedNode.Parent;
            if (selectedNode != null && parentNode != null && selectedNode.Tag is TableDTO table)
            {
                DialogResult result = MessageBox.Show($"Delete table: {selectedNode.Text}?", "Delete Table", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        await apiService.DeleteTableAsync(parentNode.Text, table.Id);
                        treeView.Nodes.Remove(selectedNode);
                        ClearAndLockDataGridView();
                        AppendMessageToRichTextBox($"Table {selectedNode.Text} has been deleted from {parentNode.Text}.");
                    }
                    catch (Exception ex)
                    {
                        AppendMessageToRichTextBox($"Error deleting table: {ex.Message}");
                    }
                }
            }

        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Check that Ctrl + S are pressed
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;

                // Call the method to save the table
                switch (mode)
                {
                    case Mode.CreateTable:
                        CTRL_S_OnAddTableClick(sender, e);
                        break;
                    case Mode.UpdateTable:
                        CTRL_S_OnUpdateTableClick(sender, e);
                        break;
                    case Mode.FillInTable:
                        CTRL_S_OnFillInTableClick(sender, e);
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion









        /*      HELPER      */
        private void OnClearTextBoxClick(object sender, EventArgs e)
        {
            richTextBox.Clear();
        }
        private async Task OpenDatabase(string dbName)
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
                AppendMessageToRichTextBox($"The database has been opened successfully.");
            }
            catch (Exception ex)
            {
                AppendMessageToRichTextBox($"Error creating database: {ex.Message}");
            }

            
        }

        private async Task SelectDirectory()
        {
            var apiService = new ApiService();

            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a directory to save the database to";

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    Constants.BasePath = $@"{folderDialog.SelectedPath}";

                    var dto = new UpdatePathDTO()
                    {
                        NewBasePath = Constants.BasePath
                    };
                    await apiService.UpdateBasePathAsync(dto);
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
                AppendMessageToRichTextBox($"The specified directory does not exist.");
                return new List<string>();
            }
        }
        private void ClearAndLockDataGridView()
        {
            dataGridView.Rows.Clear();
            dataGridView.Columns.Clear();

            dataGridView.ReadOnly = true;

            dataGridView.AllowUserToAddRows = false;

            dataGridView.EditMode = DataGridViewEditMode.EditProgrammatically;

            dataGridView.Enabled = false;

            mode = Mode.None;

            this.Text = formName;

            dataGridView.RowsAdded -= DataGridView_RowsAdded;
        }
        private void ClearAndUnlockDataGridView()
        {
            dataGridView.Rows.Clear();
            dataGridView.Columns.Clear();

            dataGridView.ReadOnly = false;

            dataGridView.AllowUserToAddRows = true;

            dataGridView.EditMode = DataGridViewEditMode.EditOnKeystroke;

            dataGridView.Enabled = true;

            this.Text = formName;

            dataGridView.RowsAdded -= DataGridView_RowsAdded;
        }

        private void AppendMessageToRichTextBox(string message)
        {
            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(new Action(() => AppendMessageToRichTextBox(message)));
            }
            else
            {
                richTextBox.AppendText(message + Environment.NewLine);

                richTextBox.ScrollToCaret();
            }
        }

        private void DataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.IsNewRow) continue;

                row.Cells["DataType"].Value = Constants.DataType.Integer; // Default value
            }
        }


        public class ActionDialog : Form
        {
            private TextBox txtName;
            private Button btnSave;
            private string action;
            private string entity;
            public string Name => txtName.Text;

            public ActionDialog(string action, string entity)
            {
                this.action = action;
                this.entity = entity;
                InitializeDialogComponents();
            }

            private void InitializeDialogComponents()
            {
                this.Text = $"{action} {entity}";
                this.Size = new System.Drawing.Size(300, 160);
                this.MinimumSize = new System.Drawing.Size(300, 160);
                this.MaximumSize = new System.Drawing.Size(300, 160);
                this.StartPosition = FormStartPosition.CenterParent;

                Label lblName = new Label
                {
                    Text = $"Enter {entity} Name:",
                    AutoSize = true,
                    Location = new System.Drawing.Point(10, 10),
                };

                txtName = new TextBox
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

                this.Controls.Add(lblName);
                this.Controls.Add(txtName);
                this.Controls.Add(btnSave);

                this.AcceptButton = btnSave;
            }
        }

        #region optional methods
        private void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView.Columns[e.ColumnIndex].Name == "DataType")
            {
                var cellValue = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                if (cellValue != null && !Enum.IsDefined(typeof(Constants.DataType), cellValue))
                {
                    MessageBox.Show("Invalid Data Type. Please select a valid type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = null;
                }
            }
        }
        #endregion
    }
}