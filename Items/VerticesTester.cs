using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Projectiles;

namespace Kourindou.Items
{
    public class VerticesTester : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vertices Tester");
            Tooltip.SetDefault("test");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Red;

            // Hitbox
            Item.width = 32;
            Item.height = 32;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.autoReuse = true;
            Item.useTurn = true;

            Item.shoot = ProjectileType<VerticesProjectile>();
            Item.shootSpeed = 5f;
        }
    }
}
