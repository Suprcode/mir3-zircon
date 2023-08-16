using ManagedSquish;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryEditor;

namespace LibraryEditor
{
    public class MILLibrary
    {
        private readonly string _fileName;

        public MILImage[] Images;

        private BinaryReader _bReader;
        private int _count;
        private FileStream _fStream;
        private bool _initialized;
        
        public MILLibrary(string filename)
        {
            _fileName = Path.ChangeExtension(filename, null);
            Initialize();
        }

        public void Initialize()
        {
            _initialized = true;
            if (!File.Exists(_fileName + ".mil")) return;
            _fStream = new FileStream(_fileName + ".mil", FileMode.Open, FileAccess.ReadWrite);
            _bReader = new BinaryReader(_fStream);
            LoadImageInfo();

            for (int i = 0; i < _count; i++)
            {
                CheckMImage(i);
            }

            string fname = _fileName + ".mil";
            
        }

        private void LoadImageInfo()
        {

            _fStream.Seek(0, SeekOrigin.Begin);
            
            _count = _bReader.ReadInt32();
            
            Images = new MILImage[_count];

            for (int index = 0; index < this.Images.Length; ++index)
            {
                int position = _bReader.ReadInt32();
                if (position == 0)
                    this._bReader.BaseStream.Seek(16L, SeekOrigin.Current);
                else
                    this.Images[index] = new MILImage(position, this._bReader);
            }
            
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
            
            MILImage image = Images[index];

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
                        MILImage image = Images[i];

                        if (image != null)
                        {
                            library.Images[i] = new Mir3Library.Mir3Image(image.Image, null, null) { OffSetX = (short)image.Offset.X, OffSetY = (short)image.Offset.Y };
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

    public class MILImage
    {
        public const int HeaderSize = 20;

        public int Position { get; private set; }
        public Size Size { get; private set; }
        public Point Offset { get; private set; }
        
        public Bitmap Image;
        public bool Error;
        public unsafe byte* Data;
        public bool TextureValid { get; set; }

        public int DataSize
        {
            get
            {
                return (this.Size.Width + (4 - this.Size.Width % 4) % 4) * (this.Size.Height + (4 - this.Size.Height % 4) % 4) / 2;
            }
        }

        public MILImage(int position, BinaryReader bReader)
        {

            this.Position = position;
            this.Size = new Size(bReader.ReadInt32(), bReader.ReadInt32());
            this.Offset = new Point(bReader.ReadInt32(), bReader.ReadInt32());
            
        }

        public unsafe void CreateTexture(BinaryReader bReader)
        {
            if (this.Position == 0 || this.Error)
                return;
            try
            {
                bReader.BaseStream.Seek((long)this.Position, SeekOrigin.Begin);
                int width = this.Size.Width + (4 - this.Size.Width % 4) % 4;
                int height = this.Size.Height + (4 - this.Size.Height % 4) % 4;
                if (width == 0 || height == 0)
                    return;

                Image = new Bitmap(width, height);

                BitmapData data = Image.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb
                );

                var buffer = bReader.ReadBytes(this.DataSize);

                fixed (byte* source = buffer)
                    Squish.DecompressImage(data.Scan0, width, height, (IntPtr)source, SquishFlags.Dxt1);

                byte* dest = (byte*)data.Scan0;

                for (int i = 0; i < height * width * 4; i += 4)
                {
                    byte b = dest[i];
                    dest[i] = dest[i + 2];
                    dest[i + 2] = b;
                }

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
