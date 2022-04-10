using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.Plushies
{
    public class SuikaIbuki_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Suika Ibuki Plushie");
            Tooltip.SetDefault("The alcoholic oni. Her appearance makes you question her age, and whether she should be drinking alcohol.");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
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
            Item.createTile = TileType<SuikaIbuki_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<SuikaIbuki_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase damage by 25 percent
            player.GetDamage(DamageClass.Generic) += 0.25f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Increase player fall speed
            player.maxFallSpeed += 100;
            player.gravity += 0.5f;

            // Held Items are twice as big
            if (player.HeldItem.stack > 0 && player.HeldItem.melee && (player.HeldItem.useStyle == ItemUseStyleID.Swing || player.HeldItem.useStyle == ItemUseStyleID.Thrust))
            {   
                player.HeldItem.scale = player.HeldItem.GetGlobalItem<KourindouGlobalItemInstance>().defaultScale * 2f;
            }

            if (player.HasBuff(BuffID.Tipsy))
            {
                // Increase defense by 16 points (also to counter Tipsy)
                player.statDefense += 16;

                // Increase melee crit by 10 points
                player.meleeCrit += 10;

                // Decrease incoming damage by 10 percent
                player.endurance += 0.10f;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Ale, 1)
                .AddIngredient(ItemType<BlueFabric>(), 1)
                .AddIngredient(ItemType<BrownFabric>(), 1)
                .AddIngredient(ItemType<OrangeFabric>(), 2)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemType<BlueThread>(), 1)
                .AddIngredient(ItemType<OrangeThread>(), 2)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .SetResult(this)
                .Register();
        }
    }
}