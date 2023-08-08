using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;
using Terraria.Localization;

namespace Kourindou.Items.CraftingMaterials
{
    public class CyanFabric : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Silk);
            Item.width = 32;
            Item.height = 26;
        }

        public override void AddRecipes()
        {
            // Add recipe
            CreateRecipe(1)
                .AddIngredient(ItemType<CyanThread>(), 4)
                .AddTile(TileType<WeavingLoom_Tile>())
                .Register();

            // Recolor any fabric to this color 
            CreateRecipe(2)
                .AddIngredient(ItemType<WhiteFabric>(), 2)
                .AddIngredient(ItemID.CyanDye)
                .AddTile(TileID.DyeVat)
                .Register();  
        }
    }
}