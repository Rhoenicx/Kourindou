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
    public class MinamitsuMurasa_Plushie_Tile : PlushieTile
    {
        public override void SetStaticDefaults()
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
            // Tile Size
            TileObjectData.newTile.CoordinateHeights = new int[]{ 16, 16 };
            // Tile origin on mouse pointer
            TileObjectData.newTile.Origin = new Point16(0, 1);
            // Tile Anchors
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile | AnchorType.Table, TileObjectData.newTile.Width, 0);
            // Add tile
            TileObjectData.addTile(Type);

            // Interaction
            TileID.Sets.DisableSmartCursor[Type];

            // Map Entry
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Minamitsu Murasa Plushie");
            AddMapEntry(new Color(4, 69, 56), name);

            plushieItem = ItemType<MinamitsuMurasa_Plushie_Item>();
        }
    }
}