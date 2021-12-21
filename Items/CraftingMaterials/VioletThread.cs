using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.CraftingMaterials
{
    public class VioletThread : ModItem
    {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Violet Thread");
		}

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.BlackThread);

            // Consumable
            item.consumable = true;

            // Usage and Animation
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTime = 15;
            item.useAnimation = 15;
            item.autoReuse = true;
            item.useTurn = true;

            // Tile placement fields
            item.createTile = TileType<Thread_Tile>();
            item.placeStyle = 5;
        }

        public override void AddRecipes()
        {
            
        }
    }
}