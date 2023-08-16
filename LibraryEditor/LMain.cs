using LibraryEditor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibraryEditor
{
    public partial class LMain : Form
    {
        private readonly Dictionary<int, int> _indexList = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _subIndexList = new Dictionary<int, int>();
        private Mir3Library _library;
        private Mir3Library.Mir3Image _selectedImage, _exportImage;
        private Image _originalImage;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public LMain()
        {
            InitializeComponent();

            SendMessage(PreviewListView.Handle, 4149, 0, 5242946); //80 x 66
            SendMessage(PreviewSubListView.Handle, 4149, 0, 5242946); //80 x 66

            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
            this.Text = MakeTitle(string.Empty);
        }

        private string MakeTitle(string LibName)
        {
            string appTitle = "Zircon Library Editor [All in One Converter]";
            if (!string.IsNullOrEmpty(LibName))
                return $"{appTitle} -> [{Path.GetFileName(LibName)}]";
            else
                return $"{appTitle}";
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            return; //Not added yet

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (Path.GetExtension(files[0]).ToUpper() == ".WIL" ||
                Path.GetExtension(files[0]).ToUpper() == ".WZL" ||
                Path.GetExtension(files[0]).ToUpper() == ".MIZ")
            {
                try
                {
                    ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 8 };
                    Parallel.For(0, files.Length, options, i =>
                    {
                        if (Path.GetExtension(files[i]) == ".wtl")
                        {
                            WTLLibrary WTLlib = new WTLLibrary(files[i]);
                            WTLlib.ToMLibrary();
                        }
                        else
                        {
                            WeMadeLibrary WILlib = new WeMadeLibrary(files[i]);
                            WILlib.ToMLibrary();
                        }
                        toolStripProgressBar.Value++;
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                toolStripProgressBar.Value = 0;

                MessageBox.Show(
                    string.Format("Successfully converted {0} {1}",
                    (OpenWeMadeDialog.FileNames.Length).ToString(),
                    (OpenWeMadeDialog.FileNames.Length > 1) ? "libraries" : "library"));
            }
            else if (Path.GetExtension(files[0]).ToUpper() == ".LIB")
            {
                ClearInterface();
                ImageList.Images.Clear();
                PreviewListView.Items.Clear();

                ImageListSubItems.Images.Clear();
                PreviewSubListView.Items.Clear();

                _indexList.Clear();

                if (_library != null) _library.Close();
                //_library = new MLibraryV2(files[0]);
                //PreviewListView.VirtualListSize = _library.Images.Count;
                PreviewListView.RedrawItems(0, PreviewListView.Items.Count - 1, true);

                // Show .Lib path in application title.
                this.Text = MakeTitle(files[0].ToString());
            }
            else
            {
                return;
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void ClearInterface()
        {
            _selectedImage = null;
            ImageBox.Image = null;
            ZoomTrackBar.Value = 1;

            WidthLabel.Text = "<No Image>";
            HeightLabel.Text = "<No Image>";
            OffSetXTextBox.Text = string.Empty;
            OffSetYTextBox.Text = string.Empty;
            OffSetXTextBox.BackColor = SystemColors.Window;
            OffSetYTextBox.BackColor = SystemColors.Window;

            labelSubImages.Text = "SubImages: 0";

        }

        private void PreviewListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PreviewListView.SelectedIndices.Count == 0)
            {
                ClearInterface();
                return;
            }

            _selectedImage = _library.GetImage(PreviewListView.SelectedIndices[0]);

            if (_selectedImage == null)
            {
                ClearInterface();
                return;
            }

            labelSubImages.Text = $"SubImages: {_selectedImage.SubItems.Count}";

            _subIndexList.Clear();
            ImageListSubItems.Images.Clear();
            PreviewSubListView.Items.Clear();

            PreviewSubListView.VirtualListSize = 0;
            PreviewSubListView.VirtualListSize = _selectedImage.SubItems.Count;

            if (radioButtonImage.Checked)
            {
                WidthLabel.Text = _selectedImage.Width.ToString();
                HeightLabel.Text = _selectedImage.Height.ToString();

                OffSetXTextBox.Text = _selectedImage.OffSetX.ToString();
                OffSetYTextBox.Text = _selectedImage.OffSetY.ToString();

                ImageBox.Image = _selectedImage.Image;
            }
            else if (radioButtonShadow.Checked)
            {
                WidthLabel.Text = _selectedImage.ShadowWidth.ToString();
                HeightLabel.Text = _selectedImage.ShadowHeight.ToString();

                OffSetXTextBox.Text = _selectedImage.ShadowOffSetX.ToString();
                OffSetYTextBox.Text = _selectedImage.ShadowOffSetY.ToString();

                ImageBox.Image = _selectedImage.ShadowImage;
            }
            if (radioButtonOverlay.Checked)
            {
                WidthLabel.Text = _selectedImage.OverlayWidth.ToString();
                HeightLabel.Text = _selectedImage.OverlayHeight.ToString();

                OffSetXTextBox.Text = _selectedImage.OffSetX.ToString();
                OffSetYTextBox.Text = _selectedImage.OffSetY.ToString();

                ImageBox.Image = _selectedImage.OverlayImage;
            }

            // Keep track of what image/s are selected.
            if (PreviewListView.SelectedIndices.Count > 1)
            {
                toolStripStatusLabel.ForeColor = Color.Red;
                toolStripStatusLabel.Text = "Multiple images selected.";
            }
            else
            {
                toolStripStatusLabel.ForeColor = SystemColors.ControlText;
                toolStripStatusLabel.Text = "Selected Image: " + string.Format("{0} / {1}",
                PreviewListView.SelectedIndices[0].ToString(),
                (PreviewListView.Items.Count - 1).ToString());
            }

            nudJump.Value = PreviewListView.SelectedIndices[0];
        }

        private void PreviewListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            int index;

            if (_indexList.TryGetValue(e.ItemIndex, out index))
            {
                e.Item = new ListViewItem { ImageIndex = index, Text = e.ItemIndex.ToString() };
                return;
            }

            _indexList.Add(e.ItemIndex, ImageList.Images.Count);
            if (radioButtonImage.Checked)
                ImageList.Images.Add(_library.GetPreview(e.ItemIndex, ImageType.Image));
            else if (radioButtonShadow.Checked)
                ImageList.Images.Add(_library.GetPreview(e.ItemIndex, ImageType.Shadow));
            else if (radioButtonOverlay.Checked)
                ImageList.Images.Add(_library.GetPreview(e.ItemIndex, ImageType.Overlay));

            e.Item = new ListViewItem { ImageIndex = index, Text = e.ItemIndex.ToString() };

        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (_library == null) return;
            if (_library._fileName == null) return;

            if (ImportImageDialog.ShowDialog() != DialogResult.OK) return;

            List<string> fileNames = new List<string>(ImportImageDialog.FileNames);

            //fileNames.Sort();
            toolStripProgressBar.Value = 0;
            toolStripProgressBar.Maximum = fileNames.Count;

            for (int i = 0; i < fileNames.Count; i++)
            {
                string fileName = fileNames[i];
                Bitmap image;

                try
                {
                    image = new Bitmap(fileName);
                }
                catch
                {
                    continue;
                }

                fileName = Path.Combine(Path.GetDirectoryName(fileName), "Placements", Path.GetFileNameWithoutExtension(fileName));
                fileName = Path.ChangeExtension(fileName, ".txt");

                short x = 0;
                short y = 0;

                if (File.Exists(fileName))
                {
                    string[] placements = File.ReadAllLines(fileName);

                    if (placements.Length > 0)
                        short.TryParse(placements[0], out x);
                    if (placements.Length > 1)
                        short.TryParse(placements[1], out y);
                }

                if (radioButtonImage.Checked)
                    _library.AddImage(image, x, y);
                else if (radioButtonShadow.Checked)
                    _library.AddShadow(image, x, y);
                else if (radioButtonOverlay.Checked)
                    _library.AddOverlay(image, x, y);

                toolStripProgressBar.Value++;
                //image.Dispose();
            }

            PreviewListView.VirtualListSize = _library.Images.Count;
            toolStripProgressBar.Value = 0;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SaveLibraryDialog.ShowDialog() != DialogResult.OK) return;

            if (_library != null) _library.Close();
            _library = new Mir3Library(SaveLibraryDialog.FileName);
            PreviewListView.VirtualListSize = 0;
            PreviewSubListView.VirtualListSize = 0;

            _indexList.Clear();
            _subIndexList.Clear();

            toolStripProgressBar.Value = 0;
            toolStripProgressBar.Maximum = _library.Images.Count;

            _library.Save(SaveLibraryDialog.FileName, toolStripProgressBar, false, false);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OpenLibraryDialog.ShowDialog() != DialogResult.OK) return;

            ClearInterface();
            ImageList.Images.Clear();
            PreviewListView.Items.Clear();

            ImageListSubItems.Images.Clear();
            PreviewSubListView.Items.Clear();

            _indexList.Clear();
            _subIndexList.Clear();

            PreviewListView.VirtualListSize = 0;
            PreviewSubListView.VirtualListSize = 0;

            if (_library != null) _library.Close();

            Application.DoEvents();

            _library = new Mir3Library(OpenLibraryDialog.FileName);
            PreviewListView.VirtualListSize = _library.Images.Count;

            // Show .Lib path in application title.
            this.Text = MakeTitle(OpenLibraryDialog.FileName.ToString());

            PreviewListView.SelectedIndices.Clear();
            PreviewSubListView.SelectedIndices.Clear();

            if (PreviewListView.Items.Count > 0)
                PreviewListView.Items[0].Selected = true;

            radioButtonImage.Enabled = true;
            radioButtonShadow.Enabled = true;
            radioButtonOverlay.Enabled = true;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_library == null) return;
            _library._fileName = SaveLibraryDialog.FileName;
            toolStripProgressBar.Maximum = _library.Images.Count();

            _library.Save(_library.FileName, toolStripProgressBar);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_library == null) return;
            if (SaveLibraryDialog.ShowDialog() != DialogResult.OK) return;

            _library._fileName = SaveLibraryDialog.FileName;
            toolStripProgressBar.Maximum = _library.Images.Count();

            toolStripProgressBar.Value = 0;
            _library.Save(_library._fileName, toolStripProgressBar);


        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (_library == null) return;
            if (_library.FileName == null) return;
            if (PreviewListView.SelectedIndices.Count == 0) return;

            if (MessageBox.Show("Are you sure you want to delete the selected Image?",
                "Delete Selected.",
                MessageBoxButtons.YesNoCancel) != DialogResult.Yes) return;

            List<int> removeList = new List<int>();

            for (int i = 0; i < PreviewListView.SelectedIndices.Count; i++)
                removeList.Add(PreviewListView.SelectedIndices[i]);

            removeList.Sort();

            for (int i = removeList.Count - 1; i >= 0; i--)
            {
                if (radioButtonImage.Checked)
                    _library.RemoveImage(removeList[i]);
                else if (radioButtonShadow.Checked)
                    _library.RemoveShadow(removeList[i]);
                else if (radioButtonOverlay.Checked)
                    _library.RemoveOverlay(removeList[i]);

            }

            ImageList.Images.Clear();

            _indexList.Clear();
            PreviewListView.VirtualListSize -= removeList.Count;

        }

        private void convertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OpenWeMadeDialog.ShowDialog() != DialogResult.OK) return;

            toolStripProgressBar.Maximum = OpenWeMadeDialog.FileNames.Length;
            toolStripProgressBar.Value = 0;

            try
            {
                //ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 1 };
                //Parallel.For(0, OpenWeMadeDialog.FileNames.Length, options, i =>
                //{
                for (int i = 0; i < OpenWeMadeDialog.FileNames.Length; i++)
                {

                    if (Path.GetExtension(OpenWeMadeDialog.FileNames[i]).ToLower() == ".dxl")
                    {
                        DXLLibrary DXLlib = new DXLLibrary(OpenWeMadeDialog.FileNames[i]);
                        DXLlib.ToMLibrary();
                    }
                    else if (Path.GetExtension(OpenWeMadeDialog.FileNames[i]).ToLower() == ".mil")
                    {
                        MILLibrary MILlib = new MILLibrary(OpenWeMadeDialog.FileNames[i]);
                        MILlib.ToMLibrary();
                    }
                    else if (Path.GetExtension(OpenWeMadeDialog.FileNames[i]).ToLower() == ".wtl")
                    {
                        if (!OpenWeMadeDialog.FileNames[i].ToLower().Contains("_s."))
                        {
                            WTLLibrary WTLlib = new WTLLibrary(OpenWeMadeDialog.FileNames[i]);
                            WTLlib.ToMLibrary();
                        }
                    }
                    else if (Path.GetExtension(OpenWeMadeDialog.FileNames[i]).ToLower() == ".lib" || Path.GetExtension(OpenWeMadeDialog.FileNames[i]).ToLower() == ".alib")
                    {
                        FileStream stream = new FileStream(OpenWeMadeDialog.FileNames[i], FileMode.Open, FileAccess.ReadWrite);
                        BinaryReader reader = new BinaryReader(stream);
                        int CurrentVersion = reader.ReadInt32();
                        stream.Close();
                        stream.Dispose();
                        reader.Dispose();
                        if (CurrentVersion == 1)
                        {
                            MLibrary v1Lib = new MLibrary(OpenWeMadeDialog.FileNames[i]);
                            v1Lib.ToMLibrary();
                        }
                        else
                        {
                            MLibraryV2 v2Lib = new MLibraryV2(OpenWeMadeDialog.FileNames[i]);
                            v2Lib.ToMLibrary();
                        }
                    }
                    else
                    {
                        //WeMadeLibrary WILlib = new WeMadeLibrary(OpenWeMadeDialog.FileNames[i]);
                        //WILlib.ToMLibrary();
                        var filename = Path.GetFileName(OpenWeMadeDialog.FileNames[i]);
                        if (!filename.ToLower().Contains("_s.") && !filename.ToLower().Contains("_c."))
                        {
                            WeMadeLibrary WILlib = new WeMadeLibrary(OpenWeMadeDialog.FileNames[i]);
                            WILlib.ToMLibrary();
                        }
                    }
                    toolStripProgressBar.Value++;
                    //});
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            toolStripProgressBar.Value = 0;

            MessageBox.Show(string.Format("Successfully converted {0} {1}",
                (OpenWeMadeDialog.FileNames.Length).ToString(),
                (OpenWeMadeDialog.FileNames.Length > 1) ? "libraries" : "library"));
        }

        private void copyToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PreviewListView.SelectedIndices.Count == 0) return;
            if (SaveLibraryDialog.ShowDialog() != DialogResult.OK) return;

            Mir3Library tempLibrary = new Mir3Library(SaveLibraryDialog.FileName);

            List<int> copyList = new List<int>();

            for (int i = 0; i < PreviewListView.SelectedIndices.Count; i++)
                copyList.Add(PreviewListView.SelectedIndices[i]);

            copyList.Sort();

            for (int i = 0; i < copyList.Count; i++)
            {
                Mir3Library.Mir3Image image = _library.GetImage(copyList[i]);
                tempLibrary.AddImage(image.Image, image.OffSetX, image.OffSetY);
            }

            toolStripProgressBar.Value = 0;
            toolStripProgressBar.Maximum = tempLibrary.Images.Count;

            tempLibrary.Save(SaveLibraryDialog.FileName, toolStripProgressBar);
        }

        private void removeBlanksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove the blank images?",
                "Remove Blanks",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            _library.RemoveBlanks();
            ImageList.Images.Clear();

            ImageListSubItems.Images.Clear();
            PreviewSubListView.Items.Clear();

            _indexList.Clear();
            PreviewListView.VirtualListSize = _library.Images.Count;
        }

        private void countBlanksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenLibraryDialog.Multiselect = true;

            if (OpenLibraryDialog.ShowDialog() != DialogResult.OK)
            {
                OpenLibraryDialog.Multiselect = false;
                return;
            }

            OpenLibraryDialog.Multiselect = false;

            MLibraryV2.Load = false;

            int count = 0;

            for (int i = 0; i < OpenLibraryDialog.FileNames.Length; i++)
            {
                MLibraryV2 library = new MLibraryV2(OpenLibraryDialog.FileNames[i]);

                for (int x = 0; x < library.Count; x++)
                {
                    if (library.Images[x].Length <= 8)
                        count++;
                }

                library.Close();
            }

            MLibraryV2.Load = true;
            MessageBox.Show(count.ToString());
        }

        private void OffSetXTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox control = sender as TextBox;

            if (control == null || !control.Focused) return;

            short temp;

            if (!short.TryParse(control.Text, out temp))
            {
                control.BackColor = Color.Red;
                return;
            }

            control.BackColor = SystemColors.Window;

            for (int i = 0; i < PreviewListView.SelectedIndices.Count; i++)
            {
                Mir3Library.Mir3Image image = _library.GetImage(PreviewListView.SelectedIndices[i]);
                image.OffSetX = temp;
            }
        }

        private void OffSetYTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox control = sender as TextBox;

            if (control == null || !control.Focused) return;

            short temp;

            if (!short.TryParse(control.Text, out temp))
            {
                control.BackColor = Color.Red;
                return;
            }

            control.BackColor = SystemColors.Window;

            for (int i = 0; i < PreviewListView.SelectedIndices.Count; i++)
            {
                Mir3Library.Mir3Image image = _library.GetImage(PreviewListView.SelectedIndices[i]);
                image.OffSetY = temp;
            }
        }

        private void InsertImageButton_Click(object sender, EventArgs e)
        {
            if (_library == null) return;
            if (_library.FileName == null) return;
            if (PreviewListView.SelectedIndices.Count == 0) return;
            if (ImportImageDialog.ShowDialog() != DialogResult.OK) return;

            List<string> fileNames = new List<string>(ImportImageDialog.FileNames);

            //fileNames.Sort();

            int index = PreviewListView.SelectedIndices[0];

            toolStripProgressBar.Value = 0;
            toolStripProgressBar.Maximum = fileNames.Count;

            for (int i = fileNames.Count - 1; i >= 0; i--)
            {
                string fileName = fileNames[i];

                Bitmap image;

                try
                {
                    image = new Bitmap(fileName);
                }
                catch
                {
                    continue;
                }

                fileName = Path.Combine(Path.GetDirectoryName(fileName), "Placements", Path.GetFileNameWithoutExtension(fileName));
                fileName = Path.ChangeExtension(fileName, ".txt");

                short x = 0;
                short y = 0;

                if (File.Exists(fileName))
                {
                    string[] placements = File.ReadAllLines(fileName);

                    if (placements.Length > 0)
                        short.TryParse(placements[0], out x);
                    if (placements.Length > 1)
                        short.TryParse(placements[1], out y);
                }

                if (radioButtonImage.Checked)
                    _library.InsertImage(index, image, x, y);
                else if (radioButtonShadow.Checked)
                    _library.InsertShadow(index, image, x, y);
                else if (radioButtonOverlay.Checked)
                    _library.InsertOverlay(index, image, x, y);

                toolStripProgressBar.Value++;
            }

            ImageList.Images.Clear();
            _indexList.Clear();
            PreviewListView.VirtualListSize = _library.Images.Count;
            toolStripProgressBar.Value = 0;
        }

        private void safeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove the blank images?",
                "Remove Blanks", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            _library.RemoveBlanks(true);
            ImageList.Images.Clear();
            _indexList.Clear();
            PreviewListView.VirtualListSize = _library.Images.Count;
        }

        private const int HowDeepToScan = 6;

        public static Mir3Library.libMode LibMode { get; internal set; } = Mir3Library.libMode.V2;

        /*public static void ProcessDir(string sourceDir, int recursionLvl, string outputDir)
        {
            if (recursionLvl <= HowDeepToScan)
            {
                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(sourceDir);
                foreach (string fileName in fileEntries)
                {
                    if (Directory.Exists(outputDir) != true) Directory.CreateDirectory(outputDir);
                    MLibraryv0 OldLibrary = new MLibraryv0(fileName);
                    MLibraryV2 NewLibrary = new MLibraryV2(outputDir + Path.GetFileName(fileName)) { Images = new List<MLibraryV2.MImage>(), IndexList = new List<int>(), Count = OldLibrary.Images.Count }; ;
                    for (int i = 0; i < OldLibrary.Images.Count; i++)
                        NewLibrary.Images.Add(null);
                    for (int j = 0; j < OldLibrary.Images.Count; j++)
                    {
                        MLibraryv0.MImage oldimage = OldLibrary.GetMImage(j);
                        NewLibrary.Images[j] = new MLibraryV2.MImage(oldimage.FBytes, oldimage.Width, oldimage.Height) { X = oldimage.X, Y = oldimage.Y };
                    }
                    NewLibrary.Save();
                    for (int i = 0; i < NewLibrary.Images.Count; i++)
                    {
                        if (NewLibrary.Images[i].Preview != null)
                            NewLibrary.Images[i].Preview.Dispose();
                        if (NewLibrary.Images[i].Image != null)
                            NewLibrary.Images[i].Image.Dispose();
                        if (NewLibrary.Images[i].MaskImage != null)
                            NewLibrary.Images[i].MaskImage.Dispose();
                    }
                    for (int i = 0; i < OldLibrary.Images.Count; i++)
                    {
                        if (OldLibrary.Images[i].Preview != null)
                            OldLibrary.Images[i].Preview.Dispose();
                        if (OldLibrary.Images[i].Image != null)
                            OldLibrary.Images[i].Image.Dispose();
                    }
                    NewLibrary.Images.Clear();
                    NewLibrary.IndexList.Clear();
                    OldLibrary.Images.Clear();
                    OldLibrary.IndexList.Clear();
                    NewLibrary.Close();
                    OldLibrary.Close();
                    NewLibrary = null;
                    OldLibrary = null;
                }

                // Recurse into subdirectories of this directory.
                string[] subdirEntries = Directory.GetDirectories(sourceDir);
                foreach (string subdir in subdirEntries)
                {
                    // Do not iterate through re-parse points.
                    if (Path.GetFileName(Path.GetFullPath(subdir).TrimEnd(Path.DirectorySeparatorChar)) == Path.GetFileName(Path.GetFullPath(outputDir).TrimEnd(Path.DirectorySeparatorChar))) continue;
                    if ((File.GetAttributes(subdir) &
                         FileAttributes.ReparsePoint) !=
                             FileAttributes.ReparsePoint)
                        ProcessDir(subdir, recursionLvl + 1, outputDir + " \\" + Path.GetFileName(Path.GetFullPath(subdir).TrimEnd(Path.DirectorySeparatorChar)) + "\\");
                }
            }
        }*/

        // Export a single image.
        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (_library == null) return;
            if (_library.FileName == null) return;
            if (PreviewListView.SelectedIndices.Count == 0) return;

            string _fileName = Path.GetFileName(OpenLibraryDialog.FileName);
            string _newName = _fileName.Remove(_fileName.IndexOf('.'));
            string _folder = Application.StartupPath + "\\Exported\\" + _newName + "\\";
            string _shadowfolder = Application.StartupPath + "\\Exported\\" + _newName + "\\Shadow\\";
            string _overlayfolder = Application.StartupPath + "\\Exported\\" + _newName + "\\Overlay\\";

            Bitmap blank = new Bitmap(1, 1);

            // Create the folder if it doesn't exist.
            (new FileInfo(_folder)).Directory.Create();
            (new FileInfo(_shadowfolder)).Directory.Create();
            (new FileInfo(_overlayfolder)).Directory.Create();

            ListView.SelectedIndexCollection _col = PreviewListView.SelectedIndices;

            toolStripProgressBar.Value = 0;
            toolStripProgressBar.Maximum = _col.Count;

            for (int i = _col[0]; i < (_col[0] + _col.Count); i++)
            {
                _exportImage = _library.GetImage(i);

                if (_exportImage?.Image == null)
                {
                    blank.Save(_folder + i.ToString() + ".png", ImageFormat.Png);
                }
                else
                {
                    _exportImage.Image.Save(_folder + i.ToString() + ".png", ImageFormat.Png);

                    if (_exportImage.ShadowValid && _exportImage.ShadowImage != null)
                    {
                        _exportImage.ShadowImage.Save(_shadowfolder + i.ToString() + ".png", ImageFormat.Png);
                    }

                    if (_exportImage.OverlayValid && _exportImage.OverlayImage != null)
                    {
                        _exportImage.OverlayImage.Save(_overlayfolder + i.ToString() + ".png", ImageFormat.Png);
                    }

                }

                toolStripProgressBar.Value++;

                int offSetX = _exportImage?.OffSetX ?? 0;
                int offSetY = _exportImage?.OffSetY ?? 0;

                if (!Directory.Exists(_folder + "/Placements/"))
                    Directory.CreateDirectory(_folder + "/Placements/");

                File.WriteAllLines(_folder + "/Placements/" + i.ToString() + ".txt", new string[] { offSetX.ToString(), offSetY.ToString() });

                //shadow placements
                if (_exportImage.ShadowValid && _exportImage.ShadowImage != null)
                {
                    if (!Directory.Exists(_shadowfolder + "/Placements/"))
                        Directory.CreateDirectory(_shadowfolder + "/Placements/");

                    int offSetX2 = _exportImage?.ShadowOffSetX ?? 0;
                    int offSetY2 = _exportImage?.ShadowOffSetY ?? 0;

                    File.WriteAllLines(_shadowfolder + "/Placements/" + i.ToString() + ".txt", new string[] { offSetX2.ToString(), offSetY2.ToString() });

                }

                //overlay placements
                if (_exportImage.OverlayValid && _exportImage.OverlayImage != null)
                {
                    if (!Directory.Exists(_overlayfolder + "/Placements/"))
                        Directory.CreateDirectory(_overlayfolder + "/Placements/");

                    File.WriteAllLines(_overlayfolder + "/Placements/" + i.ToString() + ".txt", new string[] { offSetX.ToString(), offSetY.ToString() });

                }

            }

            toolStripProgressBar.Value = 0;
            MessageBox.Show("Saving to " + _folder + "...", "Image Saved", MessageBoxButtons.OK);
        }

        // Don't let the splitter go out of sight on resizing.
        private void LMain_Resize(object sender, EventArgs e)
        {
            if (splitContainer1.SplitterDistance <= this.Height - 150) return;
            if (this.Height - 150 > 0)
            {
                splitContainer1.SplitterDistance = this.Height - 150;
            }
        }

        // Resize the image(Zoom).
        private Image ImageBoxZoom(Image image, Size size)
        {
            _originalImage = _selectedImage.Image;
            Bitmap _bmp = new Bitmap(_originalImage, Convert.ToInt32(_originalImage.Width * size.Width), Convert.ToInt32(_originalImage.Height * size.Height));
            Graphics _gfx = Graphics.FromImage(_bmp);
            return _bmp;
        }

        // Zoom in and out.
        private void ZoomTrackBar_Scroll(object sender, EventArgs e)
        {
            if (ImageBox.Image == null)
            {
                ZoomTrackBar.Value = 1;
            }
            if (ZoomTrackBar.Value > 0)
            {
                try
                {
                    PreviewListView.Items[(int)nudJump.Value].EnsureVisible();

                    Bitmap _newBMP = new Bitmap(_selectedImage.Width * ZoomTrackBar.Value, _selectedImage.Height * ZoomTrackBar.Value);
                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(_newBMP))
                    {
                        if (checkBoxPreventAntiAliasing.Checked == true)
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.CompositingMode = CompositingMode.SourceCopy;
                        }

                        if (checkBoxQuality.Checked == true)
                        {
                            g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        }

                        g.DrawImage(_selectedImage.Image, new Rectangle(0, 0, _newBMP.Width, _newBMP.Height));
                    }
                    ImageBox.Image = _newBMP;

                    toolStripStatusLabel.ForeColor = SystemColors.ControlText;
                    toolStripStatusLabel.Text = "Selected Image: " + string.Format("{0} / {1}",
                        PreviewListView.SelectedIndices[0].ToString(),
                        (PreviewListView.Items.Count - 1).ToString());
                }
                catch
                {
                    return;
                }
            }
        }

        // Swap the image panel background colour Black/White.
        private void pictureBox_Click(object sender, EventArgs e)
        {
            if (panel.BackColor == Color.Black)
            {
                panel.BackColor = Color.White;//Color.GhostWhite;
            }
            else
            {
                panel.BackColor = Color.Black;
            }
        }

        private void PreviewListView_VirtualItemsSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            // Keep track of what image/s are selected.
            ListView.SelectedIndexCollection _col = PreviewListView.SelectedIndices;

            if (_col.Count > 1)
            {
                toolStripStatusLabel.ForeColor = Color.Red;
                toolStripStatusLabel.Text = "Multiple images selected.";
            }
        }

        private void buttonReplace_Click(object sender, EventArgs e)
        {
            if (_library == null) return;
            if (_library.FileName == null) return;
            if (PreviewListView.SelectedIndices.Count == 0) return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();

            if (ofd.FileName == "") return;

            Bitmap newBmp = new Bitmap(ofd.FileName);

            ImageList.Images.Clear();
            _indexList.Clear();

            if (radioButtonImage.Checked)
                _library.ReplaceImage(PreviewListView.SelectedIndices[0], newBmp, 0, 0);
            else if (radioButtonShadow.Checked)
                _library.ReplaceShadow(PreviewListView.SelectedIndices[0], newBmp, 0, 0);
            else if (radioButtonOverlay.Checked)
                _library.ReplaceOverlay(PreviewListView.SelectedIndices[0], newBmp, 0, 0);

            PreviewListView.VirtualListSize = _library.Images.Count;

            try
            {
                PreviewListView.RedrawItems(0, PreviewListView.Items.Count - 1, true);

                var obj = _library.Images[PreviewListView.SelectedIndices[0]];

                ImageBox.Image = obj != null ? radioButtonImage.Checked & obj.Image != null ? obj.Image : radioButtonShadow.Checked && obj.ShadowImage != null ? obj.ShadowImage : radioButtonOverlay.Checked && obj.OverlayImage != null ? obj.OverlayImage : null : null;

            }
            catch (Exception)
            {
                return;
            }
        }

        private void previousImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (PreviewListView.Visible && PreviewListView.Items.Count > 0)
                {
                    int index = PreviewListView.SelectedIndices[0];
                    index = index - 1;
                    PreviewListView.SelectedIndices.Clear();
                    this.PreviewListView.Items[index].Selected = true;
                    PreviewListView.Items[index].EnsureVisible();

                    if (_selectedImage == null || _selectedImage.Height == 1 && _selectedImage.Width == 1 && PreviewListView.SelectedIndices[0] != 0)
                    {
                        previousImageToolStripMenuItem_Click(null, null);
                    }
                }
            }
            catch (Exception)
            {
                PreviewListView.SelectedIndices.Clear();
                this.PreviewListView.Items[PreviewListView.Items.Count - 1].Selected = true;
            }
        }

        private void nextImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (PreviewListView.Visible && PreviewListView.Items.Count > 0)
                {
                    int index = PreviewListView.SelectedIndices[0];
                    index = index + 1;
                    PreviewListView.SelectedIndices.Clear();
                    this.PreviewListView.Items[index].Selected = true;
                    PreviewListView.Items[index].EnsureVisible();

                    if (_selectedImage == null || _selectedImage.Height == 1 && _selectedImage.Width == 1 && PreviewListView.SelectedIndices[0] != 0)
                    {
                        nextImageToolStripMenuItem_Click(null, null);
                    }
                }
            }
            catch (Exception)
            {
                PreviewListView.SelectedIndices.Clear();
                this.PreviewListView.Items[0].Selected = true;
            }
        }

        // Move Left and Right through images.
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Left)
            {
                previousImageToolStripMenuItem_Click(null, null);
                return true;
            }

            if (keyData == Keys.Right)
            {
                nextImageToolStripMenuItem_Click(null, null);
                return true;
            }

            if (keyData == Keys.Up) //Not 100% accurate but works for now.
            {
                double d = Math.Floor((double)(PreviewListView.Width / 67));
                int index = PreviewListView.SelectedIndices[0] - (int)d;

                PreviewListView.SelectedIndices.Clear();
                if (index < 0)
                    index = 0;

                this.PreviewListView.Items[index].Selected = true;

                return true;
            }

            if (keyData == Keys.Down) //Not 100% accurate but works for now.
            {
                double d = Math.Floor((double)(PreviewListView.Width / 67));
                int index = PreviewListView.SelectedIndices[0] + (int)d;

                PreviewListView.SelectedIndices.Clear();
                if (index > PreviewListView.Items.Count - 1)
                    index = PreviewListView.Items.Count - 1;

                this.PreviewListView.Items[index].Selected = true;

                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void buttonSkipNext_Click(object sender, EventArgs e)
        {
            nextImageToolStripMenuItem_Click(null, null);
        }

        private void buttonSkipPrevious_Click(object sender, EventArgs e)
        {
            previousImageToolStripMenuItem_Click(null, null);
        }

        private void checkBoxQuality_CheckedChanged(object sender, EventArgs e)
        {
            ZoomTrackBar_Scroll(null, null);
        }

        private void checkBoxPreventAntiAliasing_CheckedChanged(object sender, EventArgs e)
        {
            ZoomTrackBar_Scroll(null, null);
        }

        private void nudJump_ValueChanged(object sender, EventArgs e)
        {
            if (PreviewListView.Items.Count - 1 >= nudJump.Value)
            {
                PreviewListView.SelectedIndices.Clear();
                PreviewListView.Items[(int)nudJump.Value].Selected = true;
                PreviewListView.Items[(int)nudJump.Value].EnsureVisible();
            }
        }

        private void OpenLibraryDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void radioButtonImage_CheckedChanged(object sender, EventArgs e)
        {
            int index = PreviewListView.SelectedIndices[0];
            ImageList.Images.Clear();
            PreviewListView.Items.Clear();
            _indexList.Clear();

            PreviewListView.VirtualListSize = 0;
            PreviewListView.VirtualListSize = _library.Images.Count;

            PreviewSubListView.VirtualListSize = 0;
            //OffSetXTextBox.Enabled = true;
            //OffSetYTextBox.Enabled = true;
            //AddButton.Enabled = true;
            //DeleteButton.Enabled = true;
            //buttonReplace.Enabled = true;
            //InsertImageButton.Enabled = true;

            PreviewListView.Items[index].Selected = true;
            PreviewListView.Items[index].EnsureVisible();
        }

        private void radioButtonShadow_CheckedChanged(object sender, EventArgs e)
        {
            int index = PreviewListView.SelectedIndices[0];
            ImageList.Images.Clear();
            PreviewListView.Items.Clear();
            _indexList.Clear();

            PreviewListView.VirtualListSize = 0;
            PreviewListView.VirtualListSize = _library.Images.Count;

            PreviewSubListView.VirtualListSize = 0;
            //OffSetXTextBox.Enabled = false;
            //OffSetYTextBox.Enabled = false;
            //AddButton.Enabled = false;
            //DeleteButton.Enabled = false;
            //buttonReplace.Enabled = false;
            //InsertImageButton.Enabled = false;

            PreviewListView.Items[index].Selected = true;
            PreviewListView.Items[index].EnsureVisible();
        }

        private void radioButtonOverlay_CheckedChanged(object sender, EventArgs e)
        {
            int index = PreviewListView.SelectedIndices[0];
            ImageList.Images.Clear();
            PreviewListView.Items.Clear();
            _indexList.Clear();

            PreviewListView.VirtualListSize = 0;
            PreviewListView.VirtualListSize = _library.Images.Count;

            PreviewSubListView.VirtualListSize = 0;
            //OffSetXTextBox.Enabled = false;
            //OffSetYTextBox.Enabled = false;
            //AddButton.Enabled = false;
            //DeleteButton.Enabled = false;
            //buttonReplace.Enabled = false;
            //InsertImageButton.Enabled = false;

            PreviewListView.Items[index].Selected = true;
            PreviewListView.Items[index].EnsureVisible();
        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

            //export all
            if (_library == null) return;
            if (_library.FileName == null) return;

            string _fileName = Path.GetFileName(OpenLibraryDialog.FileName);
            string _newName = _fileName.Remove(_fileName.IndexOf('.'));
            string _folder = Application.StartupPath + "\\Exported\\" + _newName + "\\";
            string _shadowfolder = Application.StartupPath + "\\Exported\\" + _newName + "\\Shadow\\";
            string _overlayfolder = Application.StartupPath + "\\Exported\\" + _newName + "\\Overlay\\";

            Bitmap blank = new Bitmap(1, 1);

            // Create the folder if it doesn't exist.
            (new FileInfo(_folder)).Directory.Create();
            (new FileInfo(_shadowfolder)).Directory.Create();
            (new FileInfo(_overlayfolder)).Directory.Create();

            toolStripProgressBar.Value = 0;
            toolStripProgressBar.Maximum = _library.Images.Count;

            for (int i = 0; i < _library.Images.Count; i++)
            {
                _exportImage = _library.GetImage(i);

                if (_exportImage == null) continue;

                if (_exportImage.FBytes != null && _exportImage.FBytes.Length < 150 && _exportImage.FBytes.All(singleByte => singleByte == 0)) continue;

                if (_exportImage.Width > 0 && _exportImage.Height > 0)
                    if (_exportImage?.Image == null)
                    {
                        blank.Save(_folder + i.ToString() + ".png", ImageFormat.Png);
                    }
                    else
                    {
                        _exportImage.Image.Save(_folder + i.ToString() + ".png", ImageFormat.Png);

                        if (_exportImage.ShadowValid && _exportImage.ShadowImage != null)
                        {
                            _exportImage.ShadowImage.Save(_shadowfolder + i.ToString() + ".png", ImageFormat.Png);
                        }

                        if (_exportImage.OverlayValid && _exportImage.OverlayImage != null)
                        {
                            _exportImage.OverlayImage.Save(_overlayfolder + i.ToString() + ".png", ImageFormat.Png);
                        }

                    }

                toolStripProgressBar.Value++;

                int offSetX = _exportImage?.OffSetX ?? 0;
                int offSetY = _exportImage?.OffSetY ?? 0;

                if (!Directory.Exists(_folder + "/Placements/"))
                    Directory.CreateDirectory(_folder + "/Placements/");

                File.WriteAllLines(_folder + "/Placements/" + i.ToString() + ".txt", new string[] { offSetX.ToString(), offSetY.ToString() });

                //shadow placements
                if (_exportImage.ShadowValid && _exportImage.ShadowImage != null)
                {
                    if (!Directory.Exists(_shadowfolder + "/Placements/"))
                        Directory.CreateDirectory(_shadowfolder + "/Placements/");

                    int offSetX2 = _exportImage?.ShadowOffSetX ?? 0;
                    int offSetY2 = _exportImage?.ShadowOffSetY ?? 0;

                    File.WriteAllLines(_shadowfolder + "/Placements/" + i.ToString() + ".txt", new string[] { offSetX2.ToString(), offSetY2.ToString() });

                }

                //overlay placements
                if (_exportImage.OverlayValid && _exportImage.OverlayImage != null)
                {
                    if (!Directory.Exists(_overlayfolder + "/Placements/"))
                        Directory.CreateDirectory(_overlayfolder + "/Placements/");

                    File.WriteAllLines(_overlayfolder + "/Placements/" + i.ToString() + ".txt", new string[] { offSetX.ToString(), offSetY.ToString() });

                }

            }

            toolStripProgressBar.Value = 0;
            MessageBox.Show("Saving to " + _folder + "...", "Image Saved", MessageBoxButtons.OK);


        }

        private void button5_Click(object sender, EventArgs e)
        {

            if (_library == null) return;
            if (_library._fileName == null) return;

            if (ImportImageDialog.ShowDialog() != DialogResult.OK) return;

            List<string> fileNames = new List<string>(ImportImageDialog.FileNames);

            //fileNames.Sort();
            toolStripProgressBar.Value = 0;
            toolStripProgressBar.Maximum = fileNames.Count;

            for (int i = 0; i < fileNames.Count; i++)
            {
                string fileName = fileNames[i];
                Bitmap image;

                try
                {
                    image = new Bitmap(fileName);
                }
                catch
                {
                    continue;
                }

                fileName = Path.Combine(Path.GetDirectoryName(fileName), "Placements", Path.GetFileNameWithoutExtension(fileName));

                var indx = 0;
                int.TryParse(Path.GetFileNameWithoutExtension(fileName), out indx);

                fileName = Path.ChangeExtension(fileName, ".txt");

                short x = 0;
                short y = 0;

                if (File.Exists(fileName))
                {
                    string[] placements = File.ReadAllLines(fileName);

                    if (placements.Length > 0)
                        short.TryParse(placements[0], out x);
                    if (placements.Length > 1)
                        short.TryParse(placements[1], out y);

                    if (radioButtonImage.Checked)
                        _library.InsertImage(indx, image, x, y);
                    else if (radioButtonShadow.Checked)
                        _library.InsertShadow(indx, image, x, y);
                    else if (radioButtonOverlay.Checked)
                        _library.InsertOverlay(indx, image, x, y);

                }

                toolStripProgressBar.Value++;
                //image.Dispose();
            }

            PreviewListView.VirtualListSize = _library.Images.Count;
            toolStripProgressBar.Value = 0;

        }

        private void BlankAdd_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < numBlankAdd.Value; i++)
            {
                _library.AddImage(null, 0, 0);
            }

            PreviewListView.VirtualListSize = _library.Images.Count;
        }

        private void numBlankAdd_ValueChanged(object sender, EventArgs e)
        {
            BlankAdd.Enabled = numBlankAdd.Value > 0;
        }

        private void TabControl3SelectedIndexChanged(object sender, EventArgs e)
        {

            //if (_selectedImage != null)
            //{
            //    if (tabControl3.SelectedIndex == 1)
            //    {
            //        //SubItems tab selected - update items
            //        ImageListSubItems.Images.Clear();
            //        PreviewSubListView.Items.Clear();
            //    }

            //}

        }

        private void PreviewSubListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            int index;

            if (_subIndexList.TryGetValue(e.ItemIndex, out index))
            {
                e.Item = new ListViewItem { ImageIndex = index, Text = e.ItemIndex.ToString() };
                return;
            }

            _subIndexList.Add(e.ItemIndex, ImageListSubItems.Images.Count);

            if (PreviewListView.SelectedIndices.Count == 0)
            {
                return;
            }

            var selectedImage = _library.GetImage(PreviewListView.SelectedIndices[0]);

            if (selectedImage == null)
            {
                return;
            }

            var subSelected = selectedImage.SubItems[e.ItemIndex];

            if (subSelected == null || subSelected.Image == null)
                return;

            ImageListSubItems.Images.Add(subSelected.Image);


            e.Item = new ListViewItem { ImageIndex = index, Text = e.ItemIndex.ToString() };
        }

        private void PreviewSubListView_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (_selectedImage == null || _selectedImage.SubItems.Count == 0)
            {
                return;
            }

            if (PreviewSubListView.SelectedIndices == null || PreviewSubListView.SelectedIndices.Count == 0 || PreviewSubListView.SelectedIndices[0] > _selectedImage.SubItems.Count)
                return;

            var subSelected = _selectedImage.SubItems[PreviewSubListView.SelectedIndices[0]];

            if (subSelected == null || subSelected.Image == null) return;

            ImageBox.Image = subSelected.Image;

        }

        private void nudJump_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //Enter key is down.
                if (PreviewListView.Items.Count - 1 >= nudJump.Value)
                {
                    PreviewListView.SelectedIndices.Clear();
                    PreviewListView.Items[(int)nudJump.Value].Selected = true;
                    PreviewListView.Items[(int)nudJump.Value].EnsureVisible();
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }
}