using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.CraftingMaterials
{
    public class CyanThread : ModItem
    {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cyan Thread");
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
            Item.placeStyle = 9;
        }

        public override void AddRecipes()
        {
            CreateRecipe(8)
                .AddIngredient(ItemType<WhiteThread>(), 8)
                .AddIngredient(ItemID.CyanDye)
                .AddTile(TileID.DyeVat)
                .Register();
        }
    }
}