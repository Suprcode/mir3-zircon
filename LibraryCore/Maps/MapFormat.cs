using Library;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryCore.Maps
{
    public static class MapFormat
    {

        public struct MapTypeInfo
        {
            public MapType mapType;
            public byte mapCodeType;
            public Size mapSize;
        }


        public static MapTypeInfo FindType(byte[] input)
        {

            var MetaTitle = System.Text.ASCIIEncoding.Default.GetString(input.Skip(1).Take(24).ToArray()).Trim();

            //c# woool custom map format - converted from original (djdarkboyz)
            if (MetaTitle == "Zircon v2.1 - DjDarkBoyZ")
                return new MapTypeInfo { mapCodeType = 150, mapType = MapType.Woool, mapSize = LoadMapCellsV150(input)};
            
            //c# custom map format
            if ((input[2] == 0x43) && (input[3] == 0x23))
                return new MapTypeInfo { mapCodeType = 100, mapType = MapType.Other, mapSize = LoadMapCellsV100(input) };
            //wemade mir3 maps have no title they just start with blank bytes
            if (input[0] == 0)
                return new MapTypeInfo { mapCodeType = 5, mapType = MapType.Mir3, mapSize = LoadMapCellsv5(input) };
            //shanda mir3 maps start with title: (C) SNDA, MIR3.
            if ((input[0] == 0x0F) && (input[5] == 0x53) && (input[14] == 0x33))
                return new MapTypeInfo { mapCodeType = 6, mapType = MapType.Mir3, mapSize = LoadMapCellsv6(input) };

            //shanda mir3 maps start with title: (C) MIR3, MIR3. (NEW)
            if ((input[0] == 0x0F) && (input[5] == 0x4D) && (input[14] == 0x33))
                return new MapTypeInfo { mapCodeType = 8, mapType = MapType.Mir3, mapSize = LoadMapCellsv6(input) };

            //wemades antihack map (laby maps) title start with: Mir2 AntiHack
            if ((input[0] == 0x15) && (input[4] == 0x32) && (input[6] == 0x41) && (input[19] == 0x31))
                return new MapTypeInfo { mapCodeType = 4, mapType = MapType.Other, mapSize = LoadMapCellsv4(input) };

            //wemades 2010 map format i guess title starts with: Map 2010 Ver 1.0
            if ((input[0] == 0x10) && (input[2] == 0x61) && (input[7] == 0x31) && (input[14] == 0x31))
                return new MapTypeInfo { mapCodeType = 1, mapType = MapType.Other, mapSize = LoadMapCellsv1(input) };

            //shanda's 2012 format and one of shandas(wemades) older formats share same header info, only difference is the filesize
            if ((input[4] == 0x0F) && (input[18] == 0x0D) && (input[19] == 0x0A))
            {
                int W = input[0] + (input[1] << 8);
                int H = input[2] + (input[3] << 8);
                if (input.Length > (52 + (W * H * 14)))
                    return new MapTypeInfo { mapCodeType = 3, mapType = MapType.Other, mapSize = LoadMapCellsv3(input) };
                else
                    return new MapTypeInfo { mapCodeType = 2, mapType = MapType.Other, mapSize = LoadMapCellsv2(input) };
            }

            //3/4 heroes map format (myth/lifcos i guess)
            if ((input[0] == 0x0D) && (input[1] == 0x4C) && (input[7] == 0x20) && (input[11] == 0x6D))
                return new MapTypeInfo { mapCodeType = 7, mapType = MapType.Other, mapSize = LoadMapCellsv7(input) };

            return new MapTypeInfo { mapCodeType = 0, mapType = MapType.None, mapSize = LoadMapCellsv0(input) };
        }

        #region GetMap Sizes
        private static Size LoadMapCellsv0(byte[] fileBytes)
        {

            //raw
            int offSet = 0;
            int Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int Height = BitConverter.ToInt16(fileBytes, offSet);

            return new Size(Width, Height);

        }

        private static Size LoadMapCellsv1(byte[] fileBytes)
        {
            //Map 2010
            int offSet = 21;

            int w = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int xor = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int h = BitConverter.ToInt16(fileBytes, offSet);
            int Width = w ^ xor;
            int Height = h ^ xor;

            return new Size(Width, Height);

        }

        private static Size LoadMapCellsv2(byte[] fileBytes)
        {
            //Shanda 2012 old (wemades)
            int offSet = 0;
            int Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int Height = BitConverter.ToInt16(fileBytes, offSet);

            return new Size(Width, Height);

        }

        private static Size LoadMapCellsv3(byte[] fileBytes)
        {
            //Shanda 2012 old
            int offSet = 0;
            int Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int Height = BitConverter.ToInt16(fileBytes, offSet);

            return new Size(Width, Height);

        }

        private static Size LoadMapCellsv4(byte[] fileBytes)
        {
            //Mir2 AntiHack

            int offSet = 31;
            int w = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int xor = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int h = BitConverter.ToInt16(fileBytes, offSet);
            int Width = w ^ xor;
            int Height = h ^ xor;

            return new Size(Width, Height);

        }

        private static Size LoadMapCellsv5(byte[] fileBytes)
        {

            //Mir3
            int Width = fileBytes[23] << 8 | fileBytes[22];
            int Height = fileBytes[25] << 8 | fileBytes[24];

            return new Size(Width, Height);

        }

        private static Size LoadMapCellsv6(byte[] fileBytes)
        {
            //Shanda Mir3

            int offSet = 16;
            int Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int Height = BitConverter.ToInt16(fileBytes, offSet);

            return new Size(Width, Height);

        }

        private static Size LoadMapCellsv7(byte[] fileBytes)
        {
            //3/4 heroes map

            int offSet = 21;
            int Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 4;
            int Height = BitConverter.ToInt16(fileBytes, offSet);

            return new Size(Width, Height);

        }

        private static Size LoadMapCellsV100(byte[] fileBytes)
        {
            //C# custom

            int offSet = 4;
            if ((fileBytes[0] != 1) || (fileBytes[1] != 0)) return Size.Empty;//only support version 1 atm
            int Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int Height = BitConverter.ToInt16(fileBytes, offSet);

            return new Size(Width, Height);

        }

        private static Size LoadMapCellsV150(byte[] fileBytes)
        {
            
            int offset = 0;

            offset += 70;

            offset += 4;

            //width 4 bytes (int)
            int width = BitConverter.ToInt32(fileBytes, offset);
            offset += 4;
            //height 4 bytes (int)
            int height = BitConverter.ToInt32(fileBytes, offset);
            offset += 4;

            if (width > 1500 || height > 1500)
            {
                return Size.Empty;
            }

            return new Size(width, height);

        }
        #endregion

    }
}
