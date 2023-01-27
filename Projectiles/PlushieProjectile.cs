using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Items;

namespace Kourindou.Projectiles
{
    public abstract class PlushieProjectile : ModProjectile
    {	
		// Constants
		protected Vector2 gravity = new(0f, 10f);
		protected const float defaultMagnitudeX = 0.005f;
		protected const float defaultMagnitudeY = 0.01f;

		// Initialization
		private bool JustSpawned = true;
		public int plushieTile;
		public int plushieItem;

		// Owner sided Timer
		public int dropTimer = 0;

		// Tile placement location
		private int plushiePlaceTileX = 0;
		private int	plushiePlaceTileY = 0;

		// Dirt and Water mechanic
		public byte DirtAmount
		{
			get { return (byte)(PlushieDirtWater >> 8); }
			set { PlushieDirtWater = (short)((value * 256) + (byte)(PlushieDirtWater >> 8)); }
		}
		public byte WaterAmount
		{
			get { return (byte)(PlushieDirtWater); }
			set { PlushieDirtWater = (short)(value + (PlushieDirtWater >> 8) * 256); }
		}

		// AI & LocalAI
		public int Timer 
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public short PlushieDirtWater
		{
			get => (short)Projectile.ai[1];
			set => Projectile.ai[1] = value;
		} 

		public float MagnitudeX
		{
			get => Projectile.localAI[0];
			set => Projectile.localAI[0] = value;
		}

		public float MagnitudeY
		{
			get => Projectile.localAI[1];
			set => Projectile.localAI[1] = value;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			Main.EntitySpriteDraw(
				texture, 
				Projectile.Center - Main.screenPosition, 
				texture.Bounds, 
				lightColor, 
				Projectile.rotation, 
				texture.Size() * 0.5f, 
				Projectile.scale, 
				SpriteEffects.None, 
				0);
			return false;
		}

		public override bool? CanCutTiles() => false;

		public override bool OnTileCollide (Vector2 oldVelocity)
        {
			if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.5f;
            }

            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.5f;
            }

			if (Projectile.velocity.Length() < 0.2f)
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
				MagnitudeX = defaultMagnitudeX;
				MagnitudeY = defaultMagnitudeY;
				JustSpawned = false;
			}

			Projectile.damage = 0;

			// Change projectile velocity towards gravity vector based on magnitude and LERP
			Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, gravity.X, MagnitudeX);
			Projectile.velocity.Y = MathHelper.Lerp(Projectile.velocity.Y, gravity.Y, MagnitudeY);

			// Projectile rotation based on distance travelled on X axis
			Projectile.rotation += MathHelper.ToRadians(Projectile.velocity.X) * 2f;

			// Check if the Plushie is NOT changing Y position => laying on the ground so reduce X speed
			if (Vector2.Distance(new Vector2(0f, Projectile.position.Y), new Vector2(0f, Projectile.oldPos[4].Y)) < 0.25f)
			{
				MagnitudeX += 0.0025f;
			}
			else
			{
				MagnitudeX = defaultMagnitudeX;
			}

			if (Timer == 0)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					// If the projectile is not moving for 15 ticks, kill it
					if (Projectile.owner == Main.myPlayer)
					{
						if (dropTimer > 15)
						{
							Projectile.netUpdate = true;
							Projectile.Kill();
						}
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
						if (Colliding(Projectile.Hitbox, player.Hitbox) == true)
						{
							OnHitPlayer(player);
							Projectile.netUpdate = true;
						}

						Rectangle hitbox;

						if (KourindouGlobalItem.meleeHitbox[player.whoAmI].HasValue && player.itemAnimation > 0)
						{
							hitbox = KourindouGlobalItem.meleeHitbox[player.whoAmI].Value;
							hitbox.X += (int)player.Center.X;
							hitbox.Y += (int)player.Center.Y;
							hitbox = Main.ReverseGravitySupport(hitbox);
							
							if (Colliding(Projectile.Hitbox, hitbox) == true)
							{
								OnHitPlayerMelee(player);
								Projectile.netUpdate = true;
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

						if (Colliding(Projectile.Hitbox, Main.npc[i].Hitbox) == true)
						{
							OnHitNPC(Main.npc[i]);
							Projectile.netUpdate = true;
						}
					}
				}
			}

			if (Timer > 0)
			{
				Timer--;
			}
		}

		public override void Kill(int timeLeft)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				if (CanPlacePlushie() && WorldGen.PlaceObject(plushiePlaceTileX, plushiePlaceTileY, plushieTile))
				{
					KourindouWorld.SetPlushieDirtWater(plushiePlaceTileX, plushiePlaceTileY - 1, PlushieDirtWater);

					if (Main.netMode == NetmodeID.Server)
					{
						ModPacket packet = Mod.GetPacket();
						packet.Write((byte) KourindouMessageType.PlacePlushieTile);
						packet.Write((int) plushiePlaceTileX);
						packet.Write((int) plushiePlaceTileY);
						packet.Write((int) plushieTile);
						packet.Write((short) PlushieDirtWater);
						packet.Send(-1, Main.myPlayer);
					}
				}
				else
				{
					int itemSlot = Item.NewItem(
						Projectile.GetSource_DropAsItem(),
						Projectile.getRect(),
						plushieItem,
						1
					);

					if (Main.item[itemSlot].ModItem is PlushieItem plushie)
					{
						plushie.PlushieDirtWater = PlushieDirtWater;
					}

					if (Main.netMode == NetmodeID.Server)
					{
						ModPacket packet = Mod.GetPacket();
						packet.Write((byte) KourindouMessageType.PlushieItemNetUpdate);
						packet.Write((int) itemSlot);
						packet.Write((short) PlushieDirtWater);
						packet.Send(-1, Main.myPlayer);
					}
				}
			}
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			MagnitudeX = reader.ReadSingle();
			MagnitudeY = reader.ReadSingle();
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(MagnitudeX);
			writer.Write(MagnitudeY);
		}

		public void OnHitNPC(NPC target)
		{
			Vector2 direction = Vector2.Normalize(Projectile.Center - target.Center);

			Projectile.velocity = direction * (Projectile.velocity.Length() > target.velocity.Length() ? Projectile.velocity.Length() / 2f : target.velocity.Length());
			Timer = 5;
		}

		public void OnHitPlayer(Player player)
		{
			Vector2 direction = Vector2.Normalize(Projectile.Center - player.Center);

			Projectile.velocity = direction * (Projectile.velocity.Length() > player.velocity.Length() ? Projectile.velocity.Length() / 2f : player.velocity.Length());
			Timer = 5;
		}

		public void OnHitPlayerMelee(Player player)
		{
			Vector2 direction = Vector2.Normalize(Projectile.Center - player.Center);

			Projectile.velocity = direction * player.HeldItem.knockBack;
			Timer = 30;
		}

		public bool CanPlacePlushie()
		{
			// Raycast downwards until we find a solid block
			bool foundSolidTile = false;
			int searchLimit = 0;

			int tileX = (int)(Projectile.Center.X / 16);
			int tileY = (int)(Projectile.Center.Y / 16);

			while (!foundSolidTile)
			{
				if (searchLimit >= 8)
				{
					return false;
				}

				Tile tile = Framing.GetTileSafely(tileX, tileY + searchLimit);

				if (tile.HasTile)
				{
					if (Main.tileSolid[tile.TileType])
					{
						tileY += searchLimit;
						foundSolidTile = true;
					}
				}
				searchLimit++;
			}

			// Check middle tiles
			if (Framing.GetTileSafely(tileX, tileY).Slope == 0
				&& Framing.GetTileSafely(tileX, tileY).HasTile
				&& Main.tileSolid[Framing.GetTileSafely(tileX, tileY).TileType]
				&& !Framing.GetTileSafely(tileX, tileY).IsHalfBlock
				&& CheckTiles(Framing.GetTileSafely(tileX, tileY - 1), Framing.GetTileSafely(tileX, tileY - 2)))
			{
				bool leftOK = false;
				bool rightOK = false;

				// Check left tiles
				if (Framing.GetTileSafely(tileX - 1, tileY).Slope == 0
					&& Framing.GetTileSafely(tileX - 1, tileY).HasTile
					&& Main.tileSolid[Framing.GetTileSafely(tileX - 1, tileY).TileType]
					&& !Framing.GetTileSafely(tileX - 1, tileY).IsHalfBlock
					&& CheckTiles(Framing.GetTileSafely(tileX - 1, tileY - 1), Framing.GetTileSafely(tileX -1, tileY - 2)))
				{
					leftOK = true;
				}

				// check right tiles
				if (Framing.GetTileSafely(tileX + 1, tileY).Slope == 0
					&& Framing.GetTileSafely(tileX + 1, tileY).HasTile
					&& Main.tileSolid[Framing.GetTileSafely(tileX + 1, tileY).TileType]
					&& !Framing.GetTileSafely(tileX + 1, tileY).IsHalfBlock
					&& CheckTiles(Framing.GetTileSafely(tileX + 1, tileY - 1), Framing.GetTileSafely(tileX + 1, tileY - 2)))
				{
					rightOK = true;
				}

				// determine direction
				if (leftOK && rightOK)
				{
					tileX += Projectile.Center.X - Math.Floor(Projectile.Center.X) > 0.5f ? 0 : -1;
				}
				else if (!leftOK && rightOK)
				{
					// nothing to change
				}
				else if (leftOK && !rightOK)
				{
					tileX -= 1;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}

			plushiePlaceTileX = tileX;
			plushiePlaceTileY = tileY - 1;

			return true;
		}

		public static bool CheckTiles(Tile tile1, Tile tile2)
		{
			if (tile1.HasTile || tile2.HasTile ||
				tile1.TileType > 0 || tile2.TileType > 0 ||
				tile1.LiquidType > 0 || tile2.LiquidType > 0)
			{
				return false;
			}
			return true;
		}
    }
}