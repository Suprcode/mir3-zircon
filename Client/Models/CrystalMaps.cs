using Client.Envir;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public static class CrystalMaps
    {

        //Map
        public static readonly MirLibrary[] MapLibs = new MirLibrary[650];

        public static void InitMaps()
        {

            #region Maplibs

            //wemade mir2 (allowed from 0-99)
            MapLibs[0] = new MirLibrary(@"Data\\Map\\WemadeMir2\\Tiles.lib");
            MapLibs[1] = new MirLibrary(@"Data\\Map\\WemadeMir2\\Smtiles.lib");
            MapLibs[2] = new MirLibrary(@"Data\\Map\\WemadeMir2\\Objects.lib");
            for (int i = 2; i < 24; i++)
            {
                MapLibs[i + 1] = new MirLibrary(@"Data\\Map\\WemadeMir2\\Objects" + i.ToString() + ".lib");
            }

            //shanda mir2 (allowed from 100-199)
            MapLibs[100] = new MirLibrary(@"Data\\Map\\ShandaMir2\\Tiles.lib");
            for (int i = 1; i < 10; i++)
            {
                MapLibs[100 + i] = new MirLibrary(@"Data\\Map\\ShandaMir2\\Tiles" + (i + 1) + ".lib");
            }
            MapLibs[110] = new MirLibrary(@"Data\\Map\\ShandaMir2\\SmTiles.lib");
            for (int i = 1; i < 10; i++)
            {
                MapLibs[110 + i] = new MirLibrary(@"Data\\Map\\ShandaMir2\\SmTiles" + (i + 1) + ".lib");
            }
            MapLibs[120] = new MirLibrary(@"Data\\Map\\ShandaMir2\\Objects.lib");
            for (int i = 1; i < 31; i++)
            {
                MapLibs[120 + i] = new MirLibrary(@"Data\\Map\\ShandaMir2\\Objects" + (i + 1) + ".lib");
            }
            MapLibs[190] = new MirLibrary(@"Data\\Map\\ShandaMir2\\AniTiles1.lib");

            //wemade mir3 (allowed from 200-299)
            string[] Mapstate = { "", "wood\\", "sand\\", "snow\\", "forest\\" };
            for (int i = 0; i < Mapstate.Length; i++)
            {
                MapLibs[200 + (i * 15)] = new MirLibrary(@"Data\\Map\\WemadeMir3\\" + Mapstate[i] + "Tilesc.lib");
                MapLibs[201 + (i * 15)] = new MirLibrary(@"Data\\Map\\WemadeMir3\\" + Mapstate[i] + "Tiles30c.lib");
                MapLibs[202 + (i * 15)] = new MirLibrary(@"Data\\Map\\WemadeMir3\\" + Mapstate[i] + "Tiles5c.lib");
                MapLibs[203 + (i * 15)] = new MirLibrary(@"Data\\Map\\WemadeMir3\\" + Mapstate[i] + "Smtilesc.lib");
                MapLibs[204 + (i * 15)] = new MirLibrary(@"Data\\Map\\WemadeMir3\\" + Mapstate[i] + "Housesc.lib");
                MapLibs[205 + (i * 15)] = new MirLibrary(@"Data\\Map\\WemadeMir3\\" + Mapstate[i] + "Cliffsc.lib");
                MapLibs[206 + (i * 15)] = new MirLibrary(@"Data\\Map\\WemadeMir3\\" + Mapstate[i] + "Dungeonsc.lib");
                MapLibs[207 + (i * 15)] = new MirLibrary(@"Data\\Map\\WemadeMir3\\" + Mapstate[i] + "Innersc.lib");
                MapLibs[208 + (i * 15)] = new MirLibrary(@"Data\\Map\\WemadeMir3\\" + Mapstate[i] + "Furnituresc.lib");
                MapLibs[209 + (i * 15)] = new MirLibrary(@"Data\\Map\\WemadeMir3\\" + Mapstate[i] + "Wallsc.lib");
                MapLibs[210 + (i * 15)] = new MirLibrary(@"Data\\Map\\WemadeMir3\\" + Mapstate[i] + "smObjectsc.lib");
                MapLibs[211 + (i * 15)] = new MirLibrary(@"Data\\Map\\WemadeMir3\\" + Mapstate[i] + "Animationsc.lib");
                MapLibs[212 + (i * 15)] = new MirLibrary(@"Data\\Map\\WemadeMir3\\" + Mapstate[i] + "Object1c.lib");
                MapLibs[213 + (i * 15)] = new MirLibrary(@"Data\\Map\\WemadeMir3\\" + Mapstate[i] + "Object2c.lib");
            }

            Mapstate = new string[] { "", "wood", "sand", "snow", "forest" };
            //shanda mir3 (allowed from 300-399)
            for (int i = 0; i < Mapstate.Length; i++)
            {
                MapLibs[300 + (i * 15)] = new MirLibrary(@"Data\\Map\\ShandaMir3\\" + "Tilesc" + Mapstate[i] + ".lib");
                MapLibs[301 + (i * 15)] = new MirLibrary(@"Data\\Map\\ShandaMir3\\" + "Tiles30c" + Mapstate[i] + ".lib");
                MapLibs[302 + (i * 15)] = new MirLibrary(@"Data\\Map\\ShandaMir3\\" + "Tiles5c" + Mapstate[i] + ".lib");
                MapLibs[303 + (i * 15)] = new MirLibrary(@"Data\\Map\\ShandaMir3\\" + "Smtilesc" + Mapstate[i] + ".lib");
                MapLibs[304 + (i * 15)] = new MirLibrary(@"Data\\Map\\ShandaMir3\\" + "Housesc" + Mapstate[i] + ".lib");
                MapLibs[305 + (i * 15)] = new MirLibrary(@"Data\\Map\\ShandaMir3\\" + "Cliffsc" + Mapstate[i] + ".lib");
                MapLibs[306 + (i * 15)] = new MirLibrary(@"Data\\Map\\ShandaMir3\\" + "Dungeonsc" + Mapstate[i] + ".lib");
                MapLibs[307 + (i * 15)] = new MirLibrary(@"Data\\Map\\ShandaMir3\\" + "Innersc" + Mapstate[i] + ".lib");
                MapLibs[308 + (i * 15)] = new MirLibrary(@"Data\\Map\\ShandaMir3\\" + "Furnituresc" + Mapstate[i] + ".lib");
                MapLibs[309 + (i * 15)] = new MirLibrary(@"Data\\Map\\ShandaMir3\\" + "Wallsc" + Mapstate[i] + ".lib");
                MapLibs[310 + (i * 15)] = new MirLibrary(@"Data\\Map\\ShandaMir3\\" + "smObjectsc" + Mapstate[i] + ".lib");
                MapLibs[311 + (i * 15)] = new MirLibrary(@"Data\\Map\\ShandaMir3\\" + "Animationsc" + Mapstate[i] + ".lib");
                MapLibs[312 + (i * 15)] = new MirLibrary(@"Data\\Map\\ShandaMir3\\" + "Object1c" + Mapstate[i] + ".lib");
                MapLibs[313 + (i * 15)] = new MirLibrary(@"Data\\Map\\ShandaMir3\\" + "Object2c" + Mapstate[i] + ".lib");
            }

            //Mir3 Shanda - Last
            //mir3 (allowed from 400-544)
            Mapstate = new string[] { "", "wood\\", "sand\\", "snow\\", "forest\\" };
            for (int i = 0; i < Mapstate.Length; i++)
            {

                MapLibs[400 + (i * 15)] = new MirLibrary(@"Data\\Map\\NewShandaMir3\\" + Mapstate[i] + "Tilesc.lib");
                MapLibs[401 + (i * 15)] = new MirLibrary(@"Data\\Map\\NewShandaMir3\\" + Mapstate[i] + "Tiles30c.lib");
                MapLibs[402 + (i * 15)] = new MirLibrary(@"Data\\Map\\NewShandaMir3\\" + Mapstate[i] + "Tiles5c.lib");
                MapLibs[403 + (i * 15)] = new MirLibrary(@"Data\\Map\\NewShandaMir3\\" + Mapstate[i] + "Smtilesc.lib");
                MapLibs[404 + (i * 15)] = new MirLibrary(@"Data\\Map\\NewShandaMir3\\" + Mapstate[i] + "Housesc.lib");
                MapLibs[405 + (i * 15)] = new MirLibrary(@"Data\\Map\\NewShandaMir3\\" + Mapstate[i] + "Cliffsc.lib");
                MapLibs[406 + (i * 15)] = new MirLibrary(@"Data\\Map\\NewShandaMir3\\" + Mapstate[i] + "Dungeonsc.lib");
                MapLibs[407 + (i * 15)] = new MirLibrary(@"Data\\Map\\NewShandaMir3\\" + Mapstate[i] + "Innersc.lib");
                MapLibs[408 + (i * 15)] = new MirLibrary(@"Data\\Map\\NewShandaMir3\\" + Mapstate[i] + "Furnituresc.lib");
                MapLibs[409 + (i * 15)] = new MirLibrary(@"Data\\Map\\NewShandaMir3\\" + Mapstate[i] + "Wallsc.lib");
                MapLibs[410 + (i * 15)] = new MirLibrary(@"Data\\Map\\NewShandaMir3\\" + Mapstate[i] + "smObjectsc.lib");
                MapLibs[411 + (i * 15)] = new MirLibrary(@"Data\\Map\\NewShandaMir3\\" + Mapstate[i] + "Animationsc.lib");
                MapLibs[412 + (i * 15)] = new MirLibrary(@"Data\\Map\\NewShandaMir3\\" + Mapstate[i] + "Object1c.lib");
                MapLibs[413 + (i * 15)] = new MirLibrary(@"Data\\Map\\NewShandaMir3\\" + Mapstate[i] + "Object2c.lib");

            }

            MapLibs[600] = new MirLibrary(@"Data\\Map\\Woool\\tiles.zl");
            MapLibs[605] = new MirLibrary(@"Data\\Map\\Woool\\smtiles.zl");

            for (int i = 1; i < 17; i++)
            {
                MapLibs[610 + i] = new MirLibrary(@"Data\\Map\\Woool\\objects" + i.ToString() + ".zl");
            }
            #endregion

        }

    }
}
