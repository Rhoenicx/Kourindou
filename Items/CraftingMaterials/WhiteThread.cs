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
            item.CloneDefaults(ItemID.BlackThread);

            // Consumable
            item.consumable = true;

            // Usage and Animation
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTime = 15;
            item.useAnimation = 15;
            item.autoReuse = true;
            item.useTurn = true;

            // Tile placement fields
            item.createTile = TileType<Thread_Tile>();
            item.placeStyle = 0;
        }

        public override void AddRecipes()
        {
            // Cobweb
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Cobweb, 4);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();

            // Cotton
            recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<CottonFibre>(), 2);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();

            // Flax
            recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<FlaxBundle>(), 2);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();

            // Remove colors on water
            recipe = new ModRecipe(mod);
            recipe.AddRecipeGroup("Kourindou:Thread", 1);
            recipe.needWater = true;
            recipe.SetResult(this);
            recipe.AddRecipe();

            // Remove colors on dye vat
            recipe = new ModRecipe(mod);
            recipe.AddRecipeGroup("Kourindou:Thread", 1);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}