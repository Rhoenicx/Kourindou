using System;
using System.IO;
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
		private bool JustSpawned = true;

		protected Vector2 gravity = new Vector2(0f, 10f);
		protected float defaultMagnitudeX = 0.005f;
		protected float defaultMagnitudeY = 0.01f;

		public int Timer 
		{
			get => (int)projectile.ai[0];
			set => projectile.ai[0] = value;
		}

		public int dropTimer
		{
			get => (int)projectile.ai[1];
			set => projectile.ai[1] = value;
		} 

		public float magnitudeX
		{
			get => projectile.localAI[0];
			set => projectile.localAI[0] = value;
		}

		public float magnitudeY
		{
			get => projectile.localAI[1];
			set => projectile.localAI[1] = value;
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
			if (JustSpawned)
			{
				magnitudeX = defaultMagnitudeX;
				magnitudeY = defaultMagnitudeY;
				JustSpawned = false;
			}

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
				magnitudeX = defaultMagnitudeX;
			}

			if (Timer == 0)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					// If the projectile is not moving for 15 ticks, kill it
					if (dropTimer > 15)
					{
						projectile.netUpdate = true;
						projectile.Kill();
					}

					// Check collisions for players
					for (int i = 0; i < Main.player.Length; i++)
					{
						Player player = Main.player[i];

						if (!player.active || player.dead)
						{
							continue;
						}

						//player hitbox
						if (Colliding(projectile.Hitbox, player.Hitbox) == true)
						{
							OnHitPlayer(player);
							projectile.netUpdate = true;
						}

						Rectangle hitbox = new Rectangle();

						if (KourindouGlobalItem.meleeHitbox[player.whoAmI].HasValue && player.itemAnimation > 0)
						{
							hitbox = KourindouGlobalItem.meleeHitbox[player.whoAmI].Value;
							hitbox = Main.ReverseGravitySupport(hitbox);
							
							if (Colliding(projectile.Hitbox, hitbox) == true)
							{
								OnHitPlayerMelee(player);
								projectile.netUpdate = true;
							}
						}
						else
						{
							KourindouGlobalItem.meleeHitbox[player.whoAmI] = null;
						}
					}

					// Check collisions for NPC's
					for (int i = 0; i < Main.npc.Length; i++)
					{
						NPC npc = Main.npc[i];

						if (!npc.active)
						{
							continue;
						}

						if (Colliding(projectile.Hitbox, Main.npc[i].Hitbox) == true)
						{
							OnHitNPC(Main.npc[i]);
							projectile.netUpdate = true;
						}
					}
				}
			}

			if (Timer > 0)
			{
				Timer--;
			}
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			magnitudeX = reader.ReadSingle();
			magnitudeY = reader.ReadSingle();
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(magnitudeX);
			writer.Write(magnitudeY);
		}

		public void OnHitNPC(NPC target)
		{
			Vector2 direction = Vector2.Normalize(projectile.Center - target.Center);

			projectile.velocity = direction * (projectile.velocity.Length() > target.velocity.Length() ? projectile.velocity.Length() / 2f : target.velocity.Length());
			Timer = 5;
		}

		public void OnHitPlayer(Player player)
		{
			Vector2 direction = Vector2.Normalize(projectile.Center - player.Center);

			projectile.velocity = direction * (projectile.velocity.Length() > player.velocity.Length() ? projectile.velocity.Length() / 2f : player.velocity.Length());
			Timer = 5;
		}

		public void OnHitPlayerMelee(Player player)
		{
			Vector2 direction = Vector2.Normalize(projectile.Center - player.Center);

			projectile.velocity = direction * player.HeldItem.knockBack;
			Timer = 30;
		}
    }
}