using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;

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
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.Orange;

            // Hitbox
            item.width = 32;
            item.height = 32;

            // Usage and Animation
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTime = 15;
            item.useAnimation = 15;
            item.autoReuse = true;
            item.useTurn = true;

            // Tile placement fields
            item.consumable = true;
            item.createTile = TileType<SuikaIbuki_Plushie_Tile>();

            item.shootSpeed = 8f;

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
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
            player.allDamage += 0.25f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Increase player fall speed
            player.maxFallSpeed += 100;
            player.gravity += 0.5f;

            // Held items are twice as big
            Main.NewText(player.HeldItem);
            if (player.HeldItem.stack > 0 && player.HeldItem.melee && (player.HeldItem.useStyle == ItemUseStyleID.SwingThrow || player.HeldItem.useStyle == ItemUseStyleID.Stabbing))
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
    }
}