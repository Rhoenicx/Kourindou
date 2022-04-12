using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.Localization;
using Kourindou.Items;
using Kourindou.Items.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Plushies;
using Kourindou.Tiles.Plants;
using Kourindou.Projectiles.Plushies;
using static Terraria.ModLoader.ModContent;

namespace Kourindou
{
    class Kourindou : Mod
    {
        internal static Kourindou Instance;

        internal static KourindouConfigClient KourindouConfigClient;

        private static List<Func<bool>> RightClickOverrides;

        public static ModKeybind YukariYakumoTPKey;

        // Kourindou Mod Instance
        public Kourindou()
        {
            Instance = this;
        }

        // Gensokyo Mod Instance
        public static Mod Gensokyo;
        public static bool GensokyoLoaded;

        // Load
        public override void Load()
        {
            RightClickOverrides = new List<Func<bool>>();

            YukariYakumoTPKey = KeybindLoader.RegisterKeybind(this, "Yukari Yakumo Teleport Key", "Mouse2");

            //code that has to be run on clients only!
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

            YukariYakumoTPKey = null;

            Instance = null;
            base.Unload();
        }

        // PostSetupContent - Register mods for compatibility
        public override void PostSetupContent()
        {
            // Check if Gensokyo mod is also loaded
            GensokyoLoaded = ModLoader.TryGetMod("Gensokyo", out Gensokyo);

            // Support for Gensokyo Mod
            if (Gensokyo != null)
            {
                CrossModContent.SetupGensokyo(Gensokyo, this);
            }

            if (!Main.dedServ)
            {
                LoadPlushieTextures();

                // Thread
                Terraria.GameContent.TextureAssets.Item[ItemID.BlackThread]= Assets.Request<Texture2D>("Items/CraftingMaterials/BlackThread");
                Terraria.GameContent.TextureAssets.Item[ItemID.GreenThread] = Assets.Request<Texture2D>("Items/CraftingMaterials/GreenThread");
                Terraria.GameContent.TextureAssets.Item[ItemID.PinkThread] = Assets.Request<Texture2D>("Items/CraftingMaterials/PinkThread");

                // Silk
                Terraria.GameContent.TextureAssets.Item[ItemID.Silk] = Assets.Request<Texture2D>("Items/CraftingMaterials/WhiteFabric");
                Terraria.GameContent.TextureAssets.Item[ItemID.SilkRope] = Assets.Request<Texture2D>("Items/Blocks/WhiteFabric_Item_Rope");
                Terraria.GameContent.TextureAssets.Item[ItemID.SilkRopeCoil] = Assets.Request<Texture2D>("Items/Consumables/WhiteFabric_Item_RopeCoil");

                Terraria.GameContent.TextureAssets.Tile[TileID.SilkRope] = Assets.Request<Texture2D>("Tiles/Blocks/WhiteFabric_Tile");
                Terraria.GameContent.TextureAssets.Projectile[ProjectileID.SilkRopeCoil] = Assets.Request<Texture2D>("Projectiles/Fabric/WhiteFabric_Projectile");
                Terraria.GameContent.TextureAssets.Chains[4] = Assets.Request<Texture2D>("Projectiles/Fabric/WhiteFabric_Projectile_Chain1");
                Terraria.GameContent.TextureAssets.Chains[5] = Assets.Request<Texture2D>("Projectiles/Fabric/WhiteFabric_Projectile_Chain2");                
            }
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
                ModContent.ItemType<YellowThread>(),
                ModContent.ItemType<RainbowThread>()
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
                ModContent.ItemType<YellowFabric>(),
                ModContent.ItemType<RainbowFabric>()
            });
            RecipeGroup.RegisterGroup("Kourindou:Fabric", Fabric);

            // Stuffing
            RecipeGroup Stuffing = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Stuffing", new int[]
            {
                ItemID.Hay,
                ModContent.ItemType<CottonFibre>()
            });
            RecipeGroup.RegisterGroup("Kourindou:Stuffing", Stuffing);

            // Gemstone
            RecipeGroup Gemstone = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Gemstone", new int[]
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
            RecipeGroup Lens = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Lens", new int[]
            {
                ItemID.Lens,
                ItemID.BlackLens
            });
            RecipeGroup.RegisterGroup("Kourindou:Lens", Lens);

            RecipeGroup Watch = new RecipeGroup(() => Language.GetTextValue("LegacyMics.37") + " Watch", new int[]
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

            RecipeGroup CopperBar = new RecipeGroup(() => "Copper or tin bar", new int[]
            {
                ItemID.CopperBar,
                ItemID.TinBar
            });
            RecipeGroup.RegisterGroup("Kourindou:CopperBar", CopperBar);

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
                        ModContent.GetInstance<Cotton_Tile>().RightClick(i, j);
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
                        SoundEngine.PlaySound(
							soundType,
							(int)Main.LocalPlayer.Center.X,
							(int)Main.LocalPlayer.Center.Y,
							soundStyle,
							soundVolume,
							Main.rand.NextFloat(-soundVariance, soundVariance));
					}
					else
					{
                        SoundEngine.PlaySound(
							soundType,
							soundSourceX,
							soundSourceY,
							soundStyle,
							soundVolume,
							Main.rand.NextFloat(-soundVariance, soundVariance));
					}
					break;
				}

                case KourindouMessageType.PlayCustomSound:
				{
					string soundName = reader.ReadString();
					float soundVolume = reader.ReadSingle();
					float pitchVariance = reader.ReadSingle();
					int soundPositionX = reader.ReadInt32();
					int soundPositionY = reader.ReadInt32();

					if (Main.netMode == NetmodeID.Server)
					{
						ModPacket packet = GetPacket();
						packet.Write((byte) KourindouMessageType.PlayCustomSound);
						packet.Write(soundName);
						packet.Write(soundVolume);
						packet.Write(pitchVariance);
						packet.Write(soundPositionX);
						packet.Write(soundPositionY);

						packet.Send(-1, whoAmI);
						break;
					}

					if (soundPositionX == -1 || soundPositionY == -1)
					{
                        SoundLoader.GetLegacySoundSlot(Kourindou.Instance, "Sounds/Custom/" + soundName);

                            SoundEngine.PlaySound(SoundLoader.GetLegacySoundSlot(Kourindou.Instance, "Sounds/Custom/" + soundName),
                                (int)Main.LocalPlayer.position.X,
                                (int)Main.LocalPlayer.position.Y
                            );
                            //1,
                            //soundVolume,
                            //Main.rand.NextFloat(-pitchVariance, pitchVariance));
					}
					else
					{
                            SoundEngine.PlaySound(SoundLoader.GetLegacySoundSlot(Kourindou.Instance, "Sounds/Custom/" + soundName),
                                soundPositionX,
                                soundPositionY
                            );
                            //1
							//soundVolume,
							//Main.rand.NextFloat(-pitchVariance, pitchVariance));
					}
					break;
				}

                case KourindouMessageType.PlushieItemNetUpdate:
                {
                    int itemSlot = reader.ReadInt32();
                    short plushieDirtWater = reader.ReadInt16();

                    Item item = Main.item[itemSlot];

                    if (Main.netMode != NetmodeID.Server)
                    {   
                        if (item.ModItem is PlushieItem plushie)
                        {
                            plushie.plushieDirtWater = plushieDirtWater;
                        }
                    }
                    break;
                }

                case KourindouMessageType.PlacePlushieTile:
                {
                    int plushiePlaceTileX = reader.ReadInt32();
                    int plushiePlaceTileY = reader.ReadInt32();
                    int plushieTile = reader.ReadInt32();
                    short plushieDirtWater = reader.ReadInt16();

                    WorldGen.PlaceObject(plushiePlaceTileX, plushiePlaceTileY, plushieTile);
                    KourindouWorld.SetPlushieDirtWater(plushiePlaceTileX, plushiePlaceTileY - 1, plushieDirtWater);
                    break;
                }

                case KourindouMessageType.SetPlushieDirtWater:
                {
                    int i = reader.ReadInt32();
                    int j = reader.ReadInt32();
                    short plushieDirtWater = reader.ReadInt16();

                    KourindouWorld.SetPlushieDirtWater(i, j, plushieDirtWater);

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte) KourindouMessageType.SetPlushieDirtWater);
                        packet.Write((int) i);
                        packet.Write((int) j);
                        packet.Write((int) plushieDirtWater);
                        packet.Send(-1, whoAmI);
                    }
                    break;
                }

                // Server => Client
                case KourindouMessageType.RandomPlacePlantTile:
                {
                    int i = reader.ReadInt32();
                    int j = reader.ReadInt32();
                    int tile = reader.ReadInt32();

                    WorldGen.PlaceObject(i, j, tile);

                    break;
                }

                case KourindouMessageType.PlayerPlacePlantTile:
                {
                    int tile = reader.ReadInt32();

                    if (Main.netMode == NetmodeID.Server)
                    {
                        if (tile == ModContent.TileType<Cotton_Tile>())
                        { 
                            KourindouWorld.CottonPlants++;
                        }

                        if (tile == ModContent.TileType<Flax_Tile>())
                        {
                            KourindouWorld.FlaxPlants++;
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

        public void LoadPlushieTextures()
        {
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

        public void SetPlushieTextures(int item, int tile, int projectile, string itemName)
        {
            Terraria.GameContent.TextureAssets.Item[item] = Assets.Request<Texture2D>("Items/Plushies/" + itemName + "_Plushie_Item" + (Kourindou.KourindouConfigClient.UseOldTextures ? "_Old" : ""));
            Terraria.GameContent.TextureAssets.Tile[tile] = Assets.Request<Texture2D>("Tiles/Plushies/" + itemName + "_Plushie_Tile" + (Kourindou.KourindouConfigClient.UseOldTextures ? "_Old" : ""));
            Terraria.GameContent.TextureAssets.Projectile[projectile] = Assets.Request<Texture2D>("Projectiles/Plushies/" + itemName + "_Plushie_Projectile" + (Kourindou.KourindouConfigClient.UseOldTextures ? "_Old" : ""));
        }
    }
}
