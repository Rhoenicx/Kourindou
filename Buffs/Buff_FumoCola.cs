using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Kourindou.Buffs
{
    public class Buff_FumoCola : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fumo Cola");
            Description.SetDefault("Increases attack speed by {AttackSpeed}% and movement speed by {MovementSpeed}%");
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = false;
        }

        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            int stacks = Main.LocalPlayer.GetModPlayer<KourindouPlayer>().FumoColaBuffStacks;

            tip = tip.Replace("{AttackSpeed}", (stacks*5).ToString());
            tip = tip.Replace("{MovementSpeed}", (stacks*5).ToString());

            base.ModifyBuffTip(ref tip, ref rare);
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.GetModPlayer<KourindouPlayer>().FumoColaBuffStacks == 0)
            {
                player.GetModPlayer<KourindouPlayer>().FumoColaBuffStacks = 1;
            }

            // All attack speed buff
            player.GetAttackSpeed(DamageClass.Generic) += player.GetModPlayer<KourindouPlayer>().FumoColaBuffStacks * 0.05f;

            // Movement speed buff
            player.moveSpeed += player.GetModPlayer<KourindouPlayer>().FumoColaBuffStacks * 0.05f;
        }
        public override bool ReApply(Player player, int time, int buffIndex)
        {
            if (player.GetModPlayer<KourindouPlayer>().FumoColaBuffStacks < 4)
            {
                player.GetModPlayer<KourindouPlayer>().FumoColaBuffStacks += 1;
            }

            return false;
        }
    }
}