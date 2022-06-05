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
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 32;
            Item.height = 32;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.autoReuse = true;
            Item.useTurn = true;

            // Item
            Item.maxStack = 99;

            // Tile placement fields
            Item.consumable = true;
            Item.createTile = TileType<SewingMachine_Tile>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddRecipeGroup("IronBar", 8)
                .AddRecipeGroup("Wood", 12)
                .AddTile(16) //Anvil
                .Register();
        }
    }
}