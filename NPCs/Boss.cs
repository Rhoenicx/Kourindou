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
    public abstract class BossNPC : ModNPC
    {
        #region VAR_States
        protected byte Stage
        {
            get => (byte)(BitConverter.SingleToUInt32Bits(NPC.ai[0]) & 0x000000FF);
            set
            {
                NPC.ai[0] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[0]) & 0xFFFFFF00) | ((uint)value & 0x000000FF));
                Synchronize();
            }
        }

        protected byte State
        {
            get => (byte)((BitConverter.SingleToUInt32Bits(NPC.ai[0]) >> 8) & 0x000000FF);
            set
            {
                NPC.ai[0] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[0]) & 0xFFFF00FF) | (((uint)value << 8) & 0x0000FF00));
                Synchronize();
            }
        }

        protected byte SubState
        {
            get => (byte)((BitConverter.SingleToUInt32Bits(NPC.ai[0]) >> 16) & 0x000000FF);
            set
            {
                NPC.ai[0] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[0]) & 0xFF00FFFF) | (((uint)value << 16) & 0x00FF0000));
                Synchronize();
            }
        }

        protected byte Unused1
        {
            get => (byte)((BitConverter.SingleToUInt32Bits(NPC.ai[0]) >> 24) & 0x000000FF);
            set
            {
                NPC.ai[0] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[0]) & 0x00FFFFFF) | (((uint)value << 24) & 0xFF000000));
                Synchronize();
            }
        }
        #endregion

        #region VAR_Attacks
        protected bool Attacking = AttackTimer > 0 || AttackIndex > 0 || AttackState > 0;
        
        protected short AttackTimer
        {
            get => (short)(BitConverter.SingleToUInt32Bits(NPC.ai[1]) & 0x0000FFFF);
            set
            {
                NPC.ai[1] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[1]) & 0xFFFF0000) | ((uint)value & 0x0000FFFF));
            }
        }

        protected byte AttackIndex
        {
            get => (byte)((BitConverter.SingleToUInt32Bits(NPC.ai[1]) >> 16) & 0x000000FF);
            set
            {
                NPC.ai[1] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[1]) & 0xFF00FFFF) | (((uint)value << 16) & 0x00FF0000));
                Synchronize();
            }
        }
        
        protected byte AttackState
        {
            get => (byte)((BitConverter.SingleToUInt32Bits(NPC.ai[1]) >> 24) & 0x000000FF);
            set
            {
                NPC.ai[1] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[1]) & 0x00FFFFFF) | (((uint)value << 24) & 0xFF000000));
                Synchronize();
            }              
        }
        #endregion

        #region VAR_Movement
        protected bool Moving = MoveTimer > 0 || MoveIndex > 0 || MoveState > 0;
        protected short MoveTimer
        {
            get => (short)(BitConverter.SingleToUInt32Bits(NPC.ai[2]) & 0x0000FFFF);
            set
            {
                NPC.ai[2] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[2]) & 0xFFFF0000) | ((uint)value & 0x0000FFFF));
            }
        }
        
        protected byte MoveIndex
        {
            get => (byte)((BitConverter.SingleToUInt32Bits(NPC.ai[2]) >> 16) & 0x000000FF);
            set
            {
                NPC.ai[2] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[2]) & 0xFF00FFFF) | (((uint)value << 16) & 0x00FF0000));
                Synchronize();
            }
        }
        
        protected byte MoveState
        {
            get => (byte)((BitConverter.SingleToUInt32Bits(NPC.ai[2]) >> 24) & 0x000000FF);
            set
            {
                NPC.ai[2] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[2]) & 0x00FFFFFF) | (((uint)value << 24) & 0xFF000000));
                Synchronize();
            }              
        }
        #endregion
        
        #region VAR_Timers
        protected short MainTimer
        {
            get => (short)(BitConverter.SingleToUInt32Bits(NPC.ai[3]) & 0x0000FFFF)
            set
            {
                NPC.ai[3] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[3]) & 0xFFFF0000) | ((uint)value & 0x0000FFFF));
                Synchronize();
            }
        }
        
        protected short SubTimer
        {
            get => (short)((BitConverter.SingleToUInt32Bits(NPC.ai[3]) >> 16) & 0x0000FFFF)
            set
            {
                NPC.ai[3] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[3]) & 0x0000FFFF) | (((uint)value << 16) & 0xFFFF0000));
                Synchronize();
            }
        }
        
        #endregion
        
        #region VAR_Stats
        protected abstract int Difficulty { get; }
        protected abstract int StageAmount { get; }
        protected abstract int[] StageHealth { get; }
        protected abstract int[] StageDefense { get; }
        #endregion
        
        #region VAR_Other
        // Targets
        protected readonly HashSet<int> Targets = new HashSet<int>();
        protected float MaximumTargetDistance = 5000f;

        protected Vector2 TargetCenter => TargetDecoys && Main.player[NPC.target].tankPet >= 0 
            ? Main.projectile[Main.player[NPC.target].tankPet].Center 
            : Main.player[NPC.target].Center;
        
        // Decoys
        protected const bool TargetDecoys;
        protected float StageProgress => NPC.
        protected int Difficulty;

        #endregion
        
        #region AI_Stats
        protected int GetMaxHealth()
        {
            int hp;
            foreach (int i in StageHealth)
            {
                hp += StageHealth[i];
            }
            
            return hp;
        }
        
        protected int GetAverageDefense()
        {
            int def;
            foreach (int i in StageDefense)
            {
                def += StageDefense[i];
            }
            
            return def / StageDefense.Count;
        }
       
        protected void SetStageHealth()
        {
            int hp;
            foreach (int i in StageHealth)
            {
                hp += StageHealth[i];
            }
            
            float factor = (float) NPC.lifeMax / (float) hp;
            
            if (factor == 1f)
            {
                return;
            }
            
            foreach (int i in StageHealth)
            {
                StageHealth[i] *= factor;
            }
        }
        #endregion
        
        #region AI_Targeting
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
        protected void UpdateTargets()
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

        protected bool GetFirstTarget()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }
            
            NPC.TargetClosest(false);
            AddTarget(NPC.target);
        }

        protected bool GetSingleTarget()
        {
            // In multiplayer target logic should only be executed serverside
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }
            
            // Just in case update the target list to remove invalid targets
            UpdateTargets()
            
            // Check if the current targeted player is still available in the Target list
            // For example the player died, logged out or is no longer in range
            if (!Targets.Contains(NPC.target))
            {
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
                    NPC.Target = arTargets[0];
                    
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
        }

        protected int GetMultiTargetAmount()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }
            
            return Targets.Count;
        }

        protected HashSet[] GetMultiTargets(bool TargetClosest, bool RandomTargets, bool IncludeMainTarget, int TargetAmount = -1)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient || TargetAmount <= 0)
            {
                return new HashSet<int>() { NPC.target };
            }
            
            HashSet<int> MultiTargets = new HashSet<int>();
            HashSet<int> TempTargets = new HashSet<int>(Targets);
            
            if (IncludeMainTarget && TempTargets.Contains(NPC.target))
            {
                TempTargets.Remove(NPC.target);
                MultiTargets.Add(NPC.target)
            }
            
            if (TargetClosest)
            {
                for (int i = (int)IncludeMainTarget; i < TargetAmount; i++)
                {
                    int closest = TempTargets[0];
                    
                    foreach (int j in TempTargets)
                    {
                        if (NPC.Distance(Main.player[j].Center) < NPC.Distance(Main.player[closest].Center))
                        {
                            closest = j;
                        }
                    }
                    
                    MultiTargets.Add(closest);
                    TempTargets.Remove(closest);
                }
                
                return MultiTargets;
            }
            
            if (RandomTargets)
            {
                for (int i = (int)IncludeMainTarget; i < TargetAmount; i++)
                {
                    int selected = (int)Main.rand.Next(0, TempTargets.Count);
                    
                    MultiTargets.Add(TempTargets[selected]);
                    TempTargets.Remove(selected);
                }
                return MultiTargets;
            }
            
            return new HashSet<int>() { NPC.target };
        }
        #endregion

        #region AI_Main
        public override void AI()
        {
            UpdateTargets();
            
            switch (State)
            {
                case (byte) State.SpawnAnimation:
                    // Move the boss to a location around the player
                    return;
                
                case (byte)States.Trigger:
                {
                    // Check if the boss has taken damage, we use this to check if a player has attacked the boss after the spawn in animation
                    if (NPC.Health != NPC.MaxHealth)
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
        
        #endregion

        #region AI_OnHit
        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
        {
            AddTarget(player.whoAmI);
            CountDamage(player.whoAmI);
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            AddTarget(projectile.owner);
            CountDamage(projectile.owner);
        }

        protected void Synchronize()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.netUpdate = true;
            }
        }
        #endregion

        #region Networking
        
        #endregion

        #region Enums
        protected enum States
        { 
            Spawn,
            SpawnAnimation,
            Trigger,
            TriggerAnimation,
            MainLoop,
            DespawnAnimation,
            Despawn,
            DefeatAnimation,
            Defeat
        }
        
        // Store attackindex
        protected enum Attacks
        {
            none,
            idle,
            end
        }
        
        // Store moveindex
        protected enum Moves
        {
            none,
            idle
            end
        }
        
        protected enum NetworkMessages
        {
            Targets
        }
        #endregion
    }
}
