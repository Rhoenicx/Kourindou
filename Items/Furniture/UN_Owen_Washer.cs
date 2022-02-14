using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.Furniture
{
    public class UN_Owen_Washer : ModItem
    {
        public override void SetStaticDefaults() 
        {
			DisplayName.SetDefault("U.N. Owen Washer");
		}

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 1, 0, 0);
            item.rare = ItemRarityID.Expert;

            // Hitbox
            item.width = 40;
            item.height = 44;

            // Usage and Animation
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTime = 15;
            item.useAnimation = 15;
            item.autoReuse = true;
            item.useTurn = true;

            // item
            item.maxStack = 99;

            // Tile placement fields
            item.consumable = true;
            item.createTile = TileType<UN_Owen_Washer_Tile>();
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Wire, 12);
            recipe.AddIngredient(ItemID.Glass, 4);
            recipe.AddRecipeGroup("IronBar", 16);
            recipe.AddRecipeGroup("Kourindou:CopperBar", 4);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}