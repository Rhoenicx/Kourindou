using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plants;

namespace Kourindou.Items.CraftingMaterials
{
    public class FlaxBundle : ModItem
    {
        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 0, 1, 0);
            Item.rare = ItemRarityID.Blue;

            // Hitbox
            Item.width = 24;
            Item.height = 24;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.autoReuse = true;
            Item.useTurn = true;

            // Item
            Item.maxStack = 999;
        }
    }
}