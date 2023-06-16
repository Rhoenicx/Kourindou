using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace Kourindou.Projectiles.Spellcards
{
    public class ShotBlueProj : SpellCardProjectile
    {
        public override void SetProjectileDefaults()
        {
            // AI
            Projectile.aiStyle = -1;

            // Entity Interaction
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Default;

            // Stats
            Projectile.velocity = new Vector2(5f, 0f);
            Projectile.damage = 10;
            Projectile.knockBack = 1f;
            Projectile.CritChance = 5;
            Projectile.timeLeft = 300;

            // Hitbox
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;

            // Visuals
            Projectile.scale = 1f;
            Projectile.Opacity = 1f;

            // Hits
            Projectile.idStaticNPCHitCooldown = 1;
            Projectile.usesIDStaticNPCImmunity = true;

            // Default Custom Modifiers
        }
    }
}
