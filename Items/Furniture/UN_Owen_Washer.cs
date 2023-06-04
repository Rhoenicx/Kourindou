using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.Furniture
{
    public class UN_Owen_Washer : ModItem
    {
        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Expert;

            // Hitbox
            Item.width = 40;
            Item.height = 44;

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
            Item.createTile = TileType<UN_Owen_Washer_Tile>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.Wire, 12)
                .AddIngredient(ItemID.Glass, 4)
                .AddRecipeGroup("IronBar", 16)
                .AddRecipeGroup("Kourindou:CopperBar", 4)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}