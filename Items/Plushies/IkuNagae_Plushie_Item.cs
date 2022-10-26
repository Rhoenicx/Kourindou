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
    public class IkuNagae_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Iku Nagae Plushie");
            Tooltip.SetDefault("A messenger of the Dragon Palace, who observes the mood of the atmosphere");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 9, 7, 5);
            Item.rare = ItemRarityID.Blue;

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
            Item.createTile = TileType<IkuNagae_Plushie_Tile>();

            Item.shootSpeed = 8f;

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<IkuNagae_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {

        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<RedFabric>(), 1)
                .AddIngredient(ItemType<BlueFabric>(), 1)
                .AddIngredient(ItemType<BlackFabric>(), 2)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemType<BlueThread>(), 1)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemID.BlackThread, 2)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }
    }
}