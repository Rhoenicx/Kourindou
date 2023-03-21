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
    public abstract class CatalystProjectile : ModProjectile
    {
        public Cast cast;
        private bool _justSpawned = true;
        private Player _owner;
        protected float HeldProjectileOffset;

        private int LifeTime
        {
            get => (int)Projectile.ai[0]; 
            set => Projectile.ai[0] = value;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, 0, texture.Width, texture.Height),
                lightColor,
                Projectile.rotation + MathHelper.PiOver4,
                new Vector2(texture.Width, texture.Height) * 0.5f,
                Projectile.scale,
                SpriteEffects.None,
                0);

            return false;
        }

        public override void AI()
        {
            // Set player
            if (_justSpawned)
            {
                _owner = Main.player[Projectile.owner];
                _justSpawned = false;
            }

            Projectile.owner = _owner.whoAmI;
            Projectile.timeLeft = 5;

            // LifeTime has expired
            if (LifeTime <= 0)
            { 
                Projectile.Kill();
            }

            // Decrease lifetime
            LifeTime--;

            // Kill the projectile if the owner cannot hold it anymore
            if (_owner.noItems || _owner.CCed || _owner.dead)
            {
                Projectile.Kill();
                return;
            }

            TurnTowardsCursor();

            // Position item and projectile
            Projectile.position = _owner.RotatedRelativePoint(_owner.MountedCenter) - Projectile.Size / 2f + new Vector2(HeldProjectileOffset, 0f).RotatedBy(Projectile.velocity.ToRotation());
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = Projectile.direction;

            _owner.ChangeDir(Projectile.direction);
            _owner.heldProj = Projectile.whoAmI;
            _owner.itemTime = 2;
            _owner.itemAnimation = 2;
            _owner.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);
        }

        public override bool? CanDamage()
        {
            return false;
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        private void TurnTowardsCursor()
        {
            const float turnSpeed = 0.965f;

            // Player turning while firing
            Vector2 sourcePosition = _owner.RotatedRelativePoint(_owner.MountedCenter);
            if (Main.myPlayer == Projectile.owner)
            {
                float scaleFactor = _owner.HeldItem.shootSpeed * Projectile.scale;
                Vector2 sourceToMouse = Main.MouseWorld - sourcePosition;
                if (_owner.gravDir == -1f)
                {
                    sourceToMouse.Y = (Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - sourcePosition.Y;
                }
                Vector2 sourceToMouseDirection = Vector2.Normalize(sourceToMouse);
                if (sourceToMouseDirection.HasNaNs())
                {
                    sourceToMouseDirection = -Vector2.UnitY;
                }

                sourceToMouseDirection = Vector2.Normalize(Vector2.Lerp(sourceToMouseDirection,
                    Vector2.Normalize(Projectile.velocity), turnSpeed));
                sourceToMouseDirection *= scaleFactor;

                if (sourceToMouseDirection.X != Projectile.velocity.X
                    || sourceToMouseDirection.Y != Projectile.velocity.Y)
                {
                    Projectile.netUpdate = true;
                }
                Projectile.velocity = sourceToMouseDirection;
            }
        }
    }
}
