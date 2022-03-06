using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plants;

namespace Kourindou
{
    public class KourindouWorld : ModWorld
    {
        // Plushie Dirt and wet mechanic saving
        public static Dictionary<long, short> plushieTiles = new Dictionary<long, short>();

        // Cotton Plants
        public static int CottonPlants = 0;
        public static int MaxCottonPlants = 0;

        // Flax Plants
        public static int FlaxPlants = 0;
        public static int MaxFlaxPlants = 0;

        public override TagCompound Save()
        {
            List<string> plushieTileList = new List<string>();
            foreach (KeyValuePair<long, short> pt in plushieTiles)
            {
                plushieTileList.Add(pt.Key.ToString() + "/" + pt.Value.ToString());
            }

            return new TagCompound
            {
                { "plushieTiles", plushieTileList },
                { "maxCottonPlants", MaxCottonPlants },
                { "placedCottonPlants", CottonPlants },
                { "maxFlaxPlants", MaxFlaxPlants },
                { "placedFlaxPlants", FlaxPlants }
            };
        }

        public override void Load(TagCompound tag)
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

            MaxCottonPlants = tag.GetInt("maxCottonPlants");
            CottonPlants = tag.GetInt("placedCottonPlants");
            MaxFlaxPlants = tag.GetInt("maxFlaxPlants");
            FlaxPlants = tag.GetInt("placedFlaxPlants");

            // If this world is not generated with this mod active.
            if (MaxFlaxPlants == 0 || MaxCottonPlants == 0)
            {
                MaxCottonPlants = GetCottonPlantAmount();
                MaxFlaxPlants = GetFlaxPlantAmount();
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

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            int index = tasks.FindIndex(genpass => genpass.Name.Equals("Dye Plants"));
            if (index != -1)
            {
                tasks.Insert(index + 1, new PassLegacy("[Kourindou] Placing Cotton", PlacingCottonPlants));
                tasks.Insert(index + 1, new PassLegacy("[Kourindou] Placing Flax", PlacingFlaxPlants));
            }
        }

        private static void PlacingCottonPlants(GenerationProgress progress = null)
        {
            MaxCottonPlants = GetCottonPlantAmount();

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
                    while (!Main.tile[tileX, tileY].active())
                    {
                        tileY++;
                    }

                    // Check if the hit Tile is valid for the plant
                    int hitTile = Main.tile[tileX, tileY].type;

                    if (hitTile == TileID.Dirt || hitTile == TileID.Grass || hitTile == TileID.JungleGrass || hitTile == TileID.CorruptGrass
                        || hitTile == TileID.FleshGrass || hitTile == TileID.MushroomGrass || hitTile == TileID.HallowedGrass)
                    {
                        int tileRight = Main.tile[tileX + 1, tileY].type;
                        int tileLeft = Main.tile[tileX - 1, tileY].type;

                        if (tileRight == TileID.Dirt || tileRight == TileID.Grass || tileRight == TileID.JungleGrass || tileRight == TileID.CorruptGrass
                            || tileRight == TileID.FleshGrass || tileRight == TileID.MushroomGrass || tileRight == TileID.HallowedGrass)
                        {
                            WorldGen.PlaceObject(tileX, tileY - 1, TileType<Cotton_Tile>());
                            if (Framing.GetTileSafely(tileX, tileY - 1).type == TileType<Cotton_Tile>())
                            {
                                CottonPlants++;
                                break;
                            }
                        }
                        else if (tileLeft == TileID.Dirt || tileLeft == TileID.Grass || tileLeft == TileID.JungleGrass || tileLeft == TileID.CorruptGrass
                            || tileLeft == TileID.FleshGrass || tileLeft == TileID.MushroomGrass || tileLeft == TileID.HallowedGrass)
                        {
                            WorldGen.PlaceObject(tileX - 1, tileY - 1, TileType<Cotton_Tile>());
                            if (Framing.GetTileSafely(tileX - 1, tileY - 1).type == TileType<Cotton_Tile>())
                            {
                                CottonPlants++;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static int GetCottonPlantAmount()
        {
            return (int)((float)Main.maxTilesX / 4200f * 32f); //Small=32, Medium=48, Large=64
        }

        private static void PlacingFlaxPlants(GenerationProgress progress = null)
        {
            MaxFlaxPlants = GetFlaxPlantAmount();

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
                    while (!Main.tile[tileX, tileY].active())
                    {
                        tileY++;
                    }

                    // Check if the hit Tile is valid for the plant
                    int hitTile = Main.tile[tileX, tileY].type;

                    if (hitTile == TileID.Dirt || hitTile == TileID.Grass || hitTile == TileID.JungleGrass)
                    {
                        int tileRight = Main.tile[tileX + 1, tileY].type;
                        int tileLeft  = Main.tile[tileX - 1, tileY].type;

                        if (tileRight == TileID.Dirt || tileRight == TileID.Grass || tileRight == TileID.JungleGrass)
                        {
                            WorldGen.PlaceObject(tileX, tileY - 1, TileType<Flax_Tile>());
                            if (Framing.GetTileSafely(tileX, tileY - 1).type == TileType<Flax_Tile>())
                            {
                                FlaxPlants++;
                                break;
                            }
                        }
                        else if (tileLeft == TileID.Dirt || tileLeft == TileID.Grass || tileLeft == TileID.JungleGrass)
                        {
                            WorldGen.PlaceObject(tileX - 1, tileY - 1, TileType<Flax_Tile>());
                            if (Framing.GetTileSafely(tileX - 1, tileY -1).type == TileType<Flax_Tile>())
                            {
                                FlaxPlants++;
                                break;
                            }
                        }
                    }
                }
            }
        }
        private static int GetFlaxPlantAmount()
        {
            return (int)((float)Main.maxTilesX / 4200f * 32f); //Small=32, Medium=48, Large=64
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