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
		float magnitudeX = 0.005f;
		float magnitudeY = 0.01f;

		public int dropTimer 
		{
			get => (int)projectile.ai[0];
			set => projectile.ai[0] = value;
		}

		public int Timer
		{
			get => (int)projectile.ai[1];
			set => projectile.ai[1] = value;
		} 

        public override bool PreDraw (SpriteBatch spriteBatch, Color lightColor)
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

			if (projectile.velocity.Length() < 0.2f)
			{
				dropTimer++;
			}
			else
			{
				dropTimer = 0;
			}

            return false;
        }

		public override bool? Colliding (Rectangle projHitbox, Rectangle targetHitbox)
		{
			return projHitbox.Intersects(targetHitbox);
		}

		public override void AI ()
		{
			projectile.damage = 0;

			// Change projectile velocity towards gravity vector based on magnitude and LERP
			projectile.velocity.X = MathHelper.Lerp(projectile.velocity.X, gravity.X, magnitudeX);
			projectile.velocity.Y = MathHelper.Lerp(projectile.velocity.Y, gravity.Y, magnitudeY);

			// Projectile rotation based on distance travelled on X axis
			projectile.rotation += MathHelper.ToRadians(projectile.velocity.X) * 2f;

			// Check if the Plushie is NOT changing Y position => laying on the ground so reduce X speed
			if (Vector2.Distance(new Vector2(0f, projectile.position.Y), new Vector2(0f, projectile.oldPos[4].Y)) < 0.25f)
			{
				magnitudeX += 0.0025f;
			}
			else
			{
				magnitudeX = 0.005f;
			}

			// If the projectile is not moving for 15 ticks, kill it
			if (dropTimer > 15)
			{
				projectile.Kill();
			}

			if (Timer > 30)
			{
				// Check collisions for players
				for (int i = 0; i < Main.player.Length; i++)
				{
					if (Colliding(projectile.Hitbox, Main.player[i].Hitbox) == true)
					{
						OnHitPlayer(Main.player[i]);
					}
				}

				// Check collisions for NPC's
				for (int i = 0; i < Main.npc.Length; i++)
				{
					if (Colliding(projectile.Hitbox, Main.npc[i].Hitbox) == true)
					{
						OnHitNPC(Main.npc[i]);
					}
				}
			}

			Timer++;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public void OnHitNPC(NPC target)
		{

		}

		public void OnHitPlayer(Player player)
		{
			Vector2 direction = Vector2.Normalize(projectile.Center - player.Center);
			float distance = Vector2.Distance(projectile.Center, player.Center);

			projectile.velocity = direction *(projectile.velocity.Length() > player.velocity.Length() ? projectile.velocity.Length() / 2f : player.velocity.Length());
		}
    }
}