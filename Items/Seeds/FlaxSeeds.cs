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
            DisplayName.SetDefault("Flax Seeds");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 0, 1, 0);
            item.rare = ItemRarityID.White;

            // Hitbox
            item.width = 22;
            item.height = 18;

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
            item.createTile = TileType<Flax_Tile>();   
        }
    }
}