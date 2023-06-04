using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.CraftingMaterials
{
    public class WhiteThread : ModItem
    {
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
            Item.placeStyle = 0;
        }

        public override void AddRecipes()
        {
            // Cobweb
            CreateRecipe(1)
                .AddIngredient(ItemID.Cobweb, 4)
                .AddTile(TileID.Loom)
                .Register();

            // Cotton
            CreateRecipe(1)
                .AddIngredient(ItemType<CottonFibre>(), 2)
                .AddTile(TileID.Loom)
                .Register();

            // Flax
            CreateRecipe(1)
                .AddIngredient(ItemType<FlaxBundle>(), 2)
                .AddTile(TileID.Loom)
                .Register();

            foreach (int i in Kourindou.ThreadItems)
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