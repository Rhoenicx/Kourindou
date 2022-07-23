using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.GameContent;
using static Terraria.ModLoader.ModContent;
using Kourindou.Buffs;
using Kourindou.Items;

namespace Kourindou.Tiles.Plushies
{
    public abstract class PlushieTile : ModTile
    {
        public string soundName = "";
        public int plushieItem;

        public override void PlaceInWorld(int i, int j, Item item) //Runs only on SinglePlayer and MultiplayerClient!
        {
            if (item.ModItem is PlushieItem plushieItem)
            {
                // Add entry to PlushieTiles locally
                KourindouWorld.SetPlushieDirtWater(i, j - 1, plushieItem.plushieDirtWater);
                
                // If playing in multiplayer, also inform other parties of the placement
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = Mod.GetPacket();
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
                if (player.GetModPlayer<KourindouPlayer>().plushiePower)
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

        public override bool RightClick(int i, int j)
        {
            if (soundName != "")
            {
                Vector2 soundPosition = new Vector2(i * 16, j * 16);
                float soundVolume = 0.3f;
                float pitchVariance = 0f;

                SoundEngine.PlaySound(
                    new SoundStyle("Kourindou/Sounds/Music/" + soundName) with { Volume = soundVolume, PitchVariance = pitchVariance },
                    soundPosition);

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = Mod.GetPacket();
                        packet.Write((byte) KourindouMessageType.PlayCustomSound);
                        packet.Write(soundName);
                        packet.Write(soundVolume);
                        packet.Write(pitchVariance);
                        packet.Write(soundPosition.X);
                        packet.Write(soundPosition.Y);
                        packet.Send();
                }
            }
            return true;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // If SinglePlayer or server
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                KourindouWorld.GetPlushieDirtWater(i, j, true);
            }
            else
            {
                int itemSlot = Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 48, plushieItem);

                short plushieDirtWater = 0;

                // Update the variable in the newly created item
                if (Main.item[itemSlot].ModItem is PlushieItem plushie)
                {
                    plushieDirtWater = KourindouWorld.GetPlushieDirtWater(i, j, true);
                    plushie.plushieDirtWater = plushieDirtWater;
                }

                // If the tile is broken on the server also send a message to other parties
                if (Main.netMode == NetmodeID.Server)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)KourindouMessageType.PlushieItemNetUpdate);
                    packet.Write((int)itemSlot);
                    packet.Write((short)plushieDirtWater);
                    packet.Send(-1, Main.myPlayer);
                }
            }
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (Kourindou.PlushieTileTextures.ContainsKey(Type))
            {
                drawData.drawTexture = Kourindou.KourindouConfigClient.UseOldTextures ?
                    Kourindou.PlushieTileTextures[Type].oldTileTexture.Value : Kourindou.PlushieTileTextures[Type].TileTexture.Value;
            }
        }
    }
}