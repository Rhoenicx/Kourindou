﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Kourindou.Items;
using Kourindou.Items.Consumables;
using Kourindou.Items.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Plushies;
using Kourindou.Tiles.Plants;
using Kourindou.Projectiles.Plushies;
using ReLogic.Content;
using static Terraria.ModLoader.ModContent;

namespace Kourindou
{
    public class PlushieTileTexture
    {
        public Asset<Texture2D> TileTexture { get; set; }
        public Asset<Texture2D> OldTileTexture { get; set; }
    }

    public class PlushieItemTexture
    {
        public Asset<Texture2D> ItemTexture { get; set; }
        public Asset<Texture2D> OldItemTexture { get; set; }
    }

    public class PlushieProjectileTexture
    {
        public Asset<Texture2D> ProjectileTexture { get; set; }
        public Asset<Texture2D> OldProjectileTexture { get; set; }
    }

    class Kourindou : Mod
    {
        public static Dictionary<int, PlushieTileTexture> PlushieTileTextures;
        public static Dictionary<int, PlushieItemTexture> PlushieItemTextures;
        public static Dictionary<int, PlushieProjectileTexture> PlushieProjectileTextures;
        public static Dictionary<string, SoundStyle> SoundDictionary;

        // Recipegroups hack
        public static HashSet<int> FabricItems;
        public static HashSet<int> ThreadItems;

        // Mod Instance
        internal static Kourindou Instance;

        // Mod config
        internal static KourindouConfigClient KourindouConfigClient;

        // Keybinds
        public static ModKeybind SkillKey;

        // Catalyst indentifiers
        public static int NewCatalystID = 0;

        // Spear item ids
        public static HashSet<int> SpearItems;

        // Kourindou Mod Instance
        public Kourindou()
        {
            Instance = this;
        }

        #region GensokyoMod
        // Gensokyo Mod Instance
        public static Mod Gensokyo;
        public static bool GensokyoLoaded;

        // Gensokyo Mod NPC Types
        public static int Gensokyo_AliceMargatroid_Type;
        public static int Gensokyo_Cirno_Type;
        public static int Gensokyo_EternityLarva_Type;
        public static int Gensokyo_HinaKagiyama_Type;
        public static int Gensokyo_KaguyaHouraisan_Type;
        public static int Gensokyo_Kisume_Type;
        public static int Gensokyo_LilyWhite_Type;
        public static int Gensokyo_MayumiJoutouguu_Type;
        public static int Gensokyo_MedicineMelancholy_Type;
        public static int Gensokyo_MinamitsuMurasa_Type;
        public static int Gensokyo_NitoriKawashiro_Type;
        public static int Gensokyo_Rumia_Type;
        public static int Gensokyo_SakuyaIzayoi_Type;
        public static int Gensokyo_SeijaKijin_Type;
        public static int Gensokyo_Sekibanki_Type;
        public static int Gensokyo_KoishiKomeiji_Type;
        public static int Gensokyo_TenshiHinanawi_Type;
        public static int Gensokyo_ToyosatomimiNoMiko_Type;
        public static int Gensokyo_UtsuhoReiuji_Type;
        public static int Gensokyo_Seiran_Type;
        public static int Gensokyo_CasterDoll_Type;
        public static int Gensokyo_LancerDoll_Type;
        public static int Gensokyo_Fairy_Bone_Type;
        public static int Gensokyo_Fairy_Flower_Type;
        public static int Gensokyo_Fairy_Lava_Type;
        public static int Gensokyo_Fairy_Snow_Type;
        public static int Gensokyo_Fairy_Stone_Type;
        public static int Gensokyo_Fairy_Sunflower_Type;
        public static int Gensokyo_Fairy_Thorn_Type;
        public static int Gensokyo_Fairy_Crystal_Type;
        public static int Gensokyo_Fairy_Spore_Type;
        public static int Gensokyo_Fairy_Sand_Type;
        public static int Gensokyo_Fairy_Water_Type;
        public static int Gensokyo_Fairy_Blood_Type;
        public static int Gensokyo_Fairy_Metal_Type;
        #endregion

        // Load
        public override void Load()
        {
            SkillKey = KeybindLoader.RegisterKeybind(this, "Skill", "Mouse2");

            SoundDictionary = new Dictionary<string, SoundStyle>
            {
                { "Grass", SoundID.Grass },
                { "DD2_ExplosiveTrapExplode", SoundID.DD2_ExplosiveTrapExplode }
            };

            FabricItems = new HashSet<int>();
            ThreadItems = new HashSet<int>();
            SpearItems = new HashSet<int>();

            KourindouSpellcardSystem.Load();

            //code that has to be run on clients only!
            if (!Main.dedServ)
            {
                LoadPlushieTextures();

                SwitchModTextures(true);
            }
        }

        // Unload
        public override void Unload()
        {
            KourindouConfigClient = null;

            SkillKey = null;

            Instance = null;
            Gensokyo = null;

            SoundDictionary = null;

            FabricItems = null;
            ThreadItems = null;
            SpearItems = null;

            KourindouSpellcardSystem.Unload();

            //code that has to be run on clients only!
            if (!Main.dedServ)
            {
                PlushieTileTextures = null;
                PlushieItemTextures = null;
                PlushieProjectileTextures = null;

                SwitchModTextures(false);
            }

            base.Unload();
        }

        public override object Call(params object[] args)
        {
            try
            {
                string message = args[0] as string;

                switch (message)
                {
                    case "AddGensokyoShopItem":
                        if (Gensokyo != null)
                        {
                            if (Gensokyo.Version <= new Version(0, 9, 45, 10))
                            {
                                Gensokyo.Call(
                                   "AddShopItem",
                                   (int)args[1],
                                   "Consumables",
                                   ItemType<FumoCola>(),
                                   true
                               );
                            }
                            else
                            {
                                Gensokyo.Call(
                                    "AddShopItem",
                                    (int)args[1],
                                    true,
                                    "Consumables",
                                    ItemType<FumoCola>(),
                                    1,
                                    (int)ItemID.None,
                                    0
                                );
                            }
                        }

                        return "Success";
                }
                Logger.Debug("Kourindou Call Error: Unknown Message: " + message);
            }
            catch (Exception e)
            {
                Logger.Warn("Kourindou Call Error: " + e.StackTrace + e.Message);
            }

            return "Failure";
        }

        // PostSetupContent - Register mods for compatibility
        public override void PostSetupContent()
        {
            // Check loaded mods
            GensokyoLoaded = ModLoader.TryGetMod("Gensokyo", out Gensokyo);

            // Support for Gensokyo Mod
            if (Gensokyo != null)
            {
                CrossModContent.SetupGensokyo(Gensokyo, this);
            }

            // Swap vanilla textures
            if (!Main.dedServ)
            {
                SwitchPlushieTextures();
            }

            FabricSetup();
            ThreadSetup();
            SpearSetup();
        }

        // Add Crafting recipe groups
        public override void AddRecipeGroups()/* tModPorter Note: Removed. Use ModSystem.AddRecipeGroups */
        {
            // Stuffing
            RecipeGroup Stuffing = new (() => Language.GetTextValue("LegacyMisc.37") + " Stuffing", new int[]
            {
                ItemID.Hay,
                ItemType<CottonFibre>()
            });
            RecipeGroup.RegisterGroup("Kourindou:Stuffing", Stuffing);

            // Gemstone
            RecipeGroup Gemstone = new (() => Language.GetTextValue("LegacyMisc.37") + " Gemstone", new int[]
            {
                ItemID.Diamond,
                ItemID.Ruby,
                ItemID.Amber,
                ItemID.Emerald,
                ItemID.Sapphire,
                ItemID.Topaz,
                ItemID.Amethyst
            });
            RecipeGroup.RegisterGroup("Kourindou:Gemstone", Gemstone);

            // Lens
            RecipeGroup Lens = new (() => Language.GetTextValue("LegacyMisc.37") + " Lens", new int[]
            {
                ItemID.Lens,
                ItemID.BlackLens
            });
            RecipeGroup.RegisterGroup("Kourindou:Lens", Lens);

            // Watch
            RecipeGroup Watch = new (() => Language.GetTextValue("LegacyMics.37") + " Watch", new int[]
            {
                ItemID.CopperWatch,
                ItemID.TinWatch,
                ItemID.SilverWatch,
                ItemID.TungstenWatch,
                ItemID.GoldWatch,
                ItemID.PlatinumWatch,
                ItemID.Stopwatch
            });
            RecipeGroup.RegisterGroup("Kourindou:Watch", Watch);

            // Copper bar or tin bar?
            RecipeGroup CopperBar = new (() => "Copper or tin bar", new int[]
            {
                ItemID.CopperBar,
                ItemID.TinBar
            });
            RecipeGroup.RegisterGroup("Kourindou:CopperBar", CopperBar);

        }

        // Handle network packets
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            KourindouMessageType msg = (KourindouMessageType)reader.ReadByte();

            switch(msg)
            {
                // Sync right click button between in multiplayer
                case KourindouMessageType.RightClick:
                { 
                    int playerID = reader.ReadInt32();

                    // Set altFuntionUse (right click) to 2
                    Main.player[playerID].altFunctionUse = 2;

                    // Resend packet to other clients if we're the server
                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)KourindouMessageType.RightClick);
                        packet.Write(playerID);
                        packet.Send(-1, whoAmI);
                    }
                }
                break;

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
                            null,
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
                        Rectangle meleeHitbox = new (X, Y, Width, Height);
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
                        GetInstance<Cotton_Tile>().RightClick(i, j);
                    }
                    break;
                }

                case KourindouMessageType.PlaySound: 
				{
					string soundName = reader.ReadString();
					float soundVolume = reader.ReadSingle();
					float soundVariance = reader.ReadSingle();
					int soundSourceX = reader.ReadInt32();
					int soundSourceY = reader.ReadInt32();

					if (Main.netMode == NetmodeID.Server)
					{
						ModPacket packet = GetPacket();
						packet.Write((byte) KourindouMessageType.PlaySound);
						packet.Write(soundName);
						packet.Write(soundVolume);
						packet.Write(soundVariance);
						packet.Write(soundSourceX);
						packet.Write(soundSourceY);

						packet.Send(-1, whoAmI);
						break;
					}
					
					if (soundSourceX == -1 || soundSourceY == -1)
					{
                        SoundEngine.PlaySound(
							SoundDictionary[soundName] with { Volume = soundVolume, PitchVariance = soundVariance},
							Main.LocalPlayer.Center);
					}
					else
					{
                        SoundEngine.PlaySound(
                            SoundDictionary[soundName] with { Volume = soundVolume, PitchVariance = soundVariance },
                            new Vector2(soundSourceX, soundSourceY));
					}
					break;
				}

                case KourindouMessageType.PlayCustomSound:
				{
					string soundName = reader.ReadString();
					float soundVolume = reader.ReadSingle();
					float soundVariance = reader.ReadSingle();
					int soundPositionX = reader.ReadInt32();
					int soundPositionY = reader.ReadInt32();

					if (Main.netMode == NetmodeID.Server)
					{
						ModPacket packet = GetPacket();
						packet.Write((byte) KourindouMessageType.PlayCustomSound);
						packet.Write(soundName);
						packet.Write(soundVolume);
						packet.Write(soundVariance);
						packet.Write(soundPositionX);
						packet.Write(soundPositionY);

						packet.Send(-1, whoAmI);
						break;
					}

					if (soundPositionX == -1 || soundPositionY == -1)
					{
                        SoundEngine.PlaySound(
                            new SoundStyle("Kourindou/Sounds/Custom/" + soundName) with { Volume = soundVolume, PitchVariance = soundVariance},
                            Main.LocalPlayer.Center);
					}
					else
					{
                        SoundEngine.PlaySound(
                            new SoundStyle("Kourindou/Sounds/Custom/" + soundName) with { Volume = soundVolume, PitchVariance = soundVariance },
                            new Vector2(soundPositionX, soundPositionY));
					}
					break;
				}

                case KourindouMessageType.PlushieItemNetUpdate:
                {
                    int itemSlot = reader.ReadInt32();
                    short PlushieDirtWater = reader.ReadInt16();

                    Item item = Main.item[itemSlot];

                    if (Main.netMode != NetmodeID.Server)
                    {   
                        if (item.ModItem is PlushieItem plushie)
                        {
                            plushie.PlushieDirtWater = PlushieDirtWater;
                        }
                    }
                    break;
                }

                case KourindouMessageType.PlacePlushieTile:
                {
                    int plushiePlaceTileX = reader.ReadInt32();
                    int plushiePlaceTileY = reader.ReadInt32();
                    int plushieTile = reader.ReadInt32();
                    short PlushieDirtWater = reader.ReadInt16();

                    WorldGen.PlaceObject(plushiePlaceTileX, plushiePlaceTileY, plushieTile);
                    KourindouWorld.SetPlushieDirtWater(plushiePlaceTileX, plushiePlaceTileY - 1, PlushieDirtWater);
                    break;
                }

                case KourindouMessageType.SetPlushieDirtWater:
                {
                    int i = reader.ReadInt32();
                    int j = reader.ReadInt32();
                    short PlushieDirtWater = reader.ReadInt16();

                    KourindouWorld.SetPlushieDirtWater(i, j, PlushieDirtWater);

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte) KourindouMessageType.SetPlushieDirtWater);
                        packet.Write((int) i);
                        packet.Write((int) j);
                        packet.Write((int) PlushieDirtWater);
                        packet.Send(-1, whoAmI);
                    }
                    break;
                }

                case KourindouMessageType.RanPlushieStacks:
                {
                    byte PlayerID = reader.ReadByte();
                    byte Stacks = reader.ReadByte();

                    KourindouPlayer player = Main.player[PlayerID].GetModPlayer<KourindouPlayer>();
                    player.RanPlushie_Stacks = Stacks;

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)KourindouMessageType.RanPlushieStacks);
                        packet.Write((byte)PlayerID);
                        packet.Write((byte)Stacks);
                        packet.Send(-1, whoAmI);
                    }
                    break;
                }

                default:
                    Logger.Warn("Kourindou: Unknown NetMessage type: " + msg);
                    break;
            }
        }
 
        public static int GetNewCatalystID() => NewCatalystID++;

        public void LoadPlushieTextures()
        {
            if (PlushieTileTextures == null)
            {
                PlushieTileTextures = new Dictionary<int, PlushieTileTexture>();
            }

            if (PlushieItemTextures == null)
            {
                PlushieItemTextures = new Dictionary<int, PlushieItemTexture>();
            }

            if (PlushieProjectileTextures == null)
            {
                PlushieProjectileTextures = new Dictionary<int, PlushieProjectileTexture>();
            }

            SetPlushieTextures(ModContent.ItemType<ReimuHakurei_Plushie_Item>(), ModContent.TileType<ReimuHakurei_Plushie_Tile>(), ModContent.ProjectileType<ReimuHakurei_Plushie_Projectile>(), "ReimuHakurei");
            SetPlushieTextures(ModContent.ItemType<TenshiHinanawi_Plushie_Item>(), ModContent.TileType<TenshiHinanawi_Plushie_Tile>(), ModContent.ProjectileType<TenshiHinanawi_Plushie_Projectile>(), "TenshiHinanawi");
            SetPlushieTextures(ModContent.ItemType<MarisaKirisame_Plushie_Item>(), ModContent.TileType<MarisaKirisame_Plushie_Tile>(), ModContent.ProjectileType<MarisaKirisame_Plushie_Projectile>(), "MarisaKirisame");
            SetPlushieTextures(ModContent.ItemType<Kourindou_MarisaKirisame_Plushie_Item>(), ModContent.TileType<Kourindou_MarisaKirisame_Plushie_Tile>(), ModContent.ProjectileType<Kourindou_MarisaKirisame_Plushie_Projectile>(), "Kourindou_MarisaKirisame");
            SetPlushieTextures(ModContent.ItemType<AliceMargatroid_Plushie_Item>(), ModContent.TileType<AliceMargatroid_Plushie_Tile>(), ModContent.ProjectileType<AliceMargatroid_Plushie_Projectile>(), "AliceMargatroid");
            SetPlushieTextures(ModContent.ItemType<YoumuKonpaku_Plushie_Item>(), ModContent.TileType<YoumuKonpaku_Plushie_Tile>(), ModContent.ProjectileType<YoumuKonpaku_Plushie_Projectile>(), "YoumuKonpaku");
            SetPlushieTextures(ModContent.ItemType<YuyukoSaigyouji_Plushie_Item>(), ModContent.TileType<YuyukoSaigyouji_Plushie_Tile>(), ModContent.ProjectileType<YuyukoSaigyouji_Plushie_Projectile>(), "YuyukoSaigyouji");
            SetPlushieTextures(ModContent.ItemType<Kourindou_ReimuHakurei_Plushie_Item>(), ModContent.TileType<Kourindou_ReimuHakurei_Plushie_Tile>(), ModContent.ProjectileType<Kourindou_ReimuHakurei_Plushie_Projectile>(), "Kourindou_ReimuHakurei");
            SetPlushieTextures(ModContent.ItemType<Kourindou_RemiliaScarlet_Plushie_Item>(), ModContent.TileType<Kourindou_RemiliaScarlet_Plushie_Tile>(), ModContent.ProjectileType<Kourindou_RemiliaScarlet_Plushie_Projectile>(), "Kourindou_RemiliaScarlet");
            SetPlushieTextures(ModContent.ItemType<FlandreScarlet_Plushie_Item>(), ModContent.TileType<FlandreScarlet_Plushie_Tile>(), ModContent.ProjectileType<FlandreScarlet_Plushie_Projectile>(), "FlandreScarlet");
            SetPlushieTextures(ModContent.ItemType<Kourindou_SakuyaIzayoi_Plushie_Item>(), ModContent.TileType<Kourindou_SakuyaIzayoi_Plushie_Tile>(), ModContent.ProjectileType<Kourindou_SakuyaIzayoi_Plushie_Projectile>(), "Kourindou_SakuyaIzayoi");
            SetPlushieTextures(ModContent.ItemType<PatchouliKnowledge_Plushie_Item>(), ModContent.TileType<PatchouliKnowledge_Plushie_Tile>(), ModContent.ProjectileType<PatchouliKnowledge_Plushie_Projectile>(), "PatchouliKnowledge");
            SetPlushieTextures(ModContent.ItemType<HongMeiling_Plushie_Item>(), ModContent.TileType<HongMeiling_Plushie_Tile>(), ModContent.ProjectileType<HongMeiling_Plushie_Projectile>(), "HongMeiling");
            SetPlushieTextures(ModContent.ItemType<Cirno_Plushie_Item>(), ModContent.TileType<Cirno_Plushie_Tile>(), ModContent.ProjectileType<Cirno_Plushie_Projectile>(), "Cirno");
            SetPlushieTextures(ModContent.ItemType<Rumia_Plushie_Item>(), ModContent.TileType<Rumia_Plushie_Tile>(), ModContent.ProjectileType<Rumia_Plushie_Projectile>(), "Rumia");
            SetPlushieTextures(ModContent.ItemType<AyaShameimaru_Plushie_Item>(), ModContent.TileType<AyaShameimaru_Plushie_Tile>(), ModContent.ProjectileType<AyaShameimaru_Plushie_Projectile>(), "AyaShameimaru");
            SetPlushieTextures(ModContent.ItemType<EirinYagokoro_Plushie_Item>(), ModContent.TileType<EirinYagokoro_Plushie_Tile>(), ModContent.ProjectileType<EirinYagokoro_Plushie_Projectile>(), "EirinYagokoro");
            SetPlushieTextures(ModContent.ItemType<FujiwaraNoMokou_Plushie_Item>(), ModContent.TileType<FujiwaraNoMokou_Plushie_Tile>(), ModContent.ProjectileType<FujiwaraNoMokou_Plushie_Projectile>(), "FujiwaraNoMokou");
            SetPlushieTextures(ModContent.ItemType<KaguyaHouraisan_Plushie_Item>(), ModContent.TileType<KaguyaHouraisan_Plushie_Tile>(), ModContent.ProjectileType<KaguyaHouraisan_Plushie_Projectile>(), "KaguyaHouraisan");
            SetPlushieTextures(ModContent.ItemType<ReisenUdongeinInaba_Plushie_Item>(), ModContent.TileType<ReisenUdongeinInaba_Plushie_Tile>(), ModContent.ProjectileType<ReisenUdongeinInaba_Plushie_Projectile>(), "ReisenUdongeinInaba");
            SetPlushieTextures(ModContent.ItemType<SanaeKochiya_Plushie_Item>(), ModContent.TileType<SanaeKochiya_Plushie_Tile>(), ModContent.ProjectileType<SanaeKochiya_Plushie_Projectile>(), "SanaeKochiya");
            SetPlushieTextures(ModContent.ItemType<SuwakoMoriya_Plushie_Item>(), ModContent.TileType<SuwakoMoriya_Plushie_Tile>(), ModContent.ProjectileType<SuwakoMoriya_Plushie_Projectile>(), "SuwakoMoriya");
            SetPlushieTextures(ModContent.ItemType<TewiInaba_Plushie_Item>(), ModContent.TileType<TewiInaba_Plushie_Tile>(), ModContent.ProjectileType<TewiInaba_Plushie_Projectile>(), "TewiInaba");
            SetPlushieTextures(ModContent.ItemType<KoishiKomeiji_Plushie_Item>(), ModContent.TileType<KoishiKomeiji_Plushie_Tile>(), ModContent.ProjectileType<KoishiKomeiji_Plushie_Projectile>(), "KoishiKomeiji");
            SetPlushieTextures(ModContent.ItemType<ShionYorigami_Plushie_Item>(), ModContent.TileType<ShionYorigami_Plushie_Tile>(), ModContent.ProjectileType<ShionYorigami_Plushie_Projectile>(), "ShionYorigami");
            SetPlushieTextures(ModContent.ItemType<InuSakuyaIzayoi_Plushie_Item>(), ModContent.TileType<InuSakuyaIzayoi_Plushie_Tile>(), ModContent.ProjectileType<InuSakuyaIzayoi_Plushie_Projectile>(), "InuSakuyaIzayoi");
            SetPlushieTextures(ModContent.ItemType<SatoriKomeiji_Plushie_Item>(), ModContent.TileType<SatoriKomeiji_Plushie_Tile>(), ModContent.ProjectileType<SatoriKomeiji_Plushie_Projectile>(), "SatoriKomeiji");
            SetPlushieTextures(ModContent.ItemType<SuikaIbuki_Plushie_Item>(), ModContent.TileType<SuikaIbuki_Plushie_Tile>(), ModContent.ProjectileType<SuikaIbuki_Plushie_Projectile>(), "SuikaIbuki");
            SetPlushieTextures(ModContent.ItemType<YuukaKazami_Plushie_Item>(), ModContent.TileType<YuukaKazami_Plushie_Tile>(), ModContent.ProjectileType<YuukaKazami_Plushie_Projectile>(), "YuukaKazami");
            SetPlushieTextures(ModContent.ItemType<KasenIbaraki_Plushie_Item>(), ModContent.TileType<KasenIbaraki_Plushie_Tile>(), ModContent.ProjectileType<KasenIbaraki_Plushie_Projectile>(), "KasenIbaraki");
            SetPlushieTextures(ModContent.ItemType<HatsuneMiku_Plushie_Item>(), ModContent.TileType<HatsuneMiku_Plushie_Tile>(), ModContent.ProjectileType<HatsuneMiku_Plushie_Projectile>(), "HatsuneMiku");
            SetPlushieTextures(ModContent.ItemType<Chen_Plushie_Item>(), ModContent.TileType<Chen_Plushie_Tile>(), ModContent.ProjectileType<Chen_Plushie_Projectile>(), "Chen");
            SetPlushieTextures(ModContent.ItemType<YukariYakumo_Plushie_Item>(), ModContent.TileType<YukariYakumo_Plushie_Tile>(), ModContent.ProjectileType<YukariYakumo_Plushie_Projectile>(), "YukariYakumo");
        }

        public void SetPlushieTextures(int item, int tile ,int projectile, string itemName)
        {
            if (!PlushieTileTextures.ContainsKey(tile))
            {
                PlushieTileTextures.Add(tile, new PlushieTileTexture 
                {
                    TileTexture = Request<Texture2D>("Kourindou/Tiles/Plushies/" + itemName + "_Plushie_Tile"),
                    OldTileTexture = Request<Texture2D>("Kourindou/Tiles/Plushies/" + itemName + "_Plushie_Tile_Old") 
                });
            }

            if (!PlushieItemTextures.ContainsKey(item))
            {
                PlushieItemTextures.Add(item, new PlushieItemTexture 
                { 
                    ItemTexture = Request<Texture2D>("Kourindou/Items/Plushies/" + itemName + "_Plushie_Item"), 
                    OldItemTexture = Request<Texture2D>("Kourindou/Items/Plushies/" + itemName + "_Plushie_Item_Old") 
                });
            }

            if (!PlushieProjectileTextures.ContainsKey(projectile))
            {
                PlushieProjectileTextures.Add(projectile, new PlushieProjectileTexture
                {
                    ProjectileTexture = Request<Texture2D>("Kourindou/Projectiles/Plushies/" + itemName + "_Plushie_Projectile"),
                    OldProjectileTexture = Request<Texture2D>("Kourindou/Projectiles/Plushies/" + itemName + "_Plushie_Projectile_Old")
                });
            }
        }

        public static void SwitchPlushieTextures()
        {
            foreach (KeyValuePair<int, PlushieItemTexture> entry in PlushieItemTextures)
            {
                TextureAssets.Item[entry.Key] = KourindouConfigClient.UseOldTextures ? entry.Value.OldItemTexture : entry.Value.ItemTexture;
            }

            foreach (KeyValuePair<int, PlushieProjectileTexture> entry in PlushieProjectileTextures)
            { 
                TextureAssets.Projectile[entry.Key] = KourindouConfigClient.UseOldTextures ? entry.Value.OldProjectileTexture : entry.Value.ProjectileTexture;
            }

            foreach (KeyValuePair<int, PlushieTileTexture> entry in PlushieTileTextures)
            {
                TextureAssets.Tile[entry.Key] = KourindouConfigClient.UseOldTextures ? entry.Value.OldTileTexture : entry.Value.TileTexture;
            }
        }

        public static void SwitchModTextures(bool loading)
        {
            // Thread
            TextureAssets.Item[ItemID.BlackThread] = loading ? Request<Texture2D>("Kourindou/Items/CraftingMaterials/BlackThread") : Main.Assets.Request<Texture2D>("Images\\Item_254", 0);
            TextureAssets.Item[ItemID.GreenThread] = loading ? Request<Texture2D>("Kourindou/Items/CraftingMaterials/GreenThread") : Main.Assets.Request<Texture2D>("Images\\Item_255", 0);
            TextureAssets.Item[ItemID.PinkThread] = loading ? Request<Texture2D>("Kourindou/Items/CraftingMaterials/PinkThread") : Main.Assets.Request<Texture2D>("Images\\Item_981", 0);
        }

        private static void FabricSetup()
        {
            FabricItems.Add(ItemType<BlackFabric>());
            FabricItems.Add(ItemType<BlueFabric>());
            FabricItems.Add(ItemType<BrownFabric>());
            FabricItems.Add(ItemType<CyanFabric>());
            FabricItems.Add(ItemType<GreenFabric>());
            FabricItems.Add(ItemType<LimeFabric>());
            FabricItems.Add(ItemType<OrangeFabric>());
            FabricItems.Add(ItemType<PinkFabric>());
            FabricItems.Add(ItemType<PurpleFabric>());
            FabricItems.Add(ItemType<RedFabric>());
            FabricItems.Add(ItemType<SilverFabric>());
            FabricItems.Add(ItemType<SkyBlueFabric>());
            FabricItems.Add(ItemType<TealFabric>());
            FabricItems.Add(ItemType<VioletFabric>());
            FabricItems.Add(ItemID.Silk);
            FabricItems.Add(ItemType<YellowFabric>());
            FabricItems.Add(ItemType<RainbowFabric>());
        }

        private static void ThreadSetup()
        {
            ThreadItems.Add(ItemID.BlackThread);
            ThreadItems.Add(ItemType<BlueThread>());
            ThreadItems.Add(ItemType<BrownThread>());
            ThreadItems.Add(ItemType<CyanThread>());
            ThreadItems.Add(ItemID.GreenThread);
            ThreadItems.Add(ItemType<LimeThread>());
            ThreadItems.Add(ItemType<OrangeThread>());
            ThreadItems.Add(ItemID.PinkThread);
            ThreadItems.Add(ItemType<PurpleThread>());
            ThreadItems.Add(ItemType<RedThread>());
            ThreadItems.Add(ItemType<SilverThread>());
            ThreadItems.Add(ItemType<SkyBlueThread>());
            ThreadItems.Add(ItemType<TealThread>());
            ThreadItems.Add(ItemType<VioletThread>());
            ThreadItems.Add(ItemType<WhiteThread>());
            ThreadItems.Add(ItemType<YellowThread>());
            ThreadItems.Add(ItemType<RainbowThread>());
        }

        private static void SpearSetup()
        {
            // Add vanilla spears
            SpearItems.Add(ItemID.AdamantiteGlaive);
            SpearItems.Add(ItemID.ChlorophytePartisan);
            SpearItems.Add(ItemID.CobaltNaginata);
            SpearItems.Add(ItemID.DarkLance);
            SpearItems.Add(ItemID.MonkStaffT2);
            SpearItems.Add(ItemID.Gungnir);
            SpearItems.Add(ItemID.MushroomSpear);
            SpearItems.Add(ItemID.MythrilHalberd);
            SpearItems.Add(ItemID.OrichalcumHalberd);
            SpearItems.Add(ItemID.NorthPole);
            SpearItems.Add(ItemID.ObsidianSwordfish);
            SpearItems.Add(ItemID.PalladiumPike);
            SpearItems.Add(ItemID.Spear);
            SpearItems.Add(ItemID.ThunderSpear);
            SpearItems.Add(ItemID.Swordfish);
            SpearItems.Add(ItemID.TheRottedFork);
            SpearItems.Add(ItemID.TitaniumTrident);
            SpearItems.Add(ItemID.Trident);
            SpearItems.Add(ItemID.UnholyTrident);

            // Add Calamity spears
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamityMod))
            {
                if (calamityMod.TryFind<ModItem>("AmidiasTrident", out ModItem AmidiasTrident)) { SpearItems.Add(AmidiasTrident.Type); }
                if (calamityMod.TryFind<ModItem>("AstralPike", out ModItem AstralPike)) { SpearItems.Add(AstralPike.Type); }
                if (calamityMod.TryFind<ModItem>("BansheeHook", out ModItem BansheeHook)) { SpearItems.Add(BansheeHook.Type); }
                if (calamityMod.TryFind<ModItem>("BotanicPiercer", out ModItem BotanicPiercer)) { SpearItems.Add(BotanicPiercer.Type); }
                if (calamityMod.TryFind<ModItem>("BrimLance", out ModItem BrimLance)) { SpearItems.Add(BrimLance.Type); }
                if (calamityMod.TryFind<ModItem>("DiseasedPike", out ModItem DiseasedPike)) { SpearItems.Add(DiseasedPike.Type); }
                if (calamityMod.TryFind<ModItem>("EarthenPike", out ModItem EarthenPike)) { SpearItems.Add(EarthenPike.Type); }
                if (calamityMod.TryFind<ModItem>("ElementalLance", out ModItem ElementalLance)) { SpearItems.Add(ElementalLance.Type); }
                if (calamityMod.TryFind<ModItem>("GalvanizingGlaive", out ModItem GalvanizingGlaive)) { SpearItems.Add(GalvanizingGlaive.Type); }
                if (calamityMod.TryFind<ModItem>("GildedProboscis", out ModItem GildedProboscis)) { SpearItems.Add(GildedProboscis.Type); }
                if (calamityMod.TryFind<ModItem>("GoldplumeSpear", out ModItem GoldplumeSpear)) { SpearItems.Add(GoldplumeSpear.Type); }
                if (calamityMod.TryFind<ModItem>("HellionFlowerSpear", out ModItem HellionFlowerSpear)) { SpearItems.Add(HellionFlowerSpear.Type); }
                if (calamityMod.TryFind<ModItem>("Nadir", out ModItem Nadir)) { SpearItems.Add(Nadir.Type); }
                if (calamityMod.TryFind<ModItem>("RedtideSpear", out ModItem RedtideSpear)) { SpearItems.Add(RedtideSpear.Type); }
                if (calamityMod.TryFind<ModItem>("SausageMaker", out ModItem SausageMaker)) { SpearItems.Add(SausageMaker.Type); }
                if (calamityMod.TryFind<ModItem>("StarnightLance", out ModItem StarnightLance)) { SpearItems.Add(StarnightLance.Type); }
                if (calamityMod.TryFind<ModItem>("TenebreusTides", out ModItem TenebreusTides)) { SpearItems.Add(TenebreusTides.Type); }
                if (calamityMod.TryFind<ModItem>("VulcaniteLance", out ModItem VulcaniteLance)) { SpearItems.Add(VulcaniteLance.Type); }
                if (calamityMod.TryFind<ModItem>("YateveoBloom", out ModItem YateveoBloom)) { SpearItems.Add(YateveoBloom.Type); }
            }

            // Add Thorium spears
            if (ModLoader.TryGetMod("ThoriumMod", out Mod thoriumMod))
            {
                if (thoriumMod.TryFind<ModItem>("CoralPolearm", out ModItem CoralPolearm)) { SpearItems.Add(CoralPolearm.Type); }
                if (thoriumMod.TryFind<ModItem>("DemonBloodSpear", out ModItem DemonBloodSpear)) { SpearItems.Add(DemonBloodSpear.Type); }
                if (thoriumMod.TryFind<ModItem>("DragonTalon", out ModItem DragonTalon)) { SpearItems.Add(DragonTalon.Type); }
                if (thoriumMod.TryFind<ModItem>("DreadFork", out ModItem DreadFork)) { SpearItems.Add(DreadFork.Type); }
                if (thoriumMod.TryFind<ModItem>("EnergyStormPartisan", out ModItem EnergyStormPartisan)) { SpearItems.Add(EnergyStormPartisan.Type); }
                if (thoriumMod.TryFind<ModItem>("FleshSkewer", out ModItem FleshSkewer)) { SpearItems.Add(FleshSkewer.Type); }
                if (thoriumMod.TryFind<ModItem>("Fork", out ModItem Fork)) { SpearItems.Add(Fork.Type); }
                if (thoriumMod.TryFind<ModItem>("HarpyTalon", out ModItem HarpyTalon)) { SpearItems.Add(HarpyTalon.Type); }
                if (thoriumMod.TryFind<ModItem>("HellishHalberd", out ModItem HellishHalberd)) { SpearItems.Add(HellishHalberd.Type); }
                if (thoriumMod.TryFind<ModItem>("IceLance", out ModItem IceLance)) { SpearItems.Add(IceLance.Type); }
                if (thoriumMod.TryFind<ModItem>("IllumiteSpear", out ModItem IllumiteSpear)) { SpearItems.Add(IllumiteSpear.Type); }
                if (thoriumMod.TryFind<ModItem>("Moonlight", out ModItem Moonlight)) { SpearItems.Add(Moonlight.Type); }
                if (thoriumMod.TryFind<ModItem>("PearlPike", out ModItem PearlPike)) { SpearItems.Add(PearlPike.Type); }
                if (thoriumMod.TryFind<ModItem>("PollenPike", out ModItem PollenPike)) { SpearItems.Add(PollenPike.Type); }
                if (thoriumMod.TryFind<ModItem>("PoseidonCharge", out ModItem PoseidonCharge)) { SpearItems.Add(PoseidonCharge.Type); }
                if (thoriumMod.TryFind<ModItem>("RifleSpear", out ModItem RifleSpear)) { SpearItems.Add(RifleSpear.Type); }
                if (thoriumMod.TryFind<ModItem>("fSandStoneSpear", out ModItem fSandStoneSpear)) { SpearItems.Add(fSandStoneSpear.Type); }
                if (thoriumMod.TryFind<ModItem>("TerrariumSpear", out ModItem TerrariumSpear)) { SpearItems.Add(TerrariumSpear.Type); }
                if (thoriumMod.TryFind<ModItem>("ThoriumSpear", out ModItem ThoriumSpear)) { SpearItems.Add(ThoriumSpear.Type); }
                if (thoriumMod.TryFind<ModItem>("ValadiumSpear", out ModItem ValadiumSpear)) { SpearItems.Add(ValadiumSpear.Type); }
            }

            // Add TOverHaul spears
            if (ModLoader.TryGetMod("TerrariaOverhaul", out Mod terrariaOverhaul))
            {
                if (terrariaOverhaul.TryFind<ModItem>("Mop", out ModItem Mop)) { SpearItems.Add(Mop.Type); }
            }

            // Add Fargos spears
            if (ModLoader.TryGetMod("FargowiltasSouls", out Mod fargowiltasSouls))
            {
                if (fargowiltasSouls.TryFind<ModItem>("UmbraRegalia", out ModItem UmbraRegalia)) { SpearItems.Add(UmbraRegalia.Type); }
                if (fargowiltasSouls.TryFind<ModItem>("PrismaRegalia", out ModItem PrismaRegalia)) { SpearItems.Add(PrismaRegalia.Type); }
            }

            // Add TSA spears
            if (ModLoader.TryGetMod("StarsAbove", out Mod starsAbove))
            {
                if (starsAbove.TryFind<ModItem>("RexLapis", out ModItem RexLapis)) { SpearItems.Add(RexLapis.Type); }
                if (starsAbove.TryFind<ModItem>("Drachenlance", out ModItem Drachenlance)) { SpearItems.Add(Drachenlance.Type); }
            }

            // Add Starlight river spears
            if (ModLoader.TryGetMod("StarlightRiver", out Mod starlightRiver))
            {
                if (starlightRiver.TryFind<ModItem>("CrescentQuarterstaff", out ModItem CrescentQuarterstaff)) { SpearItems.Add(CrescentQuarterstaff.Type); }
                if (starlightRiver.TryFind<ModItem>("FacetAndLattice", out ModItem FacetAndLattice)) { SpearItems.Add(FacetAndLattice.Type); }
                if (starlightRiver.TryFind<ModItem>("TempleSpear", out ModItem TempleSpear)) { SpearItems.Add(TempleSpear.Type); }
                if (starlightRiver.TryFind<ModItem>("Tentalance", out ModItem Tentalance)) { SpearItems.Add(Tentalance.Type); }
            }

            // Add spirit mod spears
            if (ModLoader.TryGetMod("SpiritMod", out Mod spiritMod))
            {
                if (spiritMod.TryFind<ModItem>("BismiteSpear", out ModItem BismiteSpear)) { SpearItems.Add(BismiteSpear.Type); }
                if (spiritMod.TryFind<ModItem>("MarbleBident", out ModItem MarbleBident)) { SpearItems.Add(MarbleBident.Type); }
                if (spiritMod.TryFind<ModItem>("ReefSpear", out ModItem ReefSpear)) { SpearItems.Add(ReefSpear.Type); }
                if (spiritMod.TryFind<ModItem>("EowSpear", out ModItem EowSpear)) { SpearItems.Add(EowSpear.Type); }
                if (spiritMod.TryFind<ModItem>("Talonginus", out ModItem Talonginus)) { SpearItems.Add(Talonginus.Type); }
                if (spiritMod.TryFind<ModItem>("DuskLance", out ModItem DuskLance)) { SpearItems.Add(DuskLance.Type); }
                if (spiritMod.TryFind<ModItem>("FearsomeFork", out ModItem FearsomeFork)) { SpearItems.Add(FearsomeFork.Type); }
                if (spiritMod.TryFind<ModItem>("Sovereign_Talon", out ModItem Sovereign_Talon)) { SpearItems.Add(Sovereign_Talon.Type); }
                if (spiritMod.TryFind<ModItem>("SpiritSpear", out ModItem SpiritSpear)) { SpearItems.Add(SpiritSpear.Type); }
            }

            // Add Gensokyo spears
            if (ModLoader.TryGetMod("Gensokyo", out Mod gensokyo))
            {
                if (gensokyo.TryFind<ModItem>("BlessedSpear", out ModItem BlessedSpear)) { SpearItems.Add(BlessedSpear.Type); }
                if (gensokyo.TryFind<ModItem>("ShiningNeedleSword", out ModItem ShiningNeedleSword)) { SpearItems.Add(ShiningNeedleSword.Type); }
                if (gensokyo.TryFind<ModItem>("UncannyTrident", out ModItem UncannyTrident)) { SpearItems.Add(UncannyTrident.Type); }
            }

            // Add Vitality mod spears
            if (ModLoader.TryGetMod("VitalityMod", out Mod vitalityMod))
            {
                if (vitalityMod.TryFind<ModItem>("CopperSpear", out ModItem CopperSpear)) { SpearItems.Add(CopperSpear.Type); }
                if (vitalityMod.TryFind<ModItem>("TinSpear", out ModItem TinSpear)) { SpearItems.Add(TinSpear.Type); }
                if (vitalityMod.TryFind<ModItem>("IronSpear", out ModItem IronSpear)) { SpearItems.Add(IronSpear.Type); }
                if (vitalityMod.TryFind<ModItem>("LeadSpear", out ModItem LeadSpear)) { SpearItems.Add(LeadSpear.Type); }
                if (vitalityMod.TryFind<ModItem>("SilverSpear", out ModItem SilverSpear)) { SpearItems.Add(SilverSpear.Type); }
                if (vitalityMod.TryFind<ModItem>("TungstenSpear", out ModItem TungstenSpear)) { SpearItems.Add(TungstenSpear.Type); }
                if (vitalityMod.TryFind<ModItem>("GoldSpear", out ModItem GoldSpear)) { SpearItems.Add(GoldSpear.Type); }
                if (vitalityMod.TryFind<ModItem>("PlatinumSpear", out ModItem PlatinumSpear)) { SpearItems.Add(PlatinumSpear.Type); }
                if (vitalityMod.TryFind<ModItem>("LavanitePiercer", out ModItem LavanitePiercer)) { SpearItems.Add(LavanitePiercer.Type); }
                if (vitalityMod.TryFind<ModItem>("MarbleTrident", out ModItem MarbleTrident)) { SpearItems.Add(MarbleTrident.Type); }
                if (vitalityMod.TryFind<ModItem>("GeraniumTrident", out ModItem GeraniumTrident)) { SpearItems.Add(GeraniumTrident.Type); }
                if (vitalityMod.TryFind<ModItem>("MoltenWarPike", out ModItem MoltenWarPike)) { SpearItems.Add(MoltenWarPike.Type); }
                if (vitalityMod.TryFind<ModItem>("TheDreamcatcher", out ModItem TheDreamcatcher)) { SpearItems.Add(TheDreamcatcher.Type); }
                if (vitalityMod.TryFind<ModItem>("StarVoulge", out ModItem StarVoulge)) { SpearItems.Add(StarVoulge.Type); }
            }

            // Add DormantDawnMOD spears
            if (ModLoader.TryGetMod("DDMod", out Mod DDMod))
            {
                if (DDMod.TryFind<ModItem>("ShadowSpear", out ModItem ShadowSpear)) { SpearItems.Add(ShadowSpear.Type); }
                if (DDMod.TryFind<ModItem>("冈格尼尔", out ModItem spear1)) { SpearItems.Add(spear1.Type); }
                if (DDMod.TryFind<ModItem>("木制投矛", out ModItem spear2)) { SpearItems.Add(spear2.Type); }
            }

            // Add Spear Overhaul spears
            if (ModLoader.TryGetMod("SpearOverhaul", out Mod spearOverhaul))
            {
                if (spearOverhaul.TryFind<ModItem>("WoodenPike", out ModItem WoodenPike)) { SpearItems.Add(WoodenPike.Type); }
                if (spearOverhaul.TryFind<ModItem>("CactusSpike", out ModItem CactusSpike)) { SpearItems.Add(CactusSpike.Type); }
                if (spearOverhaul.TryFind<ModItem>("FrozenSpear", out ModItem FrozenSpear)) { SpearItems.Add(FrozenSpear.Type); }
                if (spearOverhaul.TryFind<ModItem>("ReinforcedPike", out ModItem ReinforcedPike)) { SpearItems.Add(ReinforcedPike.Type); }
                if (spearOverhaul.TryFind<ModItem>("BramblePartisan", out ModItem BramblePartisan)) { SpearItems.Add(BramblePartisan.Type); }
                if (spearOverhaul.TryFind<ModItem>("HunterSpear", out ModItem HunterSpear)) { SpearItems.Add(HunterSpear.Type); }
                if (spearOverhaul.TryFind<ModItem>("CrimsonSpear", out ModItem CrimsonSpear)) { SpearItems.Add(CrimsonSpear.Type); }
                if (spearOverhaul.TryFind<ModItem>("DemonSpear", out ModItem DemonSpear)) { SpearItems.Add(DemonSpear.Type); }
                if (spearOverhaul.TryFind<ModItem>("KingSpear", out ModItem KingSpear)) { SpearItems.Add(KingSpear.Type); }
                if (spearOverhaul.TryFind<ModItem>("MoltenJabber", out ModItem MoltenJabber)) { SpearItems.Add(MoltenJabber.Type); }
                if (spearOverhaul.TryFind<ModItem>("ShadowLance", out ModItem ShadowLance)) { SpearItems.Add(ShadowLance.Type); }
                if (spearOverhaul.TryFind<ModItem>("CrystalCrusher", out ModItem CrystalCrusher)) { SpearItems.Add(CrystalCrusher.Type); }
                if (spearOverhaul.TryFind<ModItem>("GodslayerSpear", out ModItem GodslayerSpear)) { SpearItems.Add(GodslayerSpear.Type); }
                if (spearOverhaul.TryFind<ModItem>("ElementalSpear", out ModItem ElementalSpear)) { SpearItems.Add(ElementalSpear.Type); }
                if (spearOverhaul.TryFind<ModItem>("PaladinSpear", out ModItem PaladinSpear)) { SpearItems.Add(PaladinSpear.Type); }
            }
        }
    }
}
