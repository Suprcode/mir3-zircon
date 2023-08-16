using LibraryEditor;

namespace LibraryEditor
{
    partial class LMain
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LMain));
            PreviewListView = new FixedListView();
            ImageList = new System.Windows.Forms.ImageList(components);
            PreviewSubListView = new FixedListView();
            ImageListSubItems = new System.Windows.Forms.ImageList(components);
            MainMenu = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            functionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            countBlanksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeBlanksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            safeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            convertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            skinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPage1 = new System.Windows.Forms.TabPage();
            labelSubImages = new System.Windows.Forms.Label();
            numBlankAdd = new System.Windows.Forms.NumericUpDown();
            BlankAdd = new System.Windows.Forms.Button();
            button5 = new System.Windows.Forms.Button();
            button4 = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            radioButtonImage = new System.Windows.Forms.RadioButton();
            nudJump = new System.Windows.Forms.NumericUpDown();
            radioButtonOverlay = new System.Windows.Forms.RadioButton();
            radioButtonShadow = new System.Windows.Forms.RadioButton();
            label1 = new System.Windows.Forms.Label();
            buttonSkipPrevious = new System.Windows.Forms.Button();
            WidthLabel = new System.Windows.Forms.Label();
            buttonSkipNext = new System.Windows.Forms.Button();
            label6 = new System.Windows.Forms.Label();
            buttonReplace = new System.Windows.Forms.Button();
            HeightLabel = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            ExportButton = new System.Windows.Forms.Button();
            OffSetXTextBox = new System.Windows.Forms.TextBox();
            InsertImageButton = new System.Windows.Forms.Button();
            OffSetYTextBox = new System.Windows.Forms.TextBox();
            DeleteButton = new System.Windows.Forms.Button();
            AddButton = new System.Windows.Forms.Button();
            checkBoxPreventAntiAliasing = new System.Windows.Forms.CheckBox();
            checkBoxQuality = new System.Windows.Forms.CheckBox();
            pictureBox = new System.Windows.Forms.PictureBox();
            ZoomTrackBar = new System.Windows.Forms.TrackBar();
            tabControl3 = new System.Windows.Forms.TabControl();
            tabMainImage = new System.Windows.Forms.TabPage();
            panel = new System.Windows.Forms.Panel();
            ImageBox = new System.Windows.Forms.PictureBox();
            tabSubImages = new System.Windows.Forms.TabPage();
            OpenLibraryDialog = new System.Windows.Forms.OpenFileDialog();
            SaveLibraryDialog = new System.Windows.Forms.SaveFileDialog();
            ImportImageDialog = new System.Windows.Forms.OpenFileDialog();
            OpenWeMadeDialog = new System.Windows.Forms.OpenFileDialog();
            toolTip = new System.Windows.Forms.ToolTip(components);
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            MainMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numBlankAdd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudJump).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ZoomTrackBar).BeginInit();
            tabControl3.SuspendLayout();
            tabMainImage.SuspendLayout();
            panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ImageBox).BeginInit();
            tabSubImages.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // PreviewListView
            // 
            PreviewListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            PreviewListView.BackColor = System.Drawing.Color.GhostWhite;
            PreviewListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            PreviewListView.Dock = System.Windows.Forms.DockStyle.Fill;
            PreviewListView.ForeColor = System.Drawing.Color.FromArgb(142, 152, 156);
            PreviewListView.LargeImageList = ImageList;
            PreviewListView.Location = new System.Drawing.Point(0, 0);
            PreviewListView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            PreviewListView.Name = "PreviewListView";
            PreviewListView.Size = new System.Drawing.Size(1202, 573);
            PreviewListView.TabIndex = 0;
            PreviewListView.UseCompatibleStateImageBehavior = false;
            PreviewListView.VirtualMode = true;
            PreviewListView.RetrieveVirtualItem += PreviewListView_RetrieveVirtualItem;
            PreviewListView.SelectedIndexChanged += PreviewListView_SelectedIndexChanged;
            PreviewListView.VirtualItemsSelectionRangeChanged += PreviewListView_VirtualItemsSelectionRangeChanged;
            // 
            // ImageList
            // 
            ImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            ImageList.ImageSize = new System.Drawing.Size(64, 64);
            ImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // PreviewSubListView
            // 
            PreviewSubListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            PreviewSubListView.BackColor = System.Drawing.Color.GhostWhite;
            PreviewSubListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            PreviewSubListView.Dock = System.Windows.Forms.DockStyle.Fill;
            PreviewSubListView.ForeColor = System.Drawing.Color.FromArgb(142, 152, 156);
            PreviewSubListView.LargeImageList = ImageListSubItems;
            PreviewSubListView.Location = new System.Drawing.Point(4, 3);
            PreviewSubListView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            PreviewSubListView.Name = "PreviewSubListView";
            PreviewSubListView.Size = new System.Drawing.Size(881, 417);
            PreviewSubListView.TabIndex = 1;
            PreviewSubListView.UseCompatibleStateImageBehavior = false;
            PreviewSubListView.VirtualMode = true;
            PreviewSubListView.RetrieveVirtualItem += PreviewSubListView_RetrieveVirtualItem;
            PreviewSubListView.SelectedIndexChanged += PreviewSubListView_SelectedIndexChanged;
            // 
            // ImageListSubItems
            // 
            ImageListSubItems.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            ImageListSubItems.ImageSize = new System.Drawing.Size(64, 64);
            ImageListSubItems.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // MainMenu
            // 
            MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, functionsToolStripMenuItem, skinToolStripMenuItem });
            MainMenu.Location = new System.Drawing.Point(0, 0);
            MainMenu.Name = "MainMenu";
            MainMenu.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            MainMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            MainMenu.Size = new System.Drawing.Size(1204, 24);
            MainMenu.TabIndex = 0;
            MainMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { newToolStripMenuItem, openToolStripMenuItem, toolStripMenuItem1, saveToolStripMenuItem, saveAsToolStripMenuItem, toolStripMenuItem2, closeToolStripMenuItem });
            fileToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("fileToolStripMenuItem.Image");
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            newToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("newToolStripMenuItem.Image");
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            newToolStripMenuItem.Text = "New";
            newToolStripMenuItem.ToolTipText = "New .Lib";
            newToolStripMenuItem.Click += newToolStripMenuItem_Click;
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("openToolStripMenuItem.Image");
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.ToolTipText = "Open Shanda or Wemade files.";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(111, 6);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("saveToolStripMenuItem.Image");
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.ToolTipText = "Saves currently open .Lib";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("saveAsToolStripMenuItem.Image");
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            saveAsToolStripMenuItem.Text = "Save As";
            saveAsToolStripMenuItem.ToolTipText = ".Lib Only.";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(111, 6);
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("closeToolStripMenuItem.Image");
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            closeToolStripMenuItem.Text = "Close";
            closeToolStripMenuItem.ToolTipText = "Exit Application.";
            closeToolStripMenuItem.Click += closeToolStripMenuItem_Click;
            // 
            // functionsToolStripMenuItem
            // 
            functionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { copyToToolStripMenuItem, countBlanksToolStripMenuItem, removeBlanksToolStripMenuItem, convertToolStripMenuItem });
            functionsToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("functionsToolStripMenuItem.Image");
            functionsToolStripMenuItem.Name = "functionsToolStripMenuItem";
            functionsToolStripMenuItem.Size = new System.Drawing.Size(87, 20);
            functionsToolStripMenuItem.Text = "Functions";
            // 
            // copyToToolStripMenuItem
            // 
            copyToToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("copyToToolStripMenuItem.Image");
            copyToToolStripMenuItem.Name = "copyToToolStripMenuItem";
            copyToToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            copyToToolStripMenuItem.Text = "Copy To..";
            copyToToolStripMenuItem.ToolTipText = "Copy to a new .Lib or to the end of an exsisting one.";
            copyToToolStripMenuItem.Visible = false;
            copyToToolStripMenuItem.Click += copyToToolStripMenuItem_Click;
            // 
            // countBlanksToolStripMenuItem
            // 
            countBlanksToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("countBlanksToolStripMenuItem.Image");
            countBlanksToolStripMenuItem.Name = "countBlanksToolStripMenuItem";
            countBlanksToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            countBlanksToolStripMenuItem.Text = "Count Blanks";
            countBlanksToolStripMenuItem.ToolTipText = "Counts the blank images in the .Lib";
            countBlanksToolStripMenuItem.Visible = false;
            countBlanksToolStripMenuItem.Click += countBlanksToolStripMenuItem_Click;
            // 
            // removeBlanksToolStripMenuItem
            // 
            removeBlanksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { safeToolStripMenuItem });
            removeBlanksToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("removeBlanksToolStripMenuItem.Image");
            removeBlanksToolStripMenuItem.Name = "removeBlanksToolStripMenuItem";
            removeBlanksToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            removeBlanksToolStripMenuItem.Text = "Remove Blanks";
            removeBlanksToolStripMenuItem.ToolTipText = "Quick removal of blanks.";
            removeBlanksToolStripMenuItem.Click += removeBlanksToolStripMenuItem_Click;
            // 
            // safeToolStripMenuItem
            // 
            safeToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("safeToolStripMenuItem.Image");
            safeToolStripMenuItem.Name = "safeToolStripMenuItem";
            safeToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            safeToolStripMenuItem.Text = "Safe";
            safeToolStripMenuItem.ToolTipText = "Use the safe method of removing blanks.";
            safeToolStripMenuItem.Click += safeToolStripMenuItem_Click;
            // 
            // convertToolStripMenuItem
            // 
            convertToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("convertToolStripMenuItem.Image");
            convertToolStripMenuItem.Name = "convertToolStripMenuItem";
            convertToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            convertToolStripMenuItem.Text = "Converter";
            convertToolStripMenuItem.ToolTipText = "Convert Wil/Wzl/Miz to .Lib";
            convertToolStripMenuItem.Click += convertToolStripMenuItem_Click;
            // 
            // skinToolStripMenuItem
            // 
            skinToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            skinToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("skinToolStripMenuItem.Image");
            skinToolStripMenuItem.Name = "skinToolStripMenuItem";
            skinToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            skinToolStripMenuItem.Text = "Skin";
            skinToolStripMenuItem.Visible = false;
            // 
            // splitContainer1
            // 
            splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 24);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.MinimumSize = new System.Drawing.Size(740, 1033);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer2);
            splitContainer1.Panel1MinSize = 271;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.AutoScrollMargin = new System.Drawing.Size(5, 5);
            splitContainer1.Panel2.Controls.Add(PreviewListView);
            splitContainer1.Panel2.Margin = new System.Windows.Forms.Padding(1);
            splitContainer1.Size = new System.Drawing.Size(1204, 1033);
            splitContainer1.SplitterDistance = 453;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainer2.IsSplitterFixed = true;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(tabControl1);
            splitContainer2.Panel1.Controls.Add(checkBoxPreventAntiAliasing);
            splitContainer2.Panel1.Controls.Add(checkBoxQuality);
            splitContainer2.Panel1.Controls.Add(pictureBox);
            splitContainer2.Panel1.Controls.Add(ZoomTrackBar);
            splitContainer2.Panel1.ForeColor = System.Drawing.Color.Black;
            splitContainer2.Panel1MinSize = 270;
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(tabControl3);
            splitContainer2.Size = new System.Drawing.Size(1204, 453);
            splitContainer2.SplitterDistance = 300;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 0;
            splitContainer2.SplitterMoved += splitContainer2_SplitterMoved;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Location = new System.Drawing.Point(4, 2);
            tabControl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(290, 368);
            tabControl1.TabIndex = 22;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(labelSubImages);
            tabPage1.Controls.Add(numBlankAdd);
            tabPage1.Controls.Add(BlankAdd);
            tabPage1.Controls.Add(button5);
            tabPage1.Controls.Add(button4);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(radioButtonImage);
            tabPage1.Controls.Add(nudJump);
            tabPage1.Controls.Add(radioButtonOverlay);
            tabPage1.Controls.Add(radioButtonShadow);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(buttonSkipPrevious);
            tabPage1.Controls.Add(WidthLabel);
            tabPage1.Controls.Add(buttonSkipNext);
            tabPage1.Controls.Add(label6);
            tabPage1.Controls.Add(buttonReplace);
            tabPage1.Controls.Add(HeightLabel);
            tabPage1.Controls.Add(label8);
            tabPage1.Controls.Add(label10);
            tabPage1.Controls.Add(ExportButton);
            tabPage1.Controls.Add(OffSetXTextBox);
            tabPage1.Controls.Add(InsertImageButton);
            tabPage1.Controls.Add(OffSetYTextBox);
            tabPage1.Controls.Add(DeleteButton);
            tabPage1.Controls.Add(AddButton);
            tabPage1.Location = new System.Drawing.Point(4, 24);
            tabPage1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabPage1.Size = new System.Drawing.Size(282, 340);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Layer & Info";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // labelSubImages
            // 
            labelSubImages.AutoSize = true;
            labelSubImages.Location = new System.Drawing.Point(7, 112);
            labelSubImages.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelSubImages.Name = "labelSubImages";
            labelSubImages.Size = new System.Drawing.Size(77, 15);
            labelSubImages.TabIndex = 27;
            labelSubImages.Text = "SubImages: 0";
            // 
            // numBlankAdd
            // 
            numBlankAdd.Location = new System.Drawing.Point(4, 254);
            numBlankAdd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numBlankAdd.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            numBlankAdd.Name = "numBlankAdd";
            numBlankAdd.Size = new System.Drawing.Size(76, 23);
            numBlankAdd.TabIndex = 26;
            numBlankAdd.ValueChanged += numBlankAdd_ValueChanged;
            // 
            // BlankAdd
            // 
            BlankAdd.Enabled = false;
            BlankAdd.ForeColor = System.Drawing.SystemColors.ControlText;
            BlankAdd.Image = (System.Drawing.Image)resources.GetObject("BlankAdd.Image");
            BlankAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            BlankAdd.Location = new System.Drawing.Point(4, 217);
            BlankAdd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BlankAdd.Name = "BlankAdd";
            BlankAdd.Size = new System.Drawing.Size(76, 30);
            BlankAdd.TabIndex = 25;
            BlankAdd.Tag = "";
            BlankAdd.Text = "B Add";
            BlankAdd.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            BlankAdd.UseVisualStyleBackColor = true;
            BlankAdd.Click += BlankAdd_Click;
            // 
            // button5
            // 
            button5.ForeColor = System.Drawing.SystemColors.ControlText;
            button5.Image = (System.Drawing.Image)resources.GetObject("button5.Image");
            button5.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            button5.Location = new System.Drawing.Point(164, 217);
            button5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(90, 30);
            button5.TabIndex = 24;
            button5.Tag = "";
            button5.Text = "Import All";
            button5.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // button4
            // 
            button4.ForeColor = System.Drawing.SystemColors.ControlText;
            button4.Image = (System.Drawing.Image)resources.GetObject("button4.Image");
            button4.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            button4.Location = new System.Drawing.Point(163, 180);
            button4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(90, 30);
            button4.TabIndex = 23;
            button4.Tag = "";
            button4.Text = "Export All";
            button4.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(7, 3);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(38, 15);
            label2.TabIndex = 22;
            label2.Text = "Layer:";
            // 
            // radioButtonImage
            // 
            radioButtonImage.AutoSize = true;
            radioButtonImage.Checked = true;
            radioButtonImage.Enabled = false;
            radioButtonImage.Location = new System.Drawing.Point(9, 24);
            radioButtonImage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButtonImage.Name = "radioButtonImage";
            radioButtonImage.Size = new System.Drawing.Size(58, 19);
            radioButtonImage.TabIndex = 0;
            radioButtonImage.TabStop = true;
            radioButtonImage.Text = "Image";
            radioButtonImage.UseVisualStyleBackColor = true;
            radioButtonImage.CheckedChanged += radioButtonImage_CheckedChanged;
            // 
            // nudJump
            // 
            nudJump.Location = new System.Drawing.Point(93, 293);
            nudJump.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            nudJump.Maximum = new decimal(new int[] { 650000, 0, 0, 0 });
            nudJump.Name = "nudJump";
            nudJump.Size = new System.Drawing.Size(76, 23);
            nudJump.TabIndex = 21;
            nudJump.ValueChanged += nudJump_ValueChanged;
            nudJump.KeyDown += nudJump_KeyDown;
            // 
            // radioButtonOverlay
            // 
            radioButtonOverlay.AutoSize = true;
            radioButtonOverlay.Enabled = false;
            radioButtonOverlay.Location = new System.Drawing.Point(9, 77);
            radioButtonOverlay.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButtonOverlay.Name = "radioButtonOverlay";
            radioButtonOverlay.Size = new System.Drawing.Size(65, 19);
            radioButtonOverlay.TabIndex = 2;
            radioButtonOverlay.Text = "Overlay";
            radioButtonOverlay.UseVisualStyleBackColor = true;
            radioButtonOverlay.CheckedChanged += radioButtonOverlay_CheckedChanged;
            // 
            // radioButtonShadow
            // 
            radioButtonShadow.AutoSize = true;
            radioButtonShadow.Enabled = false;
            radioButtonShadow.Location = new System.Drawing.Point(9, 51);
            radioButtonShadow.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButtonShadow.Name = "radioButtonShadow";
            radioButtonShadow.Size = new System.Drawing.Size(67, 19);
            radioButtonShadow.TabIndex = 1;
            radioButtonShadow.Text = "Shadow";
            radioButtonShadow.UseVisualStyleBackColor = true;
            radioButtonShadow.CheckedChanged += radioButtonShadow_CheckedChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = System.Drawing.SystemColors.ControlText;
            label1.Location = new System.Drawing.Point(112, 3);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(42, 15);
            label1.TabIndex = 7;
            label1.Text = "Width:";
            // 
            // buttonSkipPrevious
            // 
            buttonSkipPrevious.ForeColor = System.Drawing.SystemColors.ControlText;
            buttonSkipPrevious.Image = (System.Drawing.Image)resources.GetObject("buttonSkipPrevious.Image");
            buttonSkipPrevious.Location = new System.Drawing.Point(50, 290);
            buttonSkipPrevious.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonSkipPrevious.Name = "buttonSkipPrevious";
            buttonSkipPrevious.Size = new System.Drawing.Size(34, 30);
            buttonSkipPrevious.TabIndex = 17;
            buttonSkipPrevious.Tag = "";
            buttonSkipPrevious.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            buttonSkipPrevious.UseVisualStyleBackColor = true;
            buttonSkipPrevious.Click += buttonSkipPrevious_Click;
            // 
            // WidthLabel
            // 
            WidthLabel.AutoSize = true;
            WidthLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            WidthLabel.Location = new System.Drawing.Point(163, 3);
            WidthLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            WidthLabel.Name = "WidthLabel";
            WidthLabel.Size = new System.Drawing.Size(75, 15);
            WidthLabel.TabIndex = 8;
            WidthLabel.Text = "<No Image>";
            // 
            // buttonSkipNext
            // 
            buttonSkipNext.ForeColor = System.Drawing.SystemColors.ControlText;
            buttonSkipNext.Image = (System.Drawing.Image)resources.GetObject("buttonSkipNext.Image");
            buttonSkipNext.Location = new System.Drawing.Point(176, 290);
            buttonSkipNext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonSkipNext.Name = "buttonSkipNext";
            buttonSkipNext.Size = new System.Drawing.Size(35, 30);
            buttonSkipNext.TabIndex = 16;
            buttonSkipNext.Tag = "";
            buttonSkipNext.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            buttonSkipNext.UseVisualStyleBackColor = true;
            buttonSkipNext.Click += buttonSkipNext_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.ForeColor = System.Drawing.SystemColors.ControlText;
            label6.Location = new System.Drawing.Point(108, 24);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(46, 15);
            label6.TabIndex = 9;
            label6.Text = "Height:";
            // 
            // buttonReplace
            // 
            buttonReplace.ForeColor = System.Drawing.SystemColors.ControlText;
            buttonReplace.Image = (System.Drawing.Image)resources.GetObject("buttonReplace.Image");
            buttonReplace.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            buttonReplace.Location = new System.Drawing.Point(163, 144);
            buttonReplace.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonReplace.Name = "buttonReplace";
            buttonReplace.Size = new System.Drawing.Size(91, 29);
            buttonReplace.TabIndex = 15;
            buttonReplace.Tag = "";
            buttonReplace.Text = "Replace";
            buttonReplace.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            buttonReplace.UseVisualStyleBackColor = true;
            buttonReplace.Click += buttonReplace_Click;
            // 
            // HeightLabel
            // 
            HeightLabel.AutoSize = true;
            HeightLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            HeightLabel.Location = new System.Drawing.Point(163, 24);
            HeightLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            HeightLabel.Name = "HeightLabel";
            HeightLabel.Size = new System.Drawing.Size(75, 15);
            HeightLabel.TabIndex = 10;
            HeightLabel.Text = "<No Image>";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.ForeColor = System.Drawing.SystemColors.ControlText;
            label8.Location = new System.Drawing.Point(98, 51);
            label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(53, 15);
            label8.TabIndex = 11;
            label8.Text = "OffSet X:";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.ForeColor = System.Drawing.SystemColors.ControlText;
            label10.Location = new System.Drawing.Point(98, 81);
            label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(53, 15);
            label10.TabIndex = 12;
            label10.Text = "OffSet Y:";
            // 
            // ExportButton
            // 
            ExportButton.ForeColor = System.Drawing.SystemColors.ControlText;
            ExportButton.Image = (System.Drawing.Image)resources.GetObject("ExportButton.Image");
            ExportButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            ExportButton.Location = new System.Drawing.Point(84, 180);
            ExportButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ExportButton.Name = "ExportButton";
            ExportButton.Size = new System.Drawing.Size(76, 30);
            ExportButton.TabIndex = 3;
            ExportButton.Tag = "";
            ExportButton.Text = "Export";
            ExportButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            ExportButton.UseVisualStyleBackColor = true;
            ExportButton.Click += ExportButton_Click;
            // 
            // OffSetXTextBox
            // 
            OffSetXTextBox.Location = new System.Drawing.Point(163, 47);
            OffSetXTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            OffSetXTextBox.Name = "OffSetXTextBox";
            OffSetXTextBox.Size = new System.Drawing.Size(75, 23);
            OffSetXTextBox.TabIndex = 5;
            OffSetXTextBox.TextChanged += OffSetXTextBox_TextChanged;
            // 
            // InsertImageButton
            // 
            InsertImageButton.ForeColor = System.Drawing.SystemColors.ControlText;
            InsertImageButton.Image = (System.Drawing.Image)resources.GetObject("InsertImageButton.Image");
            InsertImageButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            InsertImageButton.Location = new System.Drawing.Point(84, 143);
            InsertImageButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            InsertImageButton.Name = "InsertImageButton";
            InsertImageButton.Size = new System.Drawing.Size(76, 30);
            InsertImageButton.TabIndex = 1;
            InsertImageButton.Tag = "";
            InsertImageButton.Text = "Insert";
            InsertImageButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            InsertImageButton.UseVisualStyleBackColor = true;
            InsertImageButton.Click += InsertImageButton_Click;
            // 
            // OffSetYTextBox
            // 
            OffSetYTextBox.Location = new System.Drawing.Point(163, 77);
            OffSetYTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            OffSetYTextBox.Name = "OffSetYTextBox";
            OffSetYTextBox.Size = new System.Drawing.Size(75, 23);
            OffSetYTextBox.TabIndex = 6;
            OffSetYTextBox.TextChanged += OffSetYTextBox_TextChanged;
            // 
            // DeleteButton
            // 
            DeleteButton.ForeColor = System.Drawing.SystemColors.ControlText;
            DeleteButton.Image = (System.Drawing.Image)resources.GetObject("DeleteButton.Image");
            DeleteButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            DeleteButton.Location = new System.Drawing.Point(4, 180);
            DeleteButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            DeleteButton.Name = "DeleteButton";
            DeleteButton.Size = new System.Drawing.Size(76, 30);
            DeleteButton.TabIndex = 2;
            DeleteButton.Tag = "";
            DeleteButton.Text = "Delete";
            DeleteButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            DeleteButton.UseVisualStyleBackColor = true;
            DeleteButton.Click += DeleteButton_Click;
            // 
            // AddButton
            // 
            AddButton.ForeColor = System.Drawing.SystemColors.ControlText;
            AddButton.Image = (System.Drawing.Image)resources.GetObject("AddButton.Image");
            AddButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            AddButton.Location = new System.Drawing.Point(4, 143);
            AddButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            AddButton.Name = "AddButton";
            AddButton.Size = new System.Drawing.Size(76, 30);
            AddButton.TabIndex = 0;
            AddButton.Tag = "";
            AddButton.Text = "Add";
            AddButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            AddButton.UseVisualStyleBackColor = true;
            AddButton.Click += AddButton_Click;
            // 
            // checkBoxPreventAntiAliasing
            // 
            checkBoxPreventAntiAliasing.AutoSize = true;
            checkBoxPreventAntiAliasing.Location = new System.Drawing.Point(15, 377);
            checkBoxPreventAntiAliasing.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxPreventAntiAliasing.Name = "checkBoxPreventAntiAliasing";
            checkBoxPreventAntiAliasing.Size = new System.Drawing.Size(95, 19);
            checkBoxPreventAntiAliasing.TabIndex = 20;
            checkBoxPreventAntiAliasing.Text = "No Anti-alias";
            checkBoxPreventAntiAliasing.UseVisualStyleBackColor = true;
            checkBoxPreventAntiAliasing.CheckedChanged += checkBoxPreventAntiAliasing_CheckedChanged;
            // 
            // checkBoxQuality
            // 
            checkBoxQuality.AutoSize = true;
            checkBoxQuality.Location = new System.Drawing.Point(139, 377);
            checkBoxQuality.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxQuality.Name = "checkBoxQuality";
            checkBoxQuality.Size = new System.Drawing.Size(87, 19);
            checkBoxQuality.TabIndex = 19;
            checkBoxQuality.Text = "No Blurring";
            checkBoxQuality.UseVisualStyleBackColor = true;
            checkBoxQuality.CheckedChanged += checkBoxQuality_CheckedChanged;
            // 
            // pictureBox
            // 
            pictureBox.Image = (System.Drawing.Image)resources.GetObject("pictureBox.Image");
            pictureBox.Location = new System.Drawing.Point(251, 377);
            pictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new System.Drawing.Size(16, 16);
            pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            pictureBox.TabIndex = 14;
            pictureBox.TabStop = false;
            toolTip.SetToolTip(pictureBox, "Switch from Black to White background.");
            pictureBox.Click += pictureBox_Click;
            // 
            // ZoomTrackBar
            // 
            ZoomTrackBar.LargeChange = 1;
            ZoomTrackBar.Location = new System.Drawing.Point(4, 404);
            ZoomTrackBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ZoomTrackBar.Minimum = 1;
            ZoomTrackBar.Name = "ZoomTrackBar";
            ZoomTrackBar.Size = new System.Drawing.Size(286, 45);
            ZoomTrackBar.TabIndex = 4;
            ZoomTrackBar.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            toolTip.SetToolTip(ZoomTrackBar, "Adjust Zoom");
            ZoomTrackBar.Value = 1;
            ZoomTrackBar.Scroll += ZoomTrackBar_Scroll;
            // 
            // tabControl3
            // 
            tabControl3.Controls.Add(tabMainImage);
            tabControl3.Controls.Add(tabSubImages);
            tabControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl3.Location = new System.Drawing.Point(0, 0);
            tabControl3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabControl3.Multiline = true;
            tabControl3.Name = "tabControl3";
            tabControl3.SelectedIndex = 0;
            tabControl3.Size = new System.Drawing.Size(897, 451);
            tabControl3.TabIndex = 2;
            tabControl3.SelectedIndexChanged += TabControl3SelectedIndexChanged;
            // 
            // tabMainImage
            // 
            tabMainImage.Controls.Add(panel);
            tabMainImage.Location = new System.Drawing.Point(4, 24);
            tabMainImage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabMainImage.Name = "tabMainImage";
            tabMainImage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabMainImage.Size = new System.Drawing.Size(889, 423);
            tabMainImage.TabIndex = 0;
            tabMainImage.Text = "MainImage";
            tabMainImage.UseVisualStyleBackColor = true;
            // 
            // panel
            // 
            panel.AutoScroll = true;
            panel.BackColor = System.Drawing.Color.White;
            panel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            panel.Controls.Add(ImageBox);
            panel.Dock = System.Windows.Forms.DockStyle.Fill;
            panel.Location = new System.Drawing.Point(4, 3);
            panel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panel.Name = "panel";
            panel.Size = new System.Drawing.Size(881, 417);
            panel.TabIndex = 2;
            // 
            // ImageBox
            // 
            ImageBox.BackColor = System.Drawing.Color.Transparent;
            ImageBox.Location = new System.Drawing.Point(0, 0);
            ImageBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ImageBox.Name = "ImageBox";
            ImageBox.Size = new System.Drawing.Size(64, 64);
            ImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            ImageBox.TabIndex = 0;
            ImageBox.TabStop = false;
            // 
            // tabSubImages
            // 
            tabSubImages.Controls.Add(PreviewSubListView);
            tabSubImages.Location = new System.Drawing.Point(4, 24);
            tabSubImages.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabSubImages.Name = "tabSubImages";
            tabSubImages.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabSubImages.Size = new System.Drawing.Size(889, 423);
            tabSubImages.TabIndex = 1;
            tabSubImages.Text = "Sub Images";
            tabSubImages.UseVisualStyleBackColor = true;
            // 
            // OpenLibraryDialog
            // 
            OpenLibraryDialog.Filter = "Zircon Library|*.Zl";
            OpenLibraryDialog.FileOk += OpenLibraryDialog_FileOk;
            // 
            // SaveLibraryDialog
            // 
            SaveLibraryDialog.Filter = "Zircon Library|*.Zl";
            // 
            // ImportImageDialog
            // 
            ImportImageDialog.Filter = "Images (*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*";
            ImportImageDialog.Multiselect = true;
            // 
            // OpenWeMadeDialog
            // 
            OpenWeMadeDialog.Filter = "WeMade|*.Wil;*.Wtl|Shanda|*.Wzl;*.Miz|Lib|*.Lib|Marble|*.Mil|Saphire|*.DXl|ApocV2|*.ALib";
            OpenWeMadeDialog.Multiselect = true;
            // 
            // statusStrip
            // 
            statusStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabel, toolStripProgressBar });
            statusStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            statusStrip.Location = new System.Drawing.Point(0, 656);
            statusStrip.Name = "statusStrip";
            statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            statusStrip.Size = new System.Drawing.Size(1204, 24);
            statusStrip.TabIndex = 2;
            statusStrip.Text = "statusStrip";
            // 
            // toolStripStatusLabel
            // 
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Size = new System.Drawing.Size(90, 19);
            toolStripStatusLabel.Text = "Selected Image:";
            // 
            // toolStripProgressBar
            // 
            toolStripProgressBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripProgressBar.Name = "toolStripProgressBar";
            toolStripProgressBar.Size = new System.Drawing.Size(233, 18);
            toolStripProgressBar.Step = 1;
            toolStripProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // LMain
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1204, 680);
            Controls.Add(statusStrip);
            Controls.Add(splitContainer1);
            Controls.Add(MainMenu);
            DoubleBuffered = true;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = MainMenu;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MinimumSize = new System.Drawing.Size(756, 513);
            Name = "LMain";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Zircon Library Editor";
            Resize += LMain_Resize;
            MainMenu.ResumeLayout(false);
            MainMenu.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel1.PerformLayout();
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numBlankAdd).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudJump).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)ZoomTrackBar).EndInit();
            tabControl3.ResumeLayout(false);
            tabMainImage.ResumeLayout(false);
            panel.ResumeLayout(false);
            panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)ImageBox).EndInit();
            tabSubImages.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private FixedListView PreviewListView;
        private System.Windows.Forms.ImageList ImageList;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.Label HeightLabel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label WidthLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.OpenFileDialog OpenLibraryDialog;
        private System.Windows.Forms.SaveFileDialog SaveLibraryDialog;
        private System.Windows.Forms.OpenFileDialog ImportImageDialog;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.OpenFileDialog OpenWeMadeDialog;
        private System.Windows.Forms.ToolStripMenuItem functionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem convertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeBlanksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem countBlanksToolStripMenuItem;
        private System.Windows.Forms.TextBox OffSetYTextBox;
        private System.Windows.Forms.TextBox OffSetXTextBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button InsertImageButton;
        private System.Windows.Forms.ToolStripMenuItem safeToolStripMenuItem;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.TrackBar ZoomTrackBar;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.ToolStripMenuItem skinToolStripMenuItem;
        private System.Windows.Forms.Button buttonReplace;
        private System.Windows.Forms.Button buttonSkipPrevious;
        private System.Windows.Forms.Button buttonSkipNext;
        private System.Windows.Forms.CheckBox checkBoxQuality;
        private System.Windows.Forms.CheckBox checkBoxPreventAntiAliasing;
        private System.Windows.Forms.NumericUpDown nudJump;
        private System.Windows.Forms.RadioButton radioButtonOverlay;
        private System.Windows.Forms.RadioButton radioButtonShadow;
        private System.Windows.Forms.RadioButton radioButtonImage;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button BlankAdd;
        private System.Windows.Forms.NumericUpDown numBlankAdd;
        private System.Windows.Forms.TabControl tabControl3;
        private System.Windows.Forms.TabPage tabMainImage;
        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.PictureBox ImageBox;
        private System.Windows.Forms.TabPage tabSubImages;
        private FixedListView PreviewSubListView;
        private System.Windows.Forms.ImageList ImageListSubItems;
        private System.Windows.Forms.Label labelSubImages;
    }
}

