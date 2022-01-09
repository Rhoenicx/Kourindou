using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.Furniture
{
    public class WeavingLoom : ModItem
    {
        public override void SetStaticDefaults() 
        {
			DisplayName.SetDefault("Weaving Loom");
		}

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 1, 0, 0);
            item.rare = ItemRarityID.White;

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
            item.createTile = TileType<WeavingLoom_Tile>();
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddRecipeGroup("Wood", 16);
            recipe.AddTile(TileID.Sawmill);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}