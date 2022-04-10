using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.CraftingMaterials
{
    public class WhiteThread : ModItem
    {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("White Thread");
		}

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.BlackThread);

            // Consumable
            Item.consumable = true;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.autoReuse = true;
            Item.useTurn = true;

            // Tile placement fields
            Item.createTile = TileType<Thread_Tile>();
            Item.placeStyle = 0;
        }

        public override void AddRecipes()
        {
            // Cobweb
            CreateRecipe()
                .AddIngredient(ItemID.Cobweb, 4)
                .AddTile(TileID.Loom)
                .SetResult(this)
                .Register();

            // Cotton
            CreateRecipe()
                .AddIngredient(ItemType<CottonFibre>(), 2)
                .AddTile(TileID.Loom)
                .SetResult(this)
                .Register();

            // Flax
            CreateRecipe()
                .AddIngredient(ItemType<FlaxBundle>(), 2)
                .AddTile(TileID.Loom)
                .SetResult(this)
                .Register();

            // Remove colors on water
            CreateRecipe()
                .AddRecipeGroup("Kourindou:Thread", 1)
                .needWater = true
                .SetResult(this)
                .Register();

            // Remove colors on dye vat
            CreateRecipe()
                .AddRecipeGroup("Kourindou:Thread", 1)
                .AddTile(TileID.DyeVat)
                .SetResult(this)
                .Register();
        }
    }
}