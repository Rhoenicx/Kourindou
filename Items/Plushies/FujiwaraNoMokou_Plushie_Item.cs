using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;

namespace Kourindou.Items.Plushies
{
    public class FujiwaraNoMokou_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fujiwara No Mokou Plushie");
            Tooltip.SetDefault("The bamboo forest's guide. Rumor has it that she runs a yakitori stand.");
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
            item.createTile = TileType<FujiwaraNoMokou_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }
        
        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<FujiwaraNoMokou_Plushie_Projectile>();
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

            // Decrease incoming damage by 15 percent
            player.endurance += 0.15f;

            // Increase melee attack speed by 25 percent
            player.meleeSpeed += 0.25f;

            // Increase melee critrate by 10 percent
            player.meleeCrit += 10;

            // When you get damage that should kill you, heal for maxhp and get mortality debuff
        }
    }
}