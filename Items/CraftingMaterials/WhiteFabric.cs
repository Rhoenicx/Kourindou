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
            // Remove existing recipes
            foreach (Recipe recipe in Main.recipe)
            {
                if (recipe.TryGetResult(ItemID.Silk, out Item result))
                {
                    Main.recipe[recipe.RecipeIndex].DisableRecipe();
                }
            }

            // Add new recipe
            Recipe newRecipe = Recipe.Create(ItemID.Silk, 1);
            newRecipe.AddIngredient(ItemType<WhiteThread>(), 4);
            newRecipe.AddTile(TileType<WeavingLoom_Tile>());
            newRecipe.Register();

            // Remove colors on water
            Recipe newRecipe2 = Recipe.Create(ItemID.Silk, 1);
            newRecipe2.AddRecipeGroup("Kourindou:Fabric", 1);
            newRecipe2.AddCondition(Recipe.Condition.NearWater);
            newRecipe2.Register();

            // Remove colors on dye vat
            Recipe newRecipe3 = Recipe.Create(ItemID.Silk, 1);
            newRecipe3.AddRecipeGroup("Kourindou:Fabric", 1);
            newRecipe3.AddTile(TileID.DyeVat);
            newRecipe3.Register();
        }
    }
}