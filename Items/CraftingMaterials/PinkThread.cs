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
                item.useStyle = ItemUseStyleID.SwingThrow;
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
            RecipeFinder finder = new RecipeFinder();
            finder.SetResult(ItemID.PinkThread);

            foreach (Recipe recipe in finder.SearchRecipes())
            {
                RecipeEditor editor = new RecipeEditor(recipe);
                editor.DeleteRecipe();
            }
        }
    }
}