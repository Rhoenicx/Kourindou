using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Projectiles.Plushies.PlushieEffects;

namespace Kourindou.Items.Plushies
{
    public class Kourindou_SakuyaIzayoi_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sakuya Izayoi Plushie Kourindou ver.");
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
            item.createTile = TileType<Kourindou_SakuyaIzayoi_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<Kourindou_SakuyaIzayoi_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            if (player.whoAmI == Main.myPlayer && player.itemAnimationMax -1 == player.itemAnimation)
            {
                // Increase damage by 5 percent
                player.allDamage += 0.05f;

                // Increase life regen by 1 point
                player.lifeRegen += 1;

                // Increase Throwing damage by 40 percent
                player.thrownDamage += 0.40f;

                // Spawn 4 knifes on regular attack animations
			    for (int i = 0; i < 4; i++)
			    {
			    	Projectile.NewProjectile(
			    		player.Center,
			    		Vector2.Normalize(Main.MouseWorld - player.Center).RotatedBy(MathHelper.ToRadians(i >= 2 ? 5 * (i - 1) : -5 * (i + 1))) * (player.HeldItem.shootSpeed > 0f ? player.HeldItem.shootSpeed : 8f),
			    		ProjectileType<SakuyaIzayoi_Plushie_Knife>(),
			    		10 + (int)(player.statLifeMax2 / 15),
			    		1f,
			    		Main.myPlayer
			    	);
			    }
            }
        }
    }
}