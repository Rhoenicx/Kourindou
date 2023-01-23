using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.Plushies
{
    public class Gensokyo_Sunflower_Fairy_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sunflower Fairy Plushie");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Pink;

            // Hitbox
            Item.width = 32;
            Item.height = 32;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.autoReuse = true;
            Item.useTurn = true;

            // Tile placement fields
            Item.consumable = true;
            Item.createTile = TileType<Gensokyo_Sunflower_Fairy_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<Gensokyo_Sunflower_Fairy_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieUpdateEquips(Player player)
        {
            if (Kourindou.GensokyoLoaded)
            {
                player.npcTypeNoAggro[Kourindou.Gensokyo_Fairy_Sunflower_Type] = true;
            }
        }
        public override string AddEffectTooltip()
        {
            return "Sunflower fairies become friendly";
        }

        public override void AddRecipes()
        {
            if (Kourindou.GensokyoLoaded)
            {
                CreateRecipe(1)
                    .AddIngredient(ItemID.Sunflower, 1)
                    .AddIngredient(ItemType<BlueFabric>(), 2)
                    .AddIngredient(ItemType<PinkFabric>(), 2)
                    .AddIngredient(ItemID.Silk, 2)
                    .AddIngredient(ItemType<BlueThread>(), 2)
                    .AddIngredient(ItemID.PinkThread, 2)
                    .AddIngredient(ItemType<WhiteThread>(), 2)
                    .AddRecipeGroup("Kourindou:Stuffing", 5)
                    .AddTile(TileType<SewingMachine_Tile>())
                    .Register();
            }   
        }
    }
}