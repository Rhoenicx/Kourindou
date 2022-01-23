using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Enums;
using Terraria.DataStructures;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Kourindou.Items.Plushies;
using Kourindou.Tiles;

namespace Kourindou.Tiles.Plushies
{
    public class SeijaKijin_Plushie_Tile : PlushieTile
    {
        public override void SetDefaults()
        {
            // Make Frame Important => multiple frames
            Main.tileFrameImportant[Type] = true;
            // Make it not able to attach other tiles like torches
            Main.tileNoAttach[Type] = true;
            // Kill tile when hit by lava
            Main.tileLavaDeath[Type] = true;
            // Kill tile when hit by water
            Main.tileWaterDeath[Type] = true;
            // Draw lighting on this tile
            Main.tileLighted[Type] = true;
            // Make tile not solid, can walk in front of it
            Main.tileSolid[Type] = false;
            // Prevent destroying this tile when hit
            Main.tileCut[Type] = false;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.StyleWrapLimit = 2;
            TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
            TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile | AnchorType.Table, TileObjectData.newTile.Width, 0);
        
            // Alternate version that can hang from solid blocks
            TileObjectData.newAlternate.CopyFrom(TileObjectData.Style1x2Top);
            TileObjectData.newAlternate.StyleHorizontal = true;
            TileObjectData.newAlternate.Height = 2;
            TileObjectData.newAlternate.Width = 2;
            TileObjectData.newAlternate.CoordinateHeights = new int[] { 16, 16 };
            TileObjectData.newAlternate.Origin = new Point16(0, 1);
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidBottom | AnchorType.SolidSide | AnchorType.SolidTile, TileObjectData.newAlternate.Width, 0);
            TileObjectData.addAlternate(1);

            // Add tile
            TileObjectData.addTile(Type);

            // Interaction
            disableSmartCursor = true;

            // Map Entry
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Seija Kijin Plushie");
            AddMapEntry(new Color(0, 0, 0), name);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(i * 16, j * 16, 16, 48, ItemType<SeijaKijin_Plushie_Item>());
        }
    }
}