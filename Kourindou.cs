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
using Terraria.Localization;
using Kourindou.Items.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Plushies;
using Kourindou.Tiles.Plants;
using Kourindou.Projectiles.Plushies;

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
                LoadPlushieTextures();

                // Thread
                Main.itemTexture[ItemID.BlackThread] = GetTexture("Items/CraftingMaterials/BlackThread");
                Main.itemTexture[ItemID.GreenThread] = GetTexture("Items/CraftingMaterials/GreenThread");
                Main.itemTexture[ItemID.PinkThread] = GetTexture("Items/CraftingMaterials/PinkThread");

                // Silk
                Main.itemTexture[ItemID.Silk] = GetTexture("Items/CraftingMaterials/WhiteFabric");
            }
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch) {
            KourindouPlayer player = Main.LocalPlayer.GetModPlayer<KourindouPlayer>();
            player.Draw(spriteBatch);
        }

        // Add Crafting recipe groups
        public override void AddRecipeGroups()
        {
            // Thread
            RecipeGroup Thread = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Thread", new int[]
            {
                ItemID.BlackThread,
                ModContent.ItemType<BlueThread>(),
                ModContent.ItemType<BrownThread>(),
                ModContent.ItemType<CyanThread>(),
                ItemID.GreenThread,
                ModContent.ItemType<LimeThread>(),
                ModContent.ItemType<OrangeThread>(),
                ItemID.PinkThread,
                ModContent.ItemType<PurpleThread>(),
                ModContent.ItemType<RedThread>(),
                ModContent.ItemType<SilverThread>(),
                ModContent.ItemType<SkyBlueThread>(),
                ModContent.ItemType<TealThread>(),
                ModContent.ItemType<VioletThread>(),
                ModContent.ItemType<WhiteThread>(),
                ModContent.ItemType<YellowThread>()
            });
            RecipeGroup.RegisterGroup("Kourindou:Thread", Thread);

            // Fabric
            RecipeGroup Fabric = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Fabric", new int[]
            {
                ModContent.ItemType<BlackFabric>(),
                ModContent.ItemType<BlueFabric>(),
                ModContent.ItemType<BrownFabric>(),
                ModContent.ItemType<CyanFabric>(),
                ModContent.ItemType<GreenFabric>(),
                ModContent.ItemType<LimeFabric>(),
                ModContent.ItemType<OrangeFabric>(),
                ModContent.ItemType<PinkFabric>(),
                ModContent.ItemType<PurpleFabric>(),
                ModContent.ItemType<RedFabric>(),
                ModContent.ItemType<SilverFabric>(),
                ModContent.ItemType<SkyBlueFabric>(),
                ModContent.ItemType<TealFabric>(),
                ModContent.ItemType<VioletFabric>(),
                ItemID.Silk,
                ModContent.ItemType<YellowFabric>()
            });
            RecipeGroup.RegisterGroup("Kourindou:Fabric", Fabric); 
        }

        // Handle netwrok packets
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            KourindouMessageType msg = (KourindouMessageType)reader.ReadByte();

            Main.NewText("Packet: " + msg);

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

                case KourindouMessageType.ForceUnequipPlushie:
                {
                    byte playerID = reader.ReadByte();
                    Item plushie = ItemIO.Receive(reader);

                    Player player = Main.player[playerID];

                    if (Main.netMode == NetmodeID.Server)
                    {
                        Item.NewItem(
                            player.Center,
                            new Vector2(player.width, player.height),
                            plushie.type, 
                            1
                        );
                    }
                    break;
                }

                case KourindouMessageType.ThrowPlushie:
                {
                    //position
                    byte playerID = reader.ReadByte();
                    //speed
                    Vector2 speed = reader.ReadVector2();
                    //type
                    int type = reader.ReadInt32();
                    //damage
                    int damage = reader.ReadInt32();
                    //knockback
                    float knockBack = reader.ReadSingle();

                    if (Main.netMode == NetmodeID.Server)
                    {
                        Projectile.NewProjectile(
                            Main.player[playerID].Center + new Vector2(0f, -16f),
                            speed,
                            type,
                            damage,
                            knockBack,
                            Main.myPlayer,
                            30f,
                            0f);
                    }
                    break;
                }

                case KourindouMessageType.MeleeHitbox:
                {
                    byte playerID = reader.ReadByte();

                    int X = reader.ReadInt32();
                    int Y = reader.ReadInt32();
                    int Width = reader.ReadInt32();
                    int Height = reader.ReadInt32();

                    if (Main.netMode == NetmodeID.Server)
                    {
                        Player p = Main.player[playerID];

                        Rectangle meleeHitbox = new Rectangle(X + (int)p.Center.X, Y + (int)p.Center.Y, Width, Height);
                        KourindouGlobalItem.meleeHitbox[playerID] = meleeHitbox;
                    }
                    break;
                }

                case KourindouMessageType.ReimuPlushieTargets:
                {
                    int proj = reader.ReadInt32();
                    int npc = reader.ReadInt32();

                    KourindouGlobalProjectile.ReimuPlushieHomingTarget[proj] = npc;

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)KourindouMessageType.ReimuPlushieTargets);
                        packet.Write((int)proj);
                        packet.Write((int)npc);
                        packet.Send(-1, whoAmI);
                    }
                    break;
                }

                case KourindouMessageType.CottonRightClick:
                {
                    int i = reader.ReadInt32();
                    int j = reader.ReadInt32();

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModContent.GetInstance<Cotton_Tile>().NewRightClick(i, j);
                    }
                    break;
                }

                case KourindouMessageType.PlaySound: 
				{
					byte soundType = reader.ReadByte();
					short soundStyle = reader.ReadInt16();
					float soundVolume = reader.ReadSingle();
					float soundVariance = reader.ReadSingle();
					int soundSourceX = reader.ReadInt32();
					int soundSourceY = reader.ReadInt32();

					if (Main.netMode == NetmodeID.Server)
					{
						ModPacket packet = GetPacket();
						packet.Write((byte) KourindouMessageType.PlaySound);
						packet.Write(soundType);
						packet.Write(soundStyle);
						packet.Write(soundVolume);
						packet.Write(soundVariance);
						packet.Write(soundSourceX);
						packet.Write(soundSourceY);

						packet.Send(-1, whoAmI);
						break;
					}
					
					if (soundSourceX == -1 || soundSourceY == -1)
					{
						Main.PlaySound(
							soundType,
							(int)Main.LocalPlayer.Center.X,
							(int)Main.LocalPlayer.Center.Y,
							soundStyle,
							soundVolume,
							Main.rand.NextFloat(-soundVariance, soundVariance));
					}
					else
					{
						Main.PlaySound(
							soundType,
							soundSourceX,
							soundSourceY,
							soundStyle,
							soundVolume,
							Main.rand.NextFloat(-soundVariance, soundVariance));
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

        public void LoadPlushieTextures()
        {
            // COPY HERE
            // Main.itemTexture[ModContent.ItemType<_Plushie_Item>()] = GetTexture("Items/Plushies/_Plushie_Item_Old");
            // Main.tileTexture[ModContent.TileType<_Plushie_Tile>()] = GetTexture("Tiles/Plushies/_Plushie_Tile_Old");

            if (Kourindou.KourindouConfigClient.UseOldTextures)
            {   //------------------------------------------------------------------------- OLD -------------------------------------------------------------------------------//
                // Reimu Plushie
                Main.itemTexture[ModContent.ItemType<ReimuHakurei_Plushie_Item>()] = GetTexture("Items/Plushies/ReimuHakurei_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<ReimuHakurei_Plushie_Tile>()] = GetTexture("Tiles/Plushies/ReimuHakurei_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<ReimuHakurei_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/ReimuHakurei_Plushie_Projectile_Old");

                // Tenshi Plushie
                Main.itemTexture[ModContent.ItemType<TenshiHinanawi_Plushie_Item>()] = GetTexture("Items/Plushies/TenshiHinanawi_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<TenshiHinanawi_Plushie_Tile>()] = GetTexture("Tiles/Plushies/TenshiHinanawi_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<TenshiHinanawi_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/TenshiHinanawi_Plushie_Projectile_Old");

                // Marisa Plushie
                Main.itemTexture[ModContent.ItemType<MarisaKirisame_Plushie_Item>()] = GetTexture("Items/Plushies/MarisaKirisame_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<MarisaKirisame_Plushie_Tile>()] = GetTexture("Tiles/Plushies/MarisaKirisame_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<MarisaKirisame_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/MarisaKirisame_Plushie_Projectile_Old");

                // Marisa Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_MarisaKirisame_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_MarisaKirisame_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<Kourindou_MarisaKirisame_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_MarisaKirisame_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<Kourindou_MarisaKirisame_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/Kourindou_MarisaKirisame_Plushie_Projectile_Old");

                // Alice Plushie
                Main.itemTexture[ModContent.ItemType<AliceMargatroid_Plushie_Item>()] = GetTexture("Items/Plushies/AliceMargatroid_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<AliceMargatroid_Plushie_Tile>()] = GetTexture("Tiles/Plushies/AliceMargatroid_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<AliceMargatroid_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/AliceMargatroid_Plushie_Projectile_Old");

                // Youmu Plushie
                Main.itemTexture[ModContent.ItemType<YoumuKonpaku_Plushie_Item>()] = GetTexture("Items/Plushies/YoumuKonpaku_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<YoumuKonpaku_Plushie_Tile>()] = GetTexture("Tiles/Plushies/YoumuKonpaku_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<YoumuKonpaku_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/YoumuKonpaku_Plushie_Projectile_Old");

                // Yuyuko Plushie
                Main.itemTexture[ModContent.ItemType<YuyukoSaigyouji_Plushie_Item>()] = GetTexture("Items/Plushies/YuyukoSaigyouji_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<YuyukoSaigyouji_Plushie_Tile>()] = GetTexture("Tiles/Plushies/YuyukoSaigyouji_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<YuyukoSaigyouji_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/YuyukoSaigyouji_Plushie_Projectile_Old");

                //Lily White

                // Reimu Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_ReimuHakurei_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_ReimuHakurei_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<Kourindou_ReimuHakurei_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_ReimuHakurei_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<Kourindou_ReimuHakurei_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/Kourindou_ReimuHakurei_Plushie_Projectile_Old");

                // Remilia Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_RemiliaScarlet_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_RemiliaScarlet_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<Kourindou_RemiliaScarlet_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_RemiliaScarlet_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<Kourindou_RemiliaScarlet_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/Kourindou_RemiliaScarlet_Plushie_Projectile_Old");

                // Flandre Plushie
                Main.itemTexture[ModContent.ItemType<FlandreScarlet_Plushie_Item>()] = GetTexture("Items/Plushies/FlandreScarlet_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<FlandreScarlet_Plushie_Tile>()] = GetTexture("Tiles/Plushies/FlandreScarlet_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<FlandreScarlet_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/FlandreScarlet_Plushie_Projectile_Old");

                // Sakuya Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_SakuyaIzayoi_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_SakuyaIzayoi_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<Kourindou_SakuyaIzayoi_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_SakuyaIzayoi_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<Kourindou_SakuyaIzayoi_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/Kourindou_SakuyaIzayoi_Plushie_Projectile_Old");

                // Patchouli Plushie
                Main.itemTexture[ModContent.ItemType<PatchouliKnowledge_Plushie_Item>()] = GetTexture("Items/Plushies/PatchouliKnowledge_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<PatchouliKnowledge_Plushie_Tile>()] = GetTexture("Tiles/Plushies/PatchouliKnowledge_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<PatchouliKnowledge_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/PatchouliKnowledge_Plushie_Projectile_Old");

                // Koakuma Plushie

                // Hong Plushie
                Main.itemTexture[ModContent.ItemType<HongMeiling_Plushie_Item>()] = GetTexture("Items/Plushies/HongMeiling_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<HongMeiling_Plushie_Tile>()] = GetTexture("Tiles/Plushies/HongMeiling_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<HongMeiling_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/HongMeiling_Plushie_Projectile_Old");

                // Cirno Plushie
                Main.itemTexture[ModContent.ItemType<Cirno_Plushie_Item>()] = GetTexture("Items/Plushies/Cirno_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<Cirno_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Cirno_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<Cirno_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/Cirno_Plushie_Projectile_Old");

                // Daiyousei Plushie

                // Rumia Plushie
                Main.itemTexture[ModContent.ItemType<Rumia_Plushie_Item>()] = GetTexture("Items/Plushies/Rumia_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<Rumia_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Rumia_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<Rumia_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/Rumia_Plushie_Projectile_Old");

                // Aya Plushie
                Main.itemTexture[ModContent.ItemType<AyaShameimaru_Plushie_Item>()] = GetTexture("Items/Plushies/AyaShameimaru_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<AyaShameimaru_Plushie_Tile>()] = GetTexture("Tiles/Plushies/AyaShameimaru_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<AyaShameimaru_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/AyaShameimaru_Plushie_Projectile_Old");

                // Eirin Plushie
                Main.itemTexture[ModContent.ItemType<EirinYagokoro_Plushie_Item>()] = GetTexture("Items/Plushies/EirinYagokoro_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<EirinYagokoro_Plushie_Tile>()] = GetTexture("Tiles/Plushies/EirinYagokoro_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<EirinYagokoro_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/EirinYagokoro_Plushie_Projectile_Old");

                // Mokou Plushie
                Main.itemTexture[ModContent.ItemType<FujiwaraNoMokou_Plushie_Item>()] = GetTexture("Items/Plushies/FujiwaraNoMokou_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<FujiwaraNoMokou_Plushie_Tile>()] = GetTexture("Tiles/Plushies/FujiwaraNoMokou_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<FujiwaraNoMokou_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/FujiwaraNoMokou_Plushie_Projectile_Old");

                // Hina Plushie

                // Kaguya Plushie
                Main.itemTexture[ModContent.ItemType<KaguyaHouraisan_Plushie_Item>()] = GetTexture("Items/Plushies/KaguyaHouraisan_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<KaguyaHouraisan_Plushie_Tile>()] = GetTexture("Tiles/Plushies/KaguyaHouraisan_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<KaguyaHouraisan_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/KaguyaHouraisan_Plushie_Projectile_Old");

                // Kanako Plushie

                // Keine Plushie

                // Kisume Plushie

                // Momiji Plushie

                // Nitori Plushie

                // Reisen Plushie
                Main.itemTexture[ModContent.ItemType<ReisenUdongeinInaba_Plushie_Item>()] = GetTexture("Items/Plushies/ReisenUdongeinInaba_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<ReisenUdongeinInaba_Plushie_Tile>()] = GetTexture("Tiles/Plushies/ReisenUdongeinInaba_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<ReisenUdongeinInaba_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/ReisenUdongeinInaba_Plushie_Projectile_Old");

                // Sanea Plushie

                // Suwako Plushie
                Main.itemTexture[ModContent.ItemType<SuwakoMoriya_Plushie_Item>()] = GetTexture("Items/Plushies/SuwakoMoriya_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<SuwakoMoriya_Plushie_Tile>()] = GetTexture("Tiles/Plushies/SuwakoMoriya_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<SuwakoMoriya_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/SuwakoMoriya_Plushie_Projectile_Old");

                // Tewi Plushie
                Main.itemTexture[ModContent.ItemType<TewiInaba_Plushie_Item>()] = GetTexture("Items/Plushies/TewiInaba_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<TewiInaba_Plushie_Tile>()] = GetTexture("Tiles/Plushies/TewiInaba_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<TewiInaba_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/TewiInaba_Plushie_Projectile_Old");

                // Koishi Plushie
                Main.itemTexture[ModContent.ItemType<KoishiKomeiji_Plushie_Item>()] = GetTexture("Items/Plushies/KoishiKomeiji_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<KoishiKomeiji_Plushie_Tile>()] = GetTexture("Tiles/Plushies/KoishiKomeiji_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<KoishiKomeiji_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/KoishiKomeiji_Plushie_Projectile_Old");
				
				// Shion Plushie
				Main.itemTexture[ModContent.ItemType<ShionYorigami_Plushie_Item>()] = GetTexture("Items/Plushies/ShionYorigami_Plushie_Item_Old");
                Main.tileTexture[ModContent.TileType<ShionYorigami_Plushie_Tile>()] = GetTexture("Tiles/Plushies/ShionYorigami_Plushie_Tile_Old");
                Main.projectileTexture[ModContent.ProjectileType<ShionYorigami_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/ShionYorigami_Plushie_Projectile_Old");
				
				// Kokoro Plushie
				
            }
            else
            {
                //------------------------------------------------------------------------- NEW -------------------------------------------------------------------------------//
                // Reimu Plushie
                Main.itemTexture[ModContent.ItemType<ReimuHakurei_Plushie_Item>()] = GetTexture("Items/Plushies/ReimuHakurei_Plushie_Item");
                Main.tileTexture[ModContent.TileType<ReimuHakurei_Plushie_Tile>()] = GetTexture("Tiles/Plushies/ReimuHakurei_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<ReimuHakurei_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/ReimuHakurei_Plushie_Projectile");

                // Tenshi Plushie
                Main.itemTexture[ModContent.ItemType<TenshiHinanawi_Plushie_Item>()] = GetTexture("Items/Plushies/TenshiHinanawi_Plushie_Item");
                Main.tileTexture[ModContent.TileType<TenshiHinanawi_Plushie_Tile>()] = GetTexture("Tiles/Plushies/TenshiHinanawi_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<TenshiHinanawi_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/TenshiHinanawi_Plushie_Projectile");

                // Marisa Plushie
                Main.itemTexture[ModContent.ItemType<MarisaKirisame_Plushie_Item>()] = GetTexture("Items/Plushies/MarisaKirisame_Plushie_Item");
                Main.tileTexture[ModContent.TileType<MarisaKirisame_Plushie_Tile>()] = GetTexture("Tiles/Plushies/MarisaKirisame_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<MarisaKirisame_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/MarisaKirisame_Plushie_Projectile");

                // Marisa Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_MarisaKirisame_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_MarisaKirisame_Plushie_Item");
                Main.tileTexture[ModContent.TileType<Kourindou_MarisaKirisame_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_MarisaKirisame_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<Kourindou_MarisaKirisame_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/Kourindou_MarisaKirisame_Plushie_Projectile");

                // Alice Plushie
                Main.itemTexture[ModContent.ItemType<AliceMargatroid_Plushie_Item>()] = GetTexture("Items/Plushies/AliceMargatroid_Plushie_Item");
                Main.tileTexture[ModContent.TileType<AliceMargatroid_Plushie_Tile>()] = GetTexture("Tiles/Plushies/AliceMargatroid_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<AliceMargatroid_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/AliceMargatroid_Plushie_Projectile");

                // Youmu Plushie
                Main.itemTexture[ModContent.ItemType<YoumuKonpaku_Plushie_Item>()] = GetTexture("Items/Plushies/YoumuKonpaku_Plushie_Item");
                Main.tileTexture[ModContent.TileType<YoumuKonpaku_Plushie_Tile>()] = GetTexture("Tiles/Plushies/YoumuKonpaku_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<YoumuKonpaku_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/YoumuKonpaku_Plushie_Projectile");

                // Yuyuko Plushie
                Main.itemTexture[ModContent.ItemType<YuyukoSaigyouji_Plushie_Item>()] = GetTexture("Items/Plushies/YuyukoSaigyouji_Plushie_Item");
                Main.tileTexture[ModContent.TileType<YuyukoSaigyouji_Plushie_Tile>()] = GetTexture("Tiles/Plushies/YuyukoSaigyouji_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<YuyukoSaigyouji_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/YuyukoSaigyouji_Plushie_Projectile");

                // Lily White

                // Reimu Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_ReimuHakurei_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_ReimuHakurei_Plushie_Item");
                Main.tileTexture[ModContent.TileType<Kourindou_ReimuHakurei_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_ReimuHakurei_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<Kourindou_ReimuHakurei_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/Kourindou_ReimuHakurei_Plushie_Projectile");

                // Remilia Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_RemiliaScarlet_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_RemiliaScarlet_Plushie_Item");
                Main.tileTexture[ModContent.TileType<Kourindou_RemiliaScarlet_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_RemiliaScarlet_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<Kourindou_RemiliaScarlet_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/Kourindou_RemiliaScarlet_Plushie_Projectile");

                // Flandre Plushie
                Main.itemTexture[ModContent.ItemType<FlandreScarlet_Plushie_Item>()] = GetTexture("Items/Plushies/FlandreScarlet_Plushie_Item");
                Main.tileTexture[ModContent.TileType<FlandreScarlet_Plushie_Tile>()] = GetTexture("Tiles/Plushies/FlandreScarlet_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<FlandreScarlet_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/FlandreScarlet_Plushie_Projectile");

                // Sakuya Kourindou Plushie
                Main.itemTexture[ModContent.ItemType<Kourindou_SakuyaIzayoi_Plushie_Item>()] = GetTexture("Items/Plushies/Kourindou_SakuyaIzayoi_Plushie_Item");
                Main.tileTexture[ModContent.TileType<Kourindou_SakuyaIzayoi_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Kourindou_SakuyaIzayoi_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<Kourindou_SakuyaIzayoi_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/Kourindou_SakuyaIzayoi_Plushie_Projectile");

                // Patchouli Plushie
                Main.itemTexture[ModContent.ItemType<PatchouliKnowledge_Plushie_Item>()] = GetTexture("Items/Plushies/PatchouliKnowledge_Plushie_Item");
                Main.tileTexture[ModContent.TileType<PatchouliKnowledge_Plushie_Tile>()] = GetTexture("Tiles/Plushies/PatchouliKnowledge_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<PatchouliKnowledge_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/PatchouliKnowledge_Plushie_Projectile");

                // Koakuma Plushie

                // Hong Plushie
                Main.itemTexture[ModContent.ItemType<HongMeiling_Plushie_Item>()] = GetTexture("Items/Plushies/HongMeiling_Plushie_Item");
                Main.tileTexture[ModContent.TileType<HongMeiling_Plushie_Tile>()] = GetTexture("Tiles/Plushies/HongMeiling_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<HongMeiling_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/HongMeiling_Plushie_Projectile");

                // Cirno Plushie
                Main.itemTexture[ModContent.ItemType<Cirno_Plushie_Item>()] = GetTexture("Items/Plushies/Cirno_Plushie_Item");
                Main.tileTexture[ModContent.TileType<Cirno_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Cirno_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<Cirno_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/Cirno_Plushie_Projectile");

                // Daiyousei Plushie

                // Rumia Plushie
                Main.itemTexture[ModContent.ItemType<Rumia_Plushie_Item>()] = GetTexture("Items/Plushies/Rumia_Plushie_Item");
                Main.tileTexture[ModContent.TileType<Rumia_Plushie_Tile>()] = GetTexture("Tiles/Plushies/Rumia_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<Rumia_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/Rumia_Plushie_Projectile");

                // Aya Plushie
                Main.itemTexture[ModContent.ItemType<AyaShameimaru_Plushie_Item>()] = GetTexture("Items/Plushies/AyaShameimaru_Plushie_Item");
                Main.tileTexture[ModContent.TileType<AyaShameimaru_Plushie_Tile>()] = GetTexture("Tiles/Plushies/AyaShameimaru_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<AyaShameimaru_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/AyaShameimaru_Plushie_Projectile");

                // Eirin Plushie
                Main.itemTexture[ModContent.ItemType<EirinYagokoro_Plushie_Item>()] = GetTexture("Items/Plushies/EirinYagokoro_Plushie_Item");
                Main.tileTexture[ModContent.TileType<EirinYagokoro_Plushie_Tile>()] = GetTexture("Tiles/Plushies/EirinYagokoro_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<EirinYagokoro_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/EirinYagokoro_Plushie_Projectile");

                // Mokou Plushie
                Main.itemTexture[ModContent.ItemType<FujiwaraNoMokou_Plushie_Item>()] = GetTexture("Items/Plushies/FujiwaraNoMokou_Plushie_Item");
                Main.tileTexture[ModContent.TileType<FujiwaraNoMokou_Plushie_Tile>()] = GetTexture("Tiles/Plushies/FujiwaraNoMokou_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<FujiwaraNoMokou_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/FujiwaraNoMokou_Plushie_Projectile");

                // Hina Plushie

                // Kaguya Plushie
                Main.itemTexture[ModContent.ItemType<KaguyaHouraisan_Plushie_Item>()] = GetTexture("Items/Plushies/KaguyaHouraisan_Plushie_Item");
                Main.tileTexture[ModContent.TileType<KaguyaHouraisan_Plushie_Tile>()] = GetTexture("Tiles/Plushies/KaguyaHouraisan_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<KaguyaHouraisan_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/KaguyaHouraisan_Plushie_Projectile");

                // Kanako Plushie

                // Keine Plushie

                // Kisume Plushie

                // Momiji Plushie

                // Nitori Plushie

                // Reisen Plushie
                Main.itemTexture[ModContent.ItemType<ReisenUdongeinInaba_Plushie_Item>()] = GetTexture("Items/Plushies/ReisenUdongeinInaba_Plushie_Item");
                Main.tileTexture[ModContent.TileType<ReisenUdongeinInaba_Plushie_Tile>()] = GetTexture("Tiles/Plushies/ReisenUdongeinInaba_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<ReisenUdongeinInaba_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/ReisenUdongeinInaba_Plushie_Projectile");

                // Sanea Plushie

                // Suwako Plushie
                Main.itemTexture[ModContent.ItemType<SuwakoMoriya_Plushie_Item>()] = GetTexture("Items/Plushies/SuwakoMoriya_Plushie_Item");
                Main.tileTexture[ModContent.TileType<SuwakoMoriya_Plushie_Tile>()] = GetTexture("Tiles/Plushies/SuwakoMoriya_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<SuwakoMoriya_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/SuwakoMoriya_Plushie_Projectile");

                // Tewi Plushie
                Main.itemTexture[ModContent.ItemType<TewiInaba_Plushie_Item>()] = GetTexture("Items/Plushies/TewiInaba_Plushie_Item");
                Main.tileTexture[ModContent.TileType<TewiInaba_Plushie_Tile>()] = GetTexture("Tiles/Plushies/TewiInaba_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<TewiInaba_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/TewiInaba_Plushie_Projectile");

                // Koishi Plushie
                Main.itemTexture[ModContent.ItemType<KoishiKomeiji_Plushie_Item>()] = GetTexture("Items/Plushies/KoishiKomeiji_Plushie_Item");
                Main.tileTexture[ModContent.TileType<KoishiKomeiji_Plushie_Tile>()] = GetTexture("Tiles/Plushies/KoishiKomeiji_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<KoishiKomeiji_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/KoishiKomeiji_Plushie_Projectile");
				
				// Shion Plushie
				Main.itemTexture[ModContent.ItemType<ShionYorigami_Plushie_Item>()] = GetTexture("Items/Plushies/ShionYorigami_Plushie_Item");
                Main.tileTexture[ModContent.TileType<ShionYorigami_Plushie_Tile>()] = GetTexture("Tiles/Plushies/ShionYorigami_Plushie_Tile");
                Main.projectileTexture[ModContent.ProjectileType<ShionYorigami_Plushie_Projectile>()] = GetTexture("Projectiles/Plushies/ShionYorigami_Plushie_Projectile");
				
				// Kokoro Plushie
            }
        }
    }
}