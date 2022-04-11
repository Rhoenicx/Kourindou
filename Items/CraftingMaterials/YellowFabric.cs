using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.CraftingMaterials
{
    public class YellowFabric : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Silk);
            Item.width = 32;
            Item.height = 26;
            Item.SetNameOverride("Yellow Fabric");
        }

        public override void AddRecipes()
        {
            // Add recipe
            CreateRecipe(1)
                .AddIngredient(ItemType<YellowThread>(), 4)
                .AddTile(TileType<WeavingLoom_Tile>())
                .Register();

            // Recolor any fabric to this color 
            CreateRecipe(2)
                .AddRecipeGroup("Kourindou:Fabric", 2)
                .AddIngredient(ItemID.YellowDye)
                .AddTile(TileID.DyeVat)
                .Register();
        }
    }
}