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
                && Main.rand.Next(0, 200) == 0
                && j < Main.worldSurface
                && Main.dayTime)
            {
                int plantType = (int)Main.rand.Next(0, 2);

                // Cotton
                if (plantType == 0
                    && Cotton_Tile.CheckCottonLimits(i, j)
                    && Cotton_Tile.TileValidForCotton(i, j)
                    && Cotton_Tile.TileValidForCotton(i - 1, j)
                    && WorldGen.PlaceObject(i, j - 1, TileType<Cotton_Tile>(), true))
                {
                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendObjectPlacement(Main.myPlayer, i, j - 1, TileType<Cotton_Tile>(), 0, 0, -1, -1);
                    }
                }

                // Flax
                if (plantType == 1
                    && Flax_Tile.CheckFlaxLimits(i, j)
                    && Flax_Tile.TileValidForFlax(i, j)
                    && Flax_Tile.TileValidForFlax(i - 1, j)
                    && WorldGen.PlaceObject(i, j - 1, TileType<Flax_Tile>(), true))
                {
                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendObjectPlacement(Main.myPlayer, i, j - 1, TileType<Flax_Tile>(), 0, 0, -1, -1);
                    }
                }
            }
        }
    }
}