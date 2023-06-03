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
    public class InuSakuyaIzayoi_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Inu Sakuya Izayoi Plushie");
            // Tooltip.SetDefault("Does the real Sakuya like this? Probably. It's adorable, why wouldn't she?");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 9, 7, 5);
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
            Item.createTile = TileType<InuSakuyaIzayoi_Plushie_Tile>();

            Item.shootSpeed = 8f;

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<InuSakuyaIzayoi_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddRecipeGroup("Kourindou:Watch", 1)
                .AddIngredient(ItemType<BrownFabric>(), 2)
                .AddIngredient(ItemType<SilverFabric>(), 2)
                .AddIngredient(ItemType<TealFabric>(), 1)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemType<SilverThread>(), 2)
                .AddIngredient(ItemType<TealThread>(), 2)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {

        }
    }
}