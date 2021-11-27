using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Kourindou.Items.Plushies.Reimu_Hakurei
{
    public class ReimuHakurei_Plushie_Item : Plushie
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Reimu Hakurei Fumo");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 50, 0, 0);
            item.rare = ItemRarityID.Red;

            //Hitbox
            item.width = 32;
            item.height = 34;

            // Usage and Animation
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTime = 20;
            item.useAnimation = 20;
            item.autoReuse = true;
            item.useTurn = true;

            // Tile placement fields
            item.consumable = true;
            //item.createTile = TileType<ReimuHakureiPlushieTile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override void PlushieEquipEffects(Player player)
        {

        }
    }
}