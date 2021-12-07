using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Projectiles
{
    public abstract class PlushieProjectile : ModProjectile
    {
		Vector2 gravity = new Vector2(0f, 10f);
		float magnitudeX = 0.0025f;
		float magnitudeY = 0.01f;

		int dropTimer = 0;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = Main.projectileTexture[projectile.type];
		    
			spriteBatch.Draw(
				texture, 
				projectile.Center - Main.screenPosition, 
				texture.Bounds, 
				lightColor, 
				projectile.rotation, 
				texture.Size() * 0.5f, 
				projectile.scale, 
				SpriteEffects.None, 
				0);
			return false;
		}

        public override bool OnTileCollide (Vector2 oldVelocity)
        {
			if (projectile.velocity.X != oldVelocity.X)
            {
                projectile.velocity.X = -oldVelocity.X * 0.5f;
            }

            if (projectile.velocity.Y != oldVelocity.Y)
            {
                projectile.velocity.Y = -oldVelocity.Y * 0.5f;
            }

            return false;
        }

		public override void AI()
		{
			projectile.velocity.X = MathHelper.Lerp(projectile.velocity.X, gravity.X, magnitudeX);
			projectile.velocity.Y = MathHelper.Lerp(projectile.velocity.Y, gravity.Y, magnitudeY);

			projectile.rotation += MathHelper.ToRadians(projectile.velocity.X) * 2f;

			if (Vector2.Distance(new Vector2(0f, projectile.position.Y), new Vector2(0f, projectile.oldPos[4].Y)) < 0.25f)
			{
				magnitudeX += 0.001f;
			}

			if (projectile.velocity.Length() < 0.4f)
			{
				dropTimer++;
			}
			else
			{
				dropTimer = 0;
			}

			if (dropTimer > 15)
			{
				projectile.Kill();
			}
		}
    }
}