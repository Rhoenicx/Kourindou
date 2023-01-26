using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
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
	        get => (int)Projectile.ai[0];
	        set => Projectile.ai[0] = value;
        }

		private float IdleRotation
		{
			get => (int)Projectile.ai[1];
	        set => Projectile.ai[1] = value;
		}

		private int Target
		{
			get => (int)Projectile.localAI[0];
			set => Projectile.localAI[0] = value;
		}

		private int IdleMoveCooldown
		{
			get => (int)Projectile.localAI[1];
			set => Projectile.localAI[1] = value;
		}

		// Attack
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

		// Initialization
		private bool _justSpawned = true;
		private Player _owner;

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Half Phantom");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
			Lighting.AddLight(Projectile.Center, 1f, 1f, 1f);
		}

        public override void SetDefaults()
        {
            // AI
            Projectile.aiStyle = -1;
			Projectile.timeLeft = 5;
			Projectile.netImportant = true;
            
            // Entity Interaction
            Projectile.friendly = true; 
            Projectile.penetrate = -1;
            
            // Hitbox
            Projectile.width = 24;
            Projectile.height = 24;
			
			// Damage
			Projectile.DamageType = DamageClass.Magic;
			Projectile.damage = 20;

            // Movement
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
			Projectile.manualDirectionChange = true;

			// Minion
			Projectile.netImportant = true;
			 
			// visual
			Projectile.Opacity = 0.5f;
	    }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			int frameWidth = texture.Width / 5;
			int frameHeight = texture.Height / 3;
			
			float offsetX = frameWidth/2;
			float offsetY = frameHeight/2;
			
			
			//Draw each part manually to stack opacity
            Main.EntitySpriteDraw(
                texture, 
                Projectile.Center - Main.screenPosition, 
                new Rectangle(frameWidth * frame, 0, frameWidth, frameHeight), 
                Color.White * Projectile.Opacity, 
                Projectile.rotation, 
                new Vector2(offsetX, offsetY), 
                Projectile.scale, 
                SpriteEffects.None, 
                0);
			
			Main.EntitySpriteDraw(
                texture, 
                Projectile.Center - Main.screenPosition, 
                new Rectangle(frameWidth * frame, frameHeight * 1, frameWidth, frameHeight), 
                Color.White * Projectile.Opacity, 
                Projectile.rotation, 
                new Vector2(offsetX, offsetY), 
                Projectile.scale, 
                SpriteEffects.None, 
                0);
				
			Main.EntitySpriteDraw(
                texture, 
                Projectile.Center - Main.screenPosition, 
                new Rectangle(frameWidth * frame, frameHeight * 2, frameWidth, frameHeight), 
                Color.White * Projectile.Opacity, 
                Projectile.rotation, 
                new Vector2(offsetX, offsetY), 
                Projectile.scale, 
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
				// Get the owner of this Projectile
				_owner = Main.player[Projectile.owner];

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
			Projectile.timeLeft = 2;

			if (!_owner.active || _owner.dead) 
			{
				Projectile.Kill();
			}

			if (_owner.whoAmI == Main.myPlayer)
			{
				if (!Main.player[_owner.whoAmI].GetModPlayer<KourindouPlayer>().EquippedPlushies.Any(kvp => kvp.Key.Type == ItemType<YoumuKonpaku_Plushie_Item>()) || !_owner.GetModPlayer<KourindouPlayer>().plushiePower)
				{
					Projectile.Kill();
				}
			}
		}

		private void AcquireTarget()
		{
			float targetDist = ViewDist;
			Projectile.tileCollide = true;

			if (_owner.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[_owner.MinionAttackTargetNPC];
				//if (Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height))
		        //{
					if (npc.active)
					{
			        	Target = npc.whoAmI;
					}
		        //}
			}
			else
			{
		        // Find the closest chaseable NPC
		        foreach (NPC potentialTarget in Main.npc)
		        {
			        if (potentialTarget.CanBeChasedBy(this)) 
			        {
				        float distance = Vector2.Distance(potentialTarget.Center, Projectile.Center);
				        if ((distance < targetDist || !HasTarget) 
				            && (Collision.CanHitLine(
					            Projectile.position, 
					            Projectile.width, 
					            Projectile.height, 
					            potentialTarget.position, 
					            potentialTarget.width, 
					            potentialTarget.height)
								||
								Collision.CanHitLine(
					            _owner.position, 
					            _owner.width, 
					            _owner.height, 
					            potentialTarget.position, 
					            potentialTarget.width, 
					            potentialTarget.height))
							)
				        {
					        Target = potentialTarget.whoAmI;
					        targetDist = distance;
				        }
			        }
		        }
			}

			Projectile.tileCollide = false;
			Projectile.netUpdate = true;
		}

		private void ChaseTarget()
		{
			float chaseDistance = 1000f;
			NPC target = Main.npc[Target];

			float distToTarget = Projectile.Distance(target.Center);

			if (distToTarget < chaseDistance)
			{
				Movement(target.Center + target.velocity);
			}
			else
			{
				Target = -1;
				Projectile.netUpdate = true;
			}

			MoveUpdate = true;
		}

		private void FollowOwner()
		{
			Vector2 toOwner = _owner.Center - Projectile.Center;

			if (Main.myPlayer == _owner.whoAmI)
			{
				if (IdleMoveCooldown <= 0 && !HasTarget)
				{
					IdleMoveCooldown = (int)Main.rand.Next(120, 900);

					IdleDistance = Main.rand.NextFloat(50f,75f);

					IdleRotation = MathHelper.ToRadians(Main.rand.NextFloat(-180f,0f));

					Projectile.netUpdate = true;
				}	

				if (IdleMoveCooldown > 0)
				{
					IdleMoveCooldown --;
				}			
			}

			Vector2 position = _owner.Center + new Vector2(IdleDistance, 0f).RotatedBy(IdleRotation) + _owner.velocity;

			if (toOwner.Length() > 2000f)
			{
				Projectile.position = position;
				Projectile.netUpdate = true;
			}
			else
			{
				Movement(position);
			}
		}

		private void Movement(Vector2 position)
		{
			float bonusMagnitude = Projectile.Distance(position) > 1f ? (.1f - Magnitude) / Projectile.Distance(position) : .1f - Magnitude;

			//Get the vector2 between Projectile and target position
            Vector2 difference = position - Projectile.Center;
			
            //create a new Vector2 which is slightly less than the difference
            Vector2 offset = difference * 0.95f;

            //get the speed
			Vector2 change = CapVector(difference - offset, MaxSpeed);

			if (!HasTarget)
			{
				Vector2 Distance = position - Projectile.Center;
				if (Distance.Length() < MaxSpeed + 1f)
				{
					MoveUpdate = false;
				}

				if (MoveUpdate)
				{
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, change, Magnitude + bonusMagnitude);
				}
				else
				{
					if (Projectile.velocity.Length() > 0.5f)
					{
						Projectile.velocity /= 1.1f;
						IdlePostion = Projectile.Center;
					}
					else
					{
						Projectile.velocity = Vector2.Zero;
						IdlePostion = Projectile.Center;
					}
				}
			}
			else
			{
				MoveUpdate = true;
				if (Projectile.Distance(Main.npc[Target].Center) > MaxSpeed + 1f)
				{
					Vector2 AttackSpeed = CapVector(Main.npc[Target].Center - Projectile.Center, MaxSpeed);
            		Projectile.velocity = Vector2.Lerp(Projectile.velocity, AttackSpeed, Magnitude + bonusMagnitude);
				}
				else
				{
					Projectile.velocity /= 1.25f;
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
			if (Distance.Length() > MaxSpeed + 1f && Projectile.velocity == Vector2.Zero)
			{
				return true;
			}
			return false;
		}

		private void Animations()
		{	
			if (Projectile.velocity.Length() > 0.5f)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
			}

			//tail animations
			if (Projectile.velocity.Length() < 0.5f)
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
				
				checkDistance = Vector2.Distance(Projectile.position, Projectile.oldPos[0]);
				
				for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
				{
					checkDistance += Vector2.Distance(Projectile.oldPos[i], Projectile.oldPos[i + 1]);
					
					if (checkDistance > 25f)
					{
						changeTail = i;
						break;
					}
				}
				
				if (changeTail > 0)
				{
					Vector2 calcAngle = Vector2.Normalize(Projectile.oldPos[changeTail] - Projectile.oldPos[changeTail + 1]).RotatedBy(-(float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X));
					float angle = MathHelper.ToDegrees((float)Math.Atan2(calcAngle.Y, calcAngle.X));

					if (angle > 12f)
					{
						frame = 0;
					}
					else if (angle > 4f)
					{
						frame = 1;
					}
					else if (angle < -12f)
					{
						frame = 4;
					}
					else if (angle < -4f)
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
