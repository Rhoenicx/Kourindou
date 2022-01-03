using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.CraftingMaterials
{
    public class GreenThread : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            // Search for existing Black Thread item
            if (item.type == ItemID.GreenThread)
            {
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
                item.placeStyle = 11;
            }
        }

        public override void AddRecipes()
        {
            // Remove Crafting recipes
            RecipeFinder finder = new RecipeFinder();
            finder.SetResult(ItemID.GreenThread);

            foreach (Recipe _recipe in finder.SearchRecipes())
            {
                RecipeEditor editor = new RecipeEditor(_recipe);
                editor.DeleteRecipe();
            }

            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddRecipeGroup("Kourindou:Thread", 8);
            recipe.AddIngredient(ItemID.GreenDye);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(ItemID.GreenThread, 8);
            recipe.AddRecipe();
        }
    }
}