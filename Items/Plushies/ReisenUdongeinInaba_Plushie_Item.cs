using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;

namespace Kourindou.Items.Plushies
{
    public class ReisenUdongeinInaba_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Reisen Udongein Inaba Plushie");
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
            item.createTile = TileType<ReisenUdongeinInaba_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<ReisenUdongeinInaba_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase damage by 5 percent
            player.allDamage += 0.05f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Increase bullet damage by 20 percent
            player.bulletDamage += 0.20f;

            // Increase ranged crit by 20 percent
            player.rangedCrit += 20;

            // During blood moon gain additional stats:
            if (Main.bloodMoon)
            {
                // Increase defense by 10 points
                player.statDefense += 10;

                // Increase movement speed by 100%
                player.moveSpeed += 1f;

                // Increase armor penetration by 15 points
                player.armorPenetration += 15;

                // Increase damage by an additional 15 percent
                player.allDamage += 0.15f;

                // Increase life regen by 3 points
                player.lifeRegen += 3;

                // Increase max HP by 25 points
                player.statLifeMax2 += 25;

                // Permanent NightOwl buff
                player.AddBuff(BuffID.NightOwl, 60, true);
            }
        }
    }
}