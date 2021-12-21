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

namespace Kourindou.Tiles.Furniture
{
    public enum ThreadStyle : byte
    {
        White,
        Silver,
        Black,
        Red,
        Pink,
        Violet,
        Purple,
        Blue,
        SkyBlue,
        Cyan,
        Teal,
        Green,
        Lime,
        Yellow,
        Orange,
        Brown
    }

    public class Thread_Tile : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = false;
            Main.tileSolid[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileWaterDeath[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileNoAttach[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new int[]{ 20 };
            TileObjectData.newTile.CoordinateWidth = 32;
            TileObjectData.newTile.DrawYOffset = -4;
            TileObjectData.newTile.CoordinatePadding = 2;

            TileObjectData.newTile.Origin = new Point16(0, 0);

            TileObjectData.newTile.StyleWrapLimit = 16;
            TileObjectData.newTile.StyleMultiplier = 16;

            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile | AnchorType.Table, TileObjectData.newTile.Width, 0);
        
            TileObjectData.addTile(Type);

            disableSmartCursor = true;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Thread");
            AddMapEntry(new Color(43, 19, 103), name);
        }

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = 0;
		}

        public override void KillTile (int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            ThreadStyle style = GetStyle(i, j);

            switch (style)
            {
                case ThreadStyle.White:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<WhiteThread>());
                    break;
                }

                case ThreadStyle.Silver:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<SilverThread>());
                    break;
                }

                case ThreadStyle.Black:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemID.BlackThread);
                    break;
                }

                case ThreadStyle.Red:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<RedThread>());
                    break;
                }

                case ThreadStyle.Pink:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemID.PinkThread);
                    break;
                }

                case ThreadStyle.Violet:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<VioletThread>());
                    break;
                }

                case ThreadStyle.Purple:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<PurpleThread>());
                    break;
                }

                case ThreadStyle.Blue:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<BlueThread>());
                    break;
                }

                case ThreadStyle.SkyBlue:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<SkyBlueThread>());
                    break;
                }

                case ThreadStyle.Cyan:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<CyanThread>());
                    break;
                }

                case ThreadStyle.Teal:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<TealThread>());
                    break;
                }

                case ThreadStyle.Green:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemID.GreenThread);
                    break;
                }

                case ThreadStyle.Lime:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<LimeThread>());
                    break;
                }

                case ThreadStyle.Yellow:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<YellowThread>());
                    break;
                }

                case ThreadStyle.Orange:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<OrangeThread>());
                    break;
                }

                case ThreadStyle.Brown:
                {
                    Item.NewItem(i * 16, j * 16, 16, 16, ItemType<BrownThread>());
                    break;
                }
            }
        }

        private ThreadStyle GetStyle(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            return (ThreadStyle)(int)Math.Floor((double)tile.frameX / 34);
        }
    }
}