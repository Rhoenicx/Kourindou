using Terraria;
using Terraria.ModLoader;

namespace Kourindou.Buffs
{
    public class Buff_PlushieInRange : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Plushie Magic");
            Description.SetDefault("Slight increase to life and mana regeneration");
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            longerExpertDebuff = false;
            canBeCleared = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen += 2;
            player.manaRegenBonus += 2;
        }
    }
}