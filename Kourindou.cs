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
                LoadTextures();
            }
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

                case KourindouMessageType.PlushieSlot:
                {
                    byte playerID = reader.ReadByte();
                    
                    KourindouPlayer player = Main.player[playerID].GetModPlayer<KourindouPlayer>();
                    
                    player.plushieEquipSlot.Item = ItemIO.Receive(reader);

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)KourindouMessageType.PlushieSlot);
                        packet.Write((byte)playerID);
                        ItemIO.Send(player.plushieEquipSlot.Item, packet);
                        packet.Send(-1, whoAmI);
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

        public void LoadTextures()
        {
            // COPY HERE
            // Main.itemTexture[ModContent.ItemType<_Plushie_Item>()] = GetTexture("Items/Plushies/_Plushie_Item_Old");
            // Main.tileTexture[ModContent.TileType<_Plushie_Tile>()] = GetTexture("Tiles/Plushies/_Plushie_Tile_Old");

            if (Kourindou.KourindouConfigClient.UseOldTextures)
            {   //------------------------------------------------------------------------- OLD -------------------------------------------------------------------------------//
                // Reimu Plushie
                Main.itemTexture[ModContent.ItemType<ReimuHakurei_Plushie_Item>()] = GetTexture("Items/Plushies/ReimuHakurei_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<ReimuHakurei_Plushie_Tile>()] = GetTexture("Tiles/Plushies/ReimuHakurei_Plushie_Tile_Old");

                // Tenshi Plushie
                Main.itemTexture[ModContent.ItemType<TenshiHinanawi_Plushie_Item>()] = GetTexture("Items/Plushies/TenshiHinanawi_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<TenshiHinanawi_Plushie_Tile>()] = GetTexture("Tiles/Plushies/TenshiHinanawi_Plushie_Tile_Old");

                // Marisa Plushie
                Main.itemTexture[ModContent.ItemType<MarisaKirisame_Plushie_Item>()] = GetTexture("Items/Plushies/MarisaKirisame_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<MarisaKirisame_Plushie_Tile>()] = GetTexture("Tiles/Plushies/MarisaKirisame_Plushie_Tile_Old");

                // Marisa Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_MarisaKirisame_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_MarisaKirisame_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<Kourindou_MarisaKirisame_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_MarisaKirisame_Plushie_Tile_Old");

                // Alice Plushie
                Main.itemTexture[ModContent.ItemType<AliceMargatroid_Plushie_Item>()] = GetTexture("Items/Plushies/AliceMargatroid_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<AliceMargatroid_Plushie_Tile>()] = GetTexture("Tiles/Plushies/AliceMargatroid_Plushie_Tile_Old");

                // Youmu Plushie
                Main.itemTexture[ModContent.ItemType<YoumuKonpaku_Plushie_Item>()] = GetTexture("Items/Plushies/YoumuKonpaku_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<YoumuKonpaku_Plushie_Tile>()] = GetTexture("Tiles/Plushies/YoumuKonpaku_Plushie_Tile_Old");

                // Yuyuko Plushie
                Main.itemTexture[ModContent.ItemType<YuyukoSaigyouji_Plushie_Item>()] = GetTexture("Items/Plushies/YuyukoSaigyouji_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<YuyukoSaigyouji_Plushie_Tile>()] = GetTexture("Tiles/Plushies/YuyukoSaigyouji_Plushie_Tile_Old");

                //Lily White

                // Reimu Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_ReimuHakurei_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_ReimuHakurei_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<Kourindou_ReimuHakurei_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_ReimuHakurei_Plushie_Tile_Old");

                // Remilia Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_RemiliaScarlet_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_RemiliaScarlet_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<Kourindou_RemiliaScarlet_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_RemiliaScarlet_Plushie_Tile_Old");

                // Flandre Plushie
                Main.itemTexture[ModContent.ItemType<FlandreScarlet_Plushie_Item>()] = GetTexture("Items/Plushies/FlandreScarlet_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<FlandreScarlet_Plushie_Tile>()] = GetTexture("Tiles/Plushies/FlandreScarlet_Plushie_Tile_Old");

                // Sakuya Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_SakuyaIzayoi_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_SakuyaIzayoi_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<Kourindou_SakuyaIzayoi_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_SakuyaIzayoi_Plushie_Tile_Old");

                // Patchouli Plushie
                Main.itemTexture[ModContent.ItemType<PatchouliKnowledge_Plushie_Item>()] = GetTexture("Items/Plushies/PatchouliKnowledge_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<PatchouliKnowledge_Plushie_Tile>()] = GetTexture("Tiles/Plushies/PatchouliKnowledge_Plushie_Tile_Old");

                // Koakuma Plushie

                // Hong Plushie
                Main.itemTexture[ModContent.ItemType<HongMeiling_Plushie_Item>()] = GetTexture("Items/Plushies/HongMeiling_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<HongMeiling_Plushie_Tile>()] = GetTexture("Tiles/Plushies/HongMeiling_Plushie_Tile_Old");

                // Cirno Plushie
                Main.itemTexture[ModContent.ItemType<Cirno_Plushie_Item>()] = GetTexture("Items/Plushies/Cirno_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<Cirno_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Cirno_Plushie_Tile_Old");

                // Daiyousei Plushie

                // Rumia Plushie
                Main.itemTexture[ModContent.ItemType<Rumia_Plushie_Item>()] = GetTexture("Items/Plushies/Rumia_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<Rumia_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Rumia_Plushie_Tile_Old");
            }
            else
            {
                //------------------------------------------------------------------------- NEW -------------------------------------------------------------------------------//
                // Reimu Plushie
                Main.itemTexture[ModContent.ItemType<ReimuHakurei_Plushie_Item>()] = GetTexture("Items/Plushies/ReimuHakurei_Plushie_Item");
                Main.tileTexture[ModContent.TileType<ReimuHakurei_Plushie_Tile>()] = GetTexture("Tiles/Plushies/ReimuHakurei_Plushie_Tile");

                // Tenshi Plushie
                Main.itemTexture[ModContent.ItemType<TenshiHinanawi_Plushie_Item>()] = GetTexture("Items/Plushies/TenshiHinanawi_Plushie_Item");
                Main.tileTexture[ModContent.TileType<TenshiHinanawi_Plushie_Tile>()] = GetTexture("Tiles/Plushies/TenshiHinanawi_Plushie_Tile");

                // Marisa Plushie
                Main.itemTexture[ModContent.ItemType<MarisaKirisame_Plushie_Item>()] = GetTexture("Items/Plushies/MarisaKirisame_Plushie_Item");
                Main.tileTexture[ModContent.TileType<MarisaKirisame_Plushie_Tile>()] = GetTexture("Tiles/Plushies/MarisaKirisame_Plushie_Tile");

                // Marisa Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_MarisaKirisame_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_MarisaKirisame_Plushie_Item");
                Main.tileTexture[ModContent.TileType<Kourindou_MarisaKirisame_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_MarisaKirisame_Plushie_Tile");

                // Alice Plushie
                Main.itemTexture[ModContent.ItemType<AliceMargatroid_Plushie_Item>()] = GetTexture("Items/Plushies/AliceMargatroid_Plushie_Item");
                Main.tileTexture[ModContent.TileType<AliceMargatroid_Plushie_Tile>()] = GetTexture("Tiles/Plushies/AliceMargatroid_Plushie_Tile");

                // Youmu Plushie
                Main.itemTexture[ModContent.ItemType<YoumuKonpaku_Plushie_Item>()] = GetTexture("Items/Plushies/YoumuKonpaku_Plushie_Item");
                Main.tileTexture[ModContent.TileType<YoumuKonpaku_Plushie_Tile>()] = GetTexture("Tiles/Plushies/YoumuKonpaku_Plushie_Tile");

                // Yuyuko Plushie
                Main.itemTexture[ModContent.ItemType<YuyukoSaigyouji_Plushie_Item>()] = GetTexture("Items/Plushies/YuyukoSaigyouji_Plushie_Item");
                Main.tileTexture[ModContent.TileType<YuyukoSaigyouji_Plushie_Tile>()] = GetTexture("Tiles/Plushies/YuyukoSaigyouji_Plushie_Tile");

                // Lily White

                // Reimu Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_ReimuHakurei_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_ReimuHakurei_Plushie_Item");
                Main.tileTexture[ModContent.TileType<Kourindou_ReimuHakurei_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_ReimuHakurei_Plushie_Tile");

                // Remilia Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_RemiliaScarlet_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_RemiliaScarlet_Plushie_Item");
                Main.tileTexture[ModContent.TileType<Kourindou_RemiliaScarlet_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_RemiliaScarlet_Plushie_Tile");

                // Flandre Plushie
                Main.itemTexture[ModContent.ItemType<FlandreScarlet_Plushie_Item>()] = GetTexture("Items/Plushies/FlandreScarlet_Plushie_Item");
                Main.tileTexture[ModContent.TileType<FlandreScarlet_Plushie_Tile>()] = GetTexture("Tiles/Plushies/FlandreScarlet_Plushie_Tile");

                // Sakuya Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_SakuyaIzayoi_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_SakuyaIzayoi_Plushie_Item");
                Main.tileTexture[ModContent.TileType<Kourindou_SakuyaIzayoi_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_SakuyaIzayoi_Plushie_Tile");

                // Patchouli Plushie
                Main.itemTexture[ModContent.ItemType<PatchouliKnowledge_Plushie_Item>()] = GetTexture("Items/Plushies/PatchouliKnowledge_Plushie_Item");
                Main.tileTexture[ModContent.TileType<PatchouliKnowledge_Plushie_Tile>()] = GetTexture("Tiles/Plushies/PatchouliKnowledge_Plushie_Tile");

                // Koakuma Plushie

                // Hong Plushie
                Main.itemTexture[ModContent.ItemType<HongMeiling_Plushie_Item>()] = GetTexture("Items/Plushies/HongMeiling_Plushie_Item");
                Main.tileTexture[ModContent.TileType<HongMeiling_Plushie_Tile>()] = GetTexture("Tiles/Plushies/HongMeiling_Plushie_Tile");

                // Cirno Plushie
                Main.itemTexture[ModContent.ItemType<Cirno_Plushie_Item>()] = GetTexture("Items/Plushies/Cirno_Plushie_Item");
                Main.tileTexture[ModContent.TileType<Cirno_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Cirno_Plushie_Tile");

                // Daiyousei Plushie

                // Rumia Plushie
                Main.itemTexture[ModContent.ItemType<Rumia_Plushie_Item>()] = GetTexture("Items/Plushies/Rumia_Plushie_Item");
                Main.tileTexture[ModContent.TileType<Rumia_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Rumia_Plushie_Tile");

            }
        }
    }
}