using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using ReLogic.Content;
using ReLogic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameContent;
using static Terraria.ModLoader.ModContent;
using Kourindou.Buffs;
using Kourindou.Items;
using Kourindou.Items.Weapons;
using Kourindou.Items.Plushies;
using Kourindou.Projectiles.Plushies.PlushieEffects;
using Kourindou.Projectiles.Weapons;


namespace Kourindou
{
    public class CooldownTimes
    {
        public int CurrentTime { get; set; }
        public int MaximumTime { get; set; }
    }

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

        // Hotkeys
        public int keyTimer;
        public bool SkillKeyPressed;
        public bool SkillKeyHeld;
        public bool SkillKeyReleased;
        public bool UltimateKeyPressed;
        public bool UltimateKeyHeld;
        public bool UltimateKeyReleased;

        public bool UsedAttack;
        public int AttackID;
        public int AttackCounter;

        public int OldAttackID;
        public int OldAttackCounter;

        // Half Phantom pet active
        public bool HalfPhantomPet;

        // Cooldown
        public Dictionary<int, Dictionary<int, CooldownTimes>> Cooldowns = new Dictionary<int, Dictionary<int, CooldownTimes>>();
        public float CooldownTimeMultiplier;
        public int CooldownTimeAdditive;

        // Weapons
        public Dictionary<int, int> CrescentMoonStaffFlames = new Dictionary<int, int>();

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
            if (Player.HeldItem.ModItem is MultiUseItem item)
            {
                List<string> SkillKeys = Kourindou.SkillKey.GetAssignedKeys();
                List<string> UltimateKeys = Kourindou.UltimateKey.GetAssignedKeys();

                if (SkillKeys.Count == 0 && UltimateKeys.Count == 0)
                {
                    return;
                }

                if (item.HasSkill(Player)
                    && item.HasUltimate(Player)
                    && SkillKeys.Count > 0
                    && UltimateKeys.Count > 0
                    && SkillKeys[0] == UltimateKeys[0])
                {
                    // Skill is activated by clicking the assigned button
                    // Ultimate is activated by holding the button for 1.5 seconds (90 ticks)
                    if (Kourindou.SkillKey.JustReleased && keyTimer < 60)
                    {
                        SkillKeyPressed = true;
                    }
                    else if (Kourindou.SkillKey.JustReleased && keyTimer >= 60)
                    {
                        UltimateKeyPressed = true;
                    }

                    // When held down increase the timer
                    if (Kourindou.SkillKey.Current)
                    {
                        keyTimer++;
                    }

                    // When the button is released or no longer pressed, reset the timer
                    if (Kourindou.SkillKey.JustReleased || !Kourindou.UltimateKey.Current)
                    {
                        keyTimer = 0;
                    }
                }
                else
                {
                    if (item.HasSkill(Player))
                    {
                        if (Kourindou.SkillKey.JustPressed) SkillKeyPressed = true;
                        SkillKeyHeld = Kourindou.SkillKey.Current;
                        if (Kourindou.SkillKey.JustReleased) SkillKeyReleased = true;
                    }

                    if (item.HasUltimate(Player))
                    {
                        if (Kourindou.UltimateKey.JustPressed) UltimateKeyPressed = true;
                        UltimateKeyHeld = Kourindou.UltimateKey.Current;
                        if (Kourindou.UltimateKey.JustReleased) UltimateKeyReleased = true;
                    }
                }

                if (Player.HeldItem.ModItem.AltFunctionUse(Player))
                {
                    Player.controlUseItem = true;
                    Player.altFunctionUse = 1;
                }
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

            CooldownTimeMultiplier = 1f;
            CooldownTimeAdditive = 0;

            // Crescent Moon Staff - Check the projectiles in the dictionary, if they are wrong remove them
            if (Player.whoAmI == Main.myPlayer)
            {
                List<int> removeKey = new List<int>();
                foreach (KeyValuePair<int, int> pair in CrescentMoonStaffFlames)
                {
                    Projectile proj = Main.projectile[pair.Value];
                    if (!proj.active
                    || proj.owner != Player.whoAmI
                    || proj.type != ProjectileType<CrescentMoonStaffFlame>()
                    || pair.Key != (int)proj.ai[1])
                    {
                        removeKey.Add(pair.Key);
                    }
                }
                foreach (int i in removeKey)
                {
                    CrescentMoonStaffFlames.Remove(i);
                }
            }
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref bool crit)
        {
            // Shion Yorigami random damage increase on NPC hits 0.1% chance
            if (PlushieSlotItemID == ItemType<ShionYorigami_Plushie_Item>())
            {
                if ((int)Main.rand.Next(1, 1000) == 1)
                {
                    damage = (int)(damage * Main.rand.NextFloat(1000f, 1000000f));
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

        private void CirnoPlushie_OnHit(Player p, NPC n, bool crit)
        {
            CirnoPlushie_Attack_Counter++;

            if (CirnoPlushie_Attack_Counter == 8 && (int)Main.rand.Next(0, 10) == 9)
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
                        Player.GetSource_Accessory(GetInstance<PlushieEquipSlot>().FunctionalItem),
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
                        Player.GetSource_Accessory(GetInstance<PlushieEquipSlot>().FunctionalItem),
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
                    packet2.Write((byte)KourindouMessageType.PlaySound);
                    packet2.Write((byte)SoundID.DD2_ExplosiveTrapExplode.SoundId);
                    packet2.Write((short)SoundID.DD2_ExplosiveTrapExplode.Style);
                    packet2.Write((float)0.8f);
                    packet2.Write((float)1f);
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
                    Player.GetSource_Accessory(GetInstance<PlushieEquipSlot>().FunctionalItem),
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
            if ((int)Main.rand.Next(0, 5) == 0 && n.life <= 0)
            {
                n.NPCLoot();
            }
        }

        //--------------------------------------------------------- Multi-useItem ---------------------------------------------------------
        public bool OnCooldown(int itemID, int AttackID)
        {
            if (Cooldowns.ContainsKey(itemID))
            {
                if (Cooldowns[itemID].ContainsKey(AttackID))
                {
                    return Cooldowns[itemID][AttackID].CurrentTime > 0;
                }
            }
            return false;
        }

        public void SetCooldown(int itemID, int AttackID , int time)
        {
            if (!Cooldowns.ContainsKey(itemID))
            {
                Cooldowns.Add(itemID, new Dictionary<int, CooldownTimes>());
            }

            if (!Cooldowns[itemID].ContainsKey(AttackID))
            {
                Cooldowns[itemID].Add(AttackID, new CooldownTimes { CurrentTime = time, MaximumTime = time });
            }
            else
            {
                Cooldowns[itemID][AttackID].CurrentTime = time;
                Cooldowns[itemID][AttackID].MaximumTime = time;
            }
        }

        public override bool PreItemCheck()
        {
            // player is not holding a multiuse weapon
            if (!(Player.HeldItem.ModItem is MultiUseItem item))
            { 
                // return, we don't have a multiuse weapon
                return base.PreItemCheck();
            }

            // Get the AttackID based on the weapon and keybind used
            if (Player.itemAnimation <= 1 && Main.myPlayer == Player.whoAmI)
            {
                // Prioritize the ultimate if both keys are present at the same time
                if (Main.mouseLeft && !(UltimateKeyPressed || UltimateKeyHeld) && !(SkillKeyPressed || SkillKeyHeld) && item.HasNormal(Player))
                {
                    UsedAttack = true;
                    AttackID = item.GetNormalID(Player);
                    AttackCounter = item.GetNormalCounter(Player);
                }
                else if (!Main.mouseLeft && (SkillKeyPressed || SkillKeyHeld) && !(UltimateKeyPressed || UltimateKeyHeld) && item.HasSkill(Player))
                {
                    UsedAttack = true;
                    AttackID = item.GetSkillID(Player);
                    AttackCounter = item.GetSkillCounter(Player);
                }
                else if (!Main.mouseLeft && (UltimateKeyPressed || UltimateKeyHeld) && !(SkillKeyPressed || SkillKeyHeld) && item.HasUltimate(Player))
                {
                    UsedAttack = true;
                    AttackID = item.GetUltimateID(Player);
                    AttackCounter = item.GetUltimateCounter(Player);
                }
                else if (!Main.mouseLeft && !(UltimateKeyPressed || UltimateKeyHeld) && !(SkillKeyPressed || SkillKeyHeld))
                {
                    item.SetDefaults();
                    return base.PreItemCheck();
                }
                else
                {
                    UsedAttack = true;
                    AttackID = OldAttackID;
                    AttackCounter = OldAttackCounter;
                }
            }

            if (Player.itemAnimation <= 1 && Main.myPlayer != Player.whoAmI)
            {
                item.SetDefaults();
            }

            if (UsedAttack)
            {
                Player.altFunctionUse = 2;

                if (Main.myPlayer == Player.whoAmI && OnCooldown(item.Type, AttackID))
                {
                    return base.PreItemCheck();
                }

                if (Main.myPlayer == Player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient && !OnCooldown(item.Type, AttackID))
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte) KourindouMessageType.AlternateFire);
                    packet.Write((byte) Player.whoAmI);
                    packet.Write((bool) UsedAttack);
                    packet.Write((int) AttackID);
                    packet.Write((int) AttackCounter);
                    packet.Send();
                }

                item.SetItemStats(Player, AttackID, AttackCounter);
                Player.lastVisualizedSelectedItem = Player.HeldItem;

                if (Main.myPlayer == Player.whoAmI)
                {
                    OldAttackID = AttackID;
                    OldAttackCounter = AttackCounter;
                }
            }

            return base.PreItemCheck();
        }

        public override void PostUpdate()
        {
            // Put hotkey status to false for next tick
            SkillKeyPressed = false;
            SkillKeyHeld = false;
            SkillKeyReleased = false;

            UltimateKeyPressed = false;
            UltimateKeyHeld = false;
            UltimateKeyReleased = false;

            UsedAttack = false;
            AttackID = 0;
            AttackCounter = 0;
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

    public class HeldItemLayer : PlayerDrawLayer
    {
        Texture2D texture;
        int itemTexture;
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.HeldItem.type == ItemType<KoninginDerNacht>()
                && drawInfo.drawPlayer.itemAnimation > 0
                && !drawInfo.drawPlayer.dead
                && !drawInfo.drawPlayer.noItems
                && !drawInfo.drawPlayer.CCed;
        }
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            const float StartAngle = -140f;
            const float SwingAngle = 180f;
            const float HeldOffset = 6f;

            if (drawInfo.drawPlayer.HeldItem.type == ItemType<KoninginDerNacht>())
            {
                if (itemTexture != drawInfo.drawPlayer.HeldItem.type)
                {
                    itemTexture = drawInfo.drawPlayer.HeldItem.type;
                    texture = TextureAssets.Item[drawInfo.drawPlayer.HeldItem.type].Value;
                }
            }

            int drawX = (int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2f + drawInfo.drawPlayer.width / 2f);
            int drawY = (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4);
            Vector2 position = new Vector2(drawX, drawY) + drawInfo.drawPlayer.bodyPosition + drawInfo.bodyVect;

            float rotation;

            if (drawInfo.drawPlayer.direction == 1)
            {
                rotation = StartAngle + (SwingAngle / drawInfo.drawPlayer.itemAnimationMax * (drawInfo.drawPlayer.itemAnimationMax - drawInfo.drawPlayer.itemAnimation)) + 45f;
                position += new Vector2(HeldOffset, 0f).RotatedBy(MathHelper.ToRadians(rotation - 45f));
            }
            else
            {
                rotation = StartAngle + 100f - (SwingAngle / drawInfo.drawPlayer.itemAnimationMax * (drawInfo.drawPlayer.itemAnimationMax - drawInfo.drawPlayer.itemAnimation)) - 45f + 180f;
                position += new Vector2(HeldOffset, 0f).RotatedBy(MathHelper.ToRadians(rotation - 135f));
            }

            Vector2 offset = new Vector2(0, 0);

            // Bodyframe Y values that indicate the player is at a position in the step cycle
            // where the sprite shifts.
            if (drawInfo.drawPlayer.bodyFrame.Y == 392 || drawInfo.drawPlayer.bodyFrame.Y == 448 || drawInfo.drawPlayer.bodyFrame.Y == 504
                || drawInfo.drawPlayer.bodyFrame.Y == 784 || drawInfo.drawPlayer.bodyFrame.Y == 840 || drawInfo.drawPlayer.bodyFrame.Y == 896)
            {
                offset.Y += 2;
            }

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (drawInfo.drawPlayer.direction == -1)
            {
                spriteEffects |= SpriteEffects.FlipHorizontally;
            }
            if ((int)drawInfo.drawPlayer.gravDir == -1)
            {
                spriteEffects |= SpriteEffects.FlipVertically;
                offset.Y *= -1;
            }

            drawInfo.DrawDataCache.Add(new DrawData(
                texture,
                position,
                new Rectangle(0, 0, texture.Width, texture.Height),
                drawInfo.itemColor,
                MathHelper.ToRadians(rotation),
                new Vector2(spriteEffects.HasFlag((Enum)(object)(SpriteEffects)1) ? texture.Width : 0, texture.Height) + offset,
                1f,
                spriteEffects,
                0
            ));
        }
    }
}
