using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library_Editor
{
    public static class CompressionHelper
    {
        public static byte[] GZipHeaderBytes = { 0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 4, 0 };
        public static byte[] GZipLevel10HeaderBytes = { 0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 2, 0 };

        public static bool IsPossiblyGZippedBytes(this byte[] a)
        {
            var yes = a.Length > 10;

            if (!yes)
            {
                return false;
            }

            var header = a.SubArray(0, 10);

            return header.SequenceEqual(GZipHeaderBytes) || header.SequenceEqual(GZipLevel10HeaderBytes);
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static byte[] Reverse(this byte[] arrToReverse)
        {

            if (arrToReverse == null) return arrToReverse;

            Array.Reverse(arrToReverse);
            return arrToReverse;

        }

        #region Compression
        public static byte[] Decompress(this byte[] fBytes)
        {
            if (!fBytes.IsPossiblyGZippedBytes()) return fBytes;

            using (GZipStream stream = new GZipStream(new MemoryStream(fBytes), CompressionMode.Decompress))
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

        public static byte[] Compress(this byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory,
                CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }
        #endregion
        
    }
}
