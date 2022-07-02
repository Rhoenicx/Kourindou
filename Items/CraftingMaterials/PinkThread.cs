using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.CraftingMaterials
{
    public class PinkThread : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            // Search for existing Black Thread item
            if (item.type == ItemID.PinkThread)
            {
                // Consumable
                item.consumable = true;

                // Usage and Animation
                item.useStyle = ItemUseStyleID.Swing;
                item.useTime = 15;
                item.useAnimation = 15;
                item.autoReuse = true;
                item.useTurn = true;

                // Tile placement fields
                item.createTile = TileType<Thread_Tile>();
                item.placeStyle = 4;
            }
        }

        public override void AddRecipes()
        {
            // Remove existing recipes
            foreach (Recipe recipe in Main.recipe)
            {
                if (recipe.TryGetResult(ItemID.PinkThread, out Item result))
                {
                    Main.recipe[recipe.RecipeIndex].DisableRecipe();
                }
            }

            // Add new recipe
            Recipe newRecipe = Recipe.Create(ItemID.PinkThread, 8);
            newRecipe.AddRecipeGroup("Kourindou:Thread", 8);
            newRecipe.AddIngredient(ItemID.PinkDye);
            newRecipe.AddTile(TileID.DyeVat);
            newRecipe.Register();
        }
    }
}