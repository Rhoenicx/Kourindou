using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
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

        public override void SetDefaults()
        {
            // AI
			projectile.aiStyle = -1;

            // Entity Interaction
			projectile.friendly = true;
            projectile.hostile = false;
			projectile.penetrate = -1;
            projectile.damage = 1;

			// Hitbox
			projectile.width = 98;
			projectile.height = 98;

			// Movement
			projectile.timeLeft = TimeAlive;
			projectile.tileCollide = false;
			
			// Visual
			projectile.scale = 1.5f;
        }

        public override bool PreDraw (SpriteBatch spriteBatch, Color lightColor)
		{
            Texture2D texture = Main.projectileTexture[projectile.type];

            spriteBatch.Draw(
                texture,
                projectile.Center - Main.screenPosition,
                new Rectangle(0, 0 + (texture.Height / Frames * Frame), texture.Width, texture.Height / Frames),
                lightColor,
                projectile.rotation,
                new Vector2(texture.Width, texture.Height / Frames) * 0.5f,
                projectile.scale,
                SpriteEffects.None,
                0);

            return false;
        }

        public override void AI ()
        {
            if (JustSpawned)
            {
                if ((int)projectile.ai[0] < 10000)
                {
                    Main.npc[(int)projectile.ai[0]].immune[projectile.owner] = 0;
                }
                else
                {
                    projectile.ai[0] -= 10000f;

                    Main.player[(int)projectile.ai[0]].immuneTime = 0;
                }

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
	        target.immune[projectile.owner] = (int)projectile.ai[1];
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
	        target.immuneTime = (int)projectile.ai[1];
        }
    }
}