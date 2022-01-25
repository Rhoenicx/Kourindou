using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;

namespace Kourindou.Items.Plushies
{
    public class YuukaKazami_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Yuuka Kazami Plushie");
            Tooltip.SetDefault("The flower youkai. She's not actually a flower youkai, but she really likes flowers.");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.Green;

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
            item.createTile = TileType<YuukaKazami_Plushie_Tile>();

            item.shootSpeed = 8f;

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<YuukaKazami_Plushie_Projectile>();
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

            // Also some kind of dash
            player.dash = 1;

            // Check if player has Happy! buff
            if (player.HasBuff(BuffID.Sunflower))
            {
                // Increase defense by 7 points
                player.statDefense += 7;

                // Increase max HP by 50 points
                player.statLifeMax2 += 50;

                // Increase armor penetration by 20 points
                player.armorPenetration += 20;

                // TODO flowe boots effect

                // Double movement speed
                if (player.accRunSpeed > player.maxRunSpeed)
                {
                    player.accRunSpeed *= 2f;
                    player.maxRunSpeed *= 2f;
                }
                else
                {
                    player.maxRunSpeed *= 2f;
                    player.accRunSpeed = player.maxRunSpeed;
                }
            }
        }
    }
}