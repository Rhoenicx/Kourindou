using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;

namespace Kourindou.Items.Plushies
{
    public class YuyukoSaigyouji_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Yuyuko Saigyouji Plushie");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 1, 0, 0);
            item.rare = ItemRarityID.White;

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
            item.createTile = TileType<YuyukoSaigyouji_Plushie_Tile>();

            item.shootSpeed = 8f;

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<YuyukoSaigyouji_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase minion knockback by 2
            player.minionKB += 2;

            // Increase sentry slots by 3
            player.maxTurrets += 3;

            // Increase minion slots by 3
            player.maxMinions += 3;

            if (player.HasBuff(BuffID.WellFed))
            {
                // Increase attack damage by 25 percent
                player.allDamage += 0.25f;

                // Increase life regen by 1 point
                player.lifeRegen += 1;

                // Increase movement speed by 30 percent
                player.moveSpeed += 0.30f;
            }
            else
            {
                // Increase attack damage by 25 percent
                player.allDamage -= 0.25f;

                // Increase life regen by 1 point
                player.lifeRegen -= 1;

                // Increase movement speed by 30 percent
                player.moveSpeed -= 0.30f;
            }
        }
    }
}