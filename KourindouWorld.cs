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

namespace Kourindou
{
    public class KourindouWorld : ModWorld
    {
        // Plushie Dirt and wet mechanic saving
        public static Dictionary<long, short> plushieTiles = new Dictionary<long, short>();

        // Cotton Plants
        public int CottonPlants = 0;

        private const int Small_CottonMax = 50;
        private const int Medium_CottonMax = 75;
        private const int Large_CottonMax = 100;

        // Flax Plants
        public int FlaxPlants = 0;

        private const int Small_FlaxMax = 50;
        private const int Medium_FlaxMax = 75;
        private const int Large_FlaxMax = 100;

        public override TagCompound Save()
        {
            List<string> plushieTileList = new List<string>();
            foreach (KeyValuePair<long, short> pt in plushieTiles)
            {
                plushieTileList.Add(pt.Key.ToString() + "/" + pt.Value.ToString());
            }

            return new TagCompound
            {
                { "plushieTiles", plushieTileList }
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
            int index = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
            tasks.Insert(index + 1, new PassLegacy("[Kourindou] Placing Cotton", PlacingCottonPlants));
            tasks.Insert(index + 1, new PassLegacy("[Kourindou] Placing Flax", PlacingFlaxPlants));
        }

        public void PlacingCottonPlants(GenerationProgress progress = null)
        {

        }

        public void PlacingFlaxPlants(GenerationProgress progress = null)
        {

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