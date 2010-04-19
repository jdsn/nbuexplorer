namespace NbuExplorer
{
	partial class FormMain
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			this.textBoxLog = new System.Windows.Forms.TextBox();
			this.menuStripMain = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.exportSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportSelectedFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.saveParsingLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPageFileContent = new System.Windows.Forms.TabPage();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.treeViewDirs = new System.Windows.Forms.TreeView();
			this.contextMenuDirs = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.imageListFolder = new System.Windows.Forms.ImageList(this.components);
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.listViewFiles = new System.Windows.Forms.ListView();
			this.colName = new System.Windows.Forms.ColumnHeader();
			this.colSize = new System.Windows.Forms.ColumnHeader();
			this.colTime = new System.Windows.Forms.ColumnHeader();
			this.contextMenuFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.exportSelectedFilesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.sortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.byNameAscendingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.byExtensionAscendingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.bySizeAscendingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.byTimeAscendingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.byNameDescendingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.byExtensionDescendingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.bySizeDescendingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.byTimeDescendingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.imageListFilesLarge = new System.Windows.Forms.ImageList(this.components);
			this.imageListFilesSmall = new System.Windows.Forms.ImageList(this.components);
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.tsPreview = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsLargeIcons = new System.Windows.Forms.ToolStripButton();
			this.tsDetails = new System.Windows.Forms.ToolStripButton();
			this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
			this.textBoxPreview = new System.Windows.Forms.TextBox();
			this.tabPageLog = new System.Windows.Forms.TabPage();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.statusLabelTotal = new System.Windows.Forms.ToolStripStatusLabel();
			this.statusLabelSelected = new System.Windows.Forms.ToolStripStatusLabel();
			this.menuStripMain.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPageFileContent.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.contextMenuDirs.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.contextMenuFiles.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
			this.tabPageLog.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBoxLog
			// 
			this.textBoxLog.BackColor = System.Drawing.SystemColors.Window;
			this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxLog.Location = new System.Drawing.Point(3, 3);
			this.textBoxLog.Multiline = true;
			this.textBoxLog.Name = "textBoxLog";
			this.textBoxLog.ReadOnly = true;
			this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxLog.Size = new System.Drawing.Size(553, 322);
			this.textBoxLog.TabIndex = 1;
			this.textBoxLog.WordWrap = false;
			// 
			// menuStripMain
			// 
			this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.menuStripMain.Location = new System.Drawing.Point(0, 0);
			this.menuStripMain.Name = "menuStripMain";
			this.menuStripMain.Size = new System.Drawing.Size(567, 24);
			this.menuStripMain.TabIndex = 2;
			this.menuStripMain.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.toolStripSeparator2,
            this.exportSelectedFilesToolStripMenuItem,
            this.exportSelectedFolderToolStripMenuItem,
            this.exportAllToolStripMenuItem,
            this.toolStripMenuItem5,
            this.saveParsingLogToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
			this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			this.openToolStripMenuItem.Text = "&Open";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(226, 6);
			// 
			// exportSelectedFilesToolStripMenuItem
			// 
			this.exportSelectedFilesToolStripMenuItem.Name = "exportSelectedFilesToolStripMenuItem";
			this.exportSelectedFilesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.exportSelectedFilesToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			this.exportSelectedFilesToolStripMenuItem.Text = "Export &selected file(s)";
			this.exportSelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedFilesToolStripMenuItem_Click);
			// 
			// exportSelectedFolderToolStripMenuItem
			// 
			this.exportSelectedFolderToolStripMenuItem.Enabled = false;
			this.exportSelectedFolderToolStripMenuItem.Name = "exportSelectedFolderToolStripMenuItem";
			this.exportSelectedFolderToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
			this.exportSelectedFolderToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			this.exportSelectedFolderToolStripMenuItem.Text = "Export selected folder";
			this.exportSelectedFolderToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedFolderToolStripMenuItem_Click);
			// 
			// exportAllToolStripMenuItem
			// 
			this.exportAllToolStripMenuItem.Enabled = false;
			this.exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
			this.exportAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
			this.exportAllToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			this.exportAllToolStripMenuItem.Text = "&Export all";
			this.exportAllToolStripMenuItem.Click += new System.EventHandler(this.exportAllToolStripMenuItem_Click);
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Size = new System.Drawing.Size(226, 6);
			// 
			// saveParsingLogToolStripMenuItem
			// 
			this.saveParsingLogToolStripMenuItem.Enabled = false;
			this.saveParsingLogToolStripMenuItem.Name = "saveParsingLogToolStripMenuItem";
			this.saveParsingLogToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			this.saveParsingLogToolStripMenuItem.Text = "Save parsing log";
			this.saveParsingLogToolStripMenuItem.Click += new System.EventHandler(this.saveParsingLogToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(226, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
			this.helpToolStripMenuItem.Text = "&Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
			this.aboutToolStripMenuItem.Text = "&About...";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPageFileContent);
			this.tabControl1.Controls.Add(this.tabPageLog);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 24);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(567, 354);
			this.tabControl1.TabIndex = 3;
			// 
			// tabPageFileContent
			// 
			this.tabPageFileContent.Controls.Add(this.splitContainer1);
			this.tabPageFileContent.Location = new System.Drawing.Point(4, 22);
			this.tabPageFileContent.Name = "tabPageFileContent";
			this.tabPageFileContent.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageFileContent.Size = new System.Drawing.Size(559, 328);
			this.tabPageFileContent.TabIndex = 1;
			this.tabPageFileContent.Text = "File content";
			this.tabPageFileContent.UseVisualStyleBackColor = true;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(3, 3);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.treeViewDirs);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = new System.Drawing.Size(553, 322);
			this.splitContainer1.SplitterDistance = 183;
			this.splitContainer1.TabIndex = 0;
			// 
			// treeViewDirs
			// 
			this.treeViewDirs.ContextMenuStrip = this.contextMenuDirs;
			this.treeViewDirs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewDirs.HideSelection = false;
			this.treeViewDirs.ImageIndex = 0;
			this.treeViewDirs.ImageList = this.imageListFolder;
			this.treeViewDirs.Location = new System.Drawing.Point(0, 0);
			this.treeViewDirs.Name = "treeViewDirs";
			this.treeViewDirs.SelectedImageIndex = 0;
			this.treeViewDirs.Size = new System.Drawing.Size(183, 322);
			this.treeViewDirs.TabIndex = 0;
			this.treeViewDirs.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewDirs_AfterSelect);
			this.treeViewDirs.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeViewDirs_MouseDown);
			// 
			// contextMenuDirs
			// 
			this.contextMenuDirs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem,
            this.toolStripMenuItem4,
            this.expandAllToolStripMenuItem,
            this.collapseAllToolStripMenuItem});
			this.contextMenuDirs.Name = "contextMenuDirs";
			this.contextMenuDirs.Size = new System.Drawing.Size(139, 76);
			// 
			// exportToolStripMenuItem
			// 
			this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
			this.exportToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
			this.exportToolStripMenuItem.Text = "Export";
			this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedFolderToolStripMenuItem_Click);
			// 
			// toolStripMenuItem4
			// 
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			this.toolStripMenuItem4.Size = new System.Drawing.Size(135, 6);
			// 
			// expandAllToolStripMenuItem
			// 
			this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
			this.expandAllToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
			this.expandAllToolStripMenuItem.Text = "Expand all";
			this.expandAllToolStripMenuItem.Click += new System.EventHandler(this.expandAllToolStripMenuItem_Click);
			// 
			// collapseAllToolStripMenuItem
			// 
			this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
			this.collapseAllToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
			this.collapseAllToolStripMenuItem.Text = "Collapse all";
			this.collapseAllToolStripMenuItem.Click += new System.EventHandler(this.collapseAllToolStripMenuItem_Click);
			// 
			// imageListFolder
			// 
			this.imageListFolder.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListFolder.ImageStream")));
			this.imageListFolder.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListFolder.Images.SetKeyName(0, "drive");
			this.imageListFolder.Images.SetKeyName(1, "fclosed");
			this.imageListFolder.Images.SetKeyName(2, "fopen");
			this.imageListFolder.Images.SetKeyName(3, "bookmarks");
			this.imageListFolder.Images.SetKeyName(4, "calendar");
			this.imageListFolder.Images.SetKeyName(5, "card");
			this.imageListFolder.Images.SetKeyName(6, "contacts");
			this.imageListFolder.Images.SetKeyName(7, "groups");
			this.imageListFolder.Images.SetKeyName(8, "memo");
			this.imageListFolder.Images.SetKeyName(9, "memory");
			this.imageListFolder.Images.SetKeyName(10, "messages");
			this.imageListFolder.Images.SetKeyName(11, "settings");
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.listViewFiles);
			this.splitContainer2.Panel1.Controls.Add(this.toolStrip1);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.pictureBoxPreview);
			this.splitContainer2.Panel2.Controls.Add(this.textBoxPreview);
			this.splitContainer2.Size = new System.Drawing.Size(366, 322);
			this.splitContainer2.SplitterDistance = 161;
			this.splitContainer2.TabIndex = 1;
			// 
			// listViewFiles
			// 
			this.listViewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colSize,
            this.colTime});
			this.listViewFiles.ContextMenuStrip = this.contextMenuFiles;
			this.listViewFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewFiles.FullRowSelect = true;
			this.listViewFiles.HideSelection = false;
			this.listViewFiles.LargeImageList = this.imageListFilesLarge;
			this.listViewFiles.Location = new System.Drawing.Point(0, 25);
			this.listViewFiles.Name = "listViewFiles";
			this.listViewFiles.Size = new System.Drawing.Size(366, 136);
			this.listViewFiles.SmallImageList = this.imageListFilesSmall;
			this.listViewFiles.TabIndex = 0;
			this.listViewFiles.UseCompatibleStateImageBehavior = false;
			this.listViewFiles.View = System.Windows.Forms.View.Details;
			this.listViewFiles.Resize += new System.EventHandler(this.listViewFiles_Resize);
			this.listViewFiles.SelectedIndexChanged += new System.EventHandler(this.listViewFiles_SelectedIndexChanged);
			this.listViewFiles.DoubleClick += new System.EventHandler(this.listViewFiles_DoubleClick);
			this.listViewFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewFiles_ColumnClick);
			// 
			// colName
			// 
			this.colName.Text = "File Name";
			this.colName.Width = 220;
			// 
			// colSize
			// 
			this.colSize.Text = "File Size";
			this.colSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.colSize.Width = 100;
			// 
			// colTime
			// 
			this.colTime.Text = "Date Time";
			this.colTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.colTime.Width = 100;
			// 
			// contextMenuFiles
			// 
			this.contextMenuFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportSelectedFilesToolStripMenuItem1,
            this.toolStripMenuItem3,
            this.sortToolStripMenuItem});
			this.contextMenuFiles.Name = "contextMenuFiles";
			this.contextMenuFiles.Size = new System.Drawing.Size(191, 54);
			// 
			// exportSelectedFilesToolStripMenuItem1
			// 
			this.exportSelectedFilesToolStripMenuItem1.Name = "exportSelectedFilesToolStripMenuItem1";
			this.exportSelectedFilesToolStripMenuItem1.Size = new System.Drawing.Size(190, 22);
			this.exportSelectedFilesToolStripMenuItem1.Text = "Export selected file(s)";
			this.exportSelectedFilesToolStripMenuItem1.Click += new System.EventHandler(this.exportSelectedFilesToolStripMenuItem_Click);
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(187, 6);
			// 
			// sortToolStripMenuItem
			// 
			this.sortToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.byNameAscendingToolStripMenuItem,
            this.byExtensionAscendingToolStripMenuItem,
            this.bySizeAscendingToolStripMenuItem,
            this.byTimeAscendingToolStripMenuItem,
            this.toolStripMenuItem2,
            this.byNameDescendingToolStripMenuItem,
            this.byExtensionDescendingToolStripMenuItem,
            this.bySizeDescendingToolStripMenuItem,
            this.byTimeDescendingToolStripMenuItem});
			this.sortToolStripMenuItem.Name = "sortToolStripMenuItem";
			this.sortToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.sortToolStripMenuItem.Text = "Sort";
			this.sortToolStripMenuItem.DropDownOpening += new System.EventHandler(this.sortToolStripMenuItem_DropDownOpening);
			// 
			// byNameAscendingToolStripMenuItem
			// 
			this.byNameAscendingToolStripMenuItem.Name = "byNameAscendingToolStripMenuItem";
			this.byNameAscendingToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.byNameAscendingToolStripMenuItem.Text = "By name ascending";
			this.byNameAscendingToolStripMenuItem.Click += new System.EventHandler(this.sortToolStripMenuItem_Click);
			// 
			// byExtensionAscendingToolStripMenuItem
			// 
			this.byExtensionAscendingToolStripMenuItem.Name = "byExtensionAscendingToolStripMenuItem";
			this.byExtensionAscendingToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.byExtensionAscendingToolStripMenuItem.Text = "By extension ascending";
			this.byExtensionAscendingToolStripMenuItem.Click += new System.EventHandler(this.sortToolStripMenuItem_Click);
			// 
			// bySizeAscendingToolStripMenuItem
			// 
			this.bySizeAscendingToolStripMenuItem.Name = "bySizeAscendingToolStripMenuItem";
			this.bySizeAscendingToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.bySizeAscendingToolStripMenuItem.Text = "By size ascending";
			this.bySizeAscendingToolStripMenuItem.Click += new System.EventHandler(this.sortToolStripMenuItem_Click);
			// 
			// byTimeAscendingToolStripMenuItem
			// 
			this.byTimeAscendingToolStripMenuItem.Name = "byTimeAscendingToolStripMenuItem";
			this.byTimeAscendingToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.byTimeAscendingToolStripMenuItem.Text = "By time ascending";
			this.byTimeAscendingToolStripMenuItem.Click += new System.EventHandler(this.sortToolStripMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(201, 6);
			// 
			// byNameDescendingToolStripMenuItem
			// 
			this.byNameDescendingToolStripMenuItem.Name = "byNameDescendingToolStripMenuItem";
			this.byNameDescendingToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.byNameDescendingToolStripMenuItem.Text = "By name descending";
			this.byNameDescendingToolStripMenuItem.Click += new System.EventHandler(this.sortToolStripMenuItem_Click);
			// 
			// byExtensionDescendingToolStripMenuItem
			// 
			this.byExtensionDescendingToolStripMenuItem.Name = "byExtensionDescendingToolStripMenuItem";
			this.byExtensionDescendingToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.byExtensionDescendingToolStripMenuItem.Text = "By extension descending";
			this.byExtensionDescendingToolStripMenuItem.Click += new System.EventHandler(this.sortToolStripMenuItem_Click);
			// 
			// bySizeDescendingToolStripMenuItem
			// 
			this.bySizeDescendingToolStripMenuItem.Name = "bySizeDescendingToolStripMenuItem";
			this.bySizeDescendingToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.bySizeDescendingToolStripMenuItem.Text = "By size descending";
			this.bySizeDescendingToolStripMenuItem.Click += new System.EventHandler(this.sortToolStripMenuItem_Click);
			// 
			// byTimeDescendingToolStripMenuItem
			// 
			this.byTimeDescendingToolStripMenuItem.Name = "byTimeDescendingToolStripMenuItem";
			this.byTimeDescendingToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.byTimeDescendingToolStripMenuItem.Text = "By time descending";
			this.byTimeDescendingToolStripMenuItem.Click += new System.EventHandler(this.sortToolStripMenuItem_Click);
			// 
			// imageListFilesLarge
			// 
			this.imageListFilesLarge.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageListFilesLarge.ImageSize = new System.Drawing.Size(32, 32);
			this.imageListFilesLarge.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// imageListFilesSmall
			// 
			this.imageListFilesSmall.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageListFilesSmall.ImageSize = new System.Drawing.Size(16, 16);
			this.imageListFilesSmall.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsPreview,
            this.toolStripSeparator1,
            this.tsLargeIcons,
            this.tsDetails});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(366, 25);
			this.toolStrip1.TabIndex = 1;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// tsPreview
			// 
			this.tsPreview.Checked = true;
			this.tsPreview.CheckOnClick = true;
			this.tsPreview.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsPreview.Image = ((System.Drawing.Image)(resources.GetObject("tsPreview.Image")));
			this.tsPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsPreview.Name = "tsPreview";
			this.tsPreview.Size = new System.Drawing.Size(23, 22);
			this.tsPreview.Text = "Preview";
			this.tsPreview.Click += new System.EventHandler(this.tsPreview_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// tsLargeIcons
			// 
			this.tsLargeIcons.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsLargeIcons.Image = ((System.Drawing.Image)(resources.GetObject("tsLargeIcons.Image")));
			this.tsLargeIcons.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsLargeIcons.Name = "tsLargeIcons";
			this.tsLargeIcons.Size = new System.Drawing.Size(23, 22);
			this.tsLargeIcons.Text = "Large icons";
			this.tsLargeIcons.Click += new System.EventHandler(this.tsLargeIcons_Click);
			// 
			// tsDetails
			// 
			this.tsDetails.Checked = true;
			this.tsDetails.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsDetails.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsDetails.Image = ((System.Drawing.Image)(resources.GetObject("tsDetails.Image")));
			this.tsDetails.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsDetails.Name = "tsDetails";
			this.tsDetails.Size = new System.Drawing.Size(23, 22);
			this.tsDetails.Text = "Details";
			this.tsDetails.Click += new System.EventHandler(this.tsDetails_Click);
			// 
			// pictureBoxPreview
			// 
			this.pictureBoxPreview.BackColor = System.Drawing.SystemColors.Window;
			this.pictureBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBoxPreview.Location = new System.Drawing.Point(0, 0);
			this.pictureBoxPreview.Name = "pictureBoxPreview";
			this.pictureBoxPreview.Size = new System.Drawing.Size(366, 62);
			this.pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBoxPreview.TabIndex = 0;
			this.pictureBoxPreview.TabStop = false;
			// 
			// textBoxPreview
			// 
			this.textBoxPreview.BackColor = System.Drawing.SystemColors.Window;
			this.textBoxPreview.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.textBoxPreview.Location = new System.Drawing.Point(0, 62);
			this.textBoxPreview.Multiline = true;
			this.textBoxPreview.Name = "textBoxPreview";
			this.textBoxPreview.ReadOnly = true;
			this.textBoxPreview.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxPreview.Size = new System.Drawing.Size(366, 95);
			this.textBoxPreview.TabIndex = 1;
			this.textBoxPreview.WordWrap = false;
			// 
			// tabPageLog
			// 
			this.tabPageLog.Controls.Add(this.textBoxLog);
			this.tabPageLog.Location = new System.Drawing.Point(4, 22);
			this.tabPageLog.Name = "tabPageLog";
			this.tabPageLog.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageLog.Size = new System.Drawing.Size(559, 328);
			this.tabPageLog.TabIndex = 0;
			this.tabPageLog.Text = "File parsing log";
			this.tabPageLog.UseVisualStyleBackColor = true;
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabelTotal,
            this.statusLabelSelected});
			this.statusStrip1.Location = new System.Drawing.Point(0, 378);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(567, 22);
			this.statusStrip1.TabIndex = 4;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// statusLabelTotal
			// 
			this.statusLabelTotal.Name = "statusLabelTotal";
			this.statusLabelTotal.Size = new System.Drawing.Size(11, 17);
			this.statusLabelTotal.Text = "-";
			// 
			// statusLabelSelected
			// 
			this.statusLabelSelected.Name = "statusLabelSelected";
			this.statusLabelSelected.Size = new System.Drawing.Size(11, 17);
			this.statusLabelSelected.Text = "-";
			// 
			// FormMain
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(567, 400);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.menuStripMain);
			this.Controls.Add(this.statusStrip1);
			this.MainMenuStrip = this.menuStripMain;
			this.MinimumSize = new System.Drawing.Size(320, 240);
			this.Name = "FormMain";
			this.Text = "NbuExplorer";
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FormMain_DragDrop);
			this.DragOver += new System.Windows.Forms.DragEventHandler(this.FormMain_DragOver);
			this.menuStripMain.ResumeLayout(false);
			this.menuStripMain.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.tabPageFileContent.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.contextMenuDirs.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel1.PerformLayout();
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.Panel2.PerformLayout();
			this.splitContainer2.ResumeLayout(false);
			this.contextMenuFiles.ResumeLayout(false);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
			this.tabPageLog.ResumeLayout(false);
			this.tabPageLog.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBoxLog;
		private System.Windows.Forms.MenuStrip menuStripMain;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPageLog;
		private System.Windows.Forms.TabPage tabPageFileContent;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TreeView treeViewDirs;
		private System.Windows.Forms.ListView listViewFiles;
		private System.Windows.Forms.ImageList imageListFolder;
		private System.Windows.Forms.ImageList imageListFilesLarge;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.PictureBox pictureBoxPreview;
		private System.Windows.Forms.TextBox textBoxPreview;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ColumnHeader colName;
		private System.Windows.Forms.ColumnHeader colSize;
		private System.Windows.Forms.ToolStripButton tsLargeIcons;
		private System.Windows.Forms.ToolStripButton tsDetails;
		private System.Windows.Forms.ImageList imageListFilesSmall;
		private System.Windows.Forms.ToolStripButton tsPreview;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem exportSelectedFilesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ContextMenuStrip contextMenuFiles;
		private System.Windows.Forms.ToolStripMenuItem exportSelectedFilesToolStripMenuItem1;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem sortToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem byNameAscendingToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem bySizeAscendingToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem byNameDescendingToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem bySizeDescendingToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportSelectedFolderToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip contextMenuDirs;
		private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
		private System.Windows.Forms.ToolStripMenuItem byExtensionAscendingToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem byExtensionDescendingToolStripMenuItem;
		private System.Windows.Forms.ColumnHeader colTime;
		private System.Windows.Forms.ToolStripMenuItem byTimeAscendingToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem byTimeDescendingToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
		private System.Windows.Forms.ToolStripMenuItem saveParsingLogToolStripMenuItem;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel statusLabelTotal;
		private System.Windows.Forms.ToolStripStatusLabel statusLabelSelected;
	}
}

