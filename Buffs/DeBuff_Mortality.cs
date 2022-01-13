using Terraria;
using Terraria.ModLoader;

namespace Kourindou.Buffs
{
	public class DeBuff_Mortality : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Mortality");
			Description.SetDefault("You can die");
			Main.buffNoSave[Type] = false;
			Main.debuff[Type] = true;
			canBeCleared = false;
		}
	}
}