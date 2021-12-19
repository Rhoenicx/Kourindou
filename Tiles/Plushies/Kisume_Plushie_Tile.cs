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
    public class Kisume_Plushie_Tile : PlushieTile
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

            // Tile Style
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            // Tile Height
            TileObjectData.newTile.Height = 2;
            // Tile Width
            TileObjectData.newTile.Width = 2;
            // Tile Size
            TileObjectData.newTile.CoordinateHeights = new int[]{ 16, 16 };
            // Tile origin on mouse pointer
            TileObjectData.newTile.Origin = new Point16(0, 1);
            // Tile Anchors
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile | AnchorType.Table, TileObjectData.newTile.Width, 0);

            // Alternate version that can hang from solid blocks
            TileObjectData.newAlternate.CopyFrom(TileObjectData.Style1x2Top);
            // Tile Height
            TileObjectData.newAlternate.Height = 2;
            // Tile Width
            TileObjectData.newAlternate.Width = 2;
            // Tile Size
            TileObjectData.newAlternate.CoordinateHeights = new int[]{ 16, 16 };
            // Tile origin on mouse pointer
            TileObjectData.newAlternate.Origin = new Point16(0, 1);
            // Tile Anchors
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidBottom | AnchorType.SolidSide | AnchorType.SolidTile, TileObjectData.newAlternate.Width, 0);
            // Add Alternate Tile
            TileObjectData.addAlternate(0);

            // Add tile
            TileObjectData.addTile(Type);

            // Interaction
            disableSmartCursor = true;

            // Map Entry
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Kisume Plushie");
            AddMapEntry(new Color(0, 107, 64), name);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(i * 16, j * 16, 16, 48, ItemType<Kisume_Plushie_Item>());
        }
    }
}