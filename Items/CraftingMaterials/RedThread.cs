using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.CraftingMaterials
{
    public class RedThread : ModItem
    {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Red Thread");
		}

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.BlackThread);

            // Consumable
            Item.consumable = true;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.autoReuse = true;
            Item.useTurn = true;

            // Tile placement fields
            Item.createTile = TileType<Thread_Tile>();
            Item.placeStyle = 3;
        }

        public override void AddRecipes()
        {
            CreateRecipe(8)
                .AddRecipeGroup("Kourindou:Thread", 8)
                .AddIngredient(ItemID.RedDye)
                .AddTile(TileID.DyeVat)
                .Register();
        }
    }
}