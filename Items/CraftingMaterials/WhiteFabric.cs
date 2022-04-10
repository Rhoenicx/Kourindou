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
            CreateRecipe()
                .AddIngredient(ItemType<WhiteThread>(), 4)
                .AddTile(TileType<WeavingLoom_Tile>())
                .SetResult(ItemID.Silk)
                .Register();

            // Remove colors on water
            CreateRecipe()
                .AddRecipeGroup("Kourindou:Fabric", 1)
                .needWater = true
                .SetResult(ItemID.Silk)
                .Register();

            // Remove colors on dye vat
            CreateRecipe()
                .AddRecipeGroup("Kourindou:Fabric", 1)
                .AddTile(TileID.DyeVat)
                .SetResult(ItemID.Silk)
                .Register();

        }
    }
}