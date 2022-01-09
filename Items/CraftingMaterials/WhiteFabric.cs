using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.CraftingMaterials
{
    public class WhiteFabric : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (item.type == ItemID.Silk)
            {
                item.width = 32;
                item.height = 26;
                item.SetNameOverride("White Fabric");
            }
        }

        public override void AddRecipes()
        {
            // Remove Crafting recipes
            RecipeFinder finder = new RecipeFinder();
            finder.SetResult(ItemID.Silk);

            foreach (Recipe _recipe in finder.SearchRecipes())
            {
                RecipeEditor editor = new RecipeEditor(_recipe);
                editor.DeleteRecipe();
            }

            // Add recipe
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<WhiteThread>(), 4);
            recipe.AddTile(TileType<WeavingLoom_Tile>());
            recipe.SetResult(ItemID.Silk);
            recipe.AddRecipe();

            // Remove colors on water
            recipe = new ModRecipe(mod);
            recipe.AddRecipeGroup("Kourindou:Fabric", 1);
            recipe.needWater = true;
            recipe.SetResult(ItemID.Silk);
            recipe.AddRecipe();

            // Remove colors on dye vat
            recipe = new ModRecipe(mod);
            recipe.AddRecipeGroup("Kourindou:Fabric", 1);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(ItemID.Silk);
            recipe.AddRecipe();

        }
    }
}