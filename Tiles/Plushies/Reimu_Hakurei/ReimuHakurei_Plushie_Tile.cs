using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Enums;
using Terraria.DataStructures;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Kourindou.Items.Plushies.Reimu_Hakurei;

namespace Kourindou.Tiles.Plushies.Reimu_Hakurei
{
    public class ReimuHakurei_Plushie_Tile : PlushieTile
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

            // TileObjectData
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new int[]{ 16, 16, 16 };
            TileObjectData.newTile.Origin = new Point16(0, 2);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);

            // Interaction
            disableSmartCursor = true;

            // Map Entry
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Reimu Hakurei Fumo");
            AddMapEntry(new Color(155, 0, 0), name);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(i * 16, j * 16, 16, 48, ItemType<ReimuHakurei_Plushie_Item>());
        }
    }
}