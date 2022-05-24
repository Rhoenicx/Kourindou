using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Kourindou.Items.Weapons
{
    public class KoninginDerNacht : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Koningin Der Nacht");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(0, 40, 0, 0);
            Item.rare = ItemRarityID.Red;

            Item.width = 112;
            Item.height = 112;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.autoReuse = true;
            Item.noUseGraphic = true;

            Item.damage = 175;
            Item.crit = 10;
            Item.knockBack = 5f;
            Item.DamageType = DamageClass.Melee;

            Item.UseSound = SoundID.Item7;
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            base.UseItemHitbox(player, ref hitbox, ref noHitbox);

            int HitboxWidth = 112;
            int HitboxHeight = 112;

            hitbox = new Rectangle(
                hitbox.X + hitbox.Width / 2 - HitboxWidth / 2, 
                hitbox.Y + hitbox.Height / 2 - HitboxHeight / 2,
                HitboxWidth,
                HitboxHeight
            );
        }
    }
}
