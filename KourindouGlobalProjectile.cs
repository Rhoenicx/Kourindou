using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Items.Plushies;
using Terraria.DataStructures;

namespace Kourindou
{
    public class ReimuPlushieTarget 
    {
        public int n {get; set;}
        public float d {get; set;}

        public ReimuPlushieTarget (int n, float d)
        {
            this.n = n;
            this.d = d;
        }
    }

	public class KourindouGlobalProjectile : GlobalProjectile
	{
        internal static int?[] ReimuPlushieHomingTarget = new int?[1024];
        public bool ValidForHoming = false;

        public override bool InstancePerEntity => true;
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (!projectile.CountsAsClass(DamageClass.Default)
                && (source is EntitySource_ItemUse or EntitySource_ItemUse_WithAmmo or EntitySource_Buff or EntitySource_OnHit)
                && projectile.friendly
                && projectile.type != ProjectileID.IceBlock
                && Main.player[projectile.owner].heldProj != projectile.whoAmI
                && !projectile.minion)
            {
                ValidForHoming = true;
            }
        }

        public override void AI(Projectile projectile)
        {
            //Marisa Plushie Effect
            if (projectile.type == ProjectileID.StarWrath
                && Main.player[projectile.owner].GetModPlayer<KourindouPlayer>().EquippedPlushies.Any(kvp => kvp.Key.Type == ItemType<MarisaKirisame_Plushie_Item>()))
            {
                if (projectile.ai[0] >= 1f)
                {
                    if (projectile.ai[1] > 1f)
                    {
                        projectile.hide = false;
                    }

                    projectile.ai[1]++;

                    if (projectile.ai[1] > 300f)
                    {
                        projectile.Kill();
                    }
                }
            }

            // Reimu Plushie effect - Search for a valid target
            if (Main.player[projectile.owner].GetModPlayer<KourindouPlayer>().EquippedPlushies.Any(kvp => kvp.Key.Type == ItemType<ReimuHakurei_Plushie_Item>())
                && projectile.owner == Main.myPlayer
                && ValidForHoming
                && projectile.active
                && ReimuPlushieHomingTarget[projectile.whoAmI] == null
                && (projectile.ModProjectile == null || (projectile.ModProjectile != null && projectile.ModProjectile.ShouldUpdatePosition())))
            {
                List<ReimuPlushieTarget> target = new List<ReimuPlushieTarget>();

                foreach (NPC npc in Main.npc)
                {
                    if (npc.active
                        && !npc.friendly
                        && !npc.immortal
                        && !npc.dontTakeDamage
                        && npc.life > 5
                        && Collision.CanHit(projectile.Center, 1, 1, npc.position, npc.width, npc.height) 
                        && Vector2.Distance(npc.Center, projectile.Center) < Main.player[projectile.owner].GetModPlayer<KourindouPlayer>().ReimuPlushieMaxDistance)
                    {
                        target.Add(new ReimuPlushieTarget(npc.whoAmI, Vector2.Distance(npc.position, projectile.position)));
                    }
                }

                ReimuPlushieTarget nearest = target.Any() ? target[0] : null;
                if (nearest != null)
                {
                    for (int i = 0; i < target.Count; i++)
                    {
                        if (target[i].d < nearest.d)
                        {
                            nearest = target[i];
                        }
                    }

                    if (ReimuPlushieHomingTarget[projectile.whoAmI] != nearest.n)
                    {
                        ReimuPlushieHomingTarget[projectile.whoAmI] = nearest.n;

                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            ModPacket packet = Mod.GetPacket();
                            packet.Write((byte)KourindouMessageType.ReimuPlushieTargets);
                            packet.Write((int)projectile.whoAmI);
                            packet.Write((int)nearest.n);
                            packet.Send(-1, Main.myPlayer);
                        }
                    }
                }
            }

            if (projectile.active 
                && ValidForHoming 
                && ReimuPlushieHomingTarget[projectile.whoAmI] != null
                && (projectile.ModProjectile == null || (projectile.ModProjectile != null && projectile.ModProjectile.ShouldUpdatePosition()))
                && projectile.velocity != Vector2.Zero)
            {
                if (Main.npc[(int)ReimuPlushieHomingTarget[projectile.whoAmI]].active
                    && !Main.npc[(int)ReimuPlushieHomingTarget[projectile.whoAmI]].immortal
                    && !Main.npc[(int)ReimuPlushieHomingTarget[projectile.whoAmI]].dontTakeDamage
                    && Vector2.Distance(Main.npc[(int)ReimuPlushieHomingTarget[projectile.whoAmI]].position, projectile.position) < Main.player[projectile.owner].GetModPlayer<KourindouPlayer>().ReimuPlushieMaxDistance)
                {
                    Vector2 target = Main.npc[(int)ReimuPlushieHomingTarget[projectile.whoAmI]].Center;
                    float distance = Vector2.Distance(projectile.Center, target);
                    float magnitude = distance < Main.player[projectile.owner].GetModPlayer<KourindouPlayer>().ReimuPlushieMaxDistance ? (1f * (1f - distance / Main.player[projectile.owner].GetModPlayer<KourindouPlayer>().ReimuPlushieMaxDistance)) : 0f;
                    Vector2 direction = Vector2.Normalize(Vector2.Lerp(Vector2.Normalize(projectile.velocity), Vector2.Normalize(target - projectile.Center), magnitude));

                    // Normal Projectiles
                    if (projectile.aiStyle != 99 && projectile.aiStyle != 15)
                    {
                        projectile.velocity = direction * projectile.velocity.Length();
                    }

                    // Yoyo's
                    if (projectile.aiStyle == 99 && projectile.ai[0] > 0f)
                    {
                        projectile.velocity = direction * projectile.velocity.Length();
                    }

                    // Flails
                    if (projectile.aiStyle == 15 && projectile.ai[0] == 0f)
                    {
                        projectile.velocity = direction * projectile.velocity.Length();
                    }
                }
                else
                {
                    ReimuPlushieHomingTarget[projectile.whoAmI] = null;
                }
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
        {
            ValidForHoming = false;
        }

        public override void OnHitPvp(Projectile projectile, Player target, int damage, bool crit)
        {
            ValidForHoming = false;
        }

        public override void Kill(Projectile projectile, int timeLeft)
        {
            if (ReimuPlushieHomingTarget[projectile.whoAmI] != null)
            {
                ValidForHoming = false;
                ReimuPlushieHomingTarget[projectile.whoAmI] = null;
            }
        }
    }
}