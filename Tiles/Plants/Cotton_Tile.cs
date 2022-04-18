using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
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
    public class Cotton_Tile : ModTile
    {
        private const int FrameWidth = 36;
        private const int FrameHeight = 18;

        private const int PlantFrameHeight = 56;

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
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new int[]{ 16, 16, 18 };
            TileObjectData.newTile.Origin = new Point16(0, 2);
            TileObjectData.newTile.StyleWrapLimit = 6;
            TileObjectData.newTile.StyleMultiplier = 12;


            TileObjectData.newTile.AnchorValidTiles = new int[]
			{
                TileID.Dirt,
				TileID.Grass,
                TileID.GolfGrass,
                TileID.JungleGrass,
                TileID.CorruptGrass,
                TileID.CrimsonGrass,
                TileID.MushroomGrass,
				TileID.HallowedGrass
			};

			TileObjectData.newTile.AnchorAlternateTiles = new int[]
			{
				TileID.PlanterBox
			};

            TileObjectData.addTile(Type);

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Cotton Plant");
            AddMapEntry(new Color(155, 155, 155), name);

            
            SoundStyle = 0;
            SoundType = 0;
        }

        public override void PlaceInWorld(int i, int j, Item item) //Runs only on SinglePlayer and MultiplayerClient!
        {
            if (item.type == ItemType<CottonSeeds>())
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte) KourindouMessageType.PlayerPlacePlantTile);
                    packet.Write((int) TileType<Cotton_Tile>());
                    packet.Send(-1, Main.myPlayer);
                }
                else if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    KourindouWorld.CottonPlants++;
                }
            }
        }

        public override void MouseOver(int i, int j)
        {
            PlantStage stage = GetStage(i, j);

            if (stage >= PlantStage.Blooming1)
            {
                Player player = Main.LocalPlayer;
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = ItemType<CottonFibre>();
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
			    Item.NewItem(new EntitySource_TileBreak(i, j),i * 16, j * 16, 16, 16, ItemType<CottonSeeds>());

                PlantStage stage = (PlantStage)(int)Math.Floor((double)(frameX / FrameWidth));

                // Drop 1 Fibre
                if (stage == PlantStage.Blooming1)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ItemType<CottonFibre>());
                }

                if (stage == PlantStage.Blooming2)
                {
                    int fibreDrops = Main.rand.Next(1, 3);

                    for (int a = 0; a < fibreDrops; a++)
                    {
                        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ItemType<CottonFibre>());
                    }
                }

                if (stage == PlantStage.Blooming3)
                {
                    // Drop Fibre
                    int fibreDrops = Main.rand.Next(3, 7);

                    for (int a = 0; a < fibreDrops; a++)
                    {
                        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ItemType<CottonFibre>());
                    }

                    // Drop additional seeds
                    int seedDrops = Main.rand.Next(0, 3);

                    for (int a = 0; a < seedDrops; a++)
                    {
                        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ItemType<CottonSeeds>());
                    }
                }

                KourindouWorld.CottonPlants--;
            }
		}

		public override void RandomUpdate(int i, int j)
		{
            if (Main.netMode != NetmodeID.MultiplayerClient)
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
		}

        public override bool RightClick(int i, int j)
        {
            PlantStage stage = GetStage(i, j);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (stage == PlantStage.Blooming1)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ItemType<CottonFibre>());

                    UpdateMultiTile(i, j, -FrameWidth, (int)GetStyle(i, j));
                    SoundEngine.PlaySound(SoundID.Grass, i * 16 + 8, j * 16 + 8, 0, .8f, Main.rand.NextFloat(-.2f,.2f));
                }

                if (stage == PlantStage.Blooming2)
                {
                    int fibreDrops = Main.rand.Next(1, 2);

                    for (int a = 0; a < fibreDrops; a++)
                    {
                        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ItemType<CottonFibre>());
                    }

                    UpdateMultiTile(i, j, -FrameWidth * 2, (int)GetStyle(i, j));
                    SoundEngine.PlaySound(SoundID.Grass, i * 16 + 8, j * 16 + 8, 0, .8f, Main.rand.NextFloat(-.2f,.2f));
                }

                if (stage == PlantStage.Blooming3)
                {
                    int fibreDrops = Main.rand.Next(3, 6);

                    for (int a = 0; a < fibreDrops; a++)
                    {
                        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ItemType<CottonFibre>());
                    }

                    UpdateMultiTile(i, j, -FrameWidth * 3, (int)GetStyle(i, j));
                    SoundEngine.PlaySound(SoundID.Grass, i * 16 + 8, j * 16 + 8, 0, .8f, Main.rand.NextFloat(-.2f,.2f));
                }
            }
            else
            {
                if (stage >= PlantStage.Blooming1)
                {
                    // Send the right click event to the Server so items can be dropped
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte) KourindouMessageType.CottonRightClick);
                    packet.Write(i);
                    packet.Write(j);
                    packet.Send();

                    // Play the sound clientside
                    SoundEngine.PlaySound(SoundID.Grass, i * 16 + 8, j * 16 + 8, 0, .8f, Main.rand.NextFloat(-.2f,.2f));

                    // Send sound packet for other clients
                    ModPacket packet2 = Mod.GetPacket();
                    packet2.Write((byte) KourindouMessageType.PlaySound);
                    packet2.Write((byte) SoundID.Grass);
                    packet2.Write((short) 0);
                    packet2.Write((float) 0.8f);
                    packet2.Write((float) Main.rand.NextFloat(-.2f, .2f));
                    packet2.Write((int) i * 16 + 8);
                    packet2.Write((int) j * 16 + 8);
                    packet2.Send();
                }
            }
            return true;
        }

        public override bool TileFrame (int i, int j, ref bool resetFrame, ref bool noBreak)
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

                    case TileID.CorruptGrass:
                    {
                        if (style != PlantStyle.Corruption)
                        {
                            style = PlantStyle.Corruption;
                            update = true;
                        }
                        break;
                    }

                    case TileID.CrimsonGrass:
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
            }
            return true;
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            type = 7;
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
                for (int y = 0; y < 3; y++)
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