using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Items.Plushies;
using Kourindou.Projectiles.Plushies.PlushieEffects;

namespace Kourindou;

class KourindouGlobalItem : GlobalItem
{
	internal static Rectangle?[] meleeHitbox = new Rectangle?[256];

	private static Rectangle syncHitbox = new Rectangle();

    public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
	{
		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			syncHitbox = hitbox;
			syncHitbox.X -= (int)player.Center.X;
			syncHitbox.Y -= (int)player.Center.Y;
	            meleeHitbox[player.whoAmI] = syncHitbox;
	        }
		else if (player.whoAmI == Main.myPlayer && !noHitbox)
		{
			if (hitbox.Width != syncHitbox.Width || hitbox.Height != syncHitbox.Height)
			{
				meleeHitbox[player.whoAmI] = hitbox;
				syncHitbox = hitbox;
				
				ModPacket packet = Mod.GetPacket();
				packet.Write((byte) KourindouMessageType.MeleeHitbox);
				packet.Write((byte) player.whoAmI);
				packet.Write((int) syncHitbox.X - (int)player.Center.X);
				packet.Write((int) syncHitbox.Y - (int)player.Center.Y);
				packet.Write((int) syncHitbox.Width);
				packet.Write((int) syncHitbox.Height);
				packet.Send(-1, player.whoAmI);
			}
		}
	}

    public override void UseAnimation(Item item, Player player)
    {
	if (player.GetModPlayer<KourindouPlayer>().EquippedPlushies.Any(kvp => kvp.Key.Type == ItemType<Kourindou_SakuyaIzayoi_Plushie_Item>()) && item.damage > 0)
	{
		// Spawn 4 knifes on regular attack animations
		for (int i = 0; i < 4; i++)
		{
			Projectile.NewProjectile(
				player.GetSource_Accessory(new Item(ItemType<Kourindou_SakuyaIzayoi_Plushie_Item>())),
				player.RotatedRelativePoint(player.MountedCenter),
				Vector2.Normalize(Main.MouseWorld - player.RotatedRelativePoint(player.MountedCenter)).RotatedBy(MathHelper.ToRadians(i >= 2 ? 5 * (i - 1) : -5 * (i + 1))) * (player.HeldItem.shootSpeed > 0f ? player.HeldItem.shootSpeed : 8f),
				ProjectileType<SakuyaIzayoi_Plushie_Knife>(),
				10 + (int)(player.statLifeMax2 / 15),
				1f,
				Main.myPlayer
			);
		}
	}
    }

    public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
    {
	if (item != null
		&& item.wingSlot > 0
		&& player.wingsLogic > 0
		&& player.equippedWings != null
		&& player.equippedWings.type == ItemType<AyaShameimaru_Plushie_Item>()
		&& (!modded && slot < 13))
	{
		return false;
	}

        return base.CanEquipAccessory(item, player, slot, modded);
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

        public override GlobalItem Clone(Item item, Item itemClone)
        {
            return base.Clone(item, itemClone);
        }

        public override void SetDefaults(Item item)
        {
            defaultScale = item.scale;
        }

        public override void PostReforge(Item item)
        {
            defaultScale = item.scale;
        }
    }