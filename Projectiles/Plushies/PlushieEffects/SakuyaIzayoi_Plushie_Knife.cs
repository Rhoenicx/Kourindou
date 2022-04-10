using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Projectiles.Plushies.PlushieEffects
{
    public class SakuyaIzayoi_Plushie_Knife : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Sakuya's Knife");    
		}
		
		public override void SetDefaults()
		{
			// AI
			projectile.aiStyle = -1;

            // Entity Interaction
			projectile.friendly = true;
            projectile.hostile = false;
			projectile.penetrate = (int)Main.rand.Next(1,4);
			projectile.thrown = true;

			// Hitbox
			projectile.width = 8;
			projectile.height = 8;

			// Movement
			projectile.timeLeft = 120;
			projectile.tileCollide = true;
			projectile.ignoreWater = true; 
			
			// Visual
			projectile.scale = 1f;
		}
		
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Main.projectileTexture[projectile.type];

			spriteBatch.Draw(
				texture,
				projectile.Center - Main.screenPosition,
				texture.Bounds,
				lightColor * projectile.Opacity,
				projectile.rotation,
				texture.Size() * 0.5f,
				projectile.scale,
				SpriteEffects.None,
				0);

			return false;
		}

		public override void AI()
		{
			if (projectile.timeLeft < 30)
			{
				projectile.rotation += projectile.velocity.X > 0f ? MathHelper.ToRadians(25) : MathHelper.ToRadians(-25);
				projectile.Opacity -= 1f/30f;
				projectile.velocity = Vector2.Lerp(projectile.velocity, new Vector2(0f, 8f), 0.05f);
			}
			else
			{
				projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + MathHelper.PiOver2;
			}
		}
	}
}