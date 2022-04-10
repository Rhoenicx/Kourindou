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

        public override void SetDefaults(Item item)
        {
			if (item.type == ItemID.SilkRope)
			{
				item.SetNameOverride("White Fabric Rope");
			}

			if (item.type == ItemID.SilkRopeCoil)
			{
				item.SetNameOverride("White Fabric Rope Coil");
			}
        }

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
					
					ModPacket packet = Mod.GetPacket();
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

    public class KourindouGlobalItemInstance : GlobalItem
    {
		public float defaultScale = 1f;
        public override bool InstancePerEntity => true;
        //public override bool CloneNewInstances => true;

        public override void SetDefaults(Item item)
        {
            defaultScale = item.scale;
        }

        public override void PostReforge(Item item)
        {
            defaultScale = item.scale;
        }
    }
}