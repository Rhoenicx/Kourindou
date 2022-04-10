using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Items.Plushies;
using Kourindou.Tiles.Plushies;

namespace Kourindou.Projectiles.Plushies
{
    public class ChimataTenkyuu_Plushie_Projectile : PlushieProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Chimata Tenkyuu Plushie");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0; 
        }

        public override void SetDefaults()
        {
            // AI
			Projectile.aiStyle = -1;

			// Entity Interaction
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = 1;

			// Hitbox
			Projectile.width = 32;
			Projectile.height = 32;

			// Movement
			Projectile.timeLeft = 6000;
			Projectile.tileCollide = true;
			
			// Visual
			Projectile.scale = 1f;

			// Tile & item type
			plushieTile = TileType<ChimataTenkyuu_Plushie_Tile>();
			plushieItem = ItemType<ChimataTenkyuu_Plushie_Item>();
        }
    }
}