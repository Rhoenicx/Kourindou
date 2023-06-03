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
    public class NueHoujuu_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Nue Houjuu Plushie");
            // Tooltip.SetDefault("An alien?! Perhaps this doll isn't her true form...");

            ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 21, 21, 21);
            Item.rare = ItemRarityID.Gray;

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
            Item.createTile = TileType<NueHoujuu_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<NueHoujuu_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 3)
                .AddIngredient(ItemType<BlueFabric>(), 1)
                .AddIngredient(ItemType<RedFabric>(), 1)
                .AddIngredient(ItemID.Silk, 1)
                .AddIngredient(ItemID.BlackThread, 2)
                .AddIngredient(ItemType<BlueThread>(), 1)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 1)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {

        }
    }
}