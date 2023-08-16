using ManagedSquish;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using Library_Editor;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZirconMessageBox;

namespace LibraryEditor
{
    public enum ImageType
    {
        Image,
        Shadow,
        Overlay,
    }

    public sealed class Mir3Library
    {
        public string FileName;
        public string _fileName;

        private FileStream _fStream;
        private BinaryReader _bReader;

        public List<Mir3Image> Images;

        private static byte[] checkSum1 = new byte[] { 25, 69, 78, 32 };
        private static byte[] checkSum3 = new byte[] { 0x5A, 0x6E, 0x56, 0x32 };

        public enum libMode
        {
            Normal,
            V2,
        }

        public enum TextureFormat : byte
        {
            BC1,
            BC3,
            Auto,
        }

        public enum TextureProcessMode : byte
        {
            None,
            Mir,
            Woool,
        }

        public static TextureFormat ForcedFormat = TextureFormat.Auto;
        public static TextureProcessMode textureProcessMode = TextureProcessMode.Mir;

        public static libMode LibMode { get; private set; } = libMode.V2;
        
        public Mir3Library(string fileName)
        {
            FileName = fileName;
            _fileName = Path.ChangeExtension(fileName, null);
            Images = new List<Mir3Image>();
            if (!File.Exists(fileName))
                return;

            _fStream = File.OpenRead(fileName);
            _bReader = new BinaryReader(_fStream);

            ReadLibrary();
            Close();
        }
        public void ReadLibrary()
        {
            if (_bReader == null)
                return;

            using (MemoryStream mstream = new MemoryStream(_bReader.ReadBytes(_bReader.ReadInt32())))
            using (BinaryReader reader = new BinaryReader(mstream))
            {                
                int count = reader.ReadInt32();

                var check = reader.ReadBytes(4);

                if (check[0] == checkSum3[0] && check[1] == checkSum3[1] && check[2] == checkSum3[2] && check[3] == checkSum3[3])
                {
                    var check2 = reader.ReadBytes(3);

                    if (check2[0] == checkSum1[0] && check2[1] == checkSum1[1] && check2[2] == checkSum1[2])
                    {
                        LibMode = libMode.V2;

                    }
                    else
                        return;
                }
                else
                {
                    //fix position, normal ZL lib
                    reader.BaseStream.Position -= 4;
                    LibMode = libMode.Normal;
                }

                for (int i = 0; i < count; i++)
                    Images.Add(null);

                for (int i = 0; i < Images.Count; i++)
                {
                    if (!reader.ReadBoolean()) continue;

                    Images[i] = new Mir3Image(reader);
                }
            }

            for (int i = 0; i < Images.Count; i++)
            {
                if (Images[i] == null) continue;
                
                CreateImage(i, ImageType.Image);
                CreateImage(i, ImageType.Shadow);
                CreateImage(i, ImageType.Overlay);

                if (i % 5 == 0)
                    Application.DoEvents();

            }
        }
        public void Close()
        {
            if (_fStream != null)
                _fStream.Dispose();
            if (_bReader != null)
                _bReader.Dispose();
        }
        public Mir3Image CreateImage(int index, ImageType type)
        {
            if (!CheckImage(index)) return null;

            Mir3Image image = Images[index];
            Bitmap bmp;

            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_bReader);
                    bmp = image.Image;

                    if (image.SubItems.Count > 0)
                    {
                        foreach (var subItem in image.SubItems)
                        {
                            if (subItem == null || subItem.Position == 0) continue;//nothing inside -  || subItem.FBytes == null || subItem.FBytes.Length == 0
                            if (subItem.ImageValid) continue;//already loaded
                            subItem.CreateImage(_bReader);
                        }
                    }

                    break;
                case ImageType.Shadow:
                    if (!image.ShadowValid) image.CreateShadow(_bReader);
                    bmp = image.ShadowImage;
                    break;
                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_bReader);
                    bmp = image.OverlayImage;
                    break;
                default:
                    return null;
            }

            if (bmp == null) return null;

            return image;
        }

        private bool CheckImage(int index)
        {
            return index >= 0 && index < Images.Count && Images[index] != null;
        }

        public Mir3Image GetImage(int index)
        {
            if (index < 0 || index >= Images.Count)
                return null;

            return Images[index];
        }

        public Bitmap GetPreview(int index, ImageType type)
        {
            if (index < 0 || index >= Images.Count)
                return new Bitmap(1, 1);

            Mir3Image image = Images[index];

            switch (type)
            {
                case ImageType.Image:
                    if (image == null || image.Image == null)
                        return new Bitmap(1, 1);

                    if (image.Preview == null)
                        image.CreatePreview();

                    return image.Preview;
                case ImageType.Shadow:
                    if (image == null || image.ShadowImage == null)
                        return new Bitmap(1, 1);

                    if (image.ShadowPreview == null)
                        image.CreateShadowPreview();

                    return image.ShadowPreview;
                case ImageType.Overlay:
                    if (image == null || image.OverlayImage == null)
                        return new Bitmap(1, 1);

                    if (image.OverlayPreview == null)
                        image.CreateOverlayPreview();

                    return image.OverlayPreview;
            }

            return new Bitmap(1, 1);
        }

        public void RemoveBlanks(bool safe = false)
        {
            for (int i = Images.Count - 1; i >= 0; i--)
            {
                if (Images[i] == null)
                {
                    if (!safe)
                        RemoveImage(i);
                    else if (Images[i].OffSetX == 0 && Images[i].OffSetY == 0)
                        RemoveImage(i);
                }
            }
        }

        public void RemoveImage(int index)
        {
            if (Images == null || Images.Count <= 1)
            {
                Images = new List<Mir3Image>();
                return;
            }

            //Images.RemoveAt(index);

            if (Images[index].OverlayImage != null || Images[index].ShadowImage != null)
            {
                if (Images[index].Image != null)
                    Images[index].Image.Dispose();

                Images[index].Image = null;

                unsafe
                {
                    Images[index].ImageData = null;
                }
            }

        }

        public void RemoveOverlay(int index)
        {
            if (Images == null || Images.Count <= 1)
            {
                Images = new List<Mir3Image>();
                return;
            }

            //Images.RemoveAt(index);

            if (Images[index].Image != null || Images[index].OverlayImage != null)
            {
                if (Images[index].OverlayImage != null)
                    Images[index].OverlayImage.Dispose();

                Images[index].OverlayImage = null;

                unsafe
                {
                    Images[index].OverlayData = null;
                }
            }


        }

        public void RemoveShadow(int index)
        {
            if (Images == null || Images.Count <= 1)
            {
                Images = new List<Mir3Image>();
                return;
            }

            //Images.RemoveAt(index);

            if (Images[index].Image != null || Images[index].ShadowImage != null)
            {
                if (Images[index].ShadowImage != null)
                    Images[index].ShadowImage.Dispose();

                Images[index].ShadowImage = null;

                unsafe
                {
                    Images[index].ShadowData = null;
                }
            }

        }

        public void AddImage(Bitmap image, short x, short y)
        {
            Mir3Image mImage = new Mir3Image(image) { OffSetX = x, OffSetY = y };

            Images.Add(mImage);
        }

        public void AddOverlay(Bitmap image, short x, short y)
        {

            Mir3Image mImage = new Mir3Image(null, null, image) { OffSetX = x, OffSetY = y };

            Images.Add(mImage);

        }

        public void AddShadow(Bitmap image, short x, short y)
        {

            Mir3Image mImage = new Mir3Image(null, image, null) { ShadowOffSetX = x, ShadowOffSetY = y };

            Images.Add(mImage);

        }

        public void ReplaceImage(int Index, Bitmap image, short x, short y)
        {
            if (Images[Index] != null)
            {
                Mir3Image mImage = new Mir3Image(image) { OffSetX = x, OffSetY = y };

                var old = Images[Index];

                unsafe
                {
                    old.ImageData = mImage.ImageData;
                }

                old.Image = mImage.Image;
                old.FBytes = mImage.FBytes;
                old.Height = mImage.Height;
                old.Width = mImage.Width;
                old.OffSetX = mImage.OffSetX;
                old.OffSetY = mImage.OffSetY;
                old.Preview = mImage.Preview;
                
                Images[Index] = old;
            }
            else
            {
                Mir3Image mImage = new Mir3Image(image) { OffSetX = x, OffSetY = y };
                Images[Index] = mImage;
            }
        }

        public void ReplaceShadow(int Index, Bitmap image, short x, short y)
        {
            
            if (Images[Index] != null)
            {
                Mir3Image mImage = new Mir3Image(image) { ShadowOffSetX = x, ShadowOffSetY = y };

                var old = Images[Index];

                unsafe
                {
                    old.ShadowData = mImage.ImageData;
                }

                old.ShadowImage = mImage.Image;
                old.ShadowFBytes = mImage.FBytes;
                old.ShadowHeight = mImage.Height;
                old.ShadowWidth = mImage.Width;
                old.ShadowOffSetX = mImage.OffSetX;
                old.ShadowOffSetY = mImage.OffSetY;
                old.ShadowPreview = mImage.Preview;
                
                Images[Index] = old;

            }
            else
            {

                var nObj = new Mir3Image(new Bitmap(1, 1), image, null) { OffSetX = x, OffSetY = y }; ;
                Images[Index] = nObj;

            }

        }

        public void ReplaceOverlay(int Index, Bitmap image, short x, short y)
        {
            
            if (Images[Index] != null)
            {
                Mir3Image mImage = new Mir3Image(image) { OffSetX = x, OffSetY = y };

                var old = Images[Index];

                unsafe
                {
                    old.OverlayData = mImage.ImageData;
                }

                old.OverlayImage = mImage.Image;
                old.OverlayFBytes = mImage.FBytes;
                old.OverlayHeight = mImage.Height;
                old.OverlayWidth = mImage.Width;

                if (old.Width < mImage.Width || old.Height < mImage.Height)
                {
                    old.Width = mImage.Width;
                    old.Height = mImage.Height;
                }
                
                old.OverlayPreview = mImage.Preview;
                
                Images[Index] = old;

            } else
            {

                var nObj = new Mir3Image(new Bitmap(1,1), null, image) { OffSetX = x, OffSetY = y }; ;
                Images[Index] = nObj;

            }

        }

        public void InsertImage(int index, Bitmap image, short x, short y)
        {
            Mir3Image mImage = new Mir3Image(image) { OffSetX = x, OffSetY = y };

            if (Images.Count < index)
            {
                while (Images.Count < index)
                {
                    Images.Add(null);
                }
            }

            Images.Insert(index, mImage);

        }

        public void InsertShadow(int index, Bitmap image, short x, short y)
        {
            Mir3Image mImage = new Mir3Image(null, image, null) { ShadowOffSetX = x, ShadowOffSetY = y };

            if (Images.Count < index)
            {
                while (Images.Count < index)
                {
                    Images.Add(null);
                }
            }

            Images.Insert(index, mImage);
        }

        public void InsertOverlay(int index, Bitmap image, short x, short y)
        {
            Mir3Image mImage = new Mir3Image(null, null, image) { OffSetX = x, OffSetY = y };

            if (Images.Count < index)
            {
                while (Images.Count < index)
                {
                    Images.Add(null);
                }
            }

            Images.Insert(index, mImage);
        }

        public void Save(string path, System.Windows.Forms.ToolStripProgressBar toolStripProgressBar, bool SelectForcedFormat = true, bool SelectBuildMode = true)
        {
            //|Header Size|Count|T|Header|F|F|T|Header|F|T|Header...|Image|Image|Im...

            int StepsToDo = Images.Count * 5;

            if (toolStripProgressBar != null)
                toolStripProgressBar.Maximum = StepsToDo + 1;

            int StepsDone = 0;

            int headerSize = 4 + 3 + Images.Count + 4;

            ForcedFormat = TextureFormat.Auto;

            if (SelectForcedFormat)
            {
                switch (ZirconMessageBox.ZirconMessageBox.ShowBox($"What quality u want for build this lib? [{Path.GetFileNameWithoutExtension(path)}]", "Zircon Build Quality", "Low: DXT1", "Good: DXT5", "Automatic"))
                {
                    case ZirconMessageBox.ZirconMessageBox.ZirconMessageBoxResult.Button1://dxt1
                        ForcedFormat = TextureFormat.BC1;
                        break;
                    case ZirconMessageBox.ZirconMessageBox.ZirconMessageBoxResult.Button2://dxt5
                        ForcedFormat = TextureFormat.BC3;
                        break;
                    case ZirconMessageBox.ZirconMessageBox.ZirconMessageBoxResult.Button3://auto
                        ForcedFormat = TextureFormat.Auto;
                        break;
                    case ZirconMessageBox.ZirconMessageBox.ZirconMessageBoxResult.Cancel:
                    default:
                        return;
                }
            }

            textureProcessMode = TextureProcessMode.Mir;

            if (SelectBuildMode)
            {
                switch (ZirconMessageBox.ZirconMessageBox.ShowBox($"What method u want to build/process textures? [{Path.GetFileNameWithoutExtension(path)}]", "Zircon Build Processor", "None", "Clear Black (Mir)", "Clear Black (Woool)"))
                {
                    case ZirconMessageBox.ZirconMessageBox.ZirconMessageBoxResult.Button1://none
                        textureProcessMode = TextureProcessMode.None;
                        break;
                    case ZirconMessageBox.ZirconMessageBox.ZirconMessageBoxResult.Button2://mir
                        textureProcessMode = TextureProcessMode.Mir;
                        break;
                    case ZirconMessageBox.ZirconMessageBox.ZirconMessageBoxResult.Button3://woool
                        textureProcessMode = TextureProcessMode.Woool;
                        break;
                    case ZirconMessageBox.ZirconMessageBox.ZirconMessageBoxResult.Cancel:
                    default:
                        return;
                }
            }

            foreach (Mir3Image image in Images)
            {
                if (image == null || image.FBytes == null || image.FBytes.Length == 0) continue;

                headerSize += Mir3Image.HeaderSize;

                if (image.SubCounter > 0)
                {
                    headerSize += Mir3SubImage.HeaderSize * image.SubCounter;
                }

                StepsDone++;
                if (toolStripProgressBar != null)
                {
                    toolStripProgressBar.Value = StepsDone;
                    Application.DoEvents();
                }
            }

            int position = headerSize + 4;

            //Rebuild using Multithreading
            //ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 8 };
            bool doTheJob = toolStripProgressBar != null && toolStripProgressBar.GetCurrentParent().InvokeRequired;

            ToolStrip obj = default(ToolStrip);

            if (doTheJob)
                obj = toolStripProgressBar.GetCurrentParent();

            Parallel.For(0, Images.Count, i => // options,
            {
                
                if (Images[i] != null && Images[i].FBytes != null && Images[i].FBytes.Length > 0)
                    Images[i].Rebuild();

                if (doTheJob)
                    obj.Invoke(new MethodInvoker(delegate { toolStripProgressBar.Value = StepsDone++; }));
                
                Application.DoEvents();
            });

            foreach (Mir3Image image in Images)
            {

                StepsDone++;
                if (toolStripProgressBar != null)
                {
                    toolStripProgressBar.Value = StepsDone;
                    Application.DoEvents();
                }

                if (image == null || image.FBytes == null || image.FBytes.Length == 0) continue;

                image.Position = position;

                position += image.DataSize;
                
                if (image.SubCounter > 0)
                {
                    foreach (var item in image.SubItems)
                    {
                        item.Position = position;
                        position += item.DataSize;
                    }
                    
                }

            }

            using (MemoryStream buffer = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(buffer))
            {
                writer.Write(headerSize);
                writer.Write(Images.Count);

                if (LMain.LibMode == libMode.V2)
                {

                    writer.Write(checkSum3);

                    writer.Write(checkSum1[0]);
                    writer.Write(checkSum1[1]);
                    writer.Write(checkSum1[2]);

                }

                foreach (Mir3Image image in Images)
                {

                    StepsDone++;
                    if (toolStripProgressBar != null)
                    {
                        toolStripProgressBar.Value = StepsDone;
                        Application.DoEvents();
                    }

                    writer.Write(image != null && image.FBytes != null && image.FBytes.Length > 0);

                    if (image == null || image.FBytes == null || image.FBytes.Length == 0) continue;

                    image.SaveHeader(writer);

                }

                foreach (Mir3Image image in Images)
                {

                    StepsDone++;
                    if (toolStripProgressBar != null)
                    {
                        toolStripProgressBar.Value = StepsDone;
                        Application.DoEvents();
                    }

                    if (image == null || image.FBytes == null || image.FBytes.Length == 0) continue;

                    if (image.FBytes != null)
                    {
                        writer.Write(image.FBytes);
                    }
                    if (image.ShadowFBytes != null)
                    {
                        writer.Write(image.ShadowFBytes);
                    }
                    if (image.OverlayFBytes != null)
                    {
                        writer.Write(image.OverlayFBytes);
                    }

                    //subImages
                    if (image.SubCounter > 0)
                    {
                        foreach (var item in image.SubItems)
                        {
                            writer.Write(item.FBytes);
                        }

                    }

                }

                File.WriteAllBytes(path, buffer.ToArray());

                if (toolStripProgressBar != null)
                {
                    toolStripProgressBar.Value = toolStripProgressBar.Maximum;
                    Application.DoEvents();
                }

            }
        }

        public sealed class Mir3SubImage : IDisposable
        {
            public const int HeaderSize = 13;

            public int DataSize => (FBytes?.Length ?? 0);
            //public int DataSize => LMain.LibMode == libMode.Normal ? (FBytes?.Length ?? 0) + (ShadowFBytes?.Length ?? 0) + (OverlayFBytes?.Length ?? 0) : (FBytes?.Length + 0 ?? 0) + (ShadowFBytes?.Length + 0 ?? 0) + (OverlayFBytes?.Length + 0 ?? 0);

            public int Position;

            #region Texture

            public TextureFormat textureFormat;
            public short Width;
            public short Height;
            public short OffSetX;
            public short OffSetY;
            public Bitmap Image, Preview;
            public bool ImageValid { get; private set; }
            public unsafe byte* ImageData;
            public int ImageDataSize
            {
                get
                {
                    int w = Width + (4 - Width % 4) % 4;
                    int h = Height + (4 - Height % 4) % 4;

                    return textureFormat == TextureFormat.BC1 ? w * h / 2 : w * h;
                }
            }
            public byte[] FBytes;
            #endregion

            public DateTime ExpireTime;


            public Mir3SubImage()
            {
                
            }

            public Mir3SubImage(BinaryReader reader)
            {

                Position = reader.ReadInt32();
                Width = reader.ReadInt16();
                Height = reader.ReadInt16();
                OffSetX = reader.ReadInt16();
                OffSetY = reader.ReadInt16();

                if (LibMode == libMode.V2)
                    textureFormat = (TextureFormat)reader.ReadByte();

            }

            public void SaveHeader(BinaryWriter writer)
            {

                writer.Write(Position);
                
                writer.Write(Width);
                writer.Write(Height);
                writer.Write(OffSetX);
                writer.Write(OffSetY);

                writer.Write((byte)textureFormat);

            }

            public void Rebuild()
            {
                if (this.Image != null)
                {

                    BitmapData data = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadOnly,
                                                 PixelFormat.Format32bppArgb);

                    byte[] pixels = new byte[Image.Width * Image.Height * 4];

                    Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                    Image.UnlockBits(data);

                    textureFormat = TextureFormat.BC1;
                    for (int i = 0; i < pixels.Length; i += 4)
                    {
                        //Reverse Red/Blue
                        byte b = pixels[i];
                        pixels[i] = pixels[i + 2];
                        pixels[i + 2] = b;

                        if (pixels[i] == 0 && pixels[i + 1] == 0 && pixels[i + 2] == 0)
                            pixels[i + 3] = 0; //Make Transparent

                        if (pixels[i + 3] < 255 && pixels[i + 3] > 0)
                            textureFormat = TextureFormat.BC3;

                    }

                    if (ForcedFormat != TextureFormat.Auto)
                        textureFormat = ForcedFormat;

                    var decFormat = SquishFlags.Dxt1;

                    switch (textureFormat)
                    {
                        case TextureFormat.BC3:
                            decFormat = SquishFlags.Dxt5;
                            break;
                        case TextureFormat.BC1:
                        default:
                            decFormat = SquishFlags.Dxt1;
                            break;
                    }

                    int count = Squish.GetStorageRequirements(Image.Width, Image.Height, decFormat);

                    FBytes = new byte[count];
                    unsafe
                    {
                        fixed (byte* dest = FBytes)
                        fixed (byte* source = pixels)
                        {
                            Squish.CompressImage((IntPtr)source, Image.Width, Image.Height, (IntPtr)dest, decFormat);
                        }
                    }
                          
                }
            }

            public Bitmap GetImage()
            {
                if (Position == 0) return null;

                int w = Width + (4 - Width % 4) % 4;
                int h = Height + (4 - Height % 4) % 4;

                if (w == 0 || h == 0) return null;

                if (!ImageValid) return null;

                return Image;
                    
            }

            public unsafe void CreateImage(BinaryReader reader)
            {
                if (Position == 0) return;

                int w = Width + (4 - Width % 4) % 4;
                int h = Height + (4 - Height % 4) % 4;

                if (w == 0 || h == 0) return;

                Image = new Bitmap(w, h);
                BitmapData data = Image.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                reader.BaseStream.Seek(Position, SeekOrigin.Begin);

                var decFormat = SquishFlags.Dxt1;

                switch (textureFormat)
                {
                    case TextureFormat.BC3:
                        decFormat = SquishFlags.Dxt5;
                        
                        break;
                    case TextureFormat.BC1:
                    default:
                        decFormat = SquishFlags.Dxt1;
                        
                        break;
                }

                FBytes = reader.ReadBytes(ImageDataSize);

                if (FBytes == null || FBytes.Length == 0 || FBytes.Length < ImageDataSize)
                {
                    Image.UnlockBits(data);
                    return;
                }

                fixed (byte* source = FBytes)
                    Squish.DecompressImage(data.Scan0, w, h, (IntPtr)source, decFormat);
                        
                byte* dest = (byte*)data.Scan0;

                for (int i = 0; i < h * w * 4; i += 4)
                {
                    //Reverse Red/Blue
                    byte b = dest[i];
                    dest[i] = dest[i + 2];
                    dest[i + 2] = b;
                }

                Image.UnlockBits(data);
                ImageValid = true;
            }

            public void CreatePreview()
            {
                if (Image == null)
                {
                    Preview = new Bitmap(1, 1);
                    return;
                }

                Preview = new Bitmap(64, 64);

                using (Graphics g = Graphics.FromImage(Preview))
                {
                    g.InterpolationMode = InterpolationMode.Low;//HighQualityBicubic
                    g.Clear(Color.Transparent);
                    int w = Math.Min((int)Width, 64);
                    int h = Math.Min((int)Height, 64);
                    g.DrawImage(Image, new Rectangle((64 - w) / 2, (64 - h) / 2, w, h), new Rectangle(0, 0, Width, Height), GraphicsUnit.Pixel);

                    g.Save();
                }
            }

            #region IDisposable Support

            public bool IsDisposed { get; private set; }
            
            public void Dispose(bool disposing)
            {
                if (disposing)
                {
                    IsDisposed = true;

                    Position = 0;

                    Width = 0;
                    Height = 0;
                    OffSetX = 0;
                    OffSetY = 0;

                }

            }

            public void Dispose()
            {
                Dispose(!IsDisposed);
                GC.SuppressFinalize(this);
            }


            ~Mir3SubImage()
            {
                Dispose(false);
            }

            #endregion

        }

        public sealed class Mir3Image : IDisposable
        {
            public const int HeaderSize = 30;

            public int DataSize => (FBytes?.Length ?? 0) + (ShadowFBytes?.Length ?? 0) + (OverlayFBytes?.Length ?? 0) + GetSubItemsSize();

            private int GetSubItemsSize()
            {
                int result = 0;

                if (SubCounter > 0)
                {
                    foreach (var item in SubItems)
                    {
                        if (item == null || item.FBytes == null) continue;
                        result += item.FBytes.Length;
                    }

                }
                
                return result;

            }

            //public int DataSize => LMain.LibMode == libMode.Normal ? (FBytes?.Length ?? 0) + (ShadowFBytes?.Length ?? 0) + (OverlayFBytes?.Length ?? 0) : (FBytes?.Length + 0 ?? 0) + (ShadowFBytes?.Length + 0 ?? 0) + (OverlayFBytes?.Length + 0 ?? 0);

            public int Position;

            #region Texture

            public List<Mir3SubImage> SubItems = new List<Mir3SubImage>();
            public int SubCounter;

            public TextureFormat textureFormat;
            public short Width;
            public short Height;
            public short OffSetX;
            public short OffSetY;
            public byte ShadowType;
            public Bitmap Image, Preview;
            public bool ImageValid { get; private set; }
            public unsafe byte* ImageData;
            public int ImageDataSize
            {
                get
                {
                    int w = Width + (4 - Width % 4) % 4;
                    int h = Height + (4 - Height % 4) % 4;

                    return textureFormat == TextureFormat.BC1 ? w * h / 2 : w * h;
                }
            }
            public byte[] FBytes;
            #endregion

            #region Shadow
            public short ShadowWidth;
            public short ShadowHeight;

            public short ShadowOffSetX;
            public short ShadowOffSetY;

            public Bitmap ShadowImage, ShadowPreview;
            public bool ShadowValid { get; private set; }
            public unsafe byte* ShadowData;
            public byte[] ShadowFBytes;
            public int ShadowDataSize
            {
                get
                {
                    int w = ShadowWidth + (4 - ShadowWidth % 4) % 4;
                    int h = ShadowHeight + (4 - ShadowHeight % 4) % 4;

                    return LibMode == libMode.V2 ? w * h : w * h / 2;
                }
            }
            #endregion

            #region Overlay
            public short OverlayWidth;
            public short OverlayHeight;

            public Bitmap OverlayImage, OverlayPreview;
            public bool OverlayValid { get; private set; }
            public unsafe byte* OverlayData;
            public byte[] OverlayFBytes;
            public int OverlayDataSize
            {
                get
                {
                    int w = OverlayWidth + (4 - OverlayWidth % 4) % 4;
                    int h = OverlayHeight + (4 - OverlayHeight % 4) % 4;

                    return LibMode == libMode.V2 ? w * h: w * h / 2;
                }
            }
            #endregion


            public DateTime ExpireTime;

            public Mir3Image()
            {
                SubItems = new List<Mir3SubImage>();
            }

            public Mir3Image(BinaryReader reader)
            {

                if (LibMode == libMode.V2)
                {
                    SubCounter = reader.ReadInt32();
                }

                Position = reader.ReadInt32();

                Width = reader.ReadInt16();
                Height = reader.ReadInt16();
                OffSetX = reader.ReadInt16();
                OffSetY = reader.ReadInt16();

                if (LibMode == libMode.V2)
                    textureFormat = (TextureFormat)reader.ReadByte();
                
                ShadowType = reader.ReadByte();

                ShadowWidth = reader.ReadInt16();
                ShadowHeight = reader.ReadInt16();
                ShadowOffSetX = reader.ReadInt16();
                ShadowOffSetY = reader.ReadInt16();

                OverlayWidth = reader.ReadInt16();
                OverlayHeight = reader.ReadInt16();

                if (LibMode == libMode.V2)
                {
                    for (int i = 0; i < SubCounter; i++)
                    {
                        SubItems.Add(new Mir3SubImage(reader));
                    }
                }

            }

            public unsafe Mir3Image(Bitmap image)
            {
                if (image == null)
                {
                    FBytes = new byte[0];
                    return;
                }

                Position = -1;
                Width = (short)image.Width;
                Height = (short)image.Height;

                int w = image.Width + (4 - image.Width % 4) % 4;
                int h = image.Height + (4 - image.Height % 4) % 4;

                if (image.Width != w || image.Height != h)
                {
                    Bitmap temp = new Bitmap(w, h);
                    using (Graphics g = Graphics.FromImage(temp))
                    {
                        g.Clear(Color.Transparent);
                        g.DrawImage(image, 0, 0);
                        g.Save();
                    }
                    image.Dispose();
                    image = temp;
                }

                Image = image;

                BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly,
                                                 PixelFormat.Format32bppArgb);

                byte[] pixels = new byte[image.Width * image.Height * 4];

                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                image.UnlockBits(data);

                textureFormat = TextureFormat.BC3;//full quality when imports :D
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    //Reverse Red/Blue
                    byte b = pixels[i];
                    pixels[i] = pixels[i + 2];
                    pixels[i + 2] = b;

                    //if (pixels[i] == 0 && pixels[i + 1] == 0 && pixels[i + 2] == 0)
                    //    pixels[i + 3] = 0; //Make Transparent

                    //if (pixels[i + 3] < 255 && pixels[i + 3] > 0)
                    //    textureFormat = TextureFormat.BC3;

                }

                //FBytes = null;

                switch (LMain.LibMode)
                {
                    case LibraryEditor.Mir3Library.libMode.V2:
                        var decFormat = SquishFlags.Dxt1;

                        switch (textureFormat)
                        {
                            case TextureFormat.BC3:
                                decFormat = SquishFlags.Dxt5;
                                break;
                            case TextureFormat.BC1:
                            default:
                                decFormat = SquishFlags.Dxt1;
                                break;
                        }
                        int count = Squish.GetStorageRequirements(w, h, decFormat);

                        FBytes = new byte[count];
                        fixed (byte* dest = FBytes)

                        fixed (byte* source = pixels)
                        {
                            Squish.CompressImage((IntPtr)source, w, h, (IntPtr)dest, decFormat);
                        }

                        break;
                    case LibraryEditor.Mir3Library.libMode.Normal:

                        int count2 = Squish.GetStorageRequirements(w, h, SquishFlags.Dxt1);

                        FBytes = new byte[count2];
                        fixed (byte* dest = FBytes)

                        fixed (byte* source = pixels)
                        {
                            Squish.CompressImage((IntPtr)source, w, h, (IntPtr)dest, SquishFlags.Dxt1);
                        }

                        break;
                }
                //int count = Squish.GetStorageRequirements(image.Width, image.Height, SquishFlags.Dxt1);

                //FBytes = new byte[count];
                //fixed (byte* dest = FBytes)
                //fixed (byte* source = pixels)
                //{
                //    Squish.CompressImage((IntPtr)source, image.Width, image.Height, (IntPtr)dest, SquishFlags.Dxt1);
                //}
            }

            public unsafe Mir3Image(Bitmap image, Bitmap shadow, Bitmap overlay)
            {

                if (image == null && shadow == null && overlay == null)
                {
                    FBytes = new byte[0];
                    return;
                }

                Position = -1;
                int w;
                int h;
                byte[] pixels;
                BitmapData data;

                //image
                if (image != null)
                {

                    Width = (short)image.Width;
                    Height = (short)image.Height;

                    w = image.Width + (4 - image.Width % 4) % 4;
                    h = image.Height + (4 - image.Height % 4) % 4;

                    if (image.Width != w || image.Height != h)
                    {
                        Bitmap temp = new Bitmap(w, h);
                        using (Graphics g = Graphics.FromImage(temp))
                        {
                            g.Clear(Color.Transparent);
                            g.DrawImage(image, 0, 0);
                            g.Save();
                        }
                        image.Dispose();
                        image = temp;
                    }

                    Image = image;

                    data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly,
                                                     PixelFormat.Format32bppArgb);

                    pixels = new byte[image.Width * image.Height * 4];

                    Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                    image.UnlockBits(data);

                    textureFormat = TextureFormat.BC3;//full quality when imports :D
                    for (int i = 0; i < pixels.Length; i += 4)
                    {
                        //Reverse Red/Blue
                        byte b = pixels[i];
                        pixels[i] = pixels[i + 2];
                        pixels[i + 2] = b;

                        //if (pixels[i] == 0 && pixels[i + 1] == 0 && pixels[i + 2] == 0)
                        //    pixels[i + 3] = 0; //Make Transparent

                        //if (pixels[i + 3] < 255 && pixels[i + 3] > 0)
                        //    textureFormat = TextureFormat.BC3;

                    }

                    //FBytes = null;

                    switch (LMain.LibMode)
                    {
                        case LibraryEditor.Mir3Library.libMode.V2:
                            var decFormat = SquishFlags.Dxt1;

                            switch (textureFormat)
                            {
                                case TextureFormat.BC3:
                                    decFormat = SquishFlags.Dxt5;
                                    break;
                                case TextureFormat.BC1:
                                default:
                                    decFormat = SquishFlags.Dxt1;
                                    break;
                            }
                            int count = Squish.GetStorageRequirements(w, h, decFormat);

                            FBytes = new byte[count];
                            fixed (byte* dest = FBytes)

                            fixed (byte* source = pixels)
                            {
                                Squish.CompressImage((IntPtr)source, w, h, (IntPtr)dest, decFormat);
                            }
                            break;
                        case LibraryEditor.Mir3Library.libMode.Normal:

                            int count2 = Squish.GetStorageRequirements(w, h, SquishFlags.Dxt1);

                            FBytes = new byte[count2];
                            fixed (byte* dest = FBytes)

                            fixed (byte* source = pixels)
                            {
                                Squish.CompressImage((IntPtr)source, w, h, (IntPtr)dest, SquishFlags.Dxt1);
                            }

                            break;
                    }

                    //int count = Squish.GetStorageRequirements(image.Width, image.Height, SquishFlags.Dxt1);

                    //FBytes = new byte[count];
                    //fixed (byte* dest = FBytes)
                    //fixed (byte* source = pixels)
                    //{
                    //    Squish.CompressImage((IntPtr)source, image.Width, image.Height, (IntPtr)dest, SquishFlags.Dxt1);
                    //}
                }

                //Shadow
                if (shadow != null)
                {
                    ShadowWidth = (short)shadow.Width;
                    ShadowHeight = (short)shadow.Height;

                    w = shadow.Width + (4 - shadow.Width % 4) % 4;
                    h = shadow.Height + (4 - shadow.Height % 4) % 4;

                    if (shadow.Width != w || shadow.Height != h)
                    {
                        Bitmap temp = new Bitmap(w, h);
                        using (Graphics g = Graphics.FromImage(temp))
                        {
                            g.Clear(Color.Transparent);
                            g.DrawImage(shadow, 0, 0);
                            g.Save();
                        }
                        shadow.Dispose();
                        shadow = temp;
                    }

                    ShadowImage = shadow;

                    data = shadow.LockBits(new Rectangle(0, 0, shadow.Width, shadow.Height), ImageLockMode.ReadOnly,
                                                     PixelFormat.Format32bppArgb);

                    pixels = new byte[shadow.Width * shadow.Height * 4];

                    Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                    shadow.UnlockBits(data);

                    for (int i = 0; i < pixels.Length; i += 4)
                    {
                        //Reverse Red/Blue
                        byte b = pixels[i];
                        pixels[i] = pixels[i + 2];
                        pixels[i + 2] = b;

                        //if (pixels[i] == 0 && pixels[i + 1] == 0 && pixels[i + 2] == 0)
                        //    pixels[i + 3] = 0; //Make Transparent
                    }

                    //ShadowFBytes = null;

                    switch (LMain.LibMode)
                    {
                        case LibraryEditor.Mir3Library.libMode.V2:
                            int count1 = Squish.GetStorageRequirements(w, h, SquishFlags.Dxt5);

                            ShadowFBytes = new byte[count1];
                            fixed (byte* dest = ShadowFBytes)

                            fixed (byte* source = pixels)
                            {
                                Squish.CompressImage((IntPtr)source, w, h, (IntPtr)dest, SquishFlags.Dxt5);
                            }

                            break;
                        case LibraryEditor.Mir3Library.libMode.Normal:

                            int count = Squish.GetStorageRequirements(w, h, SquishFlags.Dxt1);

                            ShadowFBytes = new byte[count];
                            fixed (byte* dest = ShadowFBytes)

                            fixed (byte* source = pixels)
                            {
                                Squish.CompressImage((IntPtr)source, w, h, (IntPtr)dest, SquishFlags.Dxt1);
                            }

                            break;
                    }
                    //count = Squish.GetStorageRequirements(shadow.Width, shadow.Height, SquishFlags.Dxt1);

                    //ShadowFBytes = new byte[count];
                    //fixed (byte* dest = ShadowFBytes)
                    //fixed (byte* source = pixels)
                    //{
                    //    Squish.CompressImage((IntPtr)source, shadow.Width, shadow.Height, (IntPtr)dest, SquishFlags.Dxt1);
                    //}
                }

                //Overlay
                if (overlay != null)
                {
                    OverlayWidth = (short)overlay.Width;
                    OverlayHeight = (short)overlay.Height;

                    w = overlay.Width + (4 - overlay.Width % 4) % 4;
                    h = overlay.Height + (4 - overlay.Height % 4) % 4;

                    if (overlay.Width != w || overlay.Height != h)
                    {
                        Bitmap temp = new Bitmap(w, h);
                        using (Graphics g = Graphics.FromImage(temp))
                        {
                            g.Clear(Color.Transparent);
                            g.DrawImage(overlay, 0, 0);
                            g.Save();
                        }
                        overlay.Dispose();
                        overlay = temp;
                    }

                    OverlayImage = overlay;

                    data = overlay.LockBits(new Rectangle(0, 0, overlay.Width, overlay.Height), ImageLockMode.ReadOnly,
                                                     PixelFormat.Format32bppArgb);

                    pixels = new byte[overlay.Width * overlay.Height * 4];

                    Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                    overlay.UnlockBits(data);

                    for (int i = 0; i < pixels.Length; i += 4)
                    {
                        //Reverse Red/Blue
                        byte b = pixels[i];
                        pixels[i] = pixels[i + 2];
                        pixels[i + 2] = b;

                        //if (pixels[i] == 0 && pixels[i + 1] == 0 && pixels[i + 2] == 0)
                        //    pixels[i + 3] = 0; //Make Transparent

                    }

                    //OverlayFBytes = null;

                    switch (LMain.LibMode)
                    {
                        case LibraryEditor.Mir3Library.libMode.V2:
                            
                            int count = Squish.GetStorageRequirements(w, h, SquishFlags.Dxt5);

                            OverlayFBytes = new byte[count];
                            fixed (byte* dest = OverlayFBytes)

                            fixed (byte* source = pixels)
                            {
                                Squish.CompressImage((IntPtr)source, w, h, (IntPtr)dest, SquishFlags.Dxt5);
                            }
                            break;
                        case LibraryEditor.Mir3Library.libMode.Normal:

                            int count2 = Squish.GetStorageRequirements(w, h, SquishFlags.Dxt1);

                            OverlayFBytes = new byte[count2];
                            fixed (byte* dest = OverlayFBytes)

                            fixed (byte* source = pixels)
                            {
                                Squish.CompressImage((IntPtr)source, w, h, (IntPtr)dest, SquishFlags.Dxt1);
                            }

                            break;
                    }

                    //count = Squish.GetStorageRequirements(overlay.Width, overlay.Height, SquishFlags.Dxt1);

                    //OverlayFBytes = new byte[count];
                    //fixed (byte* dest = OverlayFBytes)
                    //fixed (byte* source = pixels)
                    //{
                    //    Squish.CompressImage((IntPtr)source, overlay.Width, overlay.Height, (IntPtr)dest, SquishFlags.Dxt1);
                    //}
                }

            }

            public unsafe void CreateImage(BinaryReader reader)
            {
                if (Position == 0) return;

                int w = Width + (4 - Width % 4) % 4;
                int h = Height + (4 - Height % 4) % 4;

                if (w == 0 || h == 0) return;

                Image = new Bitmap(w, h);
                BitmapData data = Image.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                reader.BaseStream.Seek(Position, SeekOrigin.Begin);

                FBytes = reader.ReadBytes(ImageDataSize);

                if (FBytes == null || FBytes.Length == 0 || FBytes.Length < ImageDataSize)
                {
                    Image.UnlockBits(data);
                    return;
                }
                
                switch (LibMode)
                {
                    case libMode.V2:
                        var decFormat = SquishFlags.Dxt1;

                        switch (textureFormat)
                        {
                            case TextureFormat.BC3:
                                decFormat = SquishFlags.Dxt5;
                                break;
                            case TextureFormat.BC1:
                            default:
                                decFormat = SquishFlags.Dxt1;
                                break;
                        }
                        fixed (byte* source = FBytes)
                            Squish.DecompressImage(data.Scan0, w, h, (IntPtr)source, decFormat);
                        break;
                    case libMode.Normal:
                        fixed (byte* source = FBytes)
                            Squish.DecompressImage(data.Scan0, w, h, (IntPtr)source, SquishFlags.Dxt1);
                        break;
                }

                byte* dest = (byte*)data.Scan0;

                for (int i = 0; i < h * w * 4; i += 4)
                {
                    //Reverse Red/Blue
                    byte b = dest[i];
                    dest[i] = dest[i + 2];
                    dest[i + 2] = b;
                }

                Image.UnlockBits(data);
                ImageValid = true;
            }
            public unsafe void CreateShadow(BinaryReader reader)
            {
                if (Position == 0) return;

                if (!ImageValid)
                    CreateImage(reader);

                int w = ShadowWidth + (4 - ShadowWidth % 4) % 4;
                int h = ShadowHeight + (4 - ShadowHeight % 4) % 4;

                if (w == 0 || h == 0) return;

                ShadowImage = new Bitmap(w, h);
                BitmapData data = ShadowImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                reader.BaseStream.Seek(Position + (LibMode == libMode.Normal || LibMode == libMode.V2 ? ImageDataSize : rImageDataSize), SeekOrigin.Begin);

                ShadowFBytes = reader.ReadBytes(ShadowDataSize);
                
                switch (LibMode)
                {
                    case libMode.V2:
                        fixed (byte* source = ShadowFBytes)
                            Squish.DecompressImage(data.Scan0, w, h, (IntPtr)source, SquishFlags.Dxt5);
                        break;
                    case libMode.Normal:
                        fixed (byte* source = ShadowFBytes)
                            Squish.DecompressImage(data.Scan0, w, h, (IntPtr)source, SquishFlags.Dxt1);
                        break;
                }

                byte* dest = (byte*)data.Scan0;

                for (int i = 0; i < h * w * 4; i += 4)
                {
                    //Reverse Red/Blue
                    byte b = dest[i];
                    dest[i] = dest[i + 2];
                    dest[i + 2] = b;
                }

                ShadowImage.UnlockBits(data);
                ShadowValid = true;
            }
            public unsafe void CreateOverlay(BinaryReader reader)
            {
                if (Position == 0) return;

                if (!ImageValid)
                    CreateImage(reader);

                int w = OverlayWidth + (4 - OverlayWidth % 4) % 4;
                int h = OverlayHeight + (4 - OverlayHeight % 4) % 4;

                if (w == 0 || h == 0) return;

                OverlayImage = new Bitmap(w, h);
                BitmapData data = OverlayImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                reader.BaseStream.Seek(Position + (LibMode == libMode.Normal || LibMode == libMode.V2 ? ImageDataSize : rImageDataSize) + (LibMode == libMode.Normal || LibMode == libMode.V2 ? ShadowDataSize : rShadowDataSize), SeekOrigin.Begin);

                OverlayFBytes = reader.ReadBytes(OverlayDataSize);
               
                switch (LibMode)
                {
                    case libMode.V2:
                        fixed (byte* source = OverlayFBytes)
                            Squish.DecompressImage(data.Scan0, w, h, (IntPtr)source, SquishFlags.Dxt5);
                        break;
                    case libMode.Normal:
                        fixed (byte* source = OverlayFBytes)
                            Squish.DecompressImage(data.Scan0, w, h, (IntPtr)source, SquishFlags.Dxt1);
                        break;
                }

                byte* dest = (byte*)data.Scan0;

                for (int i = 0; i < h * w * 4; i += 4)
                {
                    //Reverse Red/Blue
                    byte b = dest[i];
                    dest[i] = dest[i + 2];
                    dest[i + 2] = b;
                }

                OverlayImage.UnlockBits(data);
                OverlayValid = true;
            }

            public void CreatePreview()
            {
                if (Image == null)
                {
                    Preview = new Bitmap(1, 1);
                    return;
                }

                Preview = new Bitmap(64, 64);

                using (Graphics g = Graphics.FromImage(Preview))
                {
                    g.InterpolationMode = InterpolationMode.Low;//HighQualityBicubic
                    g.Clear(Color.Transparent);
                    int w = Math.Min((int)Width, 64);
                    int h = Math.Min((int)Height, 64);
                    g.DrawImage(Image, new Rectangle((64 - w) / 2, (64 - h) / 2, w, h), new Rectangle(0, 0, Width, Height), GraphicsUnit.Pixel);

                    g.Save();
                }
            }

            public void CreateShadowPreview()
            {
                if (ShadowImage == null)
                {
                    ShadowPreview = new Bitmap(1, 1);
                    return;
                }

                ShadowPreview = new Bitmap(64, 64);

                using (Graphics g = Graphics.FromImage(ShadowPreview))
                {
                    g.InterpolationMode = InterpolationMode.Low;//HighQualityBicubic
                    g.Clear(Color.Transparent);
                    int w = Math.Min((int)Width, 64);
                    int h = Math.Min((int)Height, 64);
                    g.DrawImage(ShadowImage, new Rectangle((64 - w) / 2, (64 - h) / 2, w, h), new Rectangle(0, 0, Width, Height), GraphicsUnit.Pixel);

                    g.Save();
                }
            }

            public void CreateOverlayPreview()
            {
                if (OverlayImage == null)
                {
                    OverlayPreview = new Bitmap(1, 1);
                    return;
                }

                OverlayPreview = new Bitmap(64, 64);

                using (Graphics g = Graphics.FromImage(OverlayPreview))
                {
                    g.InterpolationMode = InterpolationMode.Low;//HighQualityBicubic
                    g.Clear(Color.Transparent);
                    int w = Math.Min((int)Width, 64);
                    int h = Math.Min((int)Height, 64);
                    g.DrawImage(OverlayImage, new Rectangle((64 - w) / 2, (64 - h) / 2, w, h), new Rectangle(0, 0, Width, Height), GraphicsUnit.Pixel);

                    g.Save();
                }
            }

            public void SaveHeader(BinaryWriter writer)
            {

                if (LMain.LibMode == libMode.V2)
                {
                    writer.Write(SubItems.Count);
                    writer.Write(Position);
                }
                else
                    writer.Write(Position);
                
                writer.Write(Width);
                writer.Write(Height);
                writer.Write(OffSetX);
                writer.Write(OffSetY);

                writer.Write((byte)textureFormat);

                writer.Write(ShadowType);
                writer.Write(ShadowWidth);
                writer.Write(ShadowHeight);
                writer.Write(ShadowOffSetX);
                writer.Write(ShadowOffSetY);

                writer.Write(OverlayWidth);
                writer.Write(OverlayHeight);

                if (LibMode == libMode.V2)
                {
                    foreach (var item in SubItems)
                    {
                        item.SaveHeader(writer);
                    }
                }

            }
            internal void Rebuild()
            {
                if (this.Image != null)
                {

                    BitmapData data = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadOnly,
                                                 PixelFormat.Format32bppArgb);

                    byte[] pixels = new byte[Image.Width * Image.Height * 4];

                    Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                    Image.UnlockBits(data);

                    textureFormat = TextureFormat.BC1;
                    for (int i = 0; i < pixels.Length; i += 4)
                    {
                        //Reverse Red/Blue
                        byte b = pixels[i];
                        pixels[i] = pixels[i + 2];
                        pixels[i + 2] = b;

                        if (pixels[i] == 0 && pixels[i + 1] == 0 && pixels[i + 2] == 0)
                        {
                            if (textureProcessMode == TextureProcessMode.Mir)
                                pixels[i + 3] = 0; //Make Transparent
                            else if (textureProcessMode == TextureProcessMode.Woool)
                            {
                                pixels[i] = 1;
                                pixels[i + 1] = 1;
                                pixels[i + 2] = 1;
                            }
                        }

                        if (pixels[i + 3] < 255 && pixels[i + 3] > 0)
                            textureFormat = TextureFormat.BC3;

                    }

                    if (ForcedFormat != TextureFormat.Auto)
                        textureFormat = ForcedFormat;

                    switch (LMain.LibMode)
                    {
                        case libMode.V2:
                            var decFormat = SquishFlags.Dxt1;

                            switch (textureFormat)
                            {
                                case TextureFormat.BC3:
                                    decFormat = SquishFlags.Dxt5;
                                    break;
                                case TextureFormat.BC1:
                                default:
                                    decFormat = SquishFlags.Dxt1;
                                    break;
                            }
                            int count = Squish.GetStorageRequirements(Image.Width, Image.Height, decFormat);

                            FBytes = new byte[count];
                            unsafe
                            {
                                fixed (byte* dest = FBytes)
                                fixed (byte* source = pixels)
                                {
                                    Squish.CompressImage((IntPtr)source, Image.Width, Image.Height, (IntPtr)dest, decFormat);
                                }
                            }
                            break;
                        case libMode.Normal:
                            int count2 = Squish.GetStorageRequirements(Image.Width, Image.Height, SquishFlags.Dxt1);

                            FBytes = new byte[count2];
                            unsafe
                            {
                                fixed (byte* dest = FBytes)
                                fixed (byte* source = pixels)
                                {
                                    Squish.CompressImage((IntPtr)source, Image.Width, Image.Height, (IntPtr)dest, SquishFlags.Dxt1);
                                }
                            }
                            break;
                    }

                    //Rebuild
                    if (LMain.LibMode == libMode.V2 && SubItems.Count > 0)
                    {
                        foreach (var item in SubItems)
                        {
                            item.Rebuild();
                        }
                    }

                }
                if (this.ShadowImage != null)
                {

                    BitmapData data = ShadowImage.LockBits(new Rectangle(0, 0, ShadowImage.Width, ShadowImage.Height), ImageLockMode.ReadOnly,
                                                 PixelFormat.Format32bppArgb);

                    byte[] pixels = new byte[ShadowImage.Width * ShadowImage.Height * 4];

                    Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                    ShadowImage.UnlockBits(data);

                    for (int i = 0; i < pixels.Length; i += 4)
                    {
                        //Reverse Red/Blue
                        byte b = pixels[i];
                        pixels[i] = pixels[i + 2];
                        pixels[i + 2] = b;

                        if (pixels[i] == 0 && pixels[i + 1] == 0 && pixels[i + 2] == 0)
                        {
                            if (textureProcessMode == TextureProcessMode.Mir)
                                pixels[i + 3] = 0; //Make Transparent
                            else if (textureProcessMode == TextureProcessMode.Woool)
                            {
                                pixels[i] = 1;
                                pixels[i + 1] = 1;
                                pixels[i + 2] = 1;
                            }
                        }

                    }

                    switch (LMain.LibMode)
                    {
                        case libMode.V2:
                            int count1 = Squish.GetStorageRequirements(ShadowImage.Width, ShadowImage.Height, SquishFlags.Dxt5);

                            ShadowFBytes = new byte[count1];
                            unsafe
                            {
                                fixed (byte* dest = ShadowFBytes)
                                fixed (byte* source = pixels)
                                {
                                    Squish.CompressImage((IntPtr)source, ShadowImage.Width, ShadowImage.Height, (IntPtr)dest, SquishFlags.Dxt5);
                                }
                            }
                            break;
                        case libMode.Normal:
                            int count2 = Squish.GetStorageRequirements(ShadowImage.Width, ShadowImage.Height, SquishFlags.Dxt1);

                            ShadowFBytes = new byte[count2];
                            unsafe
                            {
                                fixed (byte* dest = ShadowFBytes)
                                fixed (byte* source = pixels)
                                {
                                    Squish.CompressImage((IntPtr)source, ShadowImage.Width, ShadowImage.Height, (IntPtr)dest, SquishFlags.Dxt1);
                                }
                            }
                            break;
                    }

                }
                if (this.OverlayImage != null)
                {

                    BitmapData data = OverlayImage.LockBits(new Rectangle(0, 0, OverlayImage.Width, OverlayImage.Height), ImageLockMode.ReadOnly,
                                                 PixelFormat.Format32bppArgb);

                    byte[] pixels = new byte[OverlayImage.Width * OverlayImage.Height * 4];

                    Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                    OverlayImage.UnlockBits(data);

                    for (int i = 0; i < pixels.Length; i += 4)
                    {
                        //Reverse Red/Blue
                        byte b = pixels[i];
                        pixels[i] = pixels[i + 2];
                        pixels[i + 2] = b;

                        if (pixels[i] == 0 && pixels[i + 1] == 0 && pixels[i + 2] == 0)
                        {
                            if (textureProcessMode == TextureProcessMode.Mir)
                                pixels[i + 3] = 0; //Make Transparent
                            else if (textureProcessMode == TextureProcessMode.Woool)
                            {
                                pixels[i] = 1;
                                pixels[i + 1] = 1;
                                pixels[i + 2] = 1;
                            }
                        }
                    }

                    switch (LMain.LibMode)
                    {
                        case libMode.V2:
                            int count = Squish.GetStorageRequirements(OverlayImage.Width, OverlayImage.Height, SquishFlags.Dxt5);

                            OverlayFBytes = new byte[count];
                            unsafe
                            {
                                fixed (byte* dest = OverlayFBytes)
                                fixed (byte* source = pixels)
                                {
                                    Squish.CompressImage((IntPtr)source, OverlayImage.Width, OverlayImage.Height, (IntPtr)dest, SquishFlags.Dxt5);
                                }
                            }
                            break;
                        case libMode.Normal:
                            int count2 = Squish.GetStorageRequirements(OverlayImage.Width, OverlayImage.Height, SquishFlags.Dxt1);

                            OverlayFBytes = new byte[count2];
                            unsafe
                            {
                                fixed (byte* dest = OverlayFBytes)
                                fixed (byte* source = pixels)
                                {
                                    Squish.CompressImage((IntPtr)source, OverlayImage.Width, OverlayImage.Height, (IntPtr)dest, SquishFlags.Dxt1);
                                }
                            }
                            break;
                    }

                }
            }

            #region IDisposable Support

            public bool IsDisposed { get; private set; }
            public int rImageDataSize { get; private set; }
            public int rShadowDataSize { get; private set; }
            public int rOverlayDataSize { get; private set; }

            public void Dispose(bool disposing)
            {
                if (disposing)
                {
                    IsDisposed = true;

                    Position = 0;

                    Width = 0;
                    Height = 0;
                    OffSetX = 0;
                    OffSetY = 0;

                    ShadowWidth = 0;
                    ShadowHeight = 0;
                    ShadowOffSetX = 0;
                    ShadowOffSetY = 0;

                    OverlayWidth = 0;
                    OverlayHeight = 0;
                }

            }

            public void Dispose()
            {
                Dispose(!IsDisposed);
                GC.SuppressFinalize(this);
            }

            ~Mir3Image()
            {
                Dispose(false);
            }

            #endregion

        }
    }
}
