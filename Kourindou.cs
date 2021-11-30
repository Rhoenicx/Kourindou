using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Kourindou.Items.Plushies;
using Kourindou.Tiles.Plushies;

namespace Kourindou
{
    class Kourindou : Mod
    {
        public const string PlushieSlotBackTex = "PlushieSlotBackground";

        internal static Kourindou Instance;

        internal static KourindouConfigClient KourindouConfigClient;

        private static List<Func<bool>> RightClickOverrides;

        // Kourindou Mod Instance
        public Kourindou()
        {
            Instance = this;
        }

        // Load
        public override void Load()
        {
            Properties = new ModProperties() {
                Autoload = true,
                AutoloadBackgrounds = true,
                AutoloadGores = true,
                AutoloadSounds = true
            };

            RightClickOverrides = new List<Func<bool>>();

            if (!Main.dedServ)
            {

            }

        }

        // Unload
        public override void Unload()
        {
            KourindouConfigClient = null;

            if(RightClickOverrides != null) {
                RightClickOverrides.Clear();
                RightClickOverrides = null;
            }

            Instance = null;
            base.Unload();
        }

        // PostSetupContent - Register mods for compatibility
        public override void PostSetupContent()
        {
            // Support for Gensokyo Mod
            Mod Gensokyo = ModLoader.GetMod("Gensokyo");
            if (Gensokyo != null && Gensokyo.Version >= new Version(0, 7, 10, 3))
            {
                CrossModContent.SetupGensokyo(Gensokyo, this);
            }

            if (!Main.dedServ)
            {
                if (Kourindou.KourindouConfigClient.UseOldTextures)
                {
                    Main.itemTexture[ModContent.ItemType<ReimuHakurei_Plushie_Item>()] = GetTexture("Items/Plushies/ReimuHakurei_Plushie_Item_Old");
                    Main.tileTexture[ModContent.TileType<ReimuHakurei_Plushie_Tile>()] = GetTexture("Tiles/Plushies/ReimuHakurei_Plushie_Tile_Old");
                }
                else
                {
                    Main.itemTexture[ModContent.ItemType<ReimuHakurei_Plushie_Item>()] = GetTexture("Items/Plushies/ReimuHakurei_Plushie_Item");
                    Main.tileTexture[ModContent.TileType<ReimuHakurei_Plushie_Tile>()] = GetTexture("Tiles/Plushies/ReimuHakurei_Plushie_Tile");
                }
            }
        }

        // Network packet enum
        public enum KourindouMessageType : byte
        {
            ClientConfig
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch) {
            KourindouPlayer player = Main.LocalPlayer.GetModPlayer<KourindouPlayer>();
            player.Draw(spriteBatch);
        }

        // Handle netwrok packets
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            KourindouMessageType msg = (KourindouMessageType)reader.ReadByte();

            switch(msg)
            {
                // Update other players Config for Multiplayer
                case KourindouMessageType.ClientConfig:
                {
                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        byte playerID = reader.ReadByte();
                        byte plushiePower = reader.ReadByte();

                        Player player = Main.player[playerID];

                        player.GetModPlayer<KourindouPlayer>().plushiePower = plushiePower;

                        // if packet is received on server, resend this packet to other clients
                        if (Main.netMode == NetmodeID.Server)
                        {
                            ModPacket packet = GetPacket();
                            packet.Write((byte)KourindouMessageType.ClientConfig);
                            packet.Write((byte)playerID);
                            packet.Write((byte)plushiePower);
                            packet.Send(-1, whoAmI);
                        }
                    }
                    break;
                }

                default:
                    Logger.Warn("Kourindou: Unknown NetMessage type: " + msg);
                    break;
            }
        }

        public static bool OverrideRightClick() 
        {
            foreach(var func in RightClickOverrides) {
                if(func()) {
                    return true;
                }
            }

            return false;
        }
    }
}