using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plants;

namespace Kourindou.Items.Seeds
{
    public class FlaxSeeds : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Flax Seeds");
            // Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 0, 1, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 22;
            Item.height = 18;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.autoReuse = true;
            Item.useTurn = true;

            // item
            Item.maxStack = 999;

            // Tile placement fields
            Item.consumable = true;
            Item.createTile = TileType<Flax_Tile>();   
        }
    }
}