using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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

            foreach (Recipe recipe in finder.SearchRecipes())
            {
                RecipeEditor editor = new RecipeEditor(recipe);
                editor.DeleteRecipe();
            }
        }
    }
}