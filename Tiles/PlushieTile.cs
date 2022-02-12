using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Kourindou.Buffs;
using Kourindou.Items;

namespace Kourindou.Tiles.Plushies
{
    public abstract class PlushieTile : ModTile
    {
        public string soundName = "";
        public int plushieItem;

        public override void PlaceInWorld(int i, int j, Item item)
        {
            if (item.modItem is PlushieItem plushieItem)
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    KourindouWorld.SetPlushieDirtWater(i, j - 1, plushieItem.plushieDirtWater);
                }

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = mod.GetPacket();
                    packet.Write((byte) KourindouMessageType.SetPlushieDirtWater);
                    packet.Write((int) i);
                    packet.Write((int) j - 1);
                    packet.Write((short) plushieItem.plushieDirtWater);
                    packet.Send();
                }
            }
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer)
            {
                Player player = Main.LocalPlayer;
                if (player.GetModPlayer<KourindouPlayer>().plushiePower == 1)
                {
                    player.AddBuff(BuffType<Buff_PlushieInRange>(), 20);
                }
            }
        }

        public override bool Drop(int i, int j)
        {
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) {
            num = 0;
		}

        public override void RightClick(int i, int j)
        {
            if (soundName != "")
            {
                Vector2 soundPosition = new Vector2(i * 16, j * 16);
                float soundVolume = 0.3f;
                float pitchVariance = 0f;

                Main.PlaySound(
                    (int)SoundType.Custom,
                    (int) soundPosition.X,
                    (int) soundPosition.Y,
                    mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + soundName),
                    soundVolume,
                    pitchVariance);

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = mod.GetPacket();
                        packet.Write((byte) KourindouMessageType.PlayCustomSound);
                        packet.Write(soundName);
                        packet.Write(soundVolume);
                        packet.Write(pitchVariance);
                        packet.Write(soundPosition.X);
                        packet.Write(soundPosition.Y);
                        packet.Send();
                }
            }
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int itemSlot = Item.NewItem(i * 16, j * 16, 16, 48, plushieItem);

                short plushieDirtWater = 0;

                if (Main.item[itemSlot].modItem is PlushieItem plushie)
                {
                    plushieDirtWater = KourindouWorld.GetPlushieDirtWater(i, j, true);
                    plushie.plushieDirtWater = plushieDirtWater;
                }

                if (Main.netMode == NetmodeID.Server)
                {
                    ModPacket packet = mod.GetPacket();
                    packet.Write((byte)KourindouMessageType.PlushieItemNetUpdate);
                    packet.Write((int)itemSlot);
                    packet.Write((short)plushieDirtWater);
                    packet.Send(-1, Main.myPlayer);
                }
            }
        }
    }
}