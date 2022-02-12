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
    public class KaguyaHouraisan_Plushie_Projectile : PlushieProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Kaguya Houraisan Plushie");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0; 
        }

        public override void SetDefaults()
        {
            // AI
			projectile.aiStyle = -1;

			// Entity Interaction
			projectile.friendly = false;
			projectile.hostile = false;
			projectile.melee = true;
			projectile.penetrate = 1;

			// Hitbox
			projectile.width = 32;
			projectile.height = 32;

			// Movement
			projectile.timeLeft = 6000;
			projectile.tileCollide = true;
			
			// Visual
			projectile.scale = 1f;

			// Tile & item type
			plushieTile = TileType<KaguyaHouraisan_Plushie_Tile>();
			plushieItem = ItemType<KaguyaHouraisan_Plushie_Item>();
        }
    }
}