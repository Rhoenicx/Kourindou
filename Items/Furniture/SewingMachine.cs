using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.Furniture
{
    public class SewingMachine : ModItem
    {
        public override void SetStaticDefaults() 
        {
			DisplayName.SetDefault("Sewing Machine");
		}

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 1, 0, 0);
            item.rare = ItemRarityID.White;

            // Hitbox
            item.width = 32;
            item.height = 32;

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
            item.createTile = TileType<SewingMachine_Tile>();
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            // Recipe 1 - Iron
            recipe.AddRecipeGroup("IronBar", 8);
            recipe.AddRecipeGroup("Wood", 12);
            recipe.AddTile(16); //Anvil
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}