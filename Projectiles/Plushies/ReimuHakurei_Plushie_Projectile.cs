using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Items.Plushies;

namespace Kourindou.Projectiles.Plushies
{
    public class ReimuHakurei_Plushie_Projectile : PlushieProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Reimu Hakurei Plushie");
        }

        public override void SetDefaults()
        {
            // AI
			projectile.aiStyle = 32;

			// Entity Interaction
			projectile.friendly = true;
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
			Item.NewItem(
				projectile.Center,
				new Vector2(0, 0),
				ItemType<ReimuHakurei_Plushie_Item>(),
				1
			);
		}
    }
}