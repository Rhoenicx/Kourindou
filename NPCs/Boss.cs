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
            FindTarget,
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
