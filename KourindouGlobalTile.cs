using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plants;

namespace Kourindou
{
    public class KourindouGlobalTile : GlobalTile
    {
        public override void RandomUpdate(int i, int j, int type)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient
                && Main.rand.Next(0, 150) == 1
                && j < Main.worldSurface
                && Main.dayTime)
            {
                int plantType = (int)Main.rand.Next(0, 2);

                // Cotton
                if (plantType == 0 && KourindouWorld.CottonPlants < KourindouWorld.MaxCottonPlants && TileValidForCotton(i, j))
                {
                    if (TileValidForCotton(i + 1, j))
                    {
                        WorldGen.PlaceObject(i, j - 1, TileType<Cotton_Tile>());
                        KourindouWorld.CottonPlants++;

                        if (Main.netMode == NetmodeID.Server)
                        {
                            ModPacket packet = Mod.GetPacket();
                            packet.Write((byte) KourindouMessageType.RandomPlacePlantTile);
                            packet.Write((int) i);
                            packet.Write((int) j - 1);
                            packet.Write((int)TileType<Cotton_Tile>());
                            packet.Send(-1, Main.myPlayer);
                        }
                    }
                    else if (TileValidForCotton(i - 1, j))
                    {
                        WorldGen.PlaceObject(i - 1, j - 1, TileType<Cotton_Tile>());
                        KourindouWorld.CottonPlants++;

                        if (Main.netMode == NetmodeID.Server)
                        {
                            ModPacket packet = Mod.GetPacket();
                            packet.Write((byte)KourindouMessageType.RandomPlacePlantTile);
                            packet.Write((int)i - 1);
                            packet.Write((int)j - 1);
                            packet.Write((int)TileType<Cotton_Tile>());
                            packet.Send(-1, Main.myPlayer);
                        }
                    }
                }

                // Flax
                if (plantType == 1 && KourindouWorld.FlaxPlants < KourindouWorld.MaxFlaxPlants && TileValidForFlax(i, j))
                {
                    if (TileValidForFlax(i + 1, j))
                    {
                        WorldGen.PlaceObject(i, j - 1, TileType<Flax_Tile>());
                        KourindouWorld.FlaxPlants++;

                        if (Main.netMode == NetmodeID.Server)
                        {
                            ModPacket packet = Mod.GetPacket();
                            packet.Write((byte)KourindouMessageType.RandomPlacePlantTile);
                            packet.Write((int)i);
                            packet.Write((int)j - 1);
                            packet.Write((int)TileType<Flax_Tile>());
                            packet.Send(-1, Main.myPlayer);
                        }
                    }
                    else if (TileValidForFlax(i - 1, j))
                    {
                        WorldGen.PlaceObject(i - 1, j - 1, TileType<Flax_Tile>());
                        KourindouWorld.FlaxPlants++;

                        if (Main.netMode == NetmodeID.Server)
                        {
                            ModPacket packet = Mod.GetPacket();
                            packet.Write((byte)KourindouMessageType.RandomPlacePlantTile);
                            packet.Write((int)i - 1);
                            packet.Write((int)j - 1);
                            packet.Write((int)TileType<Flax_Tile>());
                            packet.Send(-1, Main.myPlayer);
                        }
                    }
                }
            }
        }

        private bool TileValidForCotton(int i, int j)
        {
            if ((Main.tile[i, j].TileType == TileID.Dirt
                || Main.tile[i, j].TileType == TileID.Grass
                || Main.tile[i, j].TileType == TileID.JungleGrass
                || Main.tile[i, j].TileType == TileID.CorruptGrass
                || Main.tile[i, j].TileType == TileID.CrimsonGrass
                || Main.tile[i, j].TileType == TileID.MushroomGrass
                || Main.tile[i, j].TileType == TileID.HallowedGrass)
                && !Main.tile[i, j - 1].HasTile
                && !Main.tile[i, j - 2].HasTile
                && !Main.tile[i, j - 3].HasTile
                && Main.tile[i, j].Slope == 0)
            {
                return true;
            }

            return false;
        }

        private bool TileValidForFlax(int i, int j)
        {
            if ((Main.tile[i, j].TileType == TileID.Dirt
                || Main.tile[i, j].TileType == TileID.Grass
                || Main.tile[i, j].TileType == TileID.JungleGrass)
                && !Main.tile[i, j - 1].HasTile
                && !Main.tile[i, j - 2].HasTile
                && Main.tile[i, j].Slope == 0)
            {
                return true;
            }

            return false;
        }
    }
}