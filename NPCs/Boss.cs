using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Kourindou.NPCs
{
    public abstract class Boss : ModNPC
    {
        #region States
        // Split up the first AI field into 4 'state' bytes
        protected byte Stage
        {
            get => (byte)(BitConverter.SingleToUInt32Bits(NPC.ai[0]));
            set
            {
                NPC.ai[0] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[0]) & 0xFFFFFF00) + (uint)(value));
                RunSynchronize();
            }
        }

        protected byte State
        {
            get => (byte)(BitConverter.SingleToUInt32Bits(NPC.ai[0]) >> 8);
            set
            {
                NPC.ai[0] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[0]) & 0xFFFF00FF) + (uint)(value << 8));
                RunSynchronize();
            }
        }

        protected byte MoveIndex
        {
            get => (byte)(BitConverter.SingleToUInt32Bits(NPC.ai[0]) >> 16);
            set
            {
                NPC.ai[0] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[0]) & 0xFF00FFFF) + (uint)(value << 16));
                RunSynchronize();
            }
        }

        protected byte AttackIndex
        {
            get => (byte)(BitConverter.SingleToUInt32Bits(NPC.ai[0]) >> 24);
            set
            {
                NPC.ai[0] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[0]) & 0x00FFFFFF) + (uint)(value << 24));
                RunSynchronize();
            }
        }
        #endregion

        #region Timers
        // Split up the second AI field into 2 'timer' shorts
        protected short AttackTimer
        {
            get => (short)(BitConverter.SingleToUInt32Bits(NPC.ai[1]));
            set
            {
                NPC.ai[1] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[0]) & 0xFFFF0000) + (uint)(value));
                RunSynchronize();
            }
        }

        protected short MoveTimer
        {
            get => (short)(BitConverter.SingleToUInt32Bits(NPC.ai[1]) >> 16);
            set
            {
                NPC.ai[1] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[0]) & 0x0000FFFF) + (uint)(value << 16));
                RunSynchronize();
            }
        }
        #endregion

        // Targeting logic
        protected readonly HashSet<int> Targets = new HashSet<int>();
        protected bool BossTriggered = false;
        protected float TriggerDistance = 200f;
        protected float TargetSearchDistance = 2500f;
        protected float MaximumTargetDistance = 5000f;

        #region Targeting

        // Add a target to the target list
        // Triggered: When a player damages the boss or called
        protected void AddTarget(int ID)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }
            
            if (!Targets.Contains(ID))
            {
                Targets.Add(ID);
            }
        }

        // Check if a target is still alive / logged in
        protected void UpdateAvailableTargets()
        {
            foreach (int i in Targets)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead || NPC.Distance(player.Center) > MaximumTargetDistance)
                {
                    Targets.Remove(i);
                }
            }
        }
        
        protected void SearchForTargets()
        {
            foreach (Player player in Main.player)
            {
                if (player.active && !player.dead && NPC.Distance(player.Center) < TargetSearchDistance)
                {
                    AddTarget(player.whoAmI);
                }
            }
        }

        protected bool GetTarget()
        {
            // In multiplayer target logic should only be executed serverside
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }
            
            // Just in case update the target list to remove invalid targets
            UpdateAvailableTargets()
            
            // Check if the current targeted player is still available in the Target list
            // For example the player died, logged out or is no longer in range
            if (!Targets.Contains(NPC.target))
            {
                // If the current target is no longer available, search the area for more targets close by
                SearchForTargets();
                
                // Run vanilla targeting logic to get the closest target
                NPC.TargetClosest(false); // NPC.target gets updated automatically here
                
                // Now check again if the selected target is valid / has been added by the search
                if (!Targets.Contains(NPC.target))
                {
                    // Target chosen by the vanilla logic is not in range
                    // Just in case run code to verify there are any targets (more than zero...)
                    if (Targets.Count <= 0)
                    {
                        // There don't seem to be any valid targets => despawn
                        return false;
                    }
                    
                    // Now select the closest target that is still in the target list
                    // Convert the HashSet to a array for easy access
                    int[] arTargets = Targets.ToArray();
                    
                    // Select the first entry as target
                    NPC.Target = atTargets[0];
                    
                    // loop through the entire array to find the closest one
                    foreach (int i in arTargets)
                    {
                        // Compare distance of old selected target to this one
                        if (Main.player[i].Distance(NPC.Center) < Main.player[NPC.target].Distance(NPC.Center))
                        {
                            // Update the target to the current one if closer
                            NPC.Target = i;
                        }
                    }
                }
            }
            return true;
            
            
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                // if not try to get another target
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];

                // then check if this target is valid and in range
                if (!player.active || player.dead || NPC.Distance(player.Center) > DespawnDistance)
                {
                    // if not then there are no valid targets
                    return false;
                }
            }
            // target still valid
            return true;
        }

        protected int[] GetMultipleTargets(bool TargetClosest, int TargetAmount = -1)
        {
            // Clear target list
            Targets.Clear();

            foreach (Player player in Main.player)
            {
                if ((player.dead || !player.active) 
                    && NPC.Distance(player.Center) < TargetSearchDistance)
                {
                    Targets.Add(player.whoAmI);
                }
            }

            return new int[] { 1,2,3};
        }
        #endregion

        protected void TriggerBoss()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            if (!BossTriggered)
            {
                BossTriggered = true;
            }
        }

        public override void AI()
        {
            UpdateTargets();
            
            switch (State)
            {
                case (byte)States.NotTriggered:
                {
                    if (BossTriggered)
                    {
                        NPC.TargetClosest(false);
                        State = (byte)States.TriggerAnimation;
                    }
                    return;
                }

                case (byte)States.TriggerAnimation:
                {
                    return;
                }
            }

            base.AI();
        }

        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
        {
            TriggerBoss();
            AddTarget(player.whoAmI);
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            TriggerBoss();
            AddTarget(projectile.owner);
        }

        protected virtual void RunSynchronize()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.netUpdate = true;
            }
        }

        #region Enumerators
        protected enum States
        { 
            NotTriggered,
            TriggerAnimation,
            CheckTarget,
            MovePrepare,
            Move,
            MoveEnd,
            AttackPrepare,
            Attack,
            AttackEnd,
            Despawn
        }
        #endregion
    }
}
