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
    public class HatsuneMiku_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Hatsune Miku Plushie");
            /* Tooltip.SetDefault("The voice of the future, in pixel plushie form!\n"
									+ "Fires leeks from the heavens if powered"); */
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(3, 9, 3, 9);
            Item.rare = ItemRarityID.Cyan;

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
            Item.createTile = TileType<HatsuneMiku_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;

        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<HatsuneMiku_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }
        
        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 2)
                .AddIngredient(ItemType<TealFabric>(), 3)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemID.BlackThread, 2)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<TealThread>(), 2)
                .AddIngredient(ItemType<WhiteThread>(), 1)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase damage by 39 percent [placeholder]
            player.GetDamage(DamageClass.Generic) += 0.39f;

            // Increase damage reduction by 39 percent [placeholder]
            player.endurance += 0.39f;
        }
    }
}