using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Items.Plushies;

namespace Kourindou.Projectiles.Plushies.PlushieEffects
{
    public class YoumuKonpaku_Plushie_HalfPhantom : ModProjectile
    {
        private int Type
		{
			get => (int)projectile.ai[0];
			set => projectile.ai[0] = value;
		}
		
		private int State
		{
			get => (int)projectile.ai[1];
			set => projectile.ai[1] = value;
		}

		//animation
		private int Frame_Width = 0;
		private int timer = 0;
		private bool animDirection = true;
		private float TurnResistance = 1f;
		private int Rotation;
		
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Phantom");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            // AI
            projectile.aiStyle = -1;
			projectile.timeLeft = 60;
			projectile.netImportant = true;
            
            // Entity Interaction
            projectile.friendly = true;
            projectile.hostile = false;  
            projectile.penetrate = -1;
            
            // Hitbox
            projectile.width = 24;
            projectile.height = 24;
			
            // Movement
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
			
			//visual
			projectile.Opacity = 0.5f;
	    }

        // Projectile is re-centered and drawn unaffected by the light
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
			int frameWidth = texture.Width / 5;
			int frameHeight = texture.Height / 3;
			
			float offsetX = frameWidth/2;
			float offsetY = frameHeight/2;
			
			
			//Draw each part manually to stack opacity
            spriteBatch.Draw(
                texture, 
                projectile.Center - Main.screenPosition, 
                new Rectangle(frameWidth * Frame_Width, 0, frameWidth, frameHeight), 
                Color.White * projectile.Opacity, 
                projectile.rotation, 
                new Vector2(offsetX, offsetY), 
                projectile.scale, 
                SpriteEffects.None, 
                0);
			
			spriteBatch.Draw(
                texture, 
                projectile.Center - Main.screenPosition, 
                new Rectangle(frameWidth * Frame_Width, frameHeight * 1, frameWidth, frameHeight), 
                Color.White * projectile.Opacity, 
                projectile.rotation, 
                new Vector2(offsetX, offsetY), 
                projectile.scale, 
                SpriteEffects.None, 
                0);
				
			spriteBatch.Draw(
                texture, 
                projectile.Center - Main.screenPosition, 
                new Rectangle(frameWidth * Frame_Width, frameHeight * 2, frameWidth, frameHeight), 
                Color.White * projectile.Opacity, 
                projectile.rotation, 
                new Vector2(offsetX, offsetY), 
                projectile.scale, 
                SpriteEffects.None, 
                0);
            return false;
        }

        public override void AI()
        {
			if (Type == 0) //pet AI
			{
				// LIGHT PET
				Main.projPet[projectile.type] = true;
				ProjectileID.Sets.LightPet[projectile.type] = true;
				
				// prevent vector reversal
				projectile.damage = 0;
				projectile.friendly = true;
				projectile.hostile = false;
				
				// owner of the pet
				Player player = Main.player[projectile.owner];
				
				if (player.dead) 
				{
					projectile.Kill();
				}

				if (player.GetModPlayer<KourindouPlayer>().plushieEquipSlot.Item.type == ItemType<YoumuKonpaku_Plushie_Item>())
				{
					projectile.timeLeft = 60;
				}
				else
				{
					projectile.Kill();
				}

				//light
				Lighting.AddLight(projectile.Center, 1f, 1f, 1f);

				
				//Move and idle logic
				Vector2 IdlePosition = player.Center + new Vector2(0f,-75f).RotatedBy(MathHelper.ToRadians(Rotation));
				float Distance = Vector2.Distance(IdlePosition, projectile.Center);
				
				
				switch (State)
				{
					case 0: //move
						if (projectile.velocity.Length() > 0.1f)
						{
							projectile.velocity = Vector2.Normalize(IdlePosition - projectile.Center) * Distance / 10f;
							projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 0.79f;
							Rotation = 0;
						}
						else
						{
							projectile.velocity = Vector2.Zero;
							timer = 0;
							State = 1;
						}
						break;
					
					case 1: //idle
						if (Distance > 10f)
						{
							State = 0;
							projectile.velocity = new Vector2(1f,0f).RotatedBy(projectile.rotation - 0.79f);
						}
						
						int randomChance = (int)Main.rand.Next(0,120);
						if (randomChance == 100)
						{
							State = 2;
							Rotation = (int)Main.rand.Next(-90,91);
							projectile.velocity = new Vector2(1f,0f).RotatedBy(projectile.rotation - 0.79f);
							projectile.netUpdate = true;
						}
						break;
						
					case 2: //move to randomized location
						if (player.velocity.Length() < 0.2f && Distance > 10f)
						{
							projectile.velocity = Vector2.Normalize(IdlePosition - projectile.Center) * Distance / 50f;
							projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 0.79f;
						}
						else
						{
							projectile.velocity = Vector2.Zero;
							State = 1;
							projectile.netUpdate = true;

						}
						break;
					
					default:
						break;
				}
			}
			
			Animations();
        }
		
		private void Animations()
		{		
			//tail animations
			if (projectile.velocity.Length() < 1f)
			{
				if (animDirection)
				{
					if (timer++ % 10 == 0)
					{
						if (Frame_Width == 4)
						{
							animDirection = false;
							return;
						}
						Frame_Width++;
					}
				}
				else
				{
					if (timer++ % 10 == 0)
					{
						if (Frame_Width == 0)
						{
							animDirection = true;
							return;
						}
						Frame_Width--;
					}
				}
			}
			else
			{
				int changeTail = 0;
				float checkDistance = 0;
				
				checkDistance = Vector2.Distance(projectile.position, projectile.oldPos[0]);
				
				for (int i = 0; i < projectile.oldPos.Length - 1; i++)
				{
					checkDistance += Vector2.Distance(projectile.oldPos[i], projectile.oldPos[i + 1]);
					
					if (checkDistance > 25f)
					{
						changeTail = i;
						break;
					}
				}
				
				if (changeTail > 0)
				{
					Vector2 calcAngle = Vector2.Normalize(projectile.oldPos[changeTail] - projectile.oldPos[changeTail + 1]).RotatedBy(-(float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X));
					float angle = MathHelper.ToDegrees((float)Math.Atan2(calcAngle.Y, calcAngle.X));
					//Main.NewText(angle.ToString(),155,155,155);

					if (angle > 10f)
					{
						Frame_Width = 0;
					}
					else if (angle > 3f)
					{
						Frame_Width = 1;
					}
					else if (angle < -10f)
					{
						Frame_Width = 4;
					}
					else if (angle < -3f)
					{
						Frame_Width = 3;
					}
					else
					{
						Frame_Width = 2;
					}
					
				}
				
				if (Type == 0 && State == 2)
				{
					Frame_Width = 2;
				}
			}
		}
    }
}
