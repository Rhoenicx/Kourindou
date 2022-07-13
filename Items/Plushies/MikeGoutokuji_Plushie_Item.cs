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
    public class MikeGoutokuji_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mike Goutokuji Plushie");
            Tooltip.SetDefault("MICHAEL, This is michael");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(1, 0, 0, 0);
            Item.rare = ItemRarityID.Orange;

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
            Item.createTile = TileType<MikeGoutokuji_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<MikeGoutokuji_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase damage by 5 percent
            player.GetDamage(DamageClass.Generic) += 0.05f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // LuckyCoin effect
            player.hasLuckyCoin = true;

            // Gold ring effect
            player.goldRing = true;

            // Tremendously increase luck, since luck is a hidden stat don't display this on the effect xD
            player.luckMaximumCap += 5.00f;
            player.luck += 5.00f;
        }

        public override string AddEffectTooltip()
        {
            return "Increases pickup range of coins and hitting enemies sometimes drop coins!";
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.PlatinumCoin, 1)
                .AddIngredient(ItemType<OrangeFabric>(), 2)
                .AddIngredient(ItemType<YellowFabric>(), 1)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemType<OrangeThread>(), 2)
                .AddIngredient(ItemType<YellowThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 1)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }
    }
}