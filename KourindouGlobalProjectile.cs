using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Items.Plushies;

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

        public override void PostAI(Projectile projectile)
        {
            //if this projectile is owner by my player and active
            if (projectile.owner == Main.myPlayer && projectile.active)
            {
                //Check if my player has ReimuPlushie equipped
                if (Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushieEquipSlot.Item.type == ItemType<ReimuHakurei_Plushie_Item>())
                {
                    if ((projectile.magic || projectile.melee || projectile.ranged || projectile.thrown)
                        && projectile.type != ProjectileID.IceBlock
                        )
                    {
                        //  Create list with valid targets
                        List<ReimuPlushieTarget> target = new List<ReimuPlushieTarget>();

                        // search for all the valif targets and save their id and distance
                        foreach (NPC npc in Main.npc)
                        {
                            if (!npc.friendly && npc.active && Collision.CanHit(projectile.Center, 1, 1, npc.position, npc.width, npc.height) && Vector2.Distance(npc.Center, projectile.Center) < 500f)
                            {
                                target.Add(new ReimuPlushieTarget(npc.whoAmI, Vector2.Distance(npc.position, projectile.position)));
                            }
                        }

                        // Find nearest target in the valid target list
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

                            if (KourindouGlobalProjectile.ReimuPlushieHomingTarget[projectile.whoAmI] != nearest.n)
                            {
                                KourindouGlobalProjectile.ReimuPlushieHomingTarget[projectile.whoAmI] = nearest.n;

                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                {
                                    ModPacket packet = mod.GetPacket();
                                    packet.Write((byte)KourindouMessageType.ReimuPlushieTargets);
                                    packet.Write((int)projectile.whoAmI);
                                    packet.Write((int)nearest.n);
                                    packet.Send();
                                }
                            }
                        }
                    } 
                }
            }

            //Check if this projectile is affected by a Reimu Plushie Homing Effect
            if (KourindouGlobalProjectile.ReimuPlushieHomingTarget[projectile.whoAmI] != null)
            {
                if (Main.npc[(int)KourindouGlobalProjectile.ReimuPlushieHomingTarget[projectile.whoAmI]].active)
                {
                    Vector2 target = Main.npc[(int)KourindouGlobalProjectile.ReimuPlushieHomingTarget[projectile.whoAmI]].Center;
                    float distance = Vector2.Distance(projectile.Center, target);
                    float magnitude = distance < 500f ? (1f * (1f - distance / 500f)) : 0f;
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

                    //flails
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

            if (!projectile.active)
            {
                ReimuPlushieHomingTarget[projectile.whoAmI] = null;
            }
        }

        public override void Kill(Projectile projectile, int timeLeft)
        {
            ReimuPlushieHomingTarget[projectile.whoAmI] = null;
        }
    }
}