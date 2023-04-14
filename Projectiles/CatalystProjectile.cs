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
using static Kourindou.KourindouSpellcardSystem;
using Kourindou.Items;
using Kourindou.Items.Catalysts;

namespace Kourindou.Projectiles
{
    public abstract class CatalystProjectile : ModProjectile
    {
        public CastBlock block;
        private bool _justSpawned = true;

        public Player _owner;
        public Item _heldItem;

        protected Vector2 HeldProjectileOffset;
        protected float HeldProjectileRotation = 0f;

        public int LifeTime => (int)Projectile.ai[0];
        public bool FailedToCast => ((int)Projectile.ai[1] & 1) == 1;

        public override bool? CanDamage() => false;

        public override bool ShouldUpdatePosition() => false;

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
                Projectile.timeLeft = LifeTime;
                _owner = Main.player[Projectile.owner];
                _heldItem = _owner.HeldItem;
                _justSpawned = false;
            }

            Projectile.owner = _owner.whoAmI;

            // Kill the projectile if the owner cannot hold it anymore
            if (!_owner.active || _owner.noItems || _owner.CCed || _owner.dead || (Main.myPlayer == _owner.whoAmI && Main.mapFullscreen))
            {
                Projectile.netUpdate = true;
                Projectile.Kill();
                return;
            }

            HandleCasting();

            TurnTowardsCursor();

            // Position item and projectile
            Projectile.position = _owner.RotatedRelativePoint(_owner.MountedCenter) - Projectile.Size / 2f + HeldProjectileOffset.RotatedBy(Projectile.velocity.ToRotation() - MathHelper.ToRadians(HeldProjectileRotation));
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = Projectile.direction;

            _owner.ChangeDir(Projectile.direction);
            _owner.heldProj = Projectile.whoAmI;
            _owner.itemTime = 2;
            _owner.itemAnimation = 2;
            _owner.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);
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
                float speed = Projectile.velocity.Length();

                sourceToMouseDirection = Vector2.Normalize(Vector2.Lerp(sourceToMouseDirection,
                    Vector2.Normalize(Projectile.velocity), turnSpeed));
                sourceToMouseDirection *= scaleFactor;

                if (sourceToMouseDirection.X != Projectile.velocity.X
                    || sourceToMouseDirection.Y != Projectile.velocity.Y)
                {
                    Projectile.netUpdate = true;
                }
                Projectile.velocity = sourceToMouseDirection * speed;
            }
        }

        private void HandleCasting()
        {
            // This code is only allowed to execute on the projectile owner's client
            if (Projectile.owner != Main.myPlayer)
            {
                return;
            }

            // The cast is not allowed to be executed
            if (FailedToCast)
            {
                return;
            }

            // Loop through the cast blocks
            if (block.HasChildren)
            {
                foreach (CastBlock block in block.Children)
                {
                    // If this cast block is disabled => continue
                    if (block.IsDisabled)
                    {
                        continue;
                    }

                    // This block has a timer running
                    if (block.Timer > 0)
                    {
                        block.Timer--;
                    }

                    if (block.Timer <= 0)
                    {
                        // Block has repeats and no delay, fire all at once
                        if (block.RepeatAmount > 0 && block.Delay <= 0)
                        {
                            for (int i = 0; i <= block.RepeatAmount; i++)
                            {
                                HandleCards(block);
                            }

                            block.IsDisabled = true;
                        }

                        // Block has repeats and a delay set
                        else if (block.RepeatAmount > 0 && block.Delay > 0)
                        {
                            HandleCards(block);
                            block.Timer = block.Delay;

                            block.RepeatAmount--;
                        }

                        // No repeat or delay
                        else
                        {
                            HandleCards(block);
                            block.IsDisabled = true;
                        }
                    }
                }
            }
        }

        private void HandleCards(CastBlock block)
        {
            if (_heldItem.ModItem is CatalystItem item)
            {
                ExecuteCards(
                    this.Projectile,
                    block,
                    _owner.RotatedRelativePoint(_owner.MountedCenter),
                    HeldProjectileOffset.RotatedBy(Projectile.velocity.ToRotation()),
                    Projectile.velocity,
                    item.BaseSpread + item.AddedSpread,
                    item.BaseDamageMultiplier + item.AddedDamageMultiplier,
                    item.BaseKnockback + item.AddedKnockback,
                    item.BaseCrit + item.AddedCrit,
                    true
                );
            }
        }
    }
}
