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
            RecipeFinder finder = new RecipeFinder();
            finder.SetResult(ItemID.GreenThread);

            foreach (Recipe recipe in finder.SearchRecipes())
            {
                RecipeEditor editor = new RecipeEditor(recipe);
                editor.DeleteRecipe();
            }
        }
    }
}