using System;
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
using Terraria.Initializers;
using Kourindou.Items;
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
        public Asset<Texture2D> oldTileTexture { get; set; }
    }

    public class PlushieItemTexture
    {
        public Asset<Texture2D> ItemTexture { get; set; }
        public Asset<Texture2D> oldItemTexture { get; set; }
    }

    public class PlushieProjectileTexture
    {
        public Asset<Texture2D> ProjectileTexture { get; set; }
        public Asset<Texture2D> oldProjectileTexture { get; set; }
    }

    class Kourindou : Mod
    {
        public static Dictionary<int, PlushieTileTexture> PlushieTileTextures;
        public static Dictionary<int, PlushieItemTexture> PlushieItemTextures;
        public static Dictionary<int, PlushieProjectileTexture> PlushieProjectileTextures;
        public static Dictionary<string, SoundStyle> SoundDictionary;

        public static HashSet<int> FabricItems;
        public static HashSet<int> ThreadItems;

        internal static Kourindou Instance;

        internal static KourindouConfigClient KourindouConfigClient;

        private static List<Func<bool>> RightClickOverrides;

        public static ModKeybind SkillKey;
        public static ModKeybind UltimateKey;

        // Kourindou Mod Instance
        public Kourindou()
        {
            Instance = this;
        }

        // Gensokyo Mod Instance
        public static Mod Gensokyo;
        public static bool GensokyoLoaded;

        // Hairloader Mod Instance
        public static Mod HairLoader;
        public static bool HairLoaderLoaded;

        // Load
        public override void Load()
        {
            RightClickOverrides = new List<Func<bool>>();

            SkillKey = KeybindLoader.RegisterKeybind(this, "Skill", "Mouse2");
            UltimateKey = KeybindLoader.RegisterKeybind(this, "Ultimate", "Mouse2");

            SoundDictionary = new Dictionary<string, SoundStyle>
            {
                { "Grass", SoundID.Grass },
                { "DD2_ExplosiveTrapExplode", SoundID.DD2_ExplosiveTrapExplode }
            };

            FabricItems = new HashSet<int>();
            ThreadItems = new HashSet<int>();

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

            if (RightClickOverrides != null) {
                RightClickOverrides.Clear();
                RightClickOverrides = null;
            }

            SkillKey = null;
            UltimateKey = null;

            Instance = null;
            Gensokyo = null;
            HairLoader = null;

            SoundDictionary = null;

            FabricItems = null;
            ThreadItems = null;

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

        // PostSetupContent - Register mods for compatibility
        public override void PostSetupContent()
        {
            // Check loaded mods
            GensokyoLoaded = ModLoader.TryGetMod("Gensokyo", out Gensokyo);
            HairLoaderLoaded = ModLoader.TryGetMod("HairLoader", out HairLoader);

            // Support for Gensokyo Mod
            if (Gensokyo != null)
            {
                CrossModContent.SetupGensokyo(Gensokyo, this);
            }

            // Support for HairLoader Mod
            if (HairLoader != null)
            {
                CrossModContent.SetupHairLoader(HairLoader, this);
            }

            // Swap vanilla textures
            if (!Main.dedServ)
            {
                SwitchPlushieTextures();
            }

            FabricSetup();
            ThreadSetup();
        }

        // Add Crafting recipe groups
        public override void AddRecipeGroups()
        {
            // Stuffing
            RecipeGroup Stuffing = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Stuffing", new int[]
            {
                ItemID.Hay,
                ItemType<CottonFibre>()
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

            // Watch
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

            // Copper bar or tin bar?
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
                        bool plushiePower = reader.ReadBoolean();

                        Player player = Main.player[playerID];

                        player.GetModPlayer<KourindouPlayer>().plushiePower = plushiePower;

                        // if packet is received on server, resend this packet to other clients
                        if (Main.netMode == NetmodeID.Server)
                        {
                            ModPacket packet = GetPacket();
                            packet.Write((byte)KourindouMessageType.ClientConfig);
                            packet.Write((byte)playerID);
                            packet.Write((bool)plushiePower);
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

                case KourindouMessageType.AlternateFire:
                {
                    byte PlayerID = reader.ReadByte();
                    bool UsedAttack = reader.ReadBoolean();
                    int AttackID = reader.ReadInt32();
                    int AttackCounter = reader.ReadInt32();

                    KourindouPlayer player = Main.player[PlayerID].GetModPlayer<KourindouPlayer>();
                    player.UsedAttack = UsedAttack;
                    player.AttackID = AttackID;
                    player.AttackCounter = AttackCounter;

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte) KourindouMessageType.AlternateFire);
                        packet.Write((byte) PlayerID);
                        packet.Write((bool) UsedAttack);
                        packet.Write((int) AttackID);
                        packet.Write((int) AttackCounter);
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
                    TileTexture = Assets.Request<Texture2D>("Tiles/Plushies/" + itemName + "_Plushie_Tile"),
                    oldTileTexture = Assets.Request<Texture2D>("Tiles/Plushies/" + itemName + "_Plushie_Tile_Old") 
                });
            }

            if (!PlushieItemTextures.ContainsKey(item))
            {
                PlushieItemTextures.Add(item, new PlushieItemTexture 
                { 
                    ItemTexture = Assets.Request<Texture2D>("Items/Plushies/" + itemName + "_Plushie_Item"), 
                    oldItemTexture = Assets.Request<Texture2D>("Items/Plushies/" + itemName + "_Plushie_Item_Old") 
                });
            }

            if (!PlushieProjectileTextures.ContainsKey(projectile))
            {
                PlushieProjectileTextures.Add(projectile, new PlushieProjectileTexture
                {
                    ProjectileTexture = Assets.Request<Texture2D>("Projectiles/Plushies/" + itemName + "_Plushie_Projectile"),
                    oldProjectileTexture = Assets.Request<Texture2D>("Projectiles/Plushies/" + itemName + "_Plushie_Projectile_Old")
                });
            }
        }

        public void SwitchPlushieTextures()
        {
            foreach (KeyValuePair<int, PlushieItemTexture> entry in PlushieItemTextures)
            {
                TextureAssets.Item[entry.Key] = KourindouConfigClient.UseOldTextures ? entry.Value.oldItemTexture : entry.Value.ItemTexture;
            }

            foreach (KeyValuePair<int, PlushieProjectileTexture> entry in PlushieProjectileTextures)
            { 
                TextureAssets.Projectile[entry.Key] = KourindouConfigClient.UseOldTextures ? entry.Value.oldProjectileTexture : entry.Value.ProjectileTexture;
            }
        }

        public void SwitchModTextures(bool loading)
        {
            // Thread
            TextureAssets.Item[ItemID.BlackThread] = loading ? Assets.Request<Texture2D>("Items/CraftingMaterials/BlackThread") : Main.Assets.Request<Texture2D>("Images\\Item_254", 0);
            TextureAssets.Item[ItemID.GreenThread] = loading ? Assets.Request<Texture2D>("Items/CraftingMaterials/GreenThread") : Main.Assets.Request<Texture2D>("Images\\Item_255", 0);
            TextureAssets.Item[ItemID.PinkThread] = loading ? Assets.Request<Texture2D>("Items/CraftingMaterials/PinkThread") : Main.Assets.Request<Texture2D>("Images\\Item_981", 0);

            // Silk
            TextureAssets.Item[ItemID.Silk] = loading ? Assets.Request<Texture2D>("Items/CraftingMaterials/WhiteFabric") : Main.Assets.Request<Texture2D>("Images\\Item_225", 0);
            TextureAssets.Item[ItemID.SilkRope] = loading ? Assets.Request<Texture2D>("Items/Blocks/WhiteFabric_Item_Rope") : Main.Assets.Request<Texture2D>("Images\\Item_3077", 0);
            TextureAssets.Item[ItemID.SilkRopeCoil] = loading ? Assets.Request<Texture2D>("Items/Consumables/WhiteFabric_Item_RopeCoil") : Main.Assets.Request<Texture2D>("Images\\Item_3079", 0);

            // Other
            TextureAssets.Tile[TileID.SilkRope] = loading ? Assets.Request<Texture2D>("Tiles/Blocks/WhiteFabric_Tile") : Main.Assets.Request<Texture2D>("Images\\Tiles_365", 0);
            TextureAssets.Projectile[ProjectileID.SilkRopeCoil] = loading ? Assets.Request<Texture2D>("Projectiles/Fabric/WhiteFabric_Projectile") : Main.Assets.Request<Texture2D>("Images\\Projectile_505", 0);
            TextureAssets.Chains[4] = loading ? Assets.Request<Texture2D>("Projectiles/Fabric/WhiteFabric_Projectile_Chain1") : Main.Assets.Request<Texture2D>("Images\\Chains_4", 0);
            TextureAssets.Chains[5] = loading ? Assets.Request<Texture2D>("Projectiles/Fabric/WhiteFabric_Projectile_Chain2") : Main.Assets.Request<Texture2D>("Images\\Chains_5", 0);
        }

        public void FabricSetup()
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

        public void ThreadSetup()
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
    }
}
