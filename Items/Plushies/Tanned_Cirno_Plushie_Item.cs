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
    public class Tanned_Cirno_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tanned Cirno Plushie");
            Tooltip.SetDefault("The strongest [with a tan]!");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 9, 9, 9);
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
            Item.createTile = TileType<Tanned_Cirno_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }
        
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<Tanned_Cirno_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieUpdateEquips(Player player)
        {

        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
			    .AddIngredient(ItemID.Sunflower, 1)
                .AddIngredient(ItemType<BlueFabric>(), 2)
                .AddIngredient(ItemType<SkyBlueFabric>(), 2)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemType<BlueThread>(), 2)
                .AddIngredient(ItemType<SkyBlueThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddIngredient(ItemID.IceBlock, 9)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }
    }
}