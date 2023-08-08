using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;
using Terraria.Localization;
using System.Collections.Generic;

namespace Kourindou.Items.CraftingMaterials
{
    public class WhiteFabric : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Silk);
            Item.width = 32;
            Item.height = 26;
        }   

        public override void AddRecipes()
        {
            // Craft white fabric with thread
            CreateRecipe(1)
                .AddIngredient(ItemType<WhiteThread>(), 4)
                .AddTile(TileType<WeavingLoom_Tile>())
                .Register();

            // Craft white fabric by converting silk
            CreateRecipe(1)
                .AddIngredient(ItemID.Silk, 1)
                .AddTile(TileID.Loom)
                .Register();

            // Remove colors
            foreach (int i in Kourindou.FabricItems)
            {
                if (i != this.Type)
                {
                    // Remove colors on water
                    CreateRecipe(1)
                        .AddIngredient(i, 1)
                        .AddCondition(Condition.NearWater)
                        .Register();

                    // Remove colors on dye vat
                    CreateRecipe(1)
                        .AddIngredient(i, 1)
                        .AddTile(TileID.DyeVat)
                        .Register();
                }
            }
        }
    }
}