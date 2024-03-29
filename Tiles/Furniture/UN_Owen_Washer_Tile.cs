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
using Kourindou.Items.Furniture;

namespace Kourindou.Tiles.Furniture
{
    public class UN_Owen_Washer_Tile : ModTile
    {
        public override void SetStaticDefaults()
        {
            // Tile Settings
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = false;
            Main.tileSolid[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileNoAttach[Type] = true;

            // Tile Size and direction
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.CoordinateHeights = new int[]{ 16, 16, 18 };
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;

            // Tile Style
            TileObjectData.newTile.StyleWrapLimit = 2;
            TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.StyleHorizontal = true;

            // Cursor Position
            TileObjectData.newTile.Origin = new Point16(1, 2);

            // Tile Anchors
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        
            // Other direction
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.addAlternate(1);

            // Add TileData
            TileObjectData.addTile(Type);

            TileID.Sets.DisableSmartCursor[Type] = true;
            //animationFrameHeight = 56;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(187, 199, 219), name);
        }

		public override void NumDust(int i, int j, bool fail, ref int num) 
        {
			num = 0;
		}

/*      public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            if (++frameCounter >= 6)
            {
                frameCounter = 0;

                if (++frame >= 8)
                {
                    frame = 0;
                }
            }
        }
*/  }
}