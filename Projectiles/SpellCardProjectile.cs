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
using Kourindou.Items.Spellcards;
using Kourindou.Items.Spellcards.Trajectories;

namespace Kourindou.Projectiles
{
    public abstract class SpellCardProjectile : ModProjectile
    {
        // Saved Stats - these ones get overwritten right after the first SetDefaults.
        private float BaseSpeed;
        private float BaseKnockback;
        private int BaseDamage;
        private int BaseCritChance;

        // Spawn
        private bool _JustSpawned = true;
        public Vector2 SpawnOffset = Vector2.Zero;
        public float SpawnSpread;

        // Triggers
        public List<CardItem> TriggerCards = new();
        public int TriggerAmount = 0;

        // Castblock payload
        public List<CastBlock> Payload = new();

        #region Modifiers
        public HashSet<ProjectileStats> NetModifiers = new();

        // [Formation: ArcLeft]
        public float _ArcLeft = 0f;
        public float ArcLeft
        {
            get => _ArcLeft;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_ArcLeft != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.ArcLeft))
                        {
                            NetModifiers.Add(ProjectileStats.ArcLeft);
                        }
                        Projectile.netUpdate = true;
                        _ArcLeft = value;
                    }
                }
                else
                {
                    _ArcLeft = value;
                }
            }
        }

        // [Formation: ArcRight]
        public float _ArcRight = 0f;
        public float ArcRight
        {
            get => _ArcRight;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_ArcRight != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.ArcRight))
                        {
                            NetModifiers.Add(ProjectileStats.ArcRight);
                        }
                        Projectile.netUpdate = true;
                        _ArcRight = value;
                    }
                }
                else
                {
                    _ArcRight = value;
                }
            }
        }

        // [Formation: ZigZag]
        public float _ZigZag = 0f;
        public float ZigZag
        {
            get => _ZigZag;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_ZigZag != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.ZigZag))
                        {
                            NetModifiers.Add(ProjectileStats.ZigZag);
                        }
                        Projectile.netUpdate = true;
                        _ZigZag = value;
                    }
                }
                else
                {
                    _ZigZag = value;
                }
            }
        }

        // [Formation: PingPong]
        public float _PingPong = 0f;
        public float PingPong
        {
            get => _PingPong;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_PingPong != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.PingPong))
                        {
                            NetModifiers.Add(ProjectileStats.PingPong);
                        }
                        Projectile.netUpdate = true;
                        _PingPong = value;
                    }
                }
                else
                {
                    _PingPong = value;
                }
            }
        }

        // [Formation: Snake]
        public float _Snake = 0f;
        public float Snake
        {
            get => _Snake;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Snake != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Snake))
                        {
                            NetModifiers.Add(ProjectileStats.Snake);
                        }
                        Projectile.netUpdate = true;
                        _Snake = value;
                    }
                }
                else
                {
                    _Snake = value;
                }
            }
        }

        // [Formation: Uncontrolled]
        public float _Uncontrolled = 0f;
        public float Uncontrolled
        {
            get => _Uncontrolled;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Uncontrolled != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Uncontrolled))
                        {
                            NetModifiers.Add(ProjectileStats.Uncontrolled);
                        }
                        Projectile.netUpdate = true;
                        _Uncontrolled = value;
                    }
                }
                else
                {
                    _Uncontrolled = value;
                }
            }
        }

        // [Formation: Orbit]
        public float _Orbit = 0f;
        public float Orbit
        {
            get => _Orbit;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Orbit != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Orbit))
                        {
                            NetModifiers.Add(ProjectileStats.Orbit);
                        }
                        Projectile.netUpdate = true;
                        _Orbit = value;
                    }
                }
                else
                {
                    _Orbit = value;
                }
            }
        }

        // [Formation: Boomerang]
        public float _Boomerang = 0f;
        public float Boomerang
        {
            get => _Boomerang;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Boomerang != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Boomerang))
                        {
                            NetModifiers.Add(ProjectileStats.Boomerang);
                        }
                        Projectile.netUpdate = true;
                        _Boomerang = value;
                    }
                }
                else
                {
                    _Boomerang = value;
                }
            }
        }

        // [Formation: Spiral]
        public float _Spiral = 0f;
        public float Spiral
        {
            get => _Spiral;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Spiral != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Spiral))
                        {
                            NetModifiers.Add(ProjectileStats.Spiral);
                        }
                        Projectile.netUpdate = true;
                        _Spiral = value;
                    }
                }
                else
                {
                    _Spiral = value;
                }
            }
        }

        // [Formation: Aiming]
        public float _Aiming = 0f;
        public float Aiming
        {
            get => _Aiming;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Aiming != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Aiming))
                        {
                            NetModifiers.Add(ProjectileStats.Aiming);
                        }
                        Projectile.netUpdate = true;
                        _Aiming = value;
                    }
                }
                else
                {
                    _Aiming = value;
                }
            }
        }

        // [Modifier: Acceleration]
        public float _Acceleration = 1f;
        public float Acceleration
        {
            get => _Acceleration;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Acceleration != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Acceleration))
                        {
                            NetModifiers.Add(ProjectileStats.Acceleration);
                        }
                        Projectile.netUpdate = true;
                        _Acceleration = value;
                    }
                }
                else
                {
                    _Acceleration = value;
                }
            }
        }

        // [Modifier: AccelerationMultiplier]
        public float _AccelerationMultiplier = 1f;
        public float AccelerationMultiplier
        {
            get => _AccelerationMultiplier;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_AccelerationMultiplier != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.AccelerationMultiplier))
                        {
                            NetModifiers.Add(ProjectileStats.AccelerationMultiplier);
                        }
                        Projectile.netUpdate = true;
                        _AccelerationMultiplier = value;
                    }
                }
                else
                {
                    _AccelerationMultiplier = value;
                }
            }
        }

        // [Modifier: Bounce]
        public short _Bounce = 0;
        public short Bounce
        {
            get => _Bounce;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Bounce != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Bounce))
                        {
                            NetModifiers.Add(ProjectileStats.Bounce);
                        }
                        Projectile.netUpdate = true;
                        _Bounce = value;
                    }
                }
                else
                {
                    _Bounce = value;
                }
            }
        }

        // [Modifier: Penetrate]
        public int Penetrate
        {
            get => Projectile.penetrate;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (Projectile.penetrate != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Penetrate))
                        {
                            NetModifiers.Add(ProjectileStats.Penetrate);
                        }
                        Projectile.netUpdate = true;
                        Projectile.penetrate = value;
                    }
                }
                else
                {
                    Projectile.penetrate = value;
                }
            }
        }

        // [Modifier: Gravity]
        public float _Gravity = 0f;
        public float Gravity
        {
            get => _Gravity;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Gravity != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Gravity))
                        {
                            NetModifiers.Add(ProjectileStats.Gravity);
                        }
                        Projectile.netUpdate = true;
                        _Gravity = value;
                    }
                }
                else
                {
                    _Gravity = value;
                }
            }
        }

        // [Modifier: Homing]
        public float _Homing = 0f;
        public float Homing
        {
            get => _Homing;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Homing != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Homing))
                        {
                            NetModifiers.Add(ProjectileStats.Homing);
                        }
                        Projectile.netUpdate = true;
                        _Homing = value;
                    }
                }
                else
                {
                    _Homing = value;
                }
            }
        }

        // [Modifier: RotateToEnemy]
        public float _RotateToEnemy = 0f;
        public float RotateToEnemy
        {
            get => _RotateToEnemy;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_RotateToEnemy != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.RotateToEnemy))
                        {
                            NetModifiers.Add(ProjectileStats.RotateToEnemy);
                        }
                        Projectile.netUpdate = true;
                        _RotateToEnemy = value;
                    }
                }
                else
                {
                    _RotateToEnemy = value;
                }
            }
        }

        // [Modifier: Collide]
        public bool Collide
        {
            get => Projectile.tileCollide;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (Projectile.tileCollide != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Collide))
                        {
                            NetModifiers.Add(ProjectileStats.Collide);
                        }
                        Projectile.netUpdate = true;
                        Projectile.tileCollide = value;
                    }
                }
                else
                {
                    Projectile.tileCollide = value;
                }
            }
        }

        // [Modifier: Phasing]
        public bool _Phasing = false;
        public bool Phasing
        {
            get => _Phasing;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Phasing != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Phasing))
                        {
                            NetModifiers.Add(ProjectileStats.Phasing);
                        }
                        Projectile.netUpdate = true;
                        _Phasing = value;
                    }
                }
                else
                {
                    _Phasing = value;
                }
            }
        }

        // [Modifier: LifeTime]
        public int LifeTime
        {
            get => Projectile.timeLeft;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (Projectile.timeLeft != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.LifeTime))
                        {
                            NetModifiers.Add(ProjectileStats.LifeTime);
                        }
                        Projectile.netUpdate = true;
                        Projectile.timeLeft = value;
                    }
                }
                else
                {
                    Projectile.timeLeft = value;
                }
            }
        }

        // [Modifier: Snowball]
        public float _Snowball = 0f;
        public float Snowball
        {
            get => _Snowball;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Snowball != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Snowball))
                        {
                            NetModifiers.Add(ProjectileStats.Snowball);
                        }
                        Projectile.netUpdate = true;
                        _Snowball = value;
                    }
                }
                else
                {
                    _Snowball = value;
                }
            }
        }

        // [Modifier: Scale]
        public float Scale
        {
            get => Projectile.scale;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (Projectile.scale != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Scale))
                        {
                            NetModifiers.Add(ProjectileStats.Scale);
                        }
                        Projectile.netUpdate = true;
                        Projectile.scale = value;
                    }
                }
                else
                {
                    Projectile.scale = value;
                }
            }
        }

        // [Modifier: EdgeType]
        public byte _EdgeType = 0;
        public byte EdgeType
        {
            get => _EdgeType;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_EdgeType != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.EdgeType))
                        {
                            NetModifiers.Add(ProjectileStats.EdgeType);
                        }
                        Projectile.netUpdate = true;
                        _EdgeType = value;
                    }
                }
                else
                {
                    _EdgeType = value;
                }
            }
        }

        // [Modifier: Shatter]
        public float _Shatter = 0f;
        public float Shatter
        {
            get => _Shatter;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Shatter != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Shatter))
                        {
                            NetModifiers.Add(ProjectileStats.Shatter);
                        }
                        Projectile.netUpdate = true;
                        _Shatter = value;
                    }
                }
                else
                {
                    _Shatter = value;
                }
            }
        }

        // [Modifier: Eater]
        public float _Eater = 0f;
        public float Eater
        {
            get => _Eater;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Eater != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Eater))
                        {
                            NetModifiers.Add(ProjectileStats.Eater);
                        }
                        Projectile.netUpdate = true;
                        _Eater = value;
                    }
                }
                else
                {
                    _Eater = value;
                }
            }
        }

        // [Modifier: Forcefield]
        public float _Forcefield = 0f;
        public float Forcefield
        {
            get => _Forcefield;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Forcefield != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Forcefield))
                        {
                            NetModifiers.Add(ProjectileStats.Forcefield);
                        }
                        Projectile.netUpdate = true;
                        _Forcefield = value;
                    }
                }
                else
                {
                    _Forcefield = value;
                }
            }
        }

        // [Modifier: Explosion]
        public float _Explosion = 0f;
        public float Explosion
        {
            get => _Explosion;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Explosion != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Explosion))
                        {
                            NetModifiers.Add(ProjectileStats.Explosion);
                        }
                        Projectile.netUpdate = true;
                        _Explosion = value;
                    }
                }
                else
                {
                    _Explosion = value;
                }
            }
        }

        // [Modifier: Hostile]
        public bool Hostile
        {
            get => Projectile.hostile;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (Projectile.hostile != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Hostile))
                        {
                            NetModifiers.Add(ProjectileStats.Hostile);
                        }
                        Projectile.netUpdate = true;
                        Projectile.hostile = value;
                    }
                }
                else
                {
                    Projectile.hostile = value;
                }
            }
        }

        // [Modifier: Friendly]
        public bool Friendly
        {
            get => Projectile.friendly;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (Projectile.friendly != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Friendly))
                        {
                            NetModifiers.Add(ProjectileStats.Friendly);
                        }
                        Projectile.netUpdate = true;
                        Projectile.friendly = value;
                    }
                }
                else
                {
                    Projectile.friendly = value;
                }
            }
        }

        // [Modifier: Light]
        public float Light
        {
            get => Projectile.light;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (Projectile.light != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Light))
                        {
                            NetModifiers.Add(ProjectileStats.Light);
                        }
                        Projectile.netUpdate = true;
                        Projectile.light = value;
                    }
                }
                else
                {
                    Projectile.light = value;
                }
            }
        }

        // [Modifier: CritChance]
        public int CritChance
        {
            get => Projectile.CritChance;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (Projectile.CritChance != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.CritChance))
                        {
                            NetModifiers.Add(ProjectileStats.CritChance);
                        }
                        Projectile.netUpdate = true;
                        Projectile.CritChance = value;
                    }
                }
                else
                {
                    Projectile.CritChance = value;
                }
            }
        }

        // [Modifier: ArmorPenetration]
        public int ArmorPenetration
        {
            get => Projectile.ArmorPenetration;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (Projectile.ArmorPenetration != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.ArmorPenetration))
                        {
                            NetModifiers.Add(ProjectileStats.ArmorPenetration);
                        }
                        Projectile.netUpdate = true;
                        Projectile.ArmorPenetration = value;
                    }
                }
                else
                {
                    Projectile.ArmorPenetration = value;
                }
            }
        }

        // [Modifier: Element]
        public byte _Element = 0;
        public byte Element
        {
            get => _Element;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Element != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Element))
                        {
                            NetModifiers.Add(ProjectileStats.Element);
                        }
                        Projectile.netUpdate = true;
                        _Element = value;
                    }
                }
                else
                {
                    _Element = value;
                }
            }
        }
        #endregion

        #region AI_Fields
        public int Timer
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        #endregion

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

        public abstract void SetProjectileDefaults();

        public override void SetDefaults()
        {
            SetProjectileDefaults();

            BaseSpeed = Projectile.velocity.Length();
            BaseKnockback = Projectile.knockBack;
            BaseDamage = Projectile.damage;
            BaseCritChance = Projectile.CritChance;
        }

        public void RevertStats()
        {
            Projectile.damage = BaseDamage;
            Projectile.knockBack = BaseKnockback;
            Projectile.CritChance = BaseCritChance;
            Projectile.velocity = Vector2.Normalize(Projectile.velocity) * BaseSpeed;
        }

        public override void AI()
        {
            if (_JustSpawned)
            {
                _JustSpawned = false;
            }

            // Rotation of the projectile
            Projectile.rotation = Projectile.velocity.ToRotation();

            base.AI();
        }

        public SpellCardProjectile Clone()
        {
            int newID = Terraria.Projectile.NewProjectile(
                Projectile.GetSource_FromAI(),
                Projectile.position,
                Projectile.velocity,
                Projectile.type,
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner,
                Projectile.ai[0],
                Projectile.ai[1]);

            if (Main.projectile[newID].ModProjectile is SpellCardProjectile proj)
            {
                proj.SpawnOffset = SpawnOffset;
                proj.SpawnSpread = SpawnSpread;

                proj.Projectile.position = Projectile.position;
                proj.Projectile.velocity = Projectile.velocity;
                proj.Projectile.type = Projectile.type;
                proj.Projectile.damage = Projectile.damage;
                proj.Projectile.knockBack = Projectile.knockBack;
                proj.Projectile.CritChance = Projectile.CritChance;
                proj.Projectile.timeLeft = Projectile.timeLeft;
                proj.Projectile.penetrate = Projectile.penetrate;
                proj.Projectile.ArmorPenetration = Projectile.ArmorPenetration;
                proj.Projectile.tileCollide = Projectile.tileCollide;
                proj.Projectile.scale = Projectile.scale;
                proj.Projectile.hostile = Projectile.hostile;
                proj.Projectile.friendly = Projectile.friendly;
                proj.Projectile.light = Projectile.light;

                proj.ArcLeft = ArcLeft;
                proj.ArcRight = ArcRight;
                proj.ZigZag = ZigZag;
                proj.PingPong = PingPong;
                proj.Snake = Snake;
                proj.Uncontrolled = Uncontrolled;
                proj.Orbit = Orbit;
                proj.Spiral = Spiral;
                proj.Boomerang = Boomerang;
                proj.Aiming = Aiming;
                proj.Acceleration = Acceleration;
                proj.AccelerationMultiplier = AccelerationMultiplier;
                proj.Bounce = Bounce;
                proj.Gravity = Gravity;
                proj.Homing = Homing;
                proj.RotateToEnemy = RotateToEnemy;
                proj.Phasing = Phasing;
                proj.Snowball = Snowball;
                proj.EdgeType = EdgeType;
                proj.Shatter = Shatter;
                proj.Eater = Eater;
                proj.Forcefield = Forcefield;
                proj.Explosion = Explosion;
                proj.Element = Element;

                return proj;
            }

            return null;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            // Send Modifiers to other parties
            writer.Write(NetModifiers.Count);
            foreach (ProjectileStats modifier in NetModifiers)
            {
                writer.Write((byte)modifier);
                switch (modifier)
                {
                    case ProjectileStats.ArcLeft: { writer.Write(ArcLeft); } break;
                    case ProjectileStats.ArcRight: { writer.Write(ArcRight); } break;
                    case ProjectileStats.ZigZag: { writer.Write(ZigZag); } break;
                    case ProjectileStats.PingPong: { writer.Write(PingPong); } break;
                    case ProjectileStats.Snake: { writer.Write(Snake); } break;
                    case ProjectileStats.Uncontrolled: { writer.Write(Uncontrolled); } break;
                    case ProjectileStats.Orbit: { writer.Write(Orbit); } break;
                    case ProjectileStats.Spiral: { writer.Write(Spiral); } break;
                    case ProjectileStats.Boomerang: { writer.Write(Boomerang); } break;
                    case ProjectileStats.Aiming: { writer.Write(Aiming); } break;
                    case ProjectileStats.Acceleration: { writer.Write(Acceleration); } break;
                    case ProjectileStats.AccelerationMultiplier: { writer.Write(AccelerationMultiplier); } break;
                    case ProjectileStats.Bounce: { writer.Write(Bounce); } break;
                    case ProjectileStats.Penetrate: { writer.Write(Penetrate); } break;
                    case ProjectileStats.Gravity: { writer.Write(Gravity); } break;
                    case ProjectileStats.Homing: { writer.Write(Homing); } break;
                    case ProjectileStats.RotateToEnemy: { writer.Write(RotateToEnemy); } break;
                    case ProjectileStats.Collide: { writer.Write(Collide); } break;
                    case ProjectileStats.Phasing: { writer.Write(Phasing); } break;
                    case ProjectileStats.LifeTime: { writer.Write(LifeTime); } break;
                    case ProjectileStats.Snowball: { writer.Write(Snowball); } break;
                    case ProjectileStats.Scale: { writer.Write(Scale); } break;
                    case ProjectileStats.EdgeType: { writer.Write(EdgeType); } break;
                    case ProjectileStats.Shatter: { writer.Write(Shatter); } break;
                    case ProjectileStats.Eater: { writer.Write(Eater); } break;
                    case ProjectileStats.Forcefield: { writer.Write(Forcefield); } break;
                    case ProjectileStats.Explosion: { writer.Write(Explosion); } break;
                    case ProjectileStats.Hostile: { writer.Write(Hostile); } break;
                    case ProjectileStats.Friendly: { writer.Write(Friendly); } break;
                    case ProjectileStats.Light: { writer.Write(Light); } break;
                    case ProjectileStats.CritChance: { writer.Write(CritChance); } break;
                    case ProjectileStats.ArmorPenetration: { writer.Write(ArmorPenetration); } break;
                    case ProjectileStats.Element: { writer.Write(Element); } break;
                }
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            // Receive Modifiers from projectile owner
            int Count = reader.ReadInt32();
            for (int i = 0; i < Count; i++)
            {
                switch ((ProjectileStats)reader.ReadByte())
                {
                    case ProjectileStats.ArcLeft: { ArcLeft = reader.ReadSingle(); } break;
                    case ProjectileStats.ArcRight: { ArcRight = reader.ReadSingle(); } break;
                    case ProjectileStats.ZigZag: { ZigZag = reader.ReadSingle(); } break;
                    case ProjectileStats.PingPong: { PingPong = reader.ReadSingle(); } break;
                    case ProjectileStats.Snake: { Snake = reader.ReadSingle(); } break;
                    case ProjectileStats.Uncontrolled: { Uncontrolled = reader.ReadSingle(); } break;
                    case ProjectileStats.Orbit: { Orbit = reader.ReadSingle(); } break;
                    case ProjectileStats.Spiral: { Spiral = reader.ReadSingle(); } break;
                    case ProjectileStats.Boomerang: { Boomerang = reader.ReadSingle(); } break;
                    case ProjectileStats.Aiming: { Aiming = reader.ReadSingle(); } break;
                    case ProjectileStats.Acceleration: { Acceleration = reader.ReadSingle(); } break;
                    case ProjectileStats.AccelerationMultiplier: { AccelerationMultiplier = reader.ReadSingle(); } break;
                    case ProjectileStats.Bounce: { Bounce = reader.ReadInt16(); } break;
                    case ProjectileStats.Penetrate: { Penetrate = reader.ReadInt32(); } break;
                    case ProjectileStats.Gravity: { Gravity = reader.ReadSingle(); } break;
                    case ProjectileStats.Homing: { Homing = reader.ReadSingle(); } break;
                    case ProjectileStats.RotateToEnemy: { RotateToEnemy = reader.ReadSingle(); } break;
                    case ProjectileStats.Collide: { Collide = reader.ReadBoolean(); } break;
                    case ProjectileStats.Phasing: { Phasing = reader.ReadBoolean(); } break;
                    case ProjectileStats.LifeTime: { LifeTime = reader.ReadInt32(); } break;
                    case ProjectileStats.Snowball: { Snowball = reader.ReadSingle(); } break;
                    case ProjectileStats.Scale: { Scale = reader.ReadSingle(); } break;
                    case ProjectileStats.EdgeType: { EdgeType = reader.ReadByte(); } break;
                    case ProjectileStats.Shatter: { Shatter = reader.ReadSingle(); } break;
                    case ProjectileStats.Eater: { Eater = reader.ReadSingle(); } break;
                    case ProjectileStats.Forcefield: { Forcefield = reader.ReadSingle(); } break;
                    case ProjectileStats.Explosion: { Explosion = reader.ReadSingle(); } break;
                    case ProjectileStats.Hostile: { Hostile = reader.ReadBoolean(); } break;
                    case ProjectileStats.Friendly: { Friendly = reader.ReadBoolean(); } break;
                    case ProjectileStats.Light: { Light = reader.ReadSingle(); } break;
                    case ProjectileStats.CritChance: { CritChance = reader.ReadInt32(); } break;
                    case ProjectileStats.ArmorPenetration: { ArmorPenetration = reader.ReadInt32(); } break;
                    case ProjectileStats.Element: { Element = reader.ReadByte(); } break;
                }
            }
        }
    }
}
