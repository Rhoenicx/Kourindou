using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Projectiles.Plushies.PlushieEffects
{
    public class SakuyaIzayoi_Plushie_Knife : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Sakuya's Knife");    
		}
		
		public override void SetDefaults()
		{
			// AI
			Projectile.aiStyle = -1;

            // Entity Interaction
			Projectile.friendly = true;
            Projectile.hostile = false;
			Projectile.penetrate = (int)Main.rand.Next(1,4);
			Projectile.DamageType = DamageClass.Throwing;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;

			// Hitbox
			Projectile.width = 8;
			Projectile.height = 8;

			// Movement
			Projectile.timeLeft = 120;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true; 
			
			// Visual
			Projectile.scale = 1f;
		}
		
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				texture.Bounds,
				lightColor * Projectile.Opacity,
				Projectile.rotation,
				texture.Size() * 0.5f,
				Projectile.scale,
				SpriteEffects.None,
				0);

			return false;
		}

		public override void AI()
		{
			if (Projectile.timeLeft < 30)
			{
				Projectile.rotation += Projectile.velocity.X > 0f ? MathHelper.ToRadians(25) : MathHelper.ToRadians(-25);
				Projectile.Opacity -= 1f/30f;
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, new Vector2(0f, 8f), 0.05f);
			}
			else
			{
				Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;
			}
		}
	}
}