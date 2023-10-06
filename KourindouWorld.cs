using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plants;
using Terraria.Audio;

namespace Kourindou
{
    public class KourindouWorld : ModSystem
    {
        // Plushie Dirt and wet mechanic saving
        public static Dictionary<long, short> plushieTiles = new();

        public override void SaveWorldData(TagCompound tag)
        {
            List<string> plushieTileList = new();
            foreach (KeyValuePair<long, short> pt in plushieTiles)
            {
                plushieTileList.Add(pt.Key.ToString() + "/" + pt.Value.ToString());
            }

            tag.Add("plushieTiles", plushieTileList);
        }

        public override void LoadWorldData(TagCompound tag)
        {
            var plushieTileList = tag.GetList<string>("plushieTiles");
            plushieTiles.Clear();

            for (int i = 0; i < plushieTileList.Count; i++)
            {
                string[] plushieTile = plushieTileList[i].Split('/');

                long value1 = -1;
                short value2 = -1;

                if (long.TryParse(plushieTile[0], out long v1)) value1 = v1;
                if (short.TryParse(plushieTile[1], out short v2)) value2 = v2;

                if (value1 != -1 && value2 != -1)
                {
                    plushieTiles.Add(value1, value2);
                }
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            // PlushieTiles
            writer.Write((int)plushieTiles.Count);
            foreach (KeyValuePair<long, short> plushieTile in plushieTiles)
            {
                writer.Write((long)plushieTile.Key);
                writer.Write((short)plushieTile.Value);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            // PlushieTiles
            plushieTiles.Clear();

            int plushieTileCount = reader.ReadInt32();
            for (int i = 0; i < plushieTileCount; i++)
            {
                long key = reader.ReadInt64();
                short value = reader.ReadInt16();
                plushieTiles.Add(key, value);
            }
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int index = tasks.FindIndex(genpass => genpass.Name.Equals("Dye Plants"));
            if (index != -1)
            {
                tasks.Insert(index + 1, new PassLegacy("[Kourindou] Placing Cotton", PlacingCottonPlants));
                tasks.Insert(index + 1, new PassLegacy("[Kourindou] Placing Flax", PlacingFlaxPlants));
            }
        }

        private static void PlacingCottonPlants(GenerationProgress progress, GameConfiguration config)
        {
            int MaxCottonPlants = GetCottonPlantAmount();

            for (int plant = 0; plant < MaxCottonPlants; plant++)
            {
                float currentProgress = plant / (float)MaxCottonPlants;
                progress.Set(currentProgress);

                for (int attempt = 0; attempt < 1000; attempt++)
                {
                    // Get a random X coordinate
                    int tileX = WorldGen.genRand.Next(0, Main.maxTilesX);

                    // Get the tile Y of the space layer
                    int tileY = (int)(Main.worldSurface * 0.35);

                    // Scan downwards until we've hit a block
                    while (!Main.tile[tileX, tileY].HasTile)
                    {
                        tileY++;
                    }

                    // Check if there is already a plant nearby
                    if (!Cotton_Tile.CheckCottonLimits(tileX, tileY))
                    {
                        continue;
                    }

                    // Place the tile
                    if (WorldGen.PlaceObject(tileX, tileY - 1, TileType<Cotton_Tile>(), true))
                    {
                        break;
                    }
                }
            }
        }

        private static int GetCottonPlantAmount()
        {
            return (int)((float)Main.maxTilesX / 4200f * 8f); //Small=8, Medium=12, Large=16
        }

        private static void PlacingFlaxPlants(GenerationProgress progress, GameConfiguration config)
        {
            int MaxFlaxPlants = GetFlaxPlantAmount();

            for (int plant = 0; plant < MaxFlaxPlants; plant++)
            {
                float currentProgress = plant / (float)MaxFlaxPlants;
                progress.Set(currentProgress);

                for (int attempt = 0; attempt < 1000; attempt++)
                {
                    // Get a random X coordinate
                    int tileX = WorldGen.genRand.Next(0, Main.maxTilesX);

                    // Get the tile Y of the space layer
                    int tileY = (int)(Main.worldSurface * 0.35);

                    // Scan downwards until we've hit a block
                    while (!Main.tile[tileX, tileY].HasTile)
                    {
                        tileY++;
                    }

                    if (!Flax_Tile.CheckFlaxLimits(tileX, tileY))
                    {
                        continue;
                    }

                    if (WorldGen.PlaceObject(tileX, tileY - 1, TileType<Flax_Tile>(), true))
                    {
                        break;
                    }
                }
            }
        }

        private static int GetFlaxPlantAmount()
        {
            return (int)((float)Main.maxTilesX / 4200f * 8f); //Small=8, Medium=12, Large=16
        }

        public static void SetPlushieDirtWater(int i, int j, short value)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                long key = (long)(i * 4294967296) + (long)j;

                if (plushieTiles.ContainsKey(key))
                {
                    plushieTiles[key] = value;
                }
                else
                {
                    plushieTiles.Add(key, value);
                }
            }
        }

        public static short GetPlushieDirtWater(int i, int j, bool breakTile)
        {
            short value = 0;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                long key = (long)(i * 4294967296) + (long)j;

                if (plushieTiles.ContainsKey(key))
                {
                    value = plushieTiles[key];

                    if (breakTile)
                    {
                        plushieTiles.Remove(key);
                    }
                }
            }
            return value;
        }
    }
}