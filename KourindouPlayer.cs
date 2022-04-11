using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Graphics.Effects;
using Terraria.UI;
using Terraria.ObjectData;
using Kourindou.Buffs;
using Kourindou.Items;
using Kourindou.Items.Plushies;
using Kourindou.Projectiles.Plushies.PlushieEffects;
using static Terraria.ModLoader.ModContent;

namespace Kourindou
{
    public class KourindouPlayer : ModPlayer
    {
//--------------------------------------------------------------------------------
        // Determines the power mode of all the plushies
        public byte plushiePower;
        
        // Plushie slot
        private const string PlushieTag = "plushie";

        // Cirno Plushie Effect Attack Counter
        public byte CirnoPlushie_Attack_Counter;
        public bool CirnoPlushie_TimesNine;

        // Yukari Yakumo teleport hotkey
        public bool YukariYakumoTPKeyPressed;

        // Half Phantom pet active
        public bool HalfPhantomPet;

//--------------------------------------------------------------------------------
        public override void SaveData(TagCompound tag) 
        {
            tag.Add("plushiePowerMode", plushiePower);
            tag.Add("cirnoPlushieAttackCounter", CirnoPlushie_Attack_Counter);
            tag.Add("cirnoPlushieTimesNine", CirnoPlushie_TimesNine);
        }

        public override void LoadData(TagCompound tag) 
        {
            plushiePower = tag.GetByte("plushiePowerMode");
            CirnoPlushie_Attack_Counter = tag.GetByte("cirnoPlushieAttackCounter");
            CirnoPlushie_TimesNine = tag.GetBool("cirnoPlushieTimesNine");

            ContentInstance<PlushieEquipSlot>.Instance.FunctionalItem = ItemIO.Load(tag.GetCompound(PlushieTag));
        }

        public override void OnEnterWorld(Player player)
        {
            // When player joins a singleplayer world get the PlushiePower Client Config
            plushiePower = (byte)Kourindou.KourindouConfigClient.plushiePower;

            base.OnEnterWorld(player);
        }

        public override void PlayerConnect(Player player)
        {
            // PlushiePower Client Config
            plushiePower = (byte)Kourindou.KourindouConfigClient.plushiePower;
            
            // Update other clients when joining multiplayer or when another player joins multiplayer
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)KourindouMessageType.ClientConfig);
                packet.Write((byte)Main.myPlayer);
                packet.Write((byte)plushiePower);
                packet.Send();
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Kourindou.YukariYakumoTPKey.JustPressed)
            {
                YukariYakumoTPKeyPressed = true;
            }
            
            base.ProcessTriggers(triggersSet);
        }

        public override void ResetEffects()
        {
            // Suika Ibuki Effect reset scale
            if (Player.HeldItem.stack > 0 && Player.HeldItem.CountsAsClass(DamageClass.Melee) && (Player.HeldItem.useStyle == ItemUseStyleID.Swing || Player.HeldItem.useStyle == ItemUseStyleID.Thrust))
            {   
                Player.HeldItem.scale = Player.HeldItem.GetGlobalItem<KourindouGlobalItemInstance>().defaultScale;
            }

            // Murasa Effect reset breathMax
            Player.breathMax = 200;

            // Reset buff timer visibility
            Main.buffNoTimeDisplay[146] = false;
        }   

        // Update player with the equipped plushie
        public override void PreUpdate()
        {
            // When the plushie power setting is changed to 0 or 1 clear the slot 
            // and place the item on top of the player
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref bool crit)
        {
            // Shion Yorigami random damage increase on NPC hits 0.1% chance
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<ShionYorigami_Plushie_Item>())
            {
                if ((int)Main.rand.Next(1,1000) == 1)
                {
                    damage = (int)(damage * Main.rand.NextFloat(1000f,1000000f));
                }
            }

            // Disable crit for Flandre Scarlet Plushie effect
            if (projectile.type == ProjectileType<FlandreScarlet_Plushie_Explosion>())
            {
                crit = false;
            }
        }

        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            // Cirno Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<Cirno_Plushie_Item>())
            {
                CirnoPlushie_OnHit(null, target, crit);
            }

            // Flandre Scarlet Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<FlandreScarlet_Plushie_Item>())
            {
                FlandreScarletPlushie_OnHit(target, null, damage, crit, item.useAnimation);
            }

            // Marisa Kirisame Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<MarisaKirisame_Plushie_Item>())
            {
                MarisaKirisamePlushie_OnHit(target.Center, crit, target);
            }

            // Remilia Scarlet Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<Kourindou_RemiliaScarlet_Plushie_Item>())
            {
                RemiliaScarletPlushie_OnHit(damage, target);
            }

            // Satori Komeiji Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<SatoriKomeiji_Plushie_Item>())
            {
                SatoriKomeijiPlushie_OnHit(target, null);
            }

            // Tewi Inaba Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<TewiInaba_Plushie_Item>())
            {
                TewiInabaPlushie_OnHit(target);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockBack, bool crit)
        {
            // Cirno Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<Cirno_Plushie_Item>())
            {
                CirnoPlushie_OnHit(null, target, crit);
            }

            // Flandre Scarlet Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<FlandreScarlet_Plushie_Item>())
            {
                FlandreScarletPlushie_OnHit(target, null, damage, crit, target.immune[proj.owner]);
                if (crit)
                {
                    target.immune[proj.owner] = 0;
                }      
            }

            // Marisa Kirisame Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<MarisaKirisame_Plushie_Item>())
            {
                MarisaKirisamePlushie_OnHit(target.Center, crit, target);
            }

            //Remilia Scarlet Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<Kourindou_RemiliaScarlet_Plushie_Item>())
            {
                RemiliaScarletPlushie_OnHit(damage, target);
            }

            // Satori Komeiji Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<SatoriKomeiji_Plushie_Item>())
            {
                SatoriKomeijiPlushie_OnHit(target, null);
            }

            // Tewi Inaba Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<TewiInaba_Plushie_Item>())
            {
                TewiInabaPlushie_OnHit(target);
            }
        }

        public override void OnHitPvp(Item item, Player target, int damage, bool crit)
        {
            // Cirno Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<Cirno_Plushie_Item>())
            {
                CirnoPlushie_OnHit(target, null, crit);
            }

            // Flandre Scarlet Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<FlandreScarlet_Plushie_Item>())
            {
                FlandreScarletPlushie_OnHit(null, target, damage, crit, item.useAnimation);
            }

            // Marisa Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<MarisaKirisame_Plushie_Item>())
            {
                MarisaKirisamePlushie_OnHit(target.Center, crit, target);
            }

            //Remilia Scarlet Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<Kourindou_RemiliaScarlet_Plushie_Item>())
            {
                RemiliaScarletPlushie_OnHit(damage, target);
            }

            // Satori Komeiji Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<SatoriKomeiji_Plushie_Item>())
            {
                SatoriKomeijiPlushie_OnHit(null, target);
            }
        }

        public override void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit)
        {
            // Cirno Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<Cirno_Plushie_Item>())
            {
                CirnoPlushie_OnHit(target, null, crit);
            }

            // Flandre Scarlet Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<FlandreScarlet_Plushie_Item>())
            {
                FlandreScarletPlushie_OnHit(null, target, damage, crit, target.immuneTime);
                if (crit)
                {
                    target.immuneTime = 0;
                }
            }

            // Marisa Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<MarisaKirisame_Plushie_Item>())
            {
                MarisaKirisamePlushie_OnHit(target.Center, crit, target);
            }

            //Remilia Scarlet Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<Kourindou_RemiliaScarlet_Plushie_Item>())
            {
                RemiliaScarletPlushie_OnHit(damage, target);
            }

            // Satori Komeiji Plushie Equipped
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<SatoriKomeiji_Plushie_Item>())
            {
                SatoriKomeijiPlushie_OnHit(null, target);
            }
        }

        public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
        {
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<Chen_Plushie_Item>())
            {
                Player.AddBuff(BuffID.ShadowDodge, 180);
            }
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            // Kaguya or Mokou Plushie Equipped [Mortality]
            if (GetInstance<PlushieEquipSlot>().Type == ItemType<KaguyaHouraisan_Plushie_Item>() || GetInstance<PlushieEquipSlot>().Type == ItemType<FujiwaraNoMokou_Plushie_Item>())
            {
                if (Player.HasBuff(BuffType<DeBuff_Mortality>()))
                {
                    return true;
                }
                else
                {
                    Player.AddBuff(BuffType<DeBuff_Mortality>(), 3600, true);
                    Player.statLife += Player.statLifeMax2;
                    Player.HealEffect(Player.statLifeMax2, true);
                    return false;
                }

                if (GetInstance<PlushieEquipSlot>().Type == ItemType<FujiwaraNoMokou_Plushie_Item>())
                {
                    Player.AddBuff(BuffID.Wrath, 4140);
                    Player.AddBuff(BuffID.Inferno, 4140);
                }
            }

            return true;
        }

        public override void PostUpdate()
        {
            // On the end of this player update tick put the hotkey to false
            // to prevent endless tping...
            YukariYakumoTPKeyPressed = false;
        }

        private void CirnoPlushie_OnHit(Player p, NPC n, bool crit)
        {
            CirnoPlushie_Attack_Counter++;
            
            if (CirnoPlushie_Attack_Counter == 8 && (int)Main.rand.Next(0,10) == 9)
            {
                CirnoPlushie_TimesNine = true;
            }

            if (CirnoPlushie_Attack_Counter >= 9)
            {
                CirnoPlushie_Attack_Counter = 0;
                CirnoPlushie_TimesNine = false;
            }

            // Add debuffs to players
            if (p != null)
            {
                p.AddBuff(BuffID.Chilled, 600);
                p.AddBuff(BuffID.Frostburn, 600);
                p.AddBuff(BuffID.Slow, 600);
                if (crit)
                {
                    p.AddBuff(BuffID.Frozen, 120);
                }
            }
            
            // Add debuffs to NPCs
            if (n != null)
            {
                n.AddBuff(BuffID.Chilled, 600);
                n.AddBuff(BuffID.Frostburn, 600);
                n.AddBuff(BuffID.Slow, 600);
                if (crit)
                {
                    n.AddBuff(BuffID.Frozen, 120);
                }
            }
        }

        private void FlandreScarletPlushie_OnHit(NPC n, Player p, int damage, bool crit, int immune)
        {
            if (crit)
            {
                Vector2 position = new Vector2(0, 0);

                if (n != null)
                {
                    position = n.Center;

                    Projectile.NewProjectile(
                        Player.GetProjectileSource_Accessory(Main.item[ItemType<FlandreScarlet_Plushie_Item>()]),
                        position,
                        Vector2.Zero,
                        ProjectileType<FlandreScarlet_Plushie_Explosion>(),
                        damage * 2 + 80,
                        0f,
                        Main.myPlayer,
                        n.whoAmI,
                        immune
                    );
                }

                if (p != null)
                {
                    position = p.Center;

                    Projectile.NewProjectile(
                        Player.GetProjectileSource_Accessory(Main.item[ItemType<FlandreScarlet_Plushie_Item>()]),
                        position,
                        Vector2.Zero,
                        ProjectileType<FlandreScarlet_Plushie_Explosion>(),
                        damage * 2 + 80,
                        0f,
                        Main.myPlayer,
                        p.whoAmI + 10000,
                        immune
                    );
                }



                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode.SoundId, (int)position.X, (int)position.Y, SoundID.DD2_ExplosiveTrapExplode.Style, .8f, 1f);

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Send sound packet for other clients
                    ModPacket packet2 = Mod.GetPacket();
                    packet2.Write((byte) KourindouMessageType.PlaySound);
                    packet2.Write((byte) SoundID.DD2_ExplosiveTrapExplode.SoundId);
                    packet2.Write((short) SoundID.DD2_ExplosiveTrapExplode.Style);
                    packet2.Write((float) 0.8f);
                    packet2.Write((float) 1f);
                    packet2.Write((int)position.X);
                    packet2.Write((int)position.Y);
                    packet2.Send();
                }
            }
        }

        private void MarisaKirisamePlushie_OnHit(Vector2 position, bool crit, Entity entity)
        {
            if (crit)
            {
                int star = Projectile.NewProjectile
                (
                    Player.GetProjectileSource_Accessory(Main.item[ItemType<MarisaKirisame_Plushie_Item>()]),
                    Player.Center,
                    Vector2.Normalize(Main.MouseWorld - Player.Center) * 10f,
                    ProjectileID.StarWrath,
                    50,
                    1f,
                    Main.myPlayer,
                    1f
                );

                Main.projectile[star].hide = true;
                Main.projectile[star].netUpdate = true;
            }
        }

        private void RemiliaScarletPlushie_OnHit(int damage, Entity entity)
        {
            if (Player.statLife < Player.statLifeMax2)
            {
                int healAmount = (int)Math.Ceiling((double)((damage * 0.05) < Player.statLifeMax2 - Player.statLife ? (int)(damage * 0.05) : Player.statLifeMax2 - Player.statLife));
                Player.statLife += healAmount;
                Player.HealEffect(healAmount, true);
            }
        }

        private void SatoriKomeijiPlushie_OnHit(NPC n, Player p)
        {
            if (n != null)
            {
                n.AddBuff(BuffID.CursedInferno, 600);
                n.AddBuff(BuffID.Confused, 600);
                n.AddBuff(BuffID.Ichor, 600);
            }

            if (p != null)
            {
                p.AddBuff(BuffID.CursedInferno, 600);
                p.AddBuff(BuffID.Confused, 600);
                p.AddBuff(BuffID.Ichor, 600);
            }
        }

        private void TewiInabaPlushie_OnHit(NPC n)
        {
            if ((int)Main.rand.Next(0,5) == 0 && n.life <= 0)
            {
                n.NPCLoot();
            }
        }

/*
        public void Draw(SpriteBatch spriteBatch)
        {
            //Draw the custom inventory slot
            if (!ShouldDrawSlots())
            {
                return;
            }

            int mapH = 0;

            int rX;
            int rY;
            float origScale = Main.inventoryScale;

            Main.inventoryScale = 0.85f;



            if(Main.mapEnabled) {
                int adjustY = 600;
                if(Main.player[Main.myPlayer].ExtraAccessorySlotsShouldShow) {
                    adjustY = 610 + PlayerInput.UsingGamepad.ToInt() * 30;
                }
                if((mapH + adjustY) > Main.screenHeight) {
                    mapH = Main.screenHeight - adjustY;
                }
            }
            int slotCount = 7 + Main.player[Main.myPlayer].extraAccessorySlots;
            if((Main.screenHeight < 900) && (slotCount >= 8)) {
                slotCount = 7;
            }
            rX = Main.screenWidth - 92 - 14 - (47 * 3) - (int)(Main.extraTexture[58].Width * Main.inventoryScale);
            rY = (int)(mapH + 174 + 4 + slotCount * 56 * Main.inventoryScale);

            //if Wingslot is also installed move up
            Mod Wingslot = ModLoader.GetMod("WingSlotExtra");
            if (Wingslot != null && Wingslot.Version >= new Version(1,7,3))
            {
                if (WingSlotNextToAccessories)
                {
                    rY -= 47;
                }
            }

            plushieEquipSlot.Position = new Vector2(rX, rY);
            plushieEquipSlot.Draw(spriteBatch);

            Main.inventoryScale = origScale;

            plushieEquipSlot.Update();
        }
        
        //Check WingSlot Mod Settings
        public bool WingSlotNextToAccessories
        {
            get { return WingSlot.WingSlot.SlotsNextToAccessories; }
        }
*/
    }

    internal class PlushieEquipSlotUpdateUI : ModSystem
    {
        internal static int posX;
        internal static int posY;
        public override void UpdateUI(GameTime gameTime)
        {
            if (!Main.gameMenu)
            {
                int mapH = 0;
                Main.inventoryScale = 0.85f;

                if(Main.mapEnabled && !Main.mapFullscreen && Main.mapStyle == 1) 
                {
                    mapH = 256;
                }

                if (Main.mapEnabled)
                {
                    int adjustY = 600;

                    if (Main.player[Main.myPlayer].extraAccessory)
                        adjustY = 610 + PlayerInput.UsingGamepad.ToInt() * 30;

                    if ((mapH + adjustY) > Main.screenHeight)
                        mapH = Main.screenHeight - adjustY;
                }

                int slotCount = 7 + Main.player[Main.myPlayer].GetAmountOfExtraAccessorySlotsToShow();

                if ((Main.screenHeight < 900) && (slotCount >= 8))
                    slotCount = 7;

                posX = Main.screenWidth - 82 - 14 - (47 * 3) - (int)(TextureAssets.InventoryBack.Width() * Main.inventoryScale);
                posY = (int)(mapH + 174 + 4 + slotCount * 56 * Main.inventoryScale);
            }
        }
    }

    public class PlushieEquipSlot : ModAccessorySlot
    {
        public override string Name => "Plushie Slot";

        public override bool IsHidden()
        {
            return Main.playerInventory == true && Main.EquipPage == 0 && Main.LocalPlayer.GetModPlayer<KourindouPlayer>().plushiePower == 2;
        }

        public override Vector2? CustomLocation => new Vector2(PlushieEquipSlotUpdateUI.posX, PlushieEquipSlotUpdateUI.posY);

        public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
        {
            // cannot place an item in the slot if the power mode is not 2
            if (Main.LocalPlayer.GetModPlayer<KourindouPlayer>().plushiePower != 2)
            {
                return false;
            }

            if (checkItem.ModItem is PlushieItem plushie)
            {
                return true;
            }

            return false;
        }

        public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo)
        {
            if (Main.LocalPlayer.GetModPlayer<KourindouPlayer>().plushiePower != 2)
            {
                return false;
            }

            if (item.ModItem is PlushieItem plushie)
            {
                return true;
            }

            return false;
        }

        public override bool IsVisibleWhenNotEnabled()
        {
            return false;
        }

        public override string FunctionalTexture => "Terraria/Images/Item_" + ItemID.CreativeWings;

        public override void OnMouseHover(AccessorySlotType context)
        {
            switch (context)
            {
                case AccessorySlotType.FunctionalSlot:
                    Main.hoverItemName = "Plushie Slot";
                    break;
            }
        }
    }
}
