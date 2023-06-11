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
using Terraria.DataStructures;
using Kourindou.Items.Catalysts;

namespace Kourindou.Projectiles
{
    public abstract class SpellCardProjectile : ModProjectile
    {
        // Projectile just spawned in, high only on the start of the first AI tick
        private bool _JustSpawned = true;
        private Vector2 _OldVelocity = Vector2.Zero;

        // Spawn code used in ExecuteCards()
        public Vector2 SpawnOffset = Vector2.Zero;
        public float SpawnSpread;

        // Triggers
        public List<CardItem> TriggerCards = new();
        public int TriggerAmount = 0;

        // Trigger Stats
        public float DamageMultiplier = 1f;
        public float KnockbackMultiplier = 1f;
        public float VelocityMultiplier = 1f;
        public float Spread = 0f;
        public int Crit = 0;

        // Castblock payload
        public List<CastBlock> Payload = new();

        #region Modifiers
        public HashSet<ProjectileStats> NetModifiers = new();

        // [Trajectory: Arc]
        public float _Arc = 0f;
        public float Arc
        {
            get => _Arc;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Arc != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Arc))
                        {
                            NetModifiers.Add(ProjectileStats.Arc);
                        }
                        Projectile.netUpdate = true;
                        _Arc = value;
                    }
                }
                else
                {
                    _Arc = value;
                }
            }
        }

        // [Trajectory: ZigZag]
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

        // [Trajectory: PingPong]
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

        // [Trajectory: Snake]
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

        // [Trajectory: Uncontrolled]
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

        // [Trajectory: Orbit]
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

        // [Trajectory: Boomerang]
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

        // [Trajectory: Spiral]
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

        // [Trajectory: Aiming]
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

        // [Modifier: Shimmer]
        public bool _Shimmer = false;
        public bool Shimmer
        {
            get => _Shimmer;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_Shimmer != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Shimmer))
                        {
                            NetModifiers.Add(ProjectileStats.Shimmer);
                        }
                        Projectile.netUpdate = true;
                        _Shimmer = value;
                    }
                }
                else
                {
                    _Shimmer = value;
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
        public Element Element
        {
            get => (Element)_Element;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if ((Element)_Element != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.Element))
                        {
                            NetModifiers.Add(ProjectileStats.Element);
                        }
                        Projectile.netUpdate = true;
                        _Element = (byte)value;
                    }
                }
                else
                {
                    _Element = (byte)value;
                }
            }
        }

        // Other: Save spawn velocity for acceleration cards
        public Vector2 _SpawnVelocity = Vector2.Zero;

        public Vector2 SpawnVelocity
        { 
            get => _SpawnVelocity;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_SpawnVelocity != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.SpawnVelocity))
                        {
                            NetModifiers.Add(ProjectileStats.SpawnVelocity);
                        }
                        Projectile.netUpdate = true;
                        _SpawnVelocity = value;
                    }
                }
                else
                {
                    _SpawnVelocity = value;
                }
            }
        }

        // Other: Save spawn center
        public Vector2 _SpawnCenter = Vector2.Zero;

        public Vector2 SpawnCenter
        { 
            get => _SpawnCenter;
            set
            {
                if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
                {
                    if (_SpawnCenter != value)
                    {
                        if (!NetModifiers.Contains(ProjectileStats.SpawnCenter))
                        {
                            NetModifiers.Add(ProjectileStats.SpawnCenter);
                        }
                        Projectile.netUpdate = true;
                        _SpawnCenter = value;
                    }
                }
                else
                {
                    _SpawnCenter = value;
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

        public uint ExtraModifierBits
        {
            get => BitConverter.SingleToUInt32Bits(Projectile.ai[1]);
            set => Projectile.ai[1] = BitConverter.UInt32BitsToSingle(value);
        }

        public bool SpawnDirection
        {
            get => ExtraModifierBits == 1;
            set
            {
                ExtraModifierBits = (ExtraModifierBits & 0xFFFFFFFE) | ((uint)(value ? 1 : 0) & 0x00000001);
            }
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
        }

        public override ModProjectile Clone(Terraria.Projectile newEntity)
        {
            TriggerCards = new List<CardItem>();
            return base.Clone(newEntity);
        }

        public override void AI()
        {
            // ----- Spawn ----- //
            if (_JustSpawned)
            {
                _JustSpawned = false;
            }

            // ----- Trajectory ----- //
            #region Trajectory
            List<Vector2> NextVelocity = new();

            // Arc
            if (Arc != 0f)
            {
                NextVelocity.Add(Vector2.Normalize(Projectile.velocity.RotatedBy(MathHelper.ToRadians(Arc * 0.5f))) * Math.Abs(Arc));
            }

            // ZigZag
            if (ZigZag != 0f)
            {
                int Direction = Math.Sign(ZigZag);

                int Flip1 = (int)(120f / Math.Abs(ZigZag));
                int Flip2 = (int)(Flip1 / 2);

                if (Timer % Flip1 == 0 && Timer != 0)
                {
                    NextVelocity.Add(Vector2.Normalize(Projectile.velocity.RotatedBy(MathHelper.ToRadians(+135 * Direction))) * Math.Abs(ZigZag));
                }
                else if (Timer % Flip1 == Flip2 && Timer != 0)
                {
                    NextVelocity.Add(Vector2.Normalize(Projectile.velocity.RotatedBy(MathHelper.ToRadians(-135 * Direction))) * Math.Abs(ZigZag));
                }
            }

            // PingPong
            if (PingPong != 0f)
            {
                int Direction = Math.Sign(PingPong);

                int Flip1 = (int)(120f / Math.Abs(PingPong));
                int Flip2 = (int)(Flip1 / 2);

                int TurnTime = (int)(Flip2 / 4);
                const float TotalAngle = 180f;
                float AnglePerTick = TotalAngle / TurnTime;

                if (Timer % Flip1 >= Flip1 - TurnTime)
                {
                    NextVelocity.Add(Vector2.Normalize(Projectile.velocity.RotatedBy(MathHelper.ToRadians(+AnglePerTick * Direction))) * Math.Abs(PingPong));
                }

                if (Timer % Flip1 >= Flip2 - TurnTime && Timer % Flip1 < Flip2)
                {
                    NextVelocity.Add(Vector2.Normalize(Projectile.velocity.RotatedBy(MathHelper.ToRadians(-AnglePerTick * Direction))) * Math.Abs(PingPong));
                }
            }

            // Snake
            if (Snake != 0f)
            {
                int Direction = Math.Sign(Snake);

                float WaveLength = 180f / Math.Abs(Snake);
                float Amplitude = 2f * Math.Abs(Snake);

                float Angle = (float)Math.Sin((2 * MathHelper.Pi * Timer) / WaveLength) * Amplitude * Direction;

                NextVelocity.Add(Vector2.Normalize(Projectile.velocity.RotatedBy(MathHelper.ToRadians(Angle))) * Math.Abs(Snake));
            }

            // Uncontrolled
            if (Uncontrolled != 0f)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    if (Main.rand.Next(0, (int)(100 / Math.Abs(Uncontrolled))) == 0)
                    {
                        NextVelocity.Add(Vector2.Normalize(Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(0f, 360f)))));
                        Projectile.netUpdate = true;
                    }
                }
            }

            if (NextVelocity.Count > 0)
            {
                Vector2 newVelocity = Vector2.Zero;
                for (int i = 0; i < NextVelocity.Count; i++)
                {
                    newVelocity += NextVelocity[i];
                }

                Projectile.velocity = Vector2.Normalize(newVelocity / NextVelocity.Count) * Projectile.velocity.Length();
            }
            #endregion

            // ----- Triggers ----- //
            #region Trigger
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = TriggerCards.Count - 1; i >= 0; i--)
                {
                    if (TriggerCards[i] == null || TriggerCards[i].Group != (byte)Groups.Trigger)
                    {
                        continue;
                    }

                    switch ((Trigger)TriggerCards[i].Spell)
                    {
                        case Trigger.Timer1:
                        case Trigger.Timer2:
                        case Trigger.Timer3:
                        case Trigger.Timer4:
                        case Trigger.Timer5:
                            {
                                // Trigger Condition
                                if (Timer >= GetFlooredValue(TriggerCards[i].GetValue(), 1))
                                {
                                    // Execute the payload blocks
                                    ActivateTrigger(i);

                                    // Remove this Trigger card and retain the index positions
                                    // Setting it to null retains the positions of the cards
                                    TriggerCards[i] = null;
                                }
                            }
                            break;
                    }
                }

                ExecutePayload();
            }
            #endregion

            // ----- Modifiers ----- //

            // ----- End ----- //

            // Rotation of the projectile
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Increase Timer
            Timer++;

            base.AI();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // When the projectile is spawned they can freely bounce on a tile
            if (_JustSpawned || Timer <= 1)
            {
                if (Projectile.velocity.X != oldVelocity.X)
                {
                    Projectile.velocity.X = -oldVelocity.X;
                }

                if (Projectile.velocity.Y != oldVelocity.Y)
                {
                    Projectile.velocity.Y = -oldVelocity.Y;
                }

                return false;
            }

            // Bounce modifier
            if (Bounce > 0)
            {
                if (Projectile.velocity.X != oldVelocity.X)
                {
                    Projectile.velocity.X = -oldVelocity.X;
                }

                if (Projectile.velocity.Y != oldVelocity.Y)
                {
                    Projectile.velocity.Y = -oldVelocity.Y;
                }

                Bounce--;

                return false;
            }

            _OldVelocity = oldVelocity;

            return base.OnTileCollide(oldVelocity);
        }

        public override void Kill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                // Revert timeleft
                LifeTime = timeLeft;
                Projectile.velocity = _OldVelocity;

                // Execute remaining triggers when the projectile is killed
                for (int i = 0; i < Payload.Count; i++)
                {
                    if (Payload[i] == null)
                    {
                        continue;
                    }

                    if (Payload[i].IsDisabled)
                    {
                        continue;
                    }

                    Payload[i].IsDisabled = true;

                    for (int j = 0; j <= Payload[i].RepeatAmount; j++)
                    {
                        HandleCards(Payload[i]);
                    }
                }
            }

            base.Kill(timeLeft);
        }

        private void ActivateTrigger(int ID)
        {
            for (int i = 0; i < Payload.Count; i++)
            {
                if (Payload[i] == null)
                {
                    continue;
                }

                if (Payload[i].TriggerID == ID)
                {
                    Payload[i].TriggerActivated = true;
                }
            }
        }

        private void ExecutePayload()
        {
            for (int i = 0; i < Payload.Count; i++)
            {
                if (Payload[i] == null)
                {
                    continue;
                }

                CastBlock block = Payload[i];

                // If this cast block is disabled => continue
                if (block == null || block.IsDisabled || !block.TriggerActivated)
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
                        block.IsDisabled = true;

                        for (int j = 0; j <= block.RepeatAmount; j++)
                        {
                            HandleCards(block);
                        }
                    }

                    // Block has repeats and a delay set
                    else if (block.RepeatAmount > 0 && block.Delay > 0)
                    {
                        block.Timer = block.Delay;
                        block.RepeatAmount--;

                        HandleCards(block);
                    }

                    // No repeat or delay
                    else
                    {
                        block.IsDisabled = true;

                        HandleCards(block);
                    }
                }
            }
        }

        private void HandleCards(CastBlock block)
        {
            ExecuteCards(
                this.Projectile,
                block,
                Projectile.Center,
                Vector2.Zero,
                Vector2.Normalize(Projectile.velocity),
                DamageMultiplier,
                KnockbackMultiplier,
                VelocityMultiplier,
                Spread,
                Crit
            );
        }

        public SpellCardProjectile Clone()
        {
            // Spawn the new projectile
            int newID = SpawnSpellCardProjectile(
                Projectile.GetSource_FromAI(),
                Projectile.type,
                Projectile.position,
                Projectile.velocity,
                1f,
                1f,
                1f,
                0,
                Main.myPlayer,
                Projectile.ai[0],
                Projectile.ai[1]);

            // Apply vanilla stats
            Terraria.Projectile proj = Main.projectile[newID];

            proj.position = Projectile.position;
            proj.velocity = Projectile.velocity;
            proj.type = Projectile.type;
            proj.damage = Projectile.damage;
            proj.knockBack = Projectile.knockBack;
            proj.CritChance = Projectile.CritChance;
            proj.timeLeft = Projectile.timeLeft;
            proj.penetrate = Projectile.penetrate;
            proj.ArmorPenetration = Projectile.ArmorPenetration;
            proj.tileCollide = Projectile.tileCollide;
            proj.scale = Projectile.scale;
            proj.hostile = Projectile.hostile;
            proj.friendly = Projectile.friendly;
            proj.light = Projectile.light;

            // Apply custom stats
            if (proj.ModProjectile is SpellCardProjectile SPproj)
            {
                SPproj.SpawnOffset = SpawnOffset;
                SPproj.SpawnSpread = SpawnSpread;
                SPproj.Arc = Arc;
                SPproj.ZigZag = ZigZag;
                SPproj.PingPong = PingPong;
                SPproj.Snake = Snake;
                SPproj.Uncontrolled = Uncontrolled;
                SPproj.Orbit = Orbit;
                SPproj.Spiral = Spiral;
                SPproj.Boomerang = Boomerang;
                SPproj.Aiming = Aiming;
                SPproj.Acceleration = Acceleration;
                SPproj.AccelerationMultiplier = AccelerationMultiplier;
                SPproj.Bounce = Bounce;
                SPproj.Gravity = Gravity;
                SPproj.Homing = Homing;
                SPproj.RotateToEnemy = RotateToEnemy;
                SPproj.Shimmer = Shimmer;
                SPproj.Snowball = Snowball;
                SPproj.EdgeType = EdgeType;
                SPproj.Shatter = Shatter;
                SPproj.Eater = Eater;
                SPproj.Forcefield = Forcefield;
                SPproj.Explosion = Explosion;
                SPproj.Element = Element;

                // Payload cards
                for (int i = 0; i < Payload.Count; i++)
                {
                    SPproj.Payload.Add(Payload[i].Clone());
                }

                // Trigger Cards
                for (int i = 0; i < TriggerCards.Count; i++)
                {
                    SPproj.TriggerCards.Add(TriggerCards[i]);
                }

                // Trigger amount
                SPproj.TriggerAmount = TriggerAmount;

                return SPproj;
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
                    case ProjectileStats.Arc: { writer.Write(Arc); } break;
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
                    case ProjectileStats.Shimmer: { writer.Write(Shimmer); } break;
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
                    case ProjectileStats.Element: { writer.Write((byte)Element); } break;
                    case ProjectileStats.SpawnVelocity: { writer.Write(SpawnVelocity.X); writer.Write(SpawnVelocity.Y); } break;
                    case ProjectileStats.SpawnCenter: { writer.Write(SpawnCenter.X); writer.Write(SpawnCenter.Y); } break;
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
                    case ProjectileStats.Arc: { Arc = reader.ReadSingle(); } break;
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
                    case ProjectileStats.Shimmer: { Shimmer = reader.ReadBoolean(); } break;
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
                    case ProjectileStats.Element: { Element = (Element)reader.ReadByte(); } break;
                    case ProjectileStats.SpawnVelocity: { SpawnVelocity = new Vector2(reader.ReadSingle(), reader.ReadSingle()); } break;
                    case ProjectileStats.SpawnCenter: { SpawnCenter = new Vector2(reader.ReadSingle(), reader.ReadSingle()); } break;
                }
            }
        }
    }
}
