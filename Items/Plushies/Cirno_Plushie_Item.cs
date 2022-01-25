using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;

namespace Kourindou.Items.Plushies
{
    public class Cirno_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cirno Plushie");
            Tooltip.SetDefault("The ice fairy. It's stupidly strong, and stupid as well.");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 9, 9, 9);
            item.rare = ItemRarityID.Cyan;

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
            item.createTile = TileType<Cirno_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }
        
        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<Cirno_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase damage by 25 percent OR on the 9th attack 9% chance to deal time 9 dmg
            if (player.GetModPlayer<KourindouPlayer>().CirnoPlushie_TimesNine)
            {
                player.allDamage *= 9f;
            }
            else
            {
                player.allDamage += 0.25f;
            }

            // Increase Life regen by +1 
            player.lifeRegen += 1;

            //Decrease damage taken by 17%
            player.endurance += 0.17f;
        }
    }
}