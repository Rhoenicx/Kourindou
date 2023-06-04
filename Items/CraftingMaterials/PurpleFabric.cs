using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;
using Terraria.Localization;

namespace Kourindou.Items.CraftingMaterials
{
    public class PurpleFabric : ModItem
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
                .AddIngredient(ItemType<PurpleThread>(), 4)
                .AddTile(TileType<WeavingLoom_Tile>())
                .Register();

            // Recolor any fabric to this color 
            CreateRecipe(2)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemID.PurpleDye)
                .AddTile(TileID.DyeVat)
                .Register();            
        }
    }
}