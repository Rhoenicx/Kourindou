using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Kourindou.Buffs
{
    class DeBuff_MedicineMelancholy : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        // NPC logic: when this buff is applied to an NPC
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<KourindouGlobalNPC>().DebuffMedicineMelancholy = true;
        }

        // Player logic: when this buff is applied to an PLayer
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<KourindouPlayer>().DebuffMedicineMelancholy = true;
        }

        public override bool ReApply(NPC npc, int time, int buffIndex)
        {
            if (npc.GetGlobalNPC<KourindouGlobalNPC>().DebuffMedicineMelancholyStacks < 5)
            {
                npc.GetGlobalNPC<KourindouGlobalNPC>().DebuffMedicineMelancholyStacks++;
            }

            return false;
        }

        public override bool ReApply(Player player, int time, int buffIndex)
        {
            if (player.GetModPlayer<KourindouPlayer>().DebuffMedicineMelancholyStacks < 5)
            {
                player.GetModPlayer<KourindouPlayer>().DebuffMedicineMelancholyStacks++;
            }

            return false;
        }
    }
}
