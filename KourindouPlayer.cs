using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
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

        // Item ID of the plushie slot item
        public int PlushieSlotItemID;

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
            // Reset the plushie item slot ID
            PlushieSlotItemID = 0;

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
            if (PlushieSlotItemID == ItemType<ShionYorigami_Plushie_Item>())
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
            if (PlushieSlotItemID == ItemType<Cirno_Plushie_Item>())
            {
                CirnoPlushie_OnHit(null, target, crit);
            }

            // Flandre Scarlet Plushie Equipped
            else if (PlushieSlotItemID == ItemType<FlandreScarlet_Plushie_Item>())
            {
                FlandreScarletPlushie_OnHit(target, null, damage, crit, item.useAnimation);
            }

            // Marisa Kirisame Plushie Equipped
            else if (PlushieSlotItemID == ItemType<MarisaKirisame_Plushie_Item>())
            {
                MarisaKirisamePlushie_OnHit(target.Center, crit, target);
            }

            // Remilia Scarlet Plushie Equipped
            else if (PlushieSlotItemID == ItemType<Kourindou_RemiliaScarlet_Plushie_Item>())
            {
                RemiliaScarletPlushie_OnHit(damage);
            }

            // Satori Komeiji Plushie Equipped
            else if (PlushieSlotItemID == ItemType<SatoriKomeiji_Plushie_Item>())
            {
                SatoriKomeijiPlushie_OnHit(target, null);
            }

            // Tewi Inaba Plushie Equipped
            else if (PlushieSlotItemID == ItemType<TewiInaba_Plushie_Item>())
            {
                TewiInabaPlushie_OnHit(target);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockBack, bool crit)
        {
            // Cirno Plushie Equipped
            if (PlushieSlotItemID == ItemType<Cirno_Plushie_Item>())
            {
                CirnoPlushie_OnHit(null, target, crit);
            }

            // Flandre Scarlet Plushie Equipped
            else if (PlushieSlotItemID == ItemType<FlandreScarlet_Plushie_Item>())
            {
                FlandreScarletPlushie_OnHit(target, null, damage, crit, target.immune[proj.owner]);
                if (crit)
                {
                    target.immune[proj.owner] = 0;
                }      
            }

            // Marisa Kirisame Plushie Equipped
            else if (PlushieSlotItemID == ItemType<MarisaKirisame_Plushie_Item>())
            {
                MarisaKirisamePlushie_OnHit(target.Center, crit, target);
            }

            //Remilia Scarlet Plushie Equipped
            else if (PlushieSlotItemID == ItemType<Kourindou_RemiliaScarlet_Plushie_Item>())
            {
                RemiliaScarletPlushie_OnHit(damage);
            }

            // Satori Komeiji Plushie Equipped
            else if (PlushieSlotItemID == ItemType<SatoriKomeiji_Plushie_Item>())
            {
                SatoriKomeijiPlushie_OnHit(target, null);
            }

            // Tewi Inaba Plushie Equipped
            else if (PlushieSlotItemID == ItemType<TewiInaba_Plushie_Item>())
            {
                TewiInabaPlushie_OnHit(target);
            }
        }

        public override void OnHitPvp(Item item, Player target, int damage, bool crit)
        {
            // Cirno Plushie Equipped
            if (PlushieSlotItemID == ItemType<Cirno_Plushie_Item>())
            {
                CirnoPlushie_OnHit(target, null, crit);
            }

            // Flandre Scarlet Plushie Equipped
            else if (PlushieSlotItemID == ItemType<FlandreScarlet_Plushie_Item>())
            {
                FlandreScarletPlushie_OnHit(null, target, damage, crit, item.useAnimation);
            }

            // Marisa Plushie Equipped
            else if (PlushieSlotItemID == ItemType<MarisaKirisame_Plushie_Item>())
            {
                MarisaKirisamePlushie_OnHit(target.Center, crit, target);
            }

            //Remilia Scarlet Plushie Equipped
            else if (PlushieSlotItemID == ItemType<Kourindou_RemiliaScarlet_Plushie_Item>())
            {
                RemiliaScarletPlushie_OnHit(damage);
            }

            // Satori Komeiji Plushie Equipped
            else if (PlushieSlotItemID == ItemType<SatoriKomeiji_Plushie_Item>())
            {
                SatoriKomeijiPlushie_OnHit(null, target);
            }
        }

        public override void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit)
        {
            // Cirno Plushie Equipped
            if (PlushieSlotItemID == ItemType<Cirno_Plushie_Item>())
            {
                CirnoPlushie_OnHit(target, null, crit);
            }

            // Flandre Scarlet Plushie Equipped
            else if (PlushieSlotItemID == ItemType<FlandreScarlet_Plushie_Item>())
            {
                FlandreScarletPlushie_OnHit(null, target, damage, crit, target.immuneTime);
                if (crit)
                {
                    target.immuneTime = 0;
                }
            }

            // Marisa Plushie Equipped
            else if (PlushieSlotItemID == ItemType<MarisaKirisame_Plushie_Item>())
            {
                MarisaKirisamePlushie_OnHit(target.Center, crit, target);
            }

            //Remilia Scarlet Plushie Equipped
            else if (PlushieSlotItemID == ItemType<Kourindou_RemiliaScarlet_Plushie_Item>())
            {
                RemiliaScarletPlushie_OnHit(damage);
            }

            // Satori Komeiji Plushie Equipped
            else if (PlushieSlotItemID == ItemType<SatoriKomeiji_Plushie_Item>())
            {
                SatoriKomeijiPlushie_OnHit(null, target);
            }
        }

        public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
        {
            if (PlushieSlotItemID == ItemType<Chen_Plushie_Item>())
            {
                Player.AddBuff(BuffID.ShadowDodge, 180);
            }
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            // Kaguya or Mokou Plushie Equipped [Mortality]
            if (PlushieSlotItemID == ItemType<KaguyaHouraisan_Plushie_Item>() || PlushieSlotItemID == ItemType<FujiwaraNoMokou_Plushie_Item>())
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

                    if (PlushieSlotItemID == ItemType<FujiwaraNoMokou_Plushie_Item>())
                    {
                        Player.AddBuff(BuffID.Wrath, 4140);
                        Player.AddBuff(BuffID.Inferno, 4140);
                    }

                    return false;
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
                        Player.GetProjectileSource_Accessory(GetInstance<PlushieEquipSlot>().FunctionalItem),
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
                        Player.GetProjectileSource_Accessory(GetInstance<PlushieEquipSlot>().FunctionalItem),
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
                    Player.GetProjectileSource_Accessory(GetInstance<PlushieEquipSlot>().FunctionalItem),
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

        private void RemiliaScarletPlushie_OnHit(int damage)
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
    }

    public class PlushieEquipSlot : ModAccessorySlot
    {
        public override string Name => "Plushie Slot";
        public override bool DrawDyeSlot => false;
        public override bool DrawVanitySlot => false;
        public override bool IsEnabled()
        {
            if (!Main.gameMenu)
            {
                return Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushiePower == 2;
            }

            return Kourindou.KourindouConfigClient.plushiePower == 2;
        }

        public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
        {
            // cannot place an item in the slot if the power mode is not 2
            if (Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushiePower != 2)
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
            if (Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushiePower != 2)
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
            return GetInstance<PlushieEquipSlot>().FunctionalItem.type != ItemID.None;
        }

        public override string FunctionalTexture => "Kourindou/PlushieSlotBackground";

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
