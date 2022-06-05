using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Items.Weapons;

namespace Kourindou.Projectiles.Weapons
{
    public class CrescentMoonStaffFlame : ModProjectile
    {
        // Projectile
        public Player owner;
        public float distanceFromPlayer = 50f;
        public const int FrameAmount = 4;

        public float flameCount = 0f;

        // Saved values
        public bool _JustSpawned = true;
        public int _OwnerID;
        public bool _Hostile;
        public bool _Friendly;
        public int _Damage;
        public Vector2 _Velocity;

        public Vector2 oldRotation;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crescent Moon Staff");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            // AI
            Projectile.aiStyle = -1;

            // Entity Interaction
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;

            // Hitbox
            Projectile.width = 24;
            Projectile.height = 24;

            // Movement
            Projectile.timeLeft = 10;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            // Visual
            Projectile.scale = 1f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // Afterimages
            for (int i = ProjectileID.Sets.TrailCacheLength[Projectile.type] - 1; i > -1 ; i--)
            {
                Main.EntitySpriteDraw(
                    texture,
                    Projectile.oldPos[i] + Projectile.Hitbox.Size() * 0.5f - Main.screenPosition,
                    new Rectangle(0, 32 * Projectile.frame, 32, 32),
                    Color.White * (1f - ((1f / (ProjectileID.Sets.TrailCacheLength[Projectile.type] + 1)) * (i + 1))) * Projectile.Opacity,
                    0f,
                    new Vector2(texture.Width, texture.Height / FrameAmount) * 0.5f,
                    Projectile.scale - ((1f / (ProjectileID.Sets.TrailCacheLength[Projectile.type] + 1)) * (i + 1)),
                    SpriteEffects.None,
                    0);
            }

            // Main image
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, 32 * Projectile.frame, 32, 32),
                Color.White * Projectile.Opacity,
                0f,
                new Vector2(texture.Width, texture.Height / FrameAmount) * 0.5f,
                Projectile.scale,
                SpriteEffects.None,
                0);




            return false;
        }

        public override bool? CanCutTiles() => false;

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            // Prevent Gensokyo's Vector Reversal 
            if (_JustSpawned)
            {
                _OwnerID = Projectile.owner;
                _Hostile = Projectile.hostile;
                _Friendly = Projectile.friendly;
                _Damage = Projectile.damage;
                flameCount = (int)Projectile.ai[0] >> 16;
                _JustSpawned = false;
            }
            else
            {
                // Revert changes
                Projectile.owner = _OwnerID;
                Projectile.hostile = _Hostile;
                Projectile.friendly = _Friendly;
                Projectile.damage = _Damage;
                Projectile.velocity = _Velocity;
            }

            // Owner of the projectile
            owner = Main.player[Projectile.owner];

            
            if (Projectile.owner == Main.myPlayer)
            {
                // Kill Logic - Kill is automatically synchronized; should only run the Dictionary check on the player that owns this projectile.
                if (!owner.GetModPlayer<KourindouPlayer>().CrescentMoonStaffFlames.ContainsKey((int)Projectile.ai[1])
                    || owner.GetModPlayer<KourindouPlayer>().CrescentMoonStaffFlames[(int)Projectile.ai[1]] != Projectile.whoAmI)
                {
                    Projectile.Kill();
                }

                // Get the amount of flames from the player and if it has been changed update the count
                if (((int)Projectile.ai[0] >> 16) != owner.GetModPlayer<KourindouPlayer>().CrescentMoonStaffFlames.Count)
                {
                    Projectile.ai[0] = (owner.GetModPlayer<KourindouPlayer>().CrescentMoonStaffFlames.Count << 16) | ((int)Projectile.ai[0] & 0xffff);
                    Projectile.netUpdate = true;
                }
            }

            // Move the flame gradually to the new location when a new one spawns in
            if ((int)Projectile.ai[0] >> 16 != flameCount)
            {
                if ((int)Projectile.ai[0] >> 16 > flameCount)
                {
                    flameCount += 1f/90f;
                }
                else
                { 
                    flameCount = (float)((int)Projectile.ai[0] >> 16);
                }
            }


            // Global Kill if the player dies or is no longer active/logged in
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
            }
            else if (owner.HeldItem.type == ItemType<CrescentMoonStaff>())
            {
                Projectile.timeLeft = 3600;
                Projectile.Opacity = 1f;
            }
            else
            {
                Projectile.Opacity -= 1f / 3600;
            }

            // Increase distance if just spawned
            if (Projectile.velocity.Length() < distanceFromPlayer)
            {
                Projectile.velocity *= 1f + (distanceFromPlayer - Projectile.velocity.Length()) / 500;
            }

            // Increase projectile ai 0 for the global rotation value 
            Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(((int)Projectile.ai[0] & 0xffff) == 0 ? 1 : -1));

            // Update position of the projectile
            Projectile.Center = owner.Center + Projectile.velocity.RotatedBy(MathHelper.ToRadians(360 / flameCount * Projectile.ai[1])) + new Vector2(0f, owner.gfxOffY);

            // Update timer for sync and animation purposes

            Projectile.localAI[0]++;

            if ((int)Projectile.localAI[0] % 120 == 0)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.netUpdate = true;
                }
            }

            // Drawing the frames
            if (Projectile.localAI[0] % 10 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0;
                }
            }

            // save the current velocity
            _Velocity = Projectile.velocity;
        }
    }
}
