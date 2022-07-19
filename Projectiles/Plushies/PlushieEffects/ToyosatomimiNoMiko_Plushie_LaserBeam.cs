using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Projectiles.Plushies.PlushieEffects
{
    public class ToyosatomimiNoMiko_Plushie_LaserBeam : ModProjectile
    {
        private bool JustSpawned = true;

        private const int StartTime = 5;
        private const int MiddleTime = 5;
        private const int EndTime = 5;
        private const int LifeTime = StartTime + MiddleTime + EndTime;

        private const float MaxWidth = 8f;
        private const float MaxLength = 2000f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Miko's laser");
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
            Projectile.width = 10;
            Projectile.height = 10;

            // Movement
            Projectile.timeLeft = LifeTime;
            Projectile.tileCollide = false;

            // Visual
            Projectile.scale = 1f;
            Projectile.Opacity = 1f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.EntitySpriteDraw(
                TextureAssets.MagicPixel.Value,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, 0, (int)Projectile.ai[0], (int)MaxLength),
                new Color(207, 192, 56),
                Projectile.rotation,
                new Vector2(Projectile.ai[0], 0) * 0.5f,
                Projectile.scale,
                SpriteEffects.None,
                0
           );

           Main.EntitySpriteDraw(
                TextureAssets.MagicPixel.Value,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, 0, (int)(Projectile.ai[0] * 0.5f), (int)MaxLength),
                new Color(255, 253, 186),
                Projectile.rotation,
                new Vector2((Projectile.ai[0] * 0.5f), 0) * 0.5f,
                Projectile.scale,
                SpriteEffects.None,
                0
           );

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Projectile.position,
                Projectile.position + new Vector2(0f, MaxLength).RotatedBy(Projectile.rotation),
                Projectile.ai[0] * Projectile.scale,
                ref point);
        }

        public override bool? CanDamage()
        {
            return (Projectile.timeLeft < LifeTime - StartTime) || (Projectile.timeLeft > EndTime);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.immune[Projectile.owner] = 20;
        }

        public override void AI()
        {
            // Set the rotation of the projectile on spawn
            if (JustSpawned)
            {
                Projectile.rotation = MathHelper.ToRadians(Main.rand.NextFloat(-30f, 30f)) + MathHelper.Pi;
                Projectile.netUpdate = true;
                Projectile.damage = Main.player[Projectile.owner].GetWeaponDamage(Main.player[Projectile.owner].HeldItem);
                Projectile.DamageType = Main.player[Projectile.owner].HeldItem.DamageType;
                JustSpawned = false;
            }

            // When the projectile has spawned in increase the width of the beam
            if (Projectile.timeLeft > LifeTime - StartTime)
            {
                Projectile.ai[0] += MaxWidth / StartTime;
            }

            // On the end decrease the width of the beam
            if (Projectile.timeLeft <= EndTime)
            {
                Projectile.ai[0] -= MaxWidth / EndTime;
            }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            crit = Projectile.ai[1] == 1f ? true : false;
        }

        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
        {
            crit = Projectile.ai[1] == 1f ? true : false;
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }
    }
}
