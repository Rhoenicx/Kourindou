using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Kourindou.Buffs
{
	public class DeBuff_Mortality : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = false;
			Main.debuff[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = false;
		}
	}
}