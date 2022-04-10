using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.CraftingMaterials
{
    public class SkyBlueFabric : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Silk);
            Item.width = 32;
            Item.height = 26;
            Item.SetNameOverride("Sky Blue Fabric");
        }

        public override void AddRecipes()
        {
            // Add recipe
            CreateRecipe()
                .AddIngredient(ItemType<SkyBlueThread>(), 4)
                .AddTile(TileType<WeavingLoom_Tile>())
                .SetResult(this)
                .Register();

            // Recolor any fabric to this color 
            CreateRecipe()
                .AddRecipeGroup("Kourindou:Fabric", 2)
                .AddIngredient(ItemID.SkyBlueDye)
                .AddTile(TileID.DyeVat)
                .SetResult(this, 2)
                .Register();            
        }
    }
}