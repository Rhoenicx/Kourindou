using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;

namespace Kourindou.Items.Plushies
{
    public class PatchouliKnowledge_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Patchouli Knowledge Plushie");
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
            item.createTile = TileType<PatchouliKnowledge_Plushie_Tile>();
            
            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<PatchouliKnowledge_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase Magic damage by 40 percent
            player.magicDamage += 0.40f;

            // All other damage types deal zero, really into negatives because other mods might increase this
            player.rangedDamage = -1000f;
            player.minionDamage = -1000f;
            player.meleeDamage = -1000f;
            player.thrownDamage = -1000f;

            // Increase magic crit rate by 30 percent
            player.magicCrit += 30;

            // Reduce mana consumption by 50 percent
            player.manaCost -= 0.50f;

            // Reduce movement speed to 18 mph
            player.accRunSpeed = 4f;
            player.moveSpeed -= 0.5f;
        }
    }
}