using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Kourindou.Buffs
{
	public class DeBuff_Mortality : ModBuff
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Mortality");
			Description.SetDefault("You can die");
			Main.buffNoSave[Type] = false;
			Main.debuff[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = false;
		}
	}
}