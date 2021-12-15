using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Kourindou
{
	class KourindouGlobalItem : GlobalItem
	{
		internal static Rectangle?[] meleeHitbox = new Rectangle?[256];
		// Is this ok to load in server?
		public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
		{
			meleeHitbox[player.whoAmI] = hitbox;
		}

		public override void PostUpdate(Item item)
		{
			base.PostUpdate(item);
		}
	}
}