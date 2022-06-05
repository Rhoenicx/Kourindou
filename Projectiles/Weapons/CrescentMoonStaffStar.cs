using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Kourindou.Projectiles.Weapons
{
    public class CrescentMoonStaffStar : ModProjectile
    {
        public bool resized = false;
        public List<int> npcHit = new List<int>();

        public override ModProjectile Clone(Projectile projectile)
        {
            npcHit = new List<int>();
            return base.Clone(projectile);
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crescent Moon Staff");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            // AI
            Projectile.aiStyle = -1;

            // Entity Interaction
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 5;

            // Hitbox
            Projectile.width = 12;
            Projectile.height = 12;

            // Movement
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            // Visual
            Projectile.scale = 1f;
            Projectile.Opacity = 0f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // Afterimages
            for (int i = ProjectileID.Sets.TrailCacheLength[Projectile.type] - 1; i > -1; i--)
            {
                Main.EntitySpriteDraw(
                    texture,
                    Projectile.oldPos[i] + Projectile.Hitbox.Size() * 0.5f - Main.screenPosition,
                    new Rectangle(0, (texture.Height / 2) * (int)Projectile.ai[1], texture.Width, texture.Height / 2),
                    Color.White * (1f - ((1f / (ProjectileID.Sets.TrailCacheLength[Projectile.type] + 1)) * (i + 1))),
                    Projectile.oldRot[i],
                    new Vector2(texture.Width, texture.Height / 2) * 0.5f,
                    Projectile.scale - ((1f / (ProjectileID.Sets.TrailCacheLength[Projectile.type] + 1)) * (i + 1)),
                    SpriteEffects.None,
                    0);
            }

            // Main sprite
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, (texture.Height / 2) * (int)Projectile.ai[1], texture.Width, texture.Height / 2),
                Color.White * Projectile.Opacity,
                Projectile.rotation,
                new Vector2(texture.Width, texture.Height / 2) * 0.5f,
                Projectile.scale,
                SpriteEffects.None,
                0);

            return false;
        }

        public override void AI()
        {
            if (Projectile.Opacity < 1f)
            {
                Projectile.Opacity += 1f / 15f;
            }

            Projectile.rotation += Projectile.ai[0] == 0f ? MathHelper.ToRadians(7f) : MathHelper.ToRadians(-7f);

            if (!Projectile.tileCollide && (!Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height) || Projectile.localAI[0] > 60))
            {
                Projectile.tileCollide = true;
                Projectile.netUpdate = true;
            }


            if (Projectile.localAI[0] % 6 == 0)
            {
                Dust.NewDust(
                    Projectile.position,
                    Projectile.Hitbox.Width,
                    Projectile.Hitbox.Height,
                    Projectile.ai[1] == 0f ? 292 : 15

                    );
            }
            
            Projectile.localAI[0]++;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (resized)
            {
                return Projectile.tileCollide;
            }

            if (Projectile.tileCollide)
            {
                foreach (int npcID in npcHit)
                {
                    Main.npc[npcID].immune[Projectile.owner] = 0;
                }

                int NewSize = 120;
                Projectile.position.X -= (NewSize - Projectile.width) / 2;
                Projectile.position.Y -= (NewSize - Projectile.height) / 2;
                Projectile.width = NewSize;
                Projectile.height = NewSize;
                Projectile.damage *= 3;
                resized = true;
                Projectile.timeLeft = 1;
                return false;
            }

            return Projectile.tileCollide && resized;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            npcHit.Add(target.whoAmI);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 24; i++)
            {
                Dust.NewDustPerfect(
                    Projectile.Center,
                    Projectile.ai[1] == 0f ? 292 : 15,
                    new Vector2(0f, Main.rand.NextFloat(1f, 4f)).RotatedBy(MathHelper.ToRadians(360 / 10 * i + Main.rand.NextFloat(-36f, 36f))),
                    Main.rand.Next(100, 255),
                    newColor: default,
                    Main.rand.NextFloat(0.5f, 2f)
                );
            }
        }
    }
}
