using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.CraftingMaterials
{
    public class BlackThread : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            // Search for existing Black Thread item
            if (item.type == ItemID.BlackThread)
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
                item.placeStyle = 2;
            }
        }

        public override void AddRecipes()
        {
            // Add new recipe
            Recipe recipe = Mod.CreateRecipe(ItemID.BlackThread, 8);
            recipe.AddRecipeGroup("Kourindou:Thread", 8);
            recipe.AddIngredient(ItemID.BlackDye);
            recipe.AddTile(TileID.DyeVat);
            recipe.Register();
        }
    }
}