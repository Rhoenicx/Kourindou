using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plants;

namespace Kourindou.Items.CraftingMaterials
{
    public class FlaxBundle : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flax Bundle");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 0, 1, 0);
            item.rare = ItemRarityID.Blue;

            // Hitbox
            item.width = 24;
            item.height = 24;

            // Usage and Animation
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTime = 15;
            item.useAnimation = 15;
            item.autoReuse = true;
            item.useTurn = true;

            // item
            item.maxStack = 999;

            // Tile placement fields
            item.consumable = true;
            //item.createTile = TileType<Flax_Tile>(); 
        }
    }
}