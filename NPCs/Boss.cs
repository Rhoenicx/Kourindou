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
        protected float DespawnDistance = 5000f;

        protected bool GetTarget()
        {
            // Check if the current target is still alive
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

        // Fill target list with players
        protected int[] GetMultiTargets(bool TargetClosest, int TargetAmount = -1)
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
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            TriggerBoss();
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
