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
        Brown,
		Rainbow
    }

    public class Thread_Tile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = false;
            Main.tileSolid[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileNoAttach[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new int[]{ 20 };
            TileObjectData.newTile.CoordinateWidth = 32;
            TileObjectData.newTile.DrawYOffset = -4;
            TileObjectData.newTile.CoordinatePadding = 2;

            TileObjectData.newTile.Origin = new Point16(0, 0);

            TileObjectData.newTile.StyleWrapLimit = 17;
            TileObjectData.newTile.StyleMultiplier = 17;

            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile | AnchorType.Table, TileObjectData.newTile.Width, 0);
        
            TileObjectData.addTile(Type);

            TileID.Sets.DisableSmartCursor[Type] = true;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(43, 19, 103), name);
        }

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = 0;
		}

        private ThreadStyle GetStyle(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            return (ThreadStyle)(int)Math.Floor((double)tile.TileFrameX / 34);
        }
    }
}