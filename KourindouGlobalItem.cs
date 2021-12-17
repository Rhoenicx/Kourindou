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

		private static Rectangle syncHitbox = new Rectangle();

		// Is this ok to load in server?
		public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				meleeHitbox[player.whoAmI] = hitbox;
			}
			else
			{
				if (hitbox.X - (int)player.Center.X != syncHitbox.X 
				 || hitbox.Y - (int)player.Center.Y != syncHitbox.Y 
				 || hitbox.Width != syncHitbox.Width 
				 || hitbox.Height != syncHitbox.Height)
				{
					syncHitbox = hitbox;
					syncHitbox.X -= (int)player.Center.X;
					syncHitbox.Y -= (int)player.Center.Y;
					
					ModPacket packet = mod.GetPacket();
					packet.Write((byte) KourindouMessageType.MeleeHitbox);
					packet.Write((byte) player.whoAmI);
					packet.Write((int) syncHitbox.X);
					packet.Write((int) syncHitbox.Y);
					packet.Write((int) syncHitbox.Width);
					packet.Write((int) syncHitbox.Height);
					packet.Send();
				}
			}
		}

		public override void PostUpdate(Item item)
		{
			base.PostUpdate(item);
		}
	}
}