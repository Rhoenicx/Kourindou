using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Enums;
using Terraria.DataStructures;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Items.Seeds;

namespace Kourindou.Tiles.Plants
{
    public enum PlantStage : byte
    {
        Planted,
        Growing,
        Grown,
        Blooming1,
        Blooming2,
        Blooming3
    }

    public enum PlantStyle : byte
    {
        Forest,
        Jungle,
        Corruption,
        Crimson,
        Mushroom,
        HallowBlue,
        HallowDarkBlue,
        HallowPink,
        HallowPurple,
        HallowRed,
        HallowGreen,
        HallowYellow
    }

    public class Cotton_Tile : ModTile
    {
        private const int FrameWidth = 36;
        private const int FrameHeight = 18;

        private const int PlantFrameHeight = 56;

        private PlantStyle OldStyle;

        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = false;
            Main.tileSolid[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileWaterDeath[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileNoAttach[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new int[]{ 16, 16, 18 };
            TileObjectData.newTile.Origin = new Point16(0, 2);
            TileObjectData.newTile.StyleWrapLimit = 6;
            TileObjectData.newTile.StyleMultiplier = 12;


            TileObjectData.newTile.AnchorValidTiles = new int[]
			{
                TileID.Dirt,
				TileID.Grass,
                TileID.JungleGrass,
                TileID.CorruptGrass,
                TileID.FleshGrass,
                TileID.MushroomGrass,
				TileID.HallowedGrass
			};

			TileObjectData.newTile.AnchorAlternateTiles = new int[]
			{
				TileID.PlanterBox
			};

            TileObjectData.addTile(Type);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			PlantStage stage = (PlantStage)(int)Math.Floor((double)(frameX / FrameWidth));

			Item.NewItem(i * 16, j * 16, 16, 16, ItemType<CottonSeeds>());

            // Drop Wood
            if (stage >= PlantStage.Grown)
            {
                int woodDrops = (int)Main.rand.Next(1, 4);

                for (int a = 0; a < woodDrops; a++)
                {
                    switch (OldStyle)
                    {
                        case PlantStyle.Forest:
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, ItemID.Wood);
                            break;
                        }
                        case PlantStyle.Jungle:
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, ItemID.RichMahogany);
                            break;
                        }
                        case PlantStyle.Corruption:
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, ItemID.Ebonwood);
                            break;
                        }
                        case PlantStyle.Crimson:
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, ItemID.Shadewood);
                            break;
                        }
                        case PlantStyle.Mushroom:
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, ItemID.GlowingMushroom);
                            break;
                        }
                        case PlantStyle.HallowBlue:
                        case PlantStyle.HallowDarkBlue:
                        case PlantStyle.HallowPink:
                        case PlantStyle.HallowPurple:
                        case PlantStyle.HallowRed:
                        case PlantStyle.HallowGreen:
                        case PlantStyle.HallowYellow:
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, ItemID.Pearlwood);
                            break;
                        }
                    }
                }
            }

            // Drop 1 Fibre
            if (stage == PlantStage.Blooming1)
            {
                Item.NewItem(i * 16, j * 16, 16, 16, ItemType<CottonFibre>());
            }

            if (stage == PlantStage.Blooming2)
            {
                int fibreDrops = Main.rand.Next(1, 3);

                for (int a = 0; a < fibreDrops; a++)
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<CottonFibre>());
                }
            }

            if (stage == PlantStage.Blooming3)
            {
                // Drop Fibre
                int fibreDrops = Main.rand.Next(3, 7);

                for (int a = 0; a < fibreDrops; a++)
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<CottonFibre>());
                }

                // Drop additional seeds
                int seedDrops = Main.rand.Next(0, 3);

                for (int a = 0; a < seedDrops; a++)
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<CottonSeeds>());
                }
            }
		}

		public override void RandomUpdate(int i, int j)
		{
            if (Main.rand.Next(0, 13) == 0 && Main.dayTime && !Main.eclipse)
            {
                PlantStage stage = GetStage(i, j);

			    // Only grow to the next stage if there is a next stage. We dont want our tile turning pink!
			    if (stage != PlantStage.Blooming3)
                {
                    if (stage == PlantStage.Planted || stage == PlantStage.Growing)
                    {
			    	    // Increase the x frame to change the stage
			    	    UpdateMultiTile(i, j, FrameWidth, (int)GetStyle(i, j));
                    }

                    if (stage == PlantStage.Grown || stage == PlantStage.Blooming1 || stage == PlantStage.Blooming2)
                    {
                        if ((int)Main.rand.Next(0, 2) == 0)
                        {
                            // Increase the x frame to change the stage
			    	        UpdateMultiTile(i, j, FrameWidth, (int)GetStyle(i, j));
                        }
                    }
			    }
            }
		}

        public override bool NewRightClick(int i, int j)
        {
            PlantStage stage = GetStage(i, j);

            if (stage == PlantStage.Blooming1)
            {
                Item.NewItem(i * 16, j * 16, 16, 16, ItemType<CottonFibre>());

                UpdateMultiTile(i, j, -FrameWidth, (int)GetStyle(i, j));
            }

            if (stage == PlantStage.Blooming2)
            {
                int fibreDrops = Main.rand.Next(1, 2);

                for (int a = 0; a < fibreDrops; a++)
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<CottonFibre>());
                }

                UpdateMultiTile(i, j, -FrameWidth * 2, (int)GetStyle(i, j));
            }

            if (stage == PlantStage.Blooming3)
            {
                int fibreDrops = Main.rand.Next(3, 6);

                for (int a = 0; a < fibreDrops; a++)
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<CottonFibre>());
                }

                UpdateMultiTile(i, j, -FrameWidth * 3, (int)GetStyle(i, j));
            }

            return true;
        }

        public override bool CanKillTile (int i, int j, ref bool blockDamaged)
        {
            PlantStyle style = GetStyle(i, j);
            OldStyle = style;

            return true;
        }

        public override bool TileFrame (int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile tileUnder = Framing.GetTileSafely(i, j + 1);
            PlantStyle style = GetStyle(i, j);

            bool update = false;

            switch (tileUnder.type)
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

                case TileID.CorruptGrass:
                {
                    if (style != PlantStyle.Corruption)
                    {
                        style = PlantStyle.Corruption;
                        update = true;
                    }
                    break;
                }

                case TileID.FleshGrass:
                {
                    if (style != PlantStyle.Crimson)
                    {
                        style = PlantStyle.Crimson;
                        update = true;
                    }
                    break;
                }

                case TileID.MushroomGrass:
                {
                    if (style != PlantStyle.Mushroom)
                    {
                        style = PlantStyle.Mushroom;
                        update = true;
                    }
                    break;
                }

				case TileID.HallowedGrass:
                {
                    if (style != PlantStyle.HallowBlue && style != PlantStyle.HallowDarkBlue &&
                        style != PlantStyle.HallowPink && style != PlantStyle.HallowPurple &&
                        style != PlantStyle.HallowRed && style != PlantStyle.HallowGreen &&
                        style != PlantStyle.HallowYellow)
                    {
                        style = (PlantStyle)(int)Main.rand.Next((int)PlantStyle.HallowBlue, (int)PlantStyle.HallowYellow + 1);
                        update = true;
                    }
                    break;
                }
            }

            if (update)
            {
                UpdateMultiTile(i, j, 0, (int)style);
            }

            return true;
        }

        private PlantStage GetStage(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);
            return (PlantStage)(int)Math.Floor((double)(tile.frameX / FrameWidth));
		}

        private PlantStyle GetStyle(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            return (PlantStyle)(int)Math.Floor((double)(tile.frameY / PlantFrameHeight));
        }

        private void UpdateMultiTile(int i, int j, int width, int style)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            int tileX = (int)Math.Floor((double)(tile.frameX / (FrameWidth / 2)));
            int tileY = (int)Math.Floor((double)(((tile.frameY - ((int)GetStyle(i, j) * PlantFrameHeight))) / FrameHeight));

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
                for (int y = 0; y < 3; y++)
                {
                    Tile currentTile = Framing.GetTileSafely(i + (direction ? 0 : -1) + x, j - tileY + y);

                    currentTile.frameY = (short)((style * PlantFrameHeight) + y * 18);
                    currentTile.frameX += (short)width;

                    // If in multiplayer, sync the frame change
				    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
				    	NetMessage.SendTileSquare(-1, i, j, 1);
                    }
                }
            }
        }
    }
}