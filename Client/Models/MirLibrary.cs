using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Library;
using Sentry;
using SlimDX;
using SlimDX.Direct3D9;
using SlimDX.XAudio2;
using static Client.Envir.MirLibrary;

namespace Client.Envir
{
    public static class MirLibraryFunctions
    {
        #region GZip functions
        public static byte[] GZipHeaderBytes = { 0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 4, 0 };
        public static byte[] GZipLevel10HeaderBytes = { 0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 2, 0 };

        public static bool IsPossiblyGZippedBytes(this byte[] a)
        {
            var yes = a.Length > 10;

            if (!yes)
            {
                return false;
            }

            var header = SubArray(a, 0, 10);

            return header.SequenceEqual(GZipHeaderBytes) || header.SequenceEqual(GZipLevel10HeaderBytes);
        }
        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
        #endregion
    }
    public sealed class MirLibrary : IDisposable
    {
        public readonly object LoadLocker = new object();

        public string FileName;

        private FileStream _FStream;
        private BinaryReader _BReader;

        public bool Loaded, Loading;

        public MirImage[] Images;

        public enum libMode
        {
            Normal,
            CrystalLib,
            V2, // with sub images for Woool format
        }
        public enum TextureFormat : byte
        {
            BC1,
            BC3,
        }

        public libMode LibMode { get; private set; } = libMode.Normal;

        private int[] _indexList;

        private static byte[] checkSum1 = new byte[] { 25, 69, 78, 32 };
        private static byte[] checkSum3 = new byte[] { 0x5A, 0x6E, 0x56, 0x32 };

        public static bool TextureIsNull(Texture texture)
        {

            if (texture == null) return true;

            if (texture.Disposed) return true;

            return false;

        }

        public MirLibrary(string fileName)
        {
            if(!File.Exists(fileName))
            {
                Loaded = true;
                return;
            }

            FileName = fileName;
            _FStream = File.OpenRead(fileName);
            _BReader = new BinaryReader(_FStream);

            var Pext = Path.GetExtension(fileName).ToString().ToLower();

            LibMode = Pext == ".lib" ? libMode.CrystalLib : libMode.Normal;
        }
        public void ReadLibrary()
        {
            lock (LoadLocker)
            {
                if (Loading) return;
                Loading = true;
            }

            if (_BReader == null)
            {
                Loaded = true;
                return;
            }

            using (MemoryStream mstream = new MemoryStream(LibMode == libMode.CrystalLib ? _BReader.ReadBytes((int)_BReader.BaseStream.Length) : _BReader.ReadBytes(_BReader.ReadInt32())))
            using (BinaryReader reader = new BinaryReader(mstream))
            {

                if (LibMode == libMode.CrystalLib)
                {
                    //crystal Lib format
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);

                    var CurrentVersion = reader.ReadInt32();
                    if (CurrentVersion != 2)
                    {
                        //cant use a directx based error popup cause it could be the lib file containing the interface is invalid :(
                        System.Windows.Forms.MessageBox.Show("Wrong version, expecting lib version: " + 2.ToString() + " found version: " + CurrentVersion.ToString() + ".", FileName, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error, System.Windows.Forms.MessageBoxDefaultButton.Button1);
                        System.Windows.Forms.Application.Exit();
                        return;
                    }

                    var count = reader.ReadInt32();
                    Images = new MirImage[count];

                    
                    _indexList = new int[count];

                    for (int i = 0; i < count; i++)
                        _indexList[i] = reader.ReadInt32();

                    for (int index = 0; index < count; ++index)
                    {
                        Images[index] = ((MirImage)null);
                    }

                    Loaded = true;

                    return;
                }
                else
                {
                    //zircon lib & v2 lib
                    var count = reader.ReadInt32();
                    Images = new MirImage[count];

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

                    _indexList = new int[count];

                    for (int i = 0; i < Images.Length; i++)
                    {
                        if (!reader.ReadBoolean()) continue;

                        _indexList[i] = (int)reader.BaseStream.Position;

                        Images[i] = new MirImage(reader, LibMode, FileName);
                    }

                }
            }



            //using (MemoryStream mstream = new MemoryStream(_BReader.ReadBytes(_BReader.ReadInt32())))
            //using (BinaryReader reader = new BinaryReader(mstream))
            //{
            //    Images = new MirImage[reader.ReadInt32()];

            //    for (int i = 0; i < Images.Length; i++)
            //    {
            //        if (!reader.ReadBoolean()) continue;

            //        Images[i] = new MirImage(reader);
            //    }
            //}


            Loaded = true;
        }

        public Size GetSize(int index)
        {
            if (!CheckImage(index)) return Size.Empty;

            return new Size(Images[index].Width, Images[index].Height);
        }
        public Point GetOffSet(int index)
        {
            if (!CheckImage(index)) return Point.Empty;

            return new Point(Images[index].OffSetX, Images[index].OffSetY);
        }
        public MirImage GetImage(int index)
        {
            if (!CheckImage(index)) return null;

            return Images[index];
        }
        public MirImage CreateImage(int index, ImageType type)
        {
            if (!CheckImage(index)) return null;

            MirImage image = Images[index];

            Texture texture;

            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_BReader);
                    texture = image.Image;
                    break;
                case ImageType.Shadow:
                    if (!image.ShadowValid) image.CreateShadow(_BReader);
                    texture = image.Shadow;
                    break;
                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;
                    break;
                default:
                    return null;
            }

            if (texture == null) return null;

            return image;
        }

        private bool CheckImage(int index)
        {

            if (LibMode == libMode.CrystalLib)
            {

                if (!Loaded || this.Images == null)//|| index < 0 || index >= Images.Length
                    ReadLibrary();

                if (index < 0 || (Images != null && index >= Images.Length)) return false;

                if (this.Images[index] == null || (this.Images[index].IsDisposed && this.Images[index].Position == 0))
                {
                    this._BReader.BaseStream.Position = (long)_indexList[index];
                    this.Images[index] = new MirImage(this._BReader, LibMode, FileName);
                }

                if (!Loaded) return false;

                var image = Images[index];

                if (image.ImageValid)
                    return true;

                this._BReader.BaseStream.Seek((long)(_indexList[index] + 12), SeekOrigin.Begin);
                image.CreateImage(this._BReader);

            }

            if (!Loaded) ReadLibrary();

            while (!Loaded)
                Thread.Sleep(1);

            if (Images != null)
                return index >= 0 && index < Images.Length && Images[index] != null;
            else
                return false;
        }

        public bool VisiblePixel(int index, Point location, bool accurate = true, bool offSet = false)
        {
            if (!CheckImage(index)) return false;

            MirImage image = Images[index];

            if (offSet)
                location = new Point(location.X - image.OffSetX, location.Y - image.OffSetY);

            return image.VisiblePixel(location, accurate);
        }

        public void Draw(int index, float x, float y, Color4 colour, Rectangle area, float opacity, ImageType type, byte shadow = 0)
        {
            if (!CheckImage(index)) return;

            MirImage image = Images[index];

            Texture texture;

            float oldOpacity = DXManager.Opacity;
            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_BReader);
                    texture = image.Image;
                    break;
                case ImageType.Shadow:
                    if (!image.ShadowValid) image.CreateShadow(_BReader);
                    texture = image.Shadow;

                    if (texture == null)
                    {
                        if (!image.ImageValid) image.CreateImage(_BReader);
                        texture = image.Image;

                        switch (image.ShadowType)
                        {
                            case 177:
                            case 176:
                            case 49:
                                Matrix m = Matrix.Scaling(1F, 0.5f, 0);

                                m.M21 = -0.50F;
                                DXManager.Sprite.Transform = m*Matrix.Translation(x + image.Height/2, y, 0);

                                DXManager.Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.None);
                                if (oldOpacity != 0.5F) DXManager.SetOpacity(0.5F);

                                DXManager.Sprite.Draw(texture, Vector3.Zero, Vector3.Zero, Color.Black);
                                CEnvir.DPSCounter++;

                                DXManager.SetOpacity(oldOpacity);
                                DXManager.Sprite.Transform = Matrix.Identity;
                                DXManager.Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);

                                image.ExpireTime = Time.Now + Config.CacheDuration;
                                break;
                            case 50:
                                if (oldOpacity != 0.5F) DXManager.SetOpacity(0.5F);

                                DXManager.Sprite.Draw(texture, Vector3.Zero, new Vector3(x, y, 0), Color.Black);
                                CEnvir.DPSCounter++;
                                DXManager.SetOpacity(oldOpacity);

                                image.ExpireTime = Time.Now + Config.CacheDuration;
                                break;
                        }
                        return;
                    }
                    break;
                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;
                    break;
                default:
                    return;
            }

            if (texture == null) return;

            DXManager.SetOpacity(opacity);

            DXManager.Sprite.Draw(texture, area, Vector3.Zero, new Vector3(x, y, 0), colour);
            CEnvir.DPSCounter++;
            DXManager.SetOpacity(oldOpacity);

            image.ExpireTime = Time.Now + Config.CacheDuration;
        }
        public void Draw(int index, float x, float y, Color4 colour, bool useOffSet, float opacity, ImageType type, float scale = 1F, int DrawSubIndex = -1)
        {
            if (!CheckImage(index)) return;

            MirImage image = Images[index];
            
            Texture texture;

            Matrix scaling, rotationZ, translation;

            float oldOpacity = DXManager.Opacity;
            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_BReader);

                    if (DrawSubIndex > -1 && image.SubItems.Count > 0)
                    {
                        foreach (var subItem in image.SubItems)
                        {
                            if (subItem.ImageValid) continue;

                            subItem.CreateImage(_BReader);

                        }

                        if (image.SubItems.Count > DrawSubIndex && image.SubItems[DrawSubIndex] != null && image.SubItems[DrawSubIndex].ImageValid)
                        {
                            texture = image.SubItems[DrawSubIndex].Image;
                            image.SubItems[DrawSubIndex].ExpireTime = Time.Now + Config.CacheDuration;
                        }
                        else
                            texture = image.Image;//we return first frame index
                    }
                    else
                        texture = image.Image;

                    if (useOffSet)
                    {
                        x += image.OffSetX;
                        y += image.OffSetY;
                    }
                    break;
                case ImageType.Shadow:
                    {
                        if (!image.ShadowValid) image.CreateShadow(_BReader);
                        texture = image.Shadow;

                        if (useOffSet)
                        {
                            x += image.ShadowOffSetX;
                            y += image.ShadowOffSetY;
                        }

                        if (texture == null)
                        {
                            if (!image.ImageValid) image.CreateImage(_BReader);
                            texture = image.Image;

                            switch (image.ShadowType)
                            {
                                case 177:
                                case 176:
                                case 49:
                                    Matrix m = Matrix.Scaling(1F * scale, 0.5f * scale, 0);

                                    m.M21 = -0.50F;
                                    DXManager.Sprite.Transform = m * Matrix.Translation(x + image.Height / 2, y, 0);

                                    DXManager.Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.None);
                                    if (oldOpacity != 0.5F) DXManager.SetOpacity(0.5F);

                                    DXManager.Sprite.Draw(texture, Vector3.Zero, Vector3.Zero, Color.Black);
                                    CEnvir.DPSCounter++;

                                    DXManager.SetOpacity(oldOpacity);
                                    DXManager.Sprite.Transform = Matrix.Identity;
                                    DXManager.Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);

                                    image.ExpireTime = Time.Now + Config.CacheDuration;
                                    break;
                                case 50:
                                    if (oldOpacity != 0.5F) DXManager.SetOpacity(0.5F);

                                    scaling = Matrix.Scaling(scale, scale, 0f);
                                    rotationZ = Matrix.RotationZ(0F);
                                    translation = Matrix.Translation(x + (image.Width / 2), y + (image.Height / 2), 0);

                                    DXManager.Sprite.Transform = scaling * rotationZ * translation;

                                    DXManager.Sprite.Draw(texture, Vector3.Zero, new Vector3((image.Width / 2) * -1, (image.Height / 2) * -1, 0), Color.Black);

                                    CEnvir.DPSCounter++;
                                    DXManager.SetOpacity(oldOpacity);

                                    image.ExpireTime = Time.Now + Config.CacheDuration;
                                    break;
                            }

                            return;
                        }
                    }
                    break;
                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;

                    if (useOffSet)
                    {
                        x += image.OffSetX;
                        y += image.OffSetY;
                    }
                    break;
                default:
                    return;
            }

            if (texture == null) return;

            scaling = Matrix.Scaling(scale, scale, 0f);
            rotationZ = Matrix.RotationZ(0F);
            translation = Matrix.Translation(x + (image.Width / 2), y + (image.Height / 2), 0);

            DXManager.SetOpacity(opacity);

            DXManager.Sprite.Transform = scaling * rotationZ * translation;

            DXManager.Sprite.Draw(texture, Vector3.Zero, new Vector3((image.Width / 2) * -1, (image.Height / 2) * -1, 0), colour);

            DXManager.Sprite.Transform = Matrix.Identity;

            CEnvir.DPSCounter++;
            
            DXManager.SetOpacity(oldOpacity);

            image.ExpireTime = Time.Now + Config.CacheDuration;
        }
        public void DrawBlend(int index, float size, Color4 colour, float x, float y, float angle, float opacity, ImageType type, bool useOffSet = false, byte shadow = 0)
        {
            if (!CheckImage(index)) return;

            MirImage image = Images[index];

            Texture texture;

            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_BReader);
                    texture = image.Image;
                    if (useOffSet)
                    {
                        x += image.OffSetX;
                        y += image.OffSetY;
                    }
                    break;
                case ImageType.Shadow:
                    return;
                /*     if (!image.ShadowValid) image.CreateShadow(_BReader);
                     texture = image.Shadow;

                     if (useOffSet)
                     {
                         x += image.ShadowOffSetX;
                         y += image.ShadowOffSetY;
                     }
                     break;*/
                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;

                    if (useOffSet)
                    {
                        x += image.OffSetX;
                        y += image.OffSetY;
                    }
                    break;
                default:
                    return;
            }
            if (texture == null) return;


            bool oldBlend = DXManager.Blending;
            float oldRate = DXManager.BlendRate;

            DXManager.SetBlend(true, opacity);

            var scaling = Matrix.Scaling(size, size, 0f);
            var rotationZ = Matrix.RotationZ(angle);
            var translation = Matrix.Translation(x + (image.Width / 2), y + (image.Height / 2), 0);

            DXManager.Sprite.Transform = scaling * rotationZ * translation;

            DXManager.Sprite.Draw(texture, Vector3.Zero, new Vector3((image.Width / 2) * -1, (image.Height / 2) * -1, 0), colour);

            DXManager.Sprite.Transform = Matrix.Identity;

            CEnvir.DPSCounter++;

            DXManager.SetBlend(oldBlend, oldRate);

            image.ExpireTime = Time.Now + Config.CacheDuration;
        }
        public void DrawBlend(int index, float x, float y, Color4 colour, bool useOffSet, float rate, ImageType type, byte shadow = 0, int DrawSubIndex = -1)
        {
            if (!CheckImage(index)) return;

            MirImage image = Images[index];

            Texture texture;

            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_BReader);

                    if (DrawSubIndex > -1)
                    {
                        foreach (var subItem in image.SubItems)
                        {
                            if (subItem.ImageValid) continue;

                            subItem.CreateImage(_BReader);

                        }

                        if (image.SubItems.Count > DrawSubIndex && image.SubItems[DrawSubIndex] != null && image.SubItems[DrawSubIndex].ImageValid)
                            texture = image.SubItems[DrawSubIndex].Image;
                        else
                            texture = image.Image;//we return first frame index
                    }
                    else
                        texture = image.Image;

                    if (useOffSet)
                    {
                        x += image.OffSetX;
                        y += image.OffSetY;
                    }
                    break;
                case ImageType.Shadow:
                    return;
               /*     if (!image.ShadowValid) image.CreateShadow(_BReader);
                    texture = image.Shadow;

                    if (useOffSet)
                    {
                        x += image.ShadowOffSetX;
                        y += image.ShadowOffSetY;
                    }
                    break;*/
                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;

                    if (useOffSet)
                    {
                        x += image.OffSetX;
                        y += image.OffSetY;
                    }
                    break;
                default:
                    return;
            }
            if (texture == null) return;


            bool oldBlend = DXManager.Blending;
            float oldRate = DXManager.BlendRate;

            DXManager.SetBlend(true, rate);
            
            DXManager.Sprite.Draw(texture, Vector3.Zero, new Vector3(x, y, 0), colour);
            CEnvir.DPSCounter++;

            DXManager.SetBlend(oldBlend, oldRate);

            image.ExpireTime = Time.Now + Config.CacheDuration;
        }

        public void DrawSpecialBlend(int index, float x, float y, Color4 colour, bool useOffSet, float rate, ImageType type, byte shadow = 0, int DrawSubIndex = -1)
        {
            if (!CheckImage(index)) return;

            MirImage image = Images[index];

            Texture texture;

            //strange dx bug fix
            switch (type)
            {
                case ImageType.Image:
                    if (TextureIsNull(image.Image))
                        image.ImageValid = false;
                    break;
                case ImageType.Overlay:
                    if (TextureIsNull(image.Overlay))
                        image.OverlayValid = false;
                    break;
                case ImageType.Shadow:
                    if (TextureIsNull(image.Shadow))
                        image.ShadowValid = false;
                    break;
            }


            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_BReader);

                    if (DrawSubIndex > -1)
                    {
                        foreach (var subItem in image.SubItems)
                        {
                            if (subItem.ImageValid) continue;

                            subItem.CreateImage(_BReader);

                        }

                        if (image.SubItems.Count > DrawSubIndex && image.SubItems[DrawSubIndex] != null && image.SubItems[DrawSubIndex].ImageValid)
                            texture = image.SubItems[DrawSubIndex].Image;
                        else
                            texture = image.Image;//we return first frame index
                    }
                    else
                        texture = image.Image;

                    if (useOffSet)
                    {
                        x += image.OffSetX;
                        y += image.OffSetY;
                    }
                    break;
                case ImageType.Shadow:
                    return;
                /*     if (!image.ShadowValid) image.CreateShadow(_BReader);
                     texture = image.Shadow;

                     if (useOffSet)
                     {
                         x += image.ShadowOffSetX;
                         y += image.ShadowOffSetY;
                     }
                     break;*/
                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;

                    if (useOffSet)
                    {
                        x += image.OffSetX;
                        y += image.OffSetY;
                    }
                    break;
                default:
                    return;
            }
            if (texture == null) return;

            bool oldBlend = DXManager.Blending;
            float oldRate = DXManager.BlendRate;

            DXManager.SetSpecialBlend(true, rate);

            //normal draw
            DXManager.Draw(texture, Vector3.Zero, new Vector3(x, y, 0), colour);
         
            //old draw call
            //DXManager.Draw(texture, Vector3.Zero, new Vector3(x, y, 0), colour);

            CEnvir.DPSCounter++;

            DXManager.SetSpecialBlend(oldBlend, oldRate, true);

            image.ExpireTime = Time.Now + Config.CacheDuration;
        }


        #region IDisposable Support

        public bool IsDisposed { get; private set; }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsDisposed = true;

                foreach (MirImage image in Images)
                    image.Dispose();


                Images = null;


                _FStream?.Dispose();
                _FStream = null;

                _BReader?.Dispose();
                _BReader = null;

                Loading = false;
                Loaded = false;
            }

        }

        ~MirLibrary()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

    public enum ImageType
    {
        Image,
        Shadow,
        Overlay,
    }

    public sealed class MirImage : IDisposable
    {
        public int Position;
        private BinaryReader _bReader;
        public string FileName { get; private set; }

        #region Texture

        public MirLibrary.libMode LibMode { get; private set; }
        public List<MirSubImage> SubItems = new List<MirSubImage>();
        public int SubCounter;
        public TextureFormat textureFormat = TextureFormat.BC1;

        public short Width;
        public short Height;
        public short OffSetX;
        public short OffSetY;
        public byte ShadowType;
        public Texture Image;
        public bool ImageValid { get; internal set; }
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
        public int rImageDataSize { get; private set; }
        public int rShadowDataSize { get; private set; }
        public int rOverlayDataSize { get; private set; }

        //old crystal lib compatibility
        public bool HasMask { get; private set; }
        #endregion

        #region Shadow
        public short ShadowWidth;
        public short ShadowHeight;

        public short ShadowOffSetX;
        public short ShadowOffSetY;

        public Texture Shadow;
        public bool ShadowValid { get; internal set; }
        public unsafe byte* ShadowData;
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

        public Texture Overlay;
        public bool OverlayValid { get; internal set; }
        public unsafe byte* OverlayData;
        public int OverlayDataSize
        {
            get
            {
                int w = OverlayWidth + (4 - OverlayWidth % 4) % 4;
                int h = OverlayHeight + (4 - OverlayHeight % 4) % 4;

                return LibMode != libMode.V2 ? w * h / 2 : w * h;
            }
        }

        //old crystal Lib compatibility
        public short MaskX { get; private set; }
        public short MaskY { get; private set; }

        #endregion
        public int CustomDatasize { get; private set; }
        public int CustomShadowDatasize { get; private set; }
        public int CustomOverlayDatasize { get; private set; }

        public DateTime ExpireTime;

        public MirImage(BinaryReader reader, MirLibrary.libMode libMode, string fileName = "")
        {
            _bReader = reader;
            FileName = fileName;

            LibMode = libMode;

            if (LibMode == MirLibrary.libMode.V2)
            {
                SubCounter = reader.ReadInt32();
            }

            if (libMode != MirLibrary.libMode.CrystalLib)
                Position = reader.ReadInt32();

            //Crystal Lib format
            if (libMode == MirLibrary.libMode.CrystalLib)
            {

                Position = (int)reader.BaseStream.Position;

                //read layer 1
                Width = reader.ReadInt16();
                Height = reader.ReadInt16();
                OffSetX = reader.ReadInt16();
                OffSetY = reader.ReadInt16();
                ShadowOffSetX = reader.ReadInt16();
                ShadowOffSetY = reader.ReadInt16();
                ShadowType = reader.ReadByte();
                rImageDataSize = reader.ReadInt32();

                reader.ReadBytes(rImageDataSize);

                //check if there's a second layer and read it
                HasMask = ((ShadowType >> 7) == 1) ? true : false;
                if (HasMask)
                {
                    
                    OverlayWidth = reader.ReadInt16();
                    OverlayHeight = reader.ReadInt16();
                    MaskX = reader.ReadInt16();
                    MaskY = reader.ReadInt16();
                    rOverlayDataSize = reader.ReadInt32();

                    reader.ReadBytes(rOverlayDataSize);

                }

                return;
            }

            Width = reader.ReadInt16();
            Height = reader.ReadInt16();
            OffSetX = reader.ReadInt16();
            OffSetY = reader.ReadInt16();

            if (libMode == libMode.V2)
            {
                textureFormat = (TextureFormat)reader.ReadByte();
            }

            ShadowType = reader.ReadByte();

            ShadowWidth = reader.ReadInt16();
            ShadowHeight = reader.ReadInt16();
            ShadowOffSetX = reader.ReadInt16();
            ShadowOffSetY = reader.ReadInt16();

            OverlayWidth = reader.ReadInt16();
            OverlayHeight = reader.ReadInt16();

            if (LibMode == MirLibrary.libMode.V2 && SubCounter > 0)
            {
                for (int i = 0; i < SubCounter; i++)
                {
                    SubItems.Add(new MirSubImage(reader));
                }
            }

        }

        public unsafe bool VisiblePixel(Point p, bool acurrate)
        {
            if (p.X < 0 || p.Y < 0 || !ImageValid || ImageData == null) return false;
            
            int w = Width + (4 - Width % 4) % 4;
            int h = Height + (4 - Height % 4) % 4;

            if (p.X >= w || p.Y >= h)
                return false;

            int x = (p.X - p.X % 4) / 4;
            int y = (p.Y - p.Y % 4) / 4;
            int index = (y * (w / 4) + x) * 8;

            int col0 = ImageData[index + 1] << 8 | ImageData[index], col1 = ImageData[index + 3] << 8 | ImageData[index + 2];

            if (col0 == 0 && col1 == 0) return false;

            if (!acurrate || col1 < col0) return true;

            x = p.X % 4;
            y = p.Y % 4;
            x *= 2;
            
            return (ImageData[index + 4 + y] & 1 << x) >> x != 1 || (ImageData[index + 4 + y] & 1 << x + 1) >> x + 1 != 1;
        }


        public unsafe void DisposeTexture()
        {
            if (Image != null && !Image.Disposed)
                Image.Dispose();

            if (Shadow != null && !Shadow.Disposed)
                Shadow.Dispose();

            if (Overlay != null && !Overlay.Disposed)
                Overlay.Dispose();

            ImageData = null;
            ShadowData = null;
            OverlayData = null;

            Image = null;
            Shadow = null;
            Overlay = null;

            ImageValid = false;
            ShadowValid = false;
            OverlayValid = false;
            
            ExpireTime = DateTime.MinValue;

            if (SubItems.Count > 0)
            {
                foreach (var subImage in SubItems)
                {

                    if (subImage.Image != null && !subImage.Image.Disposed)
                        subImage.Image.Dispose();

                    subImage.Image = null;

                    subImage.ImageValid = false;
                }
            }

            DXManager.TextureList.Remove(this);
        }

        public unsafe byte[] ReadImage(BinaryReader reader)
        {

            if (Position == 0) return null;

            if (reader != null && reader.BaseStream == null)
            {
                var _FStream = File.OpenRead(FileName);
                _bReader = new BinaryReader(_FStream);
                reader = _bReader;
            }

            byte[] ImageArr = null;

            if (reader != null && reader.BaseStream.CanRead)
            {
                lock (reader)
                {
                    reader.BaseStream.Seek(Position, SeekOrigin.Begin);

                    if (LibMode == MirLibrary.libMode.V2)
                        CustomDatasize = ImageDataSize;
                    if (LibMode == MirLibrary.libMode.Normal)
                        CustomDatasize = ImageDataSize;
                    if (LibMode == MirLibrary.libMode.CrystalLib)
                    {

                        CustomDatasize = rImageDataSize;

                        reader.BaseStream.Seek(Position + 17, SeekOrigin.Begin);

                    }

                    ImageArr = reader.ReadBytes(CustomDatasize);

                    switch (LibMode)
                    {
                        case MirLibrary.libMode.V2:
                            //not compressed
                            break;
                        case MirLibrary.libMode.CrystalLib:
                            try
                            {
                                ImageArr = DecompressImage(ImageArr);
                            }
                            catch (InvalidDataException)
                            {
                                ImageArr = DecompressImage(ImageArr);
                            }
                            break;
                        default:
                            ImageArr = Decompress(ImageArr);
                            break;
                    }

                    #region Crystal Lib Overlay Loader
                    if (LibMode == MirLibrary.libMode.CrystalLib && HasMask)
                    {
                        //old legacy overlay load
                        reader.ReadBytes(12);
                        int w = Width;
                        int h = Height;

                        if (w != 0 || h != 0)
                        {
                            byte[] arr2 = null;

                            arr2 = reader.ReadBytes(CustomDatasize);

                            try
                            {
                                arr2 = DecompressImage(arr2);
                            }
                            catch (InvalidDataException)
                            {
                                arr2 = DecompressImage(arr2);
                            }

                            //read the mask/overlay
                            Overlay = new Texture(DXManager.Device, w, h, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);

                            DataRectangle rect = Overlay.LockRectangle(0, LockFlags.Discard);
                            OverlayData = (byte*)rect.Data.DataPointer;

                            rect.Data.Write(arr2, 0, CustomDatasize);

                            Overlay.UnlockRectangle(0);
                            rect.Data.Dispose();

                            CustomOverlayDatasize = arr2.Length;

                            Overlay.AutoMipGenerationFilter = SlimDX.Direct3D9.TextureFilter.Point;
                            

                        }

                    }
                    #endregion

                    if (ImageArr != null)
                        CustomDatasize = ImageArr.Length;

                }
            }

            return ImageArr;

        }

        private byte[] Decompress(byte[] gzip)
        {
            //old format not have compression
            if (LibMode == MirLibrary.libMode.Normal) return gzip;

            //if (gzip.All(c => c == '0')) return gzip;
            if (!MirLibraryFunctions.IsPossiblyGZippedBytes(gzip)) return gzip;

            try
            {
                // Create a GZIP stream with decompression mode.
                // ... Then create a buffer and write into while reading from the GZIP stream.
                using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
                {
                    const int size = 4096;
                    byte[] buffer = new byte[size];
                    using (MemoryStream memory = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = stream.Read(buffer, 0, size);
                            if (count > 0)
                            {
                                memory.Write(buffer, 0, count);
                            }
                        }
                        while (count > 0);

                        var arr = memory.ToArray();

                        for (int i = 0; i < arr.Length; i += 4)
                        {
                            //Reverse Red/Blue
                            byte b = arr[i];
                            arr[i] = arr[i + 2];
                            arr[i + 2] = b;

                            //if (arr[i] == 0 && arr[i + 1] == 0 && arr[i + 2] == 0)
                            //    arr[i + 3] = 0; //Make Transparent
                        }

                        return arr;
                    }
                }
            }
            catch (Exception)
            {

                return gzip;
            }
        }

        private static byte[] DecompressImage(byte[] image)
        {

            using (GZipStream stream = new GZipStream(new MemoryStream(image), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

        public unsafe void CreateImage(BinaryReader reader)
        {
            if (Position == 0) return;

            byte[] arr = new byte[0];

            lock (reader)
                arr = ReadImage(reader);
            
            int w = Width + (4 - Width % 4) % 4;
            int h = Height + (4 - Height % 4) % 4;

            if (w == 0 || h == 0) return;

            Format imgFormat = Format.R8G8B8;

            if (LibMode == MirLibrary.libMode.V2)
                imgFormat = textureFormat == TextureFormat.BC1 ? Format.Dxt1 : Format.Dxt5;
            if (LibMode == MirLibrary.libMode.Normal)
                imgFormat = Format.Dxt1;
            else if (LibMode == MirLibrary.libMode.CrystalLib)
                imgFormat = Format.A8R8G8B8;

            if (DXManager.Device == null) return;//bug when sometimes closes the game or go too fast in multithreading :D

            Image = new Texture(DXManager.Device, w, h, 1, Usage.None, imgFormat, Pool.Managed);

            DataRectangle rect = Image.LockRectangle(0, LockFlags.Discard);
            ImageData = (byte*)rect.Data.DataPointer;

            rect.Data.Write(arr, 0, CustomDatasize);

            Image.UnlockRectangle(0);
            rect.Data.Dispose();

            Image.AutoMipGenerationFilter = SlimDX.Direct3D9.TextureFilter.Point;
            Image.GenerateMipSublevels();
            Image.FilterTexture(0, Filter.None);
            
            ImageValid = true;
            ExpireTime = CEnvir.Now + Config.CacheDuration;
            DXManager.TextureList.Add(this);

            //ToDo config -> load map animations??
            if (SubItems.Count > 0)
            {
                foreach (var subImage in SubItems)
                {
                    if (subImage == null || subImage.ImageValid || subImage.Position == 0) continue;

                    lock (reader)
                        subImage.CreateImage(reader);
                }
            }

            #region old code
            //old code
            //int w = Width + (4 - Width % 4) % 4;
            //int h = Height + (4 - Height % 4) % 4;

            //if (w == 0 || h == 0) return;

            //Image = new Texture(DXManager.Device, w, h, 1, Usage.None, Format.Dxt1, Pool.Managed);
            //DataRectangle rect = Image.LockRectangle(0, LockFlags.Discard);
            //ImageData = (byte*)rect.Data.DataPointer;

            //lock (reader)
            //{
            //    reader.BaseStream.Seek(Position, SeekOrigin.Begin);
            //    rect.Data.Write(reader.ReadBytes(ImageDataSize), 0, ImageDataSize);
            //}

            //Image.UnlockRectangle(0);
            //rect.Data.Dispose();

            //ImageValid = true;
            //ExpireTime = CEnvir.Now + Config.CacheDuration;
            //DXManager.TextureList.Add(this);
            #endregion

        }
        public unsafe void CreateShadow(BinaryReader reader)
        {
            if (Position == 0) return;

            if (LibMode == MirLibrary.libMode.CrystalLib) return;

            if (!ImageValid)
                CreateImage(reader);

            int w = ShadowWidth + (4 - ShadowWidth % 4) % 4;
            int h = ShadowHeight + (4 - ShadowHeight % 4) % 4;

            if (w == 0 || h == 0) return;

            Format imgFormat = Format.A8R8G8B8;

            if (LibMode == libMode.V2)
                imgFormat = Format.Dxt5;
            else if (LibMode == MirLibrary.libMode.Normal)
                imgFormat = Format.Dxt1;
            
            Shadow = new Texture(DXManager.Device, w, h, 1, Usage.None, imgFormat, Pool.Managed);
            DataRectangle rect = Shadow.LockRectangle(0, LockFlags.Discard);
            ShadowData = (byte*)rect.Data.DataPointer;

            lock (reader)
            {
                reader.BaseStream.Seek(Position + (LibMode == MirLibrary.libMode.Normal || LibMode == libMode.V2 ? ImageDataSize : rImageDataSize), SeekOrigin.Begin);

                if (LibMode == MirLibrary.libMode.Normal || LibMode == libMode.V2)
                    CustomShadowDatasize = ShadowDataSize;
                
                var arr = Decompress(reader.ReadBytes(CustomShadowDatasize));
                CustomShadowDatasize = arr.Length;

                rect.Data.Write(arr, 0, CustomShadowDatasize);

            }

            Shadow.UnlockRectangle(0);
            rect.Data.Dispose();

            Shadow.AutoMipGenerationFilter = SlimDX.Direct3D9.TextureFilter.Point;
            
            ShadowValid = true;

            #region old code
            //if (Position == 0) return;

            //if (!ImageValid)
            //    CreateImage(reader);

            //int w = ShadowWidth + (4 - ShadowWidth % 4) % 4;
            //int h = ShadowHeight + (4 - ShadowHeight % 4) % 4;

            //if (w == 0 || h == 0) return;

            //Shadow = new Texture(DXManager.Device, w, h, 1, Usage.None, Format.Dxt1, Pool.Managed);
            //DataRectangle rect = Shadow.LockRectangle(0, LockFlags.Discard);
            //ShadowData = (byte*)rect.Data.DataPointer;

            //lock (reader)
            //{
            //    reader.BaseStream.Seek(Position + ImageDataSize, SeekOrigin.Begin);
            //    rect.Data.Write(reader.ReadBytes(ShadowDataSize), 0, ShadowDataSize);
            //}

            //Shadow.UnlockRectangle(0);
            //rect.Data.Dispose();

            //ShadowValid = true;
            #endregion
        }
        public unsafe void CreateOverlay(BinaryReader reader)
        {

            if (Position == 0) return;

            if (LibMode == MirLibrary.libMode.CrystalLib)
            {
                if (HasMask && Overlay != null)
                    OverlayValid = true;
                return;
            }

            if (!ImageValid)
                CreateImage(reader);

            int w = OverlayWidth + (4 - OverlayWidth % 4) % 4;
            int h = OverlayHeight + (4 - OverlayHeight % 4) % 4;

            if (w == 0 || h == 0) return;

            Format imgFormat = Format.A8R8G8B8;

            if (LibMode == MirLibrary.libMode.V2)
                imgFormat = Format.Dxt5;
            if (LibMode == MirLibrary.libMode.Normal)
                imgFormat = Format.Dxt1;
            
            Overlay = new Texture(DXManager.Device, w, h, 1, Usage.None, imgFormat, Pool.Managed);
            DataRectangle rect = Overlay.LockRectangle(0, LockFlags.Discard);
            OverlayData = (byte*)rect.Data.DataPointer;

            lock (reader)
            {
                reader.BaseStream.Seek(Position + (LibMode == MirLibrary.libMode.Normal || LibMode == libMode.V2 ? ImageDataSize : rImageDataSize) + (LibMode == MirLibrary.libMode.Normal || LibMode == libMode.V2 ? ShadowDataSize : rShadowDataSize), SeekOrigin.Begin);

                if (LibMode == MirLibrary.libMode.Normal || LibMode == MirLibrary.libMode.V2)
                    CustomOverlayDatasize = OverlayDataSize;
                
                var arr = Decompress(reader.ReadBytes(CustomOverlayDatasize));
                CustomOverlayDatasize = arr.Length;

                rect.Data.Write(arr, 0, CustomOverlayDatasize);

            }

            Overlay.UnlockRectangle(0);
            rect.Data.Dispose();

            Overlay.AutoMipGenerationFilter = SlimDX.Direct3D9.TextureFilter.Point;
            
            OverlayValid = true;

            #region old code
            //if (Position == 0) return;

            //if (!ImageValid)
            //    CreateImage(reader);

            //int w = OverlayWidth + (4 - OverlayWidth % 4) % 4;
            //int h = OverlayHeight + (4 - OverlayHeight % 4) % 4;

            //if (w == 0 || h == 0) return;

            //Overlay = new Texture(DXManager.Device, w, h, 1, Usage.None, Format.Dxt1, Pool.Managed);
            //DataRectangle rect = Overlay.LockRectangle(0, LockFlags.Discard);
            //OverlayData = (byte*)rect.Data.DataPointer;

            //lock (reader)
            //{
            //    reader.BaseStream.Seek(Position + ImageDataSize + ShadowDataSize, SeekOrigin.Begin);
            //    rect.Data.Write(reader.ReadBytes(OverlayDataSize), 0, OverlayDataSize);
            //}

            //Overlay.UnlockRectangle(0);
            //rect.Data.Dispose();

            //OverlayValid = true;
            #endregion
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
        ~MirImage()
        {
            Dispose(false);
        }

        #endregion

    }

    public sealed class MirSubImage : IDisposable
    {
        public int Position;
        private BinaryReader _bReader;

        public string FileName { get; private set; }

        #region Texture

        public TextureFormat textureFormat = TextureFormat.BC1;

        public short Width;
        public short Height;
        public short OffSetX;
        public short OffSetY;
        public Texture Image;
        public bool ImageValid { get; internal set; }
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
        #endregion

        public DateTime ExpireTime;

        public MirSubImage(BinaryReader reader, string fileName = "")
        {

            _bReader = reader;
            FileName = fileName;

            Position = reader.ReadInt32();

            Width = reader.ReadInt16();
            Height = reader.ReadInt16();
            OffSetX = reader.ReadInt16();
            OffSetY = reader.ReadInt16();

            textureFormat = (TextureFormat)reader.ReadByte();

        }

        public unsafe bool VisiblePixel(Point p, bool acurrate)
        {
            if (p.X < 0 || p.Y < 0 || !ImageValid || ImageData == null) return false;

            int w = Width + (4 - Width % 4) % 4;
            int h = Height + (4 - Height % 4) % 4;

            if (p.X >= w || p.Y >= h)
                return false;

            int x = (p.X - p.X % 4) / 4;
            int y = (p.Y - p.Y % 4) / 4;
            int index = (y * (w / 4) + x) * 8;

            int col0 = ImageData[index + 1] << 8 | ImageData[index], col1 = ImageData[index + 3] << 8 | ImageData[index + 2];

            if (col0 == 0 && col1 == 0) return false;

            if (!acurrate || col1 < col0) return true;

            x = p.X % 4;
            y = p.Y % 4;
            x *= 2;

            return (ImageData[index + 4 + y] & 1 << x) >> x != 1 || (ImageData[index + 4 + y] & 1 << x + 1) >> x + 1 != 1;
        }

        public unsafe void DisposeTexture()
        {

            if (Image != null && !Image.Disposed)
                Image.Dispose();

            ImageData = null;

            Image = null;

            ImageValid = false;

            ExpireTime = DateTime.MinValue;

        }

        public unsafe byte[] ReadImage(BinaryReader reader)
        {

            if (Position == 0) return null;

            if (reader.BaseStream == null)
            {
                var _FStream = File.OpenRead(FileName);
                _bReader = new BinaryReader(_FStream);
                reader = _bReader;
            }

            byte[] ImageArr = null;

            lock (reader)
            {
                reader.BaseStream.Seek(Position, SeekOrigin.Begin);

                ImageArr = reader.ReadBytes(ImageDataSize);
            }

            return ImageArr;

        }

        public unsafe void CreateImage(BinaryReader reader)
        {

            var arr = ReadImage(reader);

            int w = Width + (4 - Width % 4) % 4;
            int h = Height + (4 - Height % 4) % 4;

            if (w == 0 || h == 0 || arr == null || arr.Length == 0) return;

            Format imgFormat = Format.Dxt1;

            switch (textureFormat)
            {
                case TextureFormat.BC3:
                    imgFormat = Format.Dxt5;
                    break;
                case TextureFormat.BC1:
                default:
                    imgFormat = Format.Dxt1;
                    break;
            }

            Image = new Texture(DXManager.Device, w, h, 1, Usage.None, imgFormat, Pool.Managed);

            DataRectangle rect = Image.LockRectangle(0, LockFlags.Discard);
            ImageData = (byte*)rect.Data.DataPointer;

            rect.Data.Write(arr, 0, ImageDataSize);

            Image.UnlockRectangle(0);
            rect.Data.Dispose();

            Image.AutoMipGenerationFilter = SlimDX.Direct3D9.TextureFilter.Point;
            Image.GenerateMipSublevels();
            Image.FilterTexture(0, Filter.None);
            
            ImageValid = true;
            ExpireTime = CEnvir.Now + Config.CacheDuration;

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
        ~MirSubImage()
        {
            Dispose(false);
        }

        #endregion

    }

}
