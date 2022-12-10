using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Kourindou.NPCs.Bosses
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
        protected short AttackTimer
        {
            get => (short)(BitConverter.SingleToUInt32Bits(NPC.ai[1]) & 0x0000FFFF);
            set => NPC.ai[1] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[1]) & 0xFFFF0000) | ((uint)value & 0x0000FFFF));
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
        protected short MoveTimer
        {
            get => (short)(BitConverter.SingleToUInt32Bits(NPC.ai[2]) & 0x0000FFFF);
            set => NPC.ai[2] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[2]) & 0xFFFF0000) | ((uint)value & 0x0000FFFF));
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
            get => (short)(BitConverter.SingleToUInt32Bits(NPC.ai[3]) & 0x0000FFFF);
            set => NPC.ai[3] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[3]) & 0xFFFF0000) | ((uint)value & 0x0000FFFF));
        }
        
        protected short SubTimer
        {
            get => (short)((BitConverter.SingleToUInt32Bits(NPC.ai[3]) >> 16) & 0x0000FFFF);
            set => NPC.ai[3] = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits(NPC.ai[3]) & 0x0000FFFF) | (((uint)value << 16) & 0xFFFF0000));
        }
        
        #endregion
        
        #region VAR
        // Stats
        protected abstract int StageAmount { get; }
        protected abstract int[] StageHealth { get; }
        protected abstract int[] StageDefense { get; }

        // Times
        protected abstract short SpawnAnimationTime { get; }
        protected abstract short DefeatAnimationTime { get; }
        protected abstract short[] StageSwitchAnimationTime { get; }

        // Targets
        protected readonly HashSet<int> Targets = new HashSet<int>();
        protected float MaximumTargetDistance = 5000f;
        protected Vector2 TargetCenter => TargetDecoys && Main.player[NPC.target].tankPet >= 0 
            ? Main.projectile[Main.player[NPC.target].tankPet].Center 
            : Main.player[NPC.target].Center;

        // Spawn
        protected bool _JustSpawned = true;

        // Decoys
        protected abstract bool TargetDecoys { get; }

        // Movement
        protected Vector2 destination;
        //protected abstract float[] AccelerationMultiplier { get; }
        //protected abstract float[] DecelerationMultiplier { get; }

        // Other
        protected bool defeated = false;
        protected bool debug = true;
        protected bool NextStage = false;
        protected bool ResetNextStage = false;

        #endregion

        #region AI_Stats
        protected int GetMaxHealth()
        {
            int hp = 0;
            for (int i = 0; i < StageHealth.Length; i++)
            {
                hp += StageHealth[i];
            }
            
            return hp;
        }
        
        protected int GetAverageDefense()
        {
            int def = 0;
            for (int i = 0; i < StageDefense.Length; i++)
            {
                def += StageDefense[i];
            }
            
            return def / StageDefense.Length;
        }
       
        protected void SetStageHealth()
        {
            float factor = (float) NPC.lifeMax / (float) GetMaxHealth();
            
            if (factor == 1f)
            {
                return;
            }

            for (int i = 0; i < StageHealth.Length; i++)
            {
                StageHealth[i] = (int)(StageHealth[i] * factor);
            }
        }

        protected void SetHealth(int stage)
        {
            if (stage >= StageAmount)
            {
                return;
            }

            NPC.lifeMax = StageHealth[stage];
            NPC.life = NPC.lifeMax;
        }

        protected void SetDefense(int stage)
        {
            if (stage >= StageAmount)
            {
                return;
            }

            NPC.defense = StageDefense[stage];
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

        protected bool GetClosestTarget()
        {            
            NPC.TargetClosest(false);
            AddTarget(NPC.target);
            return true;
        }

        protected bool GetSingleTarget()
        {
            // Just in case update the target list to remove invalid targets
            UpdateTargets();
            
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
                    NPC.target = arTargets[0];
                    
                    // loop through the entire array to find the closest one
                    foreach (int i in arTargets)
                    {
                        // Compare distance of old selected target to this one
                        if (Main.player[i].Distance(NPC.Center) < Main.player[NPC.target].Distance(NPC.Center))
                        {
                            // Update the target to the current one if closer
                            NPC.target = i;
                        }
                    }
                }
            }
            return true;
        }

        protected int GetMultiTargetAmount()
        {
            return Targets.Count;
        }

        protected HashSet<int> GetMultiTargets(bool TargetClosest, bool RandomTargets, bool IncludeMainTarget, int TargetAmount = -1)
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
                MultiTargets.Add(NPC.target);
            }
            
            if (TargetClosest && !RandomTargets)
            {
                for (int i = IncludeMainTarget ? 1 : 0; i < TargetAmount; i++)
                {
                    int closest = TempTargets.First<int>();
                    
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
            
            if (RandomTargets && !TargetClosest)
            {
                if (TempTargets.Count >= TargetAmount)
                {
                    return Targets;
                }

                for (int i = IncludeMainTarget ? 1 : 0; i < TargetAmount; i++)
                {
                    List<int> list = TempTargets.ToList<int>();

                    int selected = (int)Main.rand.Next(0, TempTargets.Count);
                    
                    MultiTargets.Add(list[selected]);
                    TempTargets.Remove(list[selected]);
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
                case (byte)States.JustSpawned:
                    {
                        NPC.despawnEncouraged = false;
                        GetClosestTarget();
                        SetHealth(Stage);
                        SetDefense(Stage);
                        AttackIndex = 0;
                        MoveIndex = (byte)Moves.Straight;
                        State = (byte)States.PrepareExecution;
                        _JustSpawned = false;
                        return;
                    }
                    
                case (byte)States.CheckStage:
                    {
                        // If the stage should not be changed, just go to the next state
                        if (!NextStage)
                        {
                            State = (byte)States.SearchForTargets;
                            return;
                        }

                        // When 
                        if (ResetNextStage)
                        {
                            Main.NewText(Stage);
                            NextStage = false;
                            ResetNextStage = false;
                            SetHealth(Stage);
                            SetDefense(Stage);
                            State = (byte)States.SearchForTargets;
                            return;
                        }

                        // Increase Stage by 1 
                        Stage++;
                        Main.NewText("Stage changed to: " + Stage);

                        // Put the reset boolean for the next execution, we want to run attack 255 first
                        ResetNextStage = true;
                        GetClosestTarget();
                        AttackIndex = 255;
                        State = (byte)States.PrepareExecution;
                        return;
                    }

                case (byte)States.SearchForTargets:
                    {
                        if (!GetSingleTarget())
                        {
                            State = (byte)States.Despawn;
                            return;
                        }

                        State = (byte)States.Conditioning;
                        return;
                    }

                case (byte)States.Conditioning:
                    {
                        Conditioning();

                        State = (byte)States.PrepareExecution;
                        return;
                    }

                case (byte)States.PrepareExecution:
                    {
                        State = (byte)States.Execution;
                        return;
                    }

                case (byte)States.Execution:
                    {
                        if (Attack() && Move())
                        {
                            State = (byte)States.DoneExecution;
                        }

                        return;
                    }

                case (byte)States.DoneExecution:
                    {
                        // Reset Attack
                        AttackState = 0;
                        AttackTimer = 0;
                        AttackIndex = 0;

                        // Reset Move
                        MoveState = 0;
                        MoveTimer = 0;
                        MoveIndex = 0;

                        // Back to the top of the state machine => CheckStage
                        State = (byte)States.CheckStage;
                        return;
                    }

                case (byte)States.Despawn:
                    {
                        switch (SubState)
                        {
                            case 0:
                                {
                                    SubTimer = DefeatAnimationTime;
                                    SubState = 1;
                                    break;
                                }

                            case 1:
                                {
                                    if (SubTimer-- <= 0)
                                    {
                                        SubState = 2;
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    NPC.velocity = new Vector2(0f, -8f);
                                    NPC.despawnEncouraged = true;
                                    break;
                                }
                        }
                        return;
                    }

                case (byte)States.Defeat:
                    {
                        switch (SubState)
                        {
                            case 0:
                                {
                                    SubTimer = DefeatAnimationTime;
                                    SubState = 1;
                                    break;
                                }

                            case 1:
                                {
                                    if (SubTimer-- <= 0)
                                    {
                                        SubState = 2;
                                    }
                                    break;
                                }

                            case 2:
                                {
                                    defeated = true;
                                    break;
                                }
                        }
                        return;
                    }
            }
        }

        public virtual void Conditioning()
        {

        }

        public virtual bool Attack()
        {
            return true;
        }

        public virtual bool Move()
        {
            return true;
        }
        #endregion

        #region AI_OnHit
        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
        {
            AddTarget(player.whoAmI);
            //CountDamage(player.whoAmI);
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            AddTarget(projectile.owner);
            //CountDamage(projectile.owner);
        }

        protected void Synchronize()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.netUpdate = true;
            }
        }
        #endregion

        #region AI_CheckDead
        public override bool CheckDead()
        {
            if (defeated)
            {
                return true;
            }

            if (Stage < StageAmount - 1)
            {
                if (!NextStage)
                {
                    NextStage = true;
                }

                NPC.life = 1;
            }
            else if (State != (byte)States.Defeat)
            {
                SubState = 0;
                NPC.life = 1;
                State = (byte)States.Defeat;
            }
            else if (State == (byte)States.Defeat)
            {
                NPC.life = 1;
            }

            return false;
        }

        #endregion

        #region Networking

        #endregion

        #region Enums
        protected enum States
        { 
            JustSpawned,
            CheckStage,
            SearchForTargets,
            Conditioning,
            PrepareExecution,
            Execution,
            DoneExecution,
            Despawn,
            Defeat
        }
        
        // Store attackindex
        protected enum Attacks
        {
            MoveOnly = 0,
            StageSwitch = 255
        }
        
        // Store moveindex
        protected enum Moves
        {
            None,
            Straight,
            Bezier,
            Following
        }
        
        protected enum NetworkMessages
        {
            Targets
        }
        #endregion
    }
}
