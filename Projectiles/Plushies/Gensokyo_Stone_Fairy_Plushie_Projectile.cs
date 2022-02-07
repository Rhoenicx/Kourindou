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
    public class Gensokyo_Stone_Fairy_Plushie_Projectile : PlushieProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Stone Fairy Plushie");
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
        }
		
		public override void Kill (int timeLeft)
		{
			plushieTile = TileType<Gensokyo_Stone_Fairy_Plushie_Tile>();

			if (!CanPlacePlushie())
			{
				Item.NewItem(
					projectile.getRect(),
					ItemType<Gensokyo_Stone_Fairy_Plushie_Item>(),
					1
				);
			}
		}
    }
}