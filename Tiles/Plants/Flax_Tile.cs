using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Enums;
using Terraria.DataStructures;
using Terraria.ObjectData;
using Terraria.GameContent.Drawing;
using static Terraria.ModLoader.ModContent;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Items.Seeds;

namespace Kourindou.Tiles.Plants
{
    public class Flax_Tile : ModTile
    {
        private const int FrameWidth = 36;
        private const int FrameHeight = 18;

        private const int PlantFrameHeight = 38;

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = false;
            Main.tileSolid[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileWaterDeath[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileNoAttach[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 18 };
            TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.StyleWrapLimit = 3;
            TileObjectData.newTile.StyleMultiplier = 2;

            TileObjectData.newTile.AnchorValidTiles = new int[]
            {
                TileID.Dirt,
                TileID.Grass,
                TileID.GolfGrass,
                TileID.JungleGrass
            };

            TileObjectData.newTile.AnchorAlternateTiles = new int[]
            {
                TileID.PlanterBox
            };

            TileObjectData.addTile(Type);

            LocalizedText name = CreateMapEntryName();
            // name.SetDefault("Flax Plant");
            AddMapEntry(new Color(1, 128, 201), name);

            HitSound = SoundID.Grass;
        }

        public override void PlaceInWorld(int i, int j, Item item) //Runs only on SinglePlayer and MultiplayerClient!
        {
            if (item.type == ItemType<FlaxSeeds>())
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)KourindouMessageType.PlayerPlacePlantTile);
                    packet.Write((int)TileType<Flax_Tile>());
                    packet.Send(-1, Main.myPlayer);
                }
                else if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    KourindouWorld.FlaxPlants++;
                }
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<FlaxSeeds>());

                PlantStage stage = (PlantStage)(int)Math.Floor((double)(frameX / FrameWidth));

                if (stage == PlantStage.Grown)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<FlaxBundle>());

                    int dropFlaxSeeds = Main.rand.Next(1, 4);

                    for (int a = 0; a < dropFlaxSeeds; a++)
                    {
                        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<FlaxSeeds>());
                    }
                }

                KourindouWorld.FlaxPlants--;
            }
        }

        public override void RandomUpdate(int i, int j)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Main.rand.Next(0, 9) == 0 && Main.dayTime && !Main.eclipse)
                {
                    PlantStage stage = GetStage(i, j);

                    // Only grow to the next stage if there is a next stage. We dont want our tile turning pink!
                    if (stage != PlantStage.Grown)
                    {
                        // Increase the x frame to change the stage
                        UpdateMultiTile(i, j, FrameWidth, (int)GetStyle(i, j));
                    }
                }
            }
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Tile tileUnder = Framing.GetTileSafely(i, j + 1);
                PlantStyle style = GetStyle(i, j);

                bool update = false;

                switch (tileUnder.TileType)
                {
                    case TileID.Dirt:
                        {
                            if (style != PlantStyle.Forest)
                            {
                                style = PlantStyle.Forest;
                                update = true;
                            }
                            break;
                        }

                    case TileID.Grass:
                    case TileID.GolfGrass:
                        {
                            if (style != PlantStyle.Forest)
                            {
                                style = PlantStyle.Forest;
                                update = true;
                            }
                            break;
                        }

                    case TileID.JungleGrass:
                        {
                            if (style != PlantStyle.Jungle)
                            {
                                style = PlantStyle.Jungle;
                                update = true;
                            }
                            break;
                        }
                }

                if (update)
                {
                    UpdateMultiTile(i, j, 0, (int)style);
                }
            }
            return true;
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            type = 3;
            return true;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 4;
        }

        private PlantStage GetStage(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            return (PlantStage)(int)Math.Floor((double)(tile.TileFrameX / FrameWidth));
        }

        private PlantStyle GetStyle(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            return (PlantStyle)(int)Math.Floor((double)(tile.TileFrameY / PlantFrameHeight));
        }

        private void UpdateMultiTile(int i, int j, int width, int style)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            int tileX = (int)Math.Floor((double)(tile.TileFrameX / (FrameWidth / 2)));
            int tileY = (int)Math.Floor((double)(((tile.TileFrameY - ((int)GetStyle(i, j) * PlantFrameHeight))) / FrameHeight));

            bool direction = false;

            // Determine which direction the other plant tiles are located
            if (tileX % 2 == 1)
            {
                direction = false;
            }
            else if (tileX % 2 == 0)
            {
                direction = true;
            }

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    Tile currentTile = Framing.GetTileSafely(i + (direction ? 0 : -1) + x, j - tileY + y);

                    currentTile.TileFrameY = (short)((style * PlantFrameHeight) + y * 18);
                    currentTile.TileFrameX += (short)width;

                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendTileSquare(-1, i + (direction ? 0 : -1) + x, j - tileY + y, 1);
                    }
                }
            }
        }
    }
}