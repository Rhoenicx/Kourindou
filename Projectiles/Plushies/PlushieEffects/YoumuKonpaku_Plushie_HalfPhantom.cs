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
		// Network variables
		private float IdleDistance
        {
	        get => (int)projectile.ai[0];
	        set => projectile.ai[0] = value;
        }

		private float IdleRotation
		{
			get => (int)projectile.ai[1];
	        set => projectile.ai[1] = value;
		}

		private int Target
		{
			get => (int)projectile.localAI[0];
			set => projectile.localAI[0] = value;
		}

		private int IdleMoveCooldown
		{
			get => (int)projectile.localAI[1];
			set => projectile.localAI[1] = value;
		}

		// Attack
		private bool Attacking;
		private float ViewDist = 800f;
		private bool HasTarget => Target >= 0;

		// Movement
		private bool MoveUpdate = true;
		private float MaxSpeed = 15f;
		private float Magnitude = 0.05f;
		private float OldIdleDistance = 0f;
		private float OldIdleRotation = 0f;
		private Vector2 IdlePostion;

		// Animation
		private int frame = 0;
		private int timer = 0;
		private bool animDirection = true;
		private int Rotation;

		// Initialization
		private bool _justSpawned = true;
		private Player _owner;

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Half Phantom");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
			Lighting.AddLight(projectile.Center, 1f, 1f, 1f);
		}

        public override void SetDefaults()
        {
            // AI
            projectile.aiStyle = -1;
			projectile.timeLeft = 5;
			projectile.netImportant = true;
            
            // Entity Interaction
            projectile.friendly = true; 
            projectile.penetrate = -1;
            
            // Hitbox
            projectile.width = 24;
            projectile.height = 24;
			
			// Damage
			projectile.magic = true;
			projectile.damage = 20;

            // Movement
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
			projectile.manualDirectionChange = true;

			// Minion
			projectile.netImportant = true;
			 
			// visual
			projectile.Opacity = 0.5f;
	    }

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
                new Rectangle(frameWidth * frame, 0, frameWidth, frameHeight), 
                Color.White * projectile.Opacity, 
                projectile.rotation, 
                new Vector2(offsetX, offsetY), 
                projectile.scale, 
                SpriteEffects.None, 
                0);
			
			spriteBatch.Draw(
                texture, 
                projectile.Center - Main.screenPosition, 
                new Rectangle(frameWidth * frame, frameHeight * 1, frameWidth, frameHeight), 
                Color.White * projectile.Opacity, 
                projectile.rotation, 
                new Vector2(offsetX, offsetY), 
                projectile.scale, 
                SpriteEffects.None, 
                0);
				
			spriteBatch.Draw(
                texture, 
                projectile.Center - Main.screenPosition, 
                new Rectangle(frameWidth * frame, frameHeight * 2, frameWidth, frameHeight), 
                Color.White * projectile.Opacity, 
                projectile.rotation, 
                new Vector2(offsetX, offsetY), 
                projectile.scale, 
                SpriteEffects.None, 
                0);
            return false;
        }

	    public override bool? CanCutTiles()
	    {
		    return false;
	    }

		public override bool MinionContactDamage()
        {
	        return true;
        }

        public override void AI()
        {
			if (_justSpawned)
			{
				// Get the owner of this projectile
				_owner = Main.player[projectile.owner];

				Target = -1;

				// Justspawned to false
				_justSpawned = false;
			}
			
			CheckIfOwnerActive();

			Target = -1;

			AcquireTarget();

			if (IdlePostionChanged() || OwnerTooFar())
			{
				MoveUpdate = true;
			}

			if (HasTarget)
			{
				ChaseTarget();
			}	
			else
			{
				FollowOwner();
			}

			Animations();
		}

		private void CheckIfOwnerActive()
		{
			if (_owner.GetModPlayer<KourindouPlayer>().plushieEquipSlot.Item.type == ItemType<YoumuKonpaku_Plushie_Item>())
			{
				projectile.timeLeft = 2;
			}

			if (!_owner.active || _owner.dead) 
			{
				projectile.Kill();
			}
		}

		private void AcquireTarget()
		{
			float targetDist = ViewDist;
			projectile.tileCollide = true;

			if (_owner.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[_owner.MinionAttackTargetNPC];
				if (Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height))
		        {
			        Target = npc.whoAmI;
		        }
			}
			else
			{
		        // Find the closest chaseable NPC
		        foreach (NPC potentialTarget in Main.npc)
		        {
			        if (potentialTarget.CanBeChasedBy(this)) 
			        {
				        float distance = Vector2.Distance(potentialTarget.Center, projectile.Center);
				        if ((distance < targetDist || !HasTarget) 
				            && Collision.CanHitLine(
					            projectile.position, 
					            projectile.width, 
					            projectile.height, 
					            potentialTarget.position, 
					            potentialTarget.width, 
					            potentialTarget.height))
				        {
					        Target = potentialTarget.whoAmI;
					        targetDist = distance;
				        }
			        }
		        }
			}

			projectile.tileCollide = false;
			projectile.netUpdate = true;
		}

		private void ChaseTarget()
		{
			float chaseDistance = 1000f;
			NPC target = Main.npc[Target];

			float distToTarget = projectile.Distance(target.Center);

			if (distToTarget < chaseDistance)
			{
				Movement(target.Center + target.velocity);
			}
			else
			{
				Target = -1;
				projectile.netUpdate = true;
			}

			MoveUpdate = true;
		}

		private void FollowOwner()
		{
			Vector2 toOwner = _owner.Center - projectile.Center;

			if (Main.myPlayer == _owner.whoAmI)
			{
				if (IdleMoveCooldown <= 0 && !HasTarget)
				{
					IdleMoveCooldown = (int)Main.rand.Next(120, 900);

					IdleDistance = Main.rand.NextFloat(50f,75f);

					IdleRotation = MathHelper.ToRadians(Main.rand.NextFloat(-180f,0f));

					projectile.netUpdate = true;
				}	

				if (IdleMoveCooldown > 0)
				{
					IdleMoveCooldown --;
				}			
			}

			Vector2 position = _owner.Center + new Vector2(IdleDistance, 0f).RotatedBy(IdleRotation) + _owner.velocity;

			if (toOwner.Length() > 2000f)
			{
				projectile.position = position;
				projectile.netUpdate = true;
			}
			else
			{
				Movement(position);
			}
		}

		private void Movement(Vector2 position)
		{
			float bonusMagnitude = projectile.Distance(position) > 1f ? (.1f - Magnitude) / projectile.Distance(position) : .1f - Magnitude;

			//Get the vector2 between projectile and target position
            Vector2 difference = position - projectile.Center;
			
            //create a new Vector2 which is slightly less than the difference
            Vector2 offset = difference * 0.95f;

            //get the speed
			Vector2 change = CapVector(difference - offset, MaxSpeed);

			if (!HasTarget)
			{
				Vector2 Distance = position - projectile.Center;
				if (Distance.Length() < MaxSpeed + 1f)
				{
					MoveUpdate = false;
				}

				if (MoveUpdate)
				{
					projectile.velocity = Vector2.Lerp(projectile.velocity, change, Magnitude + bonusMagnitude);
				}
				else
				{
					if (projectile.velocity.Length() > 0.5f)
					{
						projectile.velocity /= 1.1f;
						IdlePostion = projectile.Center;
					}
					else
					{
						projectile.velocity = Vector2.Zero;
						IdlePostion = projectile.Center;
					}
				}
			}
			else
			{
				MoveUpdate = true;
				if (projectile.Distance(Main.npc[Target].Center) > MaxSpeed + 1f)
				{
					Vector2 AttackSpeed = CapVector(Main.npc[Target].Center - projectile.Center, MaxSpeed);
            		projectile.velocity = Vector2.Lerp(projectile.velocity, AttackSpeed, Magnitude + bonusMagnitude);
				}
				else
				{
					projectile.velocity /= 1.25f;
				}
			}
		}

		protected Vector2 CapVector(Vector2 vector, float max)
        {
            return vector.Length() > max ? Vector2.Normalize(vector) * max : vector;
        }

		private bool IdlePostionChanged()
		{
			if (IdleDistance != OldIdleDistance || IdleRotation != OldIdleRotation)
			{
				OldIdleDistance = IdleDistance;
				OldIdleRotation = IdleRotation;
				return true;
			}
			return false;
			
		}

		private bool OwnerTooFar()
		{
			Vector2 Distance = _owner.Center + new Vector2(IdleDistance, 0f).RotatedBy(IdleRotation) - IdlePostion;
			if (Distance.Length() > MaxSpeed + 1f && projectile.velocity == Vector2.Zero)
			{
				return true;
			}
			return false;
		}

		private void Animations()
		{	
			if (projectile.velocity.Length() > 0.5f)
			{
				projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;
			}

			//tail animations
			if (projectile.velocity.Length() < 1f)
			{
				if (animDirection)
				{
					if (timer++ % 10 == 0)
					{
						if (frame == 4)
						{
							animDirection = false;
							return;
						}
						frame++;
					}
				}
				else
				{
					if (timer++ % 10 == 0)
					{
						if (frame == 0)
						{
							animDirection = true;
							return;
						}
						frame--;
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
						frame = 0;
					}
					else if (angle > 3f)
					{
						frame = 1;
					}
					else if (angle < -10f)
					{
						frame = 4;
					}
					else if (angle < -3f)
					{
						frame = 3;
					}
					else
					{
						frame = 2;
					}
					
				}
			}
		}
    }
}
