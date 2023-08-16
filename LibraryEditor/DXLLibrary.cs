using ManagedSquish;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LibraryEditor;

namespace LibraryEditor
{
    public class DXLLibrary
    {
        private readonly string _fileName;

        public DXLImage[] Images;

        private BinaryReader _bReader;
        private int _count;
        private FileStream _fStream;
        private bool _initialized;

        public bool Loaded;

        public DXLLibrary(string filename)
        {
            Loaded = false;
            _fileName = Path.ChangeExtension(filename, null);
            Initialize();
        }

        public void Initialize()
        {
            _initialized = true;
            if (!File.Exists(_fileName + ".dxl")) return;
            _fStream = new FileStream(_fileName + ".dxl", FileMode.Open, FileAccess.ReadWrite);
            _bReader = new BinaryReader(_fStream);
            LoadImageInfo();

            for (int i = 0; i < _count; i++)
            {
                CheckMImage(i);
            }

            string fname = _fileName + ".dxl";

        }

        private void LoadImageInfo()
        {

            _fStream.Seek(0, SeekOrigin.Begin);

            _count = _bReader.ReadInt32();

            Images = new DXLImage[_count];

            for (int index = 0; index < this.Images.Length; ++index)
            {
                if (_bReader.ReadBoolean())
                    this.Images[index] = new DXLImage(_bReader)
                    {
                        Index = index
                    };
            }
            
            Loaded = true;

        }

        public void Close()
        {
            if (_fStream != null)
                _fStream.Dispose();
            if (_bReader != null)
                _bReader.Dispose();
        }

        public void CheckMImage(int index)
        {
            if (!_initialized)
                Initialize();

            if (index < 0 || index >= Images.Length) return;

            DXLImage image = Images[index];

            if (image != null && image.Image == null)
                image.CreateTexture(_bReader);

        }

        public void ToMLibrary()
        {
            string fileName = Path.ChangeExtension(_fileName, ".Zl");

            if (File.Exists(fileName))
                File.Delete(fileName);

            Mir3Library library = new Mir3Library(fileName) { Images = new List<Mir3Library.Mir3Image>() };

            library.Images.AddRange(Enumerable.Repeat(new Mir3Library.Mir3Image(), Images.Length));

            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 16 };

            try
            {

                Parallel.For(0, Images.Length, options, i =>
                {
                    try
                    {
                        DXLImage image = Images[i];

                        if (image != null)
                        {
                            library.Images[i] = new Mir3Library.Mir3Image(image.Image, null, null) { OffSetX = (short)image.OffSetX, OffSetY = (short)image.OffSetY };
                        }
                    }
                    catch
                    {
                    }

                });
            }
            catch (System.Exception)
            {
                throw;
            }
            finally
            {
                library.Save(fileName, null, false, false);
            }

        }
    }

    public class DXLImage
    {

        public const int HeaderSize = 24;

        public bool Error;

        public int Index { get; set; }

        public int Position { get; set; }
        public Size Size => new Size(Width, Height);
        public int Width { get; set; }

        public int Height { get; set; }

        public int OffSetX { get; set; }

        public int OffSetY { get; set; }

        public bool TextureValid { get; private set; }

        public byte[] RawData;
        public int DataSize;
        public Bitmap Image;

        public DXLImage(BinaryReader bReader)
        {

            this.Position = bReader.ReadInt32();
            this.Width = bReader.ReadInt32();
            this.Height = bReader.ReadInt32();
            this.OffSetX = bReader.ReadInt32();
            this.OffSetY = bReader.ReadInt32();
            this.DataSize = bReader.ReadInt32();

        }

        public void Load(BinaryReader bReader)
        {
            bReader.BaseStream.Seek((long)this.Position, SeekOrigin.Begin);
            this.RawData = bReader.ReadBytes(this.DataSize);
            using (MemoryStream memoryStream1 = new MemoryStream(this.RawData))
            {
                using (MemoryStream memoryStream2 = new MemoryStream())
                {
                    using (DeflateStream deflateStream = new DeflateStream((Stream)memoryStream1, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo((Stream)memoryStream2);
                        this.RawData = memoryStream2.ToArray();
                    }
                }
            }
        }

        public unsafe void CreateTexture(BinaryReader bReader)
        {
            if (this.Position == 0 || this.Error)
                return;
            try
            {

                lock (bReader)
                {
                    if (this.RawData == null)
                        this.Load(bReader);
                }

                int width = this.Size.Width;
                int height = this.Size.Height;

                if (width == 0 || height == 0)
                    return;

                Image = new Bitmap(width, height);

                BitmapData data = Image.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb
                );

                Marshal.Copy(RawData, 0, data.Scan0, RawData.Length);

                Image.UnlockBits(data);

                this.TextureValid = true;

            }
            catch
            {
                this.Error = true;
            }
        }

    }
}
