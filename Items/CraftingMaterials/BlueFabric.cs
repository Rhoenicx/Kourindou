using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.CraftingMaterials
{
    public class BlueFabric : ModItem
    {
        public override void SetStaticDefaults() 
        {
			DisplayName.SetDefault("Blue Fabric");
		}

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Silk);
            Item.width = 32;
            Item.height = 26;
            Item.SetNameOverride("Blue Fabric");
        }

        public override void AddRecipes()
        {
            // Add recipe
            CreateRecipe(1)
                .AddIngredient(ItemType<BlueThread>(), 4)
                .AddTile(TileType<WeavingLoom_Tile>())
                .Register();

            // Recolor any fabric to this color 
            CreateRecipe(2)
                .AddRecipeGroup("Kourindou:Fabric", 2)
                .AddIngredient(ItemID.BlueDye)
                .AddTile(TileID.DyeVat)
                .Register();  
        }
    }
}