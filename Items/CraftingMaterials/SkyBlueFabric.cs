using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;
using Terraria.Localization;

namespace Kourindou.Items.CraftingMaterials
{
    public class SkyBlueFabric : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Silk);
            Item.width = 32;
            Item.height = 26;
            Item.SetNameOverride(Language.GetTextValue("Mods.Kourindou.Items." + Name + ".DisplayName"));
        }

        public override void AddRecipes()
        {
            // Add recipe
            CreateRecipe(1)
                .AddIngredient(ItemType<SkyBlueThread>(), 4)
                .AddTile(TileType<WeavingLoom_Tile>())
                .Register();

            CreateRecipe(2)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemID.SkyBlueDye)
                .AddTile(TileID.DyeVat)
                .Register();            
        }
    }
}