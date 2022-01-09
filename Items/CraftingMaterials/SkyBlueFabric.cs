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
            item.CloneDefaults(ItemID.Silk);
            item.width = 32;
            item.height = 26;
            item.SetNameOverride("Sky Blue Fabric");
        }

        public override void AddRecipes()
        {
            // Add recipe
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<SkyBlueThread>(), 4);
            recipe.AddTile(TileType<WeavingLoom_Tile>());
            recipe.SetResult(this);
            recipe.AddRecipe();

            // Recolor any fabric to this color 
            recipe = new ModRecipe(mod);
            recipe.AddRecipeGroup("Kourindou:Fabric", 2);
            recipe.AddIngredient(ItemID.SkyBlueDye);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 2);
            recipe.AddRecipe();            
        }
    }
}