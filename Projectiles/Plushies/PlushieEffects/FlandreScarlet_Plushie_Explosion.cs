using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Projectiles.Plushies.PlushieEffects
{
    public class FlandreScarlet_Plushie_Explosion : ModProjectile
    {
        private bool JustSpawned = true;
        private const int Frames = 7;
        private int Frame = 0;
        private const int TimeAlive = 30;

        private int Timer = 0;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Flandre's Explosive Crit");    
		}
		
        public override void SetDefaults()
        {
            // AI
			Projectile.aiStyle = -1;

            // Entity Interaction
			Projectile.friendly = true;
            Projectile.hostile = false;
			Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.damage = 1;

			// Hitbox
			Projectile.width = 98;
			Projectile.height = 98;

			// Movement
			Projectile.timeLeft = TimeAlive;
			Projectile.tileCollide = false;
			
			// Visual
			Projectile.scale = 1.5f;
        }

        public override bool PreDraw(ref Color lightColor)
		{
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, 0 + (texture.Height / Frames * Frame), texture.Width, texture.Height / Frames),
                lightColor,
                Projectile.rotation,
                new Vector2(texture.Width, texture.Height / Frames) * 0.5f,
                Projectile.scale,
                SpriteEffects.None,
                0);

            return false;
        }

        public override void AI ()
        {
            if (JustSpawned)
            {
                if ((int)Projectile.ai[0] < 10000)
                {
                    Main.npc[(int)Projectile.ai[0]].immune[Projectile.owner] = 0;
                }
                else
                {
                    Projectile.ai[0] -= 10000f;

                    Main.player[(int)Projectile.ai[0]].immuneTime = 0;
                }

                Projectile.rotation = MathHelper.ToRadians(Main.rand.Next(0, 360));
                JustSpawned = false;
            }

            Frame = (int)(Timer / (TimeAlive / Frames));

            Timer += 1;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (Timer > 1f)
            {
                return false;
            }

            return base.CanHitNPC(target);
        }

        public override bool CanHitPvp(Player player)
        {
            if (Timer > 1f)
            {
                return false;
            }

            return base.CanHitPvp(player);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
	        target.immune[Projectile.owner] = (int)Projectile.ai[1];
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
	        target.immuneTime = (int)Projectile.ai[1];
        }
    }
}