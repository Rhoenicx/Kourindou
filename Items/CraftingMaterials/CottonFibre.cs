using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Items.CraftingMaterials
{
    public class CottonFibre : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cotton Fibre");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 0, 2, 50);
            item.rare = ItemRarityID.Blue;

            // Hitbox
            item.width = 24;
            item.height = 22;

            // Usage and Animation
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTime = 15;
            item.useAnimation = 15;
            item.autoReuse = true;
            item.useTurn = true;

            // item
            item.maxStack = 999;   
        }
    }
}