using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Items.CraftingMaterials
{
    public class CottonFibre : ModItem
    {
        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 0, 2, 50);
            Item.rare = ItemRarityID.Blue;

            // Hitbox
            Item.width = 24;
            Item.height = 22;

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