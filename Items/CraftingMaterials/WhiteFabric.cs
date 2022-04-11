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
            foreach (Recipe recipe in Main.recipe)
            {
                if (recipe.HasResult(ItemID.Silk))
                {
                    recipe.RemoveRecipe();
                }
            }

            // Add recipe
            Main.recipe[ItemID.Silk]
                .AddIngredient(ItemType<WhiteThread>(), 4)
                .AddTile(TileType<WeavingLoom_Tile>())
                .Register();

            // Remove colors on water
            Main.recipe[ItemID.Silk]
                .AddRecipeGroup("Kourindou:Fabric", 1)
                .AddCondition(Recipe.Condition.NearWater)
                .Register();

            // Remove colors on dye vat
            Main.recipe[ItemID.Silk]
                .AddRecipeGroup("Kourindou:Fabric", 1)
                .AddTile(TileID.DyeVat)
                .Register();
        }
    }
}