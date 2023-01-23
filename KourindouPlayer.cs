using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using ReLogic.Content;
using ReLogic;
using Terraria;
using Terraria.Chat;
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
using Kourindou.Items.Plushies;
using Kourindou.Items.Consumables;
using Kourindou.Projectiles.Plushies.PlushieEffects;
//using Kourindou.Projectiles.Weapons;


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
        public bool plushiePower;

        // Item ID of the plushie slot item
        public HashSet<int> EquippedPlushies = new HashSet<int>();

        // Reimu plushie maximum homing distance
        public float ReimuPlushieMaxDistance = 500f;

        // Cirno Plushie Effect Attack Counter
        public byte CirnoPlushie_Attack_Counter;
        public bool CirnoPlushie_TimesNine;

        // Ran Plushie kill and stack counter
        public byte RanPlushie_EnemieKillCounter;
        public byte RanPlushie_Stacks;

        // Tenshi Plushie Effect
        public bool TenshiPlushie_Revenge;
        public int TenshiPlushie_Damage;

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

        // Items
        public int FumoColaTurnIntoPlushieID = -1;
        public int FumoColaAnimationTimer;
        public int FumoColaBuffStacks;

        // Half Phantom pet active
        public bool HalfPhantomPet;

        // Buffs
        public bool DebuffMedicineMelancholy;
        public int DebuffMedicineMelancholyStacks;

        // Cooldown
        public Dictionary<int, Dictionary<int, CooldownTimes>> Cooldowns = new Dictionary<int, Dictionary<int, CooldownTimes>>();
        public float CooldownTimeMultiplier;
        public int CooldownTimeAdditive;

        // Weapons
        // public Dictionary<int, int> CrescentMoonStaffFlames = new Dictionary<int, int>();


        //--------------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------- Save/Load data --------------------------------------------------//
        //--------------------------------------------------------------------------------------------------------------------//

        public override void SaveData(TagCompound tag)
        {
            tag.Add("plushiePowerMode", plushiePower);
            tag.Add("cirnoPlushieAttackCounter", CirnoPlushie_Attack_Counter);
            tag.Add("cirnoPlushieTimesNine", CirnoPlushie_TimesNine);
            tag.Add("fumoColaBuffStacks", FumoColaBuffStacks);
            tag.Add("ranPlushieStacks", RanPlushie_Stacks);
        }

        public override void LoadData(TagCompound tag)
        {
            plushiePower = tag.GetBool("plushiePowerMode");
            CirnoPlushie_Attack_Counter = tag.GetByte("cirnoPlushieAttackCounter");
            CirnoPlushie_TimesNine = tag.GetBool("cirnoPlushieTimesNine");
            FumoColaBuffStacks = tag.GetInt("fumoColaBuffStacks");
            RanPlushie_Stacks = tag.GetByte("ranPlushieStacks");
        }

        //--------------------------------------------------------------------------------------------------------------------//
        //----------------------------------------------------- Drawing ------------------------------------------------------//
        //--------------------------------------------------------------------------------------------------------------------//

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (DebuffMedicineMelancholy)
            {
                if (Main.rand.Next(0, 3) == 0)
                {
                    Dust.NewDust(
                        drawInfo.drawPlayer.position,
                        drawInfo.drawPlayer.width,
                        drawInfo.drawPlayer.height,
                        DustID.Cloud,
                        Main.rand.NextFloat(-2f, 2f),
                        Main.rand.NextFloat(-2f, 2f),
                        Main.rand.Next(10, 255),
                        new Color(193, 11, 136),
                        Main.rand.NextFloat(0.1f, 1f)
                    );
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------//
        //----------------------------------------------------- Player methods -----------------------------------------------//
        //--------------------------------------------------------------------------------------------------------------------//
        public override void OnEnterWorld(Player player)
        {
            Kourindou.KourindouConfigClient.plushiePower = Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushiePower;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Send the configured plushie power setting
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)KourindouMessageType.ClientConfig);
                packet.Write((byte)Main.myPlayer);
                packet.Write((bool)Kourindou.KourindouConfigClient.plushiePower);
                packet.Send(-1, Main.myPlayer);

                // Send the stack amount of the Ran Plushie
                packet = Mod.GetPacket();
                packet.Write((byte)KourindouMessageType.RanPlushieStacks);
                packet.Write((byte)Main.myPlayer);
                packet.Write((byte)Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().RanPlushie_Stacks);
                packet.Send(-1, Main.myPlayer);
            }
        }


        public override void PlayerConnect(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Send the configured plushie power setting
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)KourindouMessageType.ClientConfig);
                packet.Write((byte)Main.myPlayer);
                packet.Write((bool)Kourindou.KourindouConfigClient.plushiePower);
                packet.Send(-1, Main.myPlayer);

                // Send the stack amount of the Ran Plushie
                packet = Mod.GetPacket();
                packet.Write((byte)KourindouMessageType.RanPlushieStacks);
                packet.Write((byte)Main.myPlayer);
                packet.Write((byte)Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().RanPlushie_Stacks);
                packet.Send(-1, Main.myPlayer);
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (EquippedPlushies.Contains(ItemType<YukariYakumo_Plushie_Item>()))
            {
                SkillKeyPressed = Kourindou.SkillKey.JustPressed;
            }

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

        public override void PreUpdate()
        {
            if (FumoColaAnimationTimer > 0)
            {
                FumoColaAnimationTimer--;
            }            
        }

        public override void ResetEffects()
        {
            // Reset the plushie item slots
            EquippedPlushies.Clear();

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

            //            // Crescent Moon Staff - Check the projectiles in the dictionary, if they are wrong remove them
            //            if (Player.whoAmI == Main.myPlayer)
            //            {
            //                List<int> removeKey = new List<int>();
            //                foreach (KeyValuePair<int, int> pair in CrescentMoonStaffFlames)
            //                {
            //                    Projectile proj = Main.projectile[pair.Value];
            //                    if (!proj.active
            //                    || proj.owner != Player.whoAmI
            //                    || proj.type != ProjectileType<CrescentMoonStaffFlame>()
            //                    || pair.Key != (int)proj.ai[1])
            //                    {
            //                        removeKey.Add(pair.Key);
            //                    }
            //                }
            //                foreach (int i in removeKey)
            //                {
            //                    CrescentMoonStaffFlames.Remove(i);
            //                }
            //            }

            // Reset Medicine's debuff
            DebuffMedicineMelancholy = false;

            if (!Player.HasBuff(BuffType<DeBuff_MedicineMelancholy>()))
            {
                DebuffMedicineMelancholyStacks = 0;
            }

            // Reset FumoCola fields
            if (FumoColaTurnIntoPlushieID != -1 
                && (FumoColaAnimationTimer == 0
                || Player.itemAnimation == 0
                || !Player.ItemAnimationActive))
            {
                FumoColaTurnIntoPlushieID = -1;
            }

            // Reset FumoCola buff
            if (!Player.HasBuff(BuffType<Buff_FumoCola>()))
            {
                FumoColaBuffStacks = 0;
            }
        }

        // Gensokyo Fairy Plushies
        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
        {
            if (Kourindou.GensokyoLoaded)
            {
                if (npc.type == Kourindou.Gensokyo_Fairy_Bone_Type && EquippedPlushies.Contains(ItemType<Gensokyo_Bone_Fairy_Plushie_Item>())) { return false; }
                if (npc.type == Kourindou.Gensokyo_Fairy_Flower_Type && EquippedPlushies.Contains(ItemType<Gensokyo_Flower_Fairy_Plushie_Item>())) { return false; }
                if (npc.type == Kourindou.Gensokyo_Fairy_Lava_Type && EquippedPlushies.Contains(ItemType<Gensokyo_Lava_Fairy_Plushie_Item>())) { return false; }
                if (npc.type == Kourindou.Gensokyo_Fairy_Snow_Type && EquippedPlushies.Contains(ItemType<Gensokyo_Snow_Fairy_Plushie_Item>())) { return false; }
                if (npc.type == Kourindou.Gensokyo_Fairy_Stone_Type && EquippedPlushies.Contains(ItemType<Gensokyo_Stone_Fairy_Plushie_Item>())) { return false; }
                if (npc.type == Kourindou.Gensokyo_Fairy_Sunflower_Type && EquippedPlushies.Contains(ItemType<Gensokyo_Sunflower_Fairy_Plushie_Item>())) { return false; }
                if (npc.type == Kourindou.Gensokyo_Fairy_Thorn_Type && EquippedPlushies.Contains(ItemType<Gensokyo_Thorn_Fairy_Plushie_Item>())) { return false; }
            }

            return base.CanBeHitByNPC(npc, ref cooldownSlot);
        }

        #region OnHit
        // --------- Triggers on PVE --------- //
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            // Cirno Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<Cirno_Plushie_Item>()))
            {
                CirnoPlushie_OnHit(null, target, crit);
            }

            // Flandre Scarlet Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<FlandreScarlet_Plushie_Item>()))
            {
                FlandreScarletPlushie_OnHit(target, null, damage, crit, item.useAnimation);
            }

            // Marisa Kirisame Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<MarisaKirisame_Plushie_Item>()))
            {
                MarisaKirisamePlushie_OnHit(target.Center, crit, target);
            }

            // Remilia Scarlet Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<Kourindou_RemiliaScarlet_Plushie_Item>()))
            {
                RemiliaScarletPlushie_OnHit(damage);
            }

            // Satori Komeiji Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<SatoriKomeiji_Plushie_Item>()))
            {
                SatoriKomeijiPlushie_OnHit(target, null);
            }

            // Tewi Inaba Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<TewiInaba_Plushie_Item>()))
            {
                TewiInabaPlushie_OnHit(target);
            }

            // Medicine Melancholy Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<MedicineMelancholy_Plushie_Item>()))
            {
                MedicineMelancholyPlushie_OnHit(target, null);
            }

            // Toyosatomimi No Miko Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<ToyosatomimiNoMiko_Plushie_Item>()))
            {
                ToyosatomimiNoMikoPlushie_OnHit(target, null, crit);
            }

            // Chen Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<Chen_Plushie_Item>()))
            {
                ChenPlushie_OnHit(target, null);
            }

            // Ran Yakumo Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<RanYakumo_Plushie_Item>()))
            {
                RanYakumoPlushie_OnHit(target, null);
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockBack, bool crit)
        {
            // Cirno Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<Cirno_Plushie_Item>()))
            {
                CirnoPlushie_OnHit(null, target, crit);
            }

            // Flandre Scarlet Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<FlandreScarlet_Plushie_Item>()))
            {
                FlandreScarletPlushie_OnHit(target, null, damage, crit, target.immune[proj.owner]);
                if (crit)
                {
                    target.immune[proj.owner] = 0;
                }
            }

            // Marisa Kirisame Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<MarisaKirisame_Plushie_Item>()))
            {
                MarisaKirisamePlushie_OnHit(target.Center, crit, target);
            }

            //Remilia Scarlet Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<Kourindou_RemiliaScarlet_Plushie_Item>()))
            {
                RemiliaScarletPlushie_OnHit(damage);
            }

            // Satori Komeiji Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<SatoriKomeiji_Plushie_Item>()))
            {
                SatoriKomeijiPlushie_OnHit(target, null);
            }

            // Tewi Inaba Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<TewiInaba_Plushie_Item>()))
            {
                TewiInabaPlushie_OnHit(target);
            }

            // Medicine Melancholy Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<MedicineMelancholy_Plushie_Item>()))
            {
                MedicineMelancholyPlushie_OnHit(target, null);
            }

            // Toyosatomimi No Miko Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<ToyosatomimiNoMiko_Plushie_Item>())
                && proj.type != ProjectileType<ToyosatomimiNoMiko_Plushie_LaserBeam>())
            {
                ToyosatomimiNoMikoPlushie_OnHit(target, null, crit);
            }

            // Chen Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<Chen_Plushie_Item>()))
            {
                ChenPlushie_OnHit(target, null);
            }

            // Ran Yakumo Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<RanYakumo_Plushie_Item>()))
            {
                RanYakumoPlushie_OnHit(target, null);
            }
        }

        // --------- Triggers on PVP --------- //
        public override void OnHitPvp(Item item, Player target, int damage, bool crit)
        {
            // Cirno Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<Cirno_Plushie_Item>()))
            {
                CirnoPlushie_OnHit(target, null, crit);
            }

            // Flandre Scarlet Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<FlandreScarlet_Plushie_Item>()))
            {
                FlandreScarletPlushie_OnHit(null, target, damage, crit, item.useAnimation);
            }

            // Marisa Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<MarisaKirisame_Plushie_Item>()))
            {
                MarisaKirisamePlushie_OnHit(target.Center, crit, target);
            }

            //Remilia Scarlet Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<Kourindou_RemiliaScarlet_Plushie_Item>()))
            {
                RemiliaScarletPlushie_OnHit(damage);
            }

            // Satori Komeiji Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<SatoriKomeiji_Plushie_Item>()))
            {
                SatoriKomeijiPlushie_OnHit(null, target);
            }

            // Medicine Melancholy Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<MedicineMelancholy_Plushie_Item>()))
            {
                MedicineMelancholyPlushie_OnHit(null, target);
            }

            // Toyosatomimi No Miko Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<ToyosatomimiNoMiko_Plushie_Item>()))
            {
                ToyosatomimiNoMikoPlushie_OnHit(null, target, crit);
            }

            // Chen Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<Chen_Plushie_Item>()))
            {
                ChenPlushie_OnHit(null, target);
            }

            // Ran Yakumo Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<RanYakumo_Plushie_Item>()))
            {
                RanYakumoPlushie_OnHit(null, target);
            }
        }
        public override void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit)
        {
            // Cirno Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<Cirno_Plushie_Item>()))
            {
                CirnoPlushie_OnHit(target, null, crit);
            }

            // Flandre Scarlet Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<FlandreScarlet_Plushie_Item>()))
            {
                FlandreScarletPlushie_OnHit(null, target, damage, crit, target.immuneTime);
                if (crit)
                {
                    target.immuneTime = 0;
                }
            }

            // Marisa Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<MarisaKirisame_Plushie_Item>()))
            {
                MarisaKirisamePlushie_OnHit(target.Center, crit, target);
            }

            //Remilia Scarlet Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<Kourindou_RemiliaScarlet_Plushie_Item>()))
            {
                RemiliaScarletPlushie_OnHit(damage);
            }

            // Satori Komeiji Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<SatoriKomeiji_Plushie_Item>()))
            {
                SatoriKomeijiPlushie_OnHit(null, target);
            }

            // Medicine Melancholy Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<MedicineMelancholy_Plushie_Item>()))
            {
                MedicineMelancholyPlushie_OnHit(null, target);
            }

            // Toyosatomimi No Miko Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<ToyosatomimiNoMiko_Plushie_Item>())
                && proj.type != ProjectileType<ToyosatomimiNoMiko_Plushie_LaserBeam>())
            {
                ToyosatomimiNoMikoPlushie_OnHit(null, target, crit);
            }

            // Chen Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<Chen_Plushie_Item>()))
            {
                ChenPlushie_OnHit(null, target);
            }

            // Ran Yakumo Plushie Equipped
            if (EquippedPlushies.Contains(ItemType<RanYakumo_Plushie_Item>()))
            {
                RanYakumoPlushie_OnHit(null, target);
            }
        }

        private void CirnoPlushie_OnHit(Player p, NPC n, bool crit)
        {
            CirnoPlushie_Attack_Counter++;

            if (CirnoPlushie_Attack_Counter == 9 && (int)Main.rand.Next(0, 10) == 9)
            {
                CirnoPlushie_TimesNine = true;
            }

            if (CirnoPlushie_Attack_Counter >= 9)
            {
                CirnoPlushie_Attack_Counter = 0;
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

                SoundEngine.PlaySound(
                    SoundID.DD2_ExplosiveTrapExplode with { Volume = .8f, PitchVariance = .1f },
                    position);

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Send sound packet for other clients
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)KourindouMessageType.PlaySound);
                    packet.Write((string)"DD2_ExplosiveTrapExplode");
                    packet.Write((float)0.8f);
                    packet.Write((float)1f);
                    packet.Write((int)position.X);
                    packet.Write((int)position.Y);
                    packet.Send(-1, Main.myPlayer);
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

        private void MedicineMelancholyPlushie_OnHit(NPC n, Player p)
        {
            if (n != null)
            {
                if ((int)Main.rand.Next(0, 100) < 12)
                {
                    n.AddBuff(BuffType<DeBuff_MedicineMelancholy>(), 600);
                }
                n.AddBuff(BuffID.Poisoned, 600);
            }

            if (p != null)
            {
                if ((int)Main.rand.Next(0, 100) < 12)
                {
                    p.AddBuff(BuffType<DeBuff_MedicineMelancholy>(), 300);
                }
                p.AddBuff(BuffID.Poisoned, 300);
            }
        }

        private void ToyosatomimiNoMikoPlushie_OnHit(NPC n, Player p, bool crit)
        {
            Vector2 SpawnPosition = Vector2.Zero;
            if (n != null)
            {
                SpawnPosition = n.Center;
            }
            if (p != null)
            {
                SpawnPosition = p.Center;
            }

            Projectile.NewProjectile(
                Player.GetSource_Accessory(GetInstance<PlushieEquipSlot>().FunctionalItem),
                SpawnPosition,
                Vector2.Zero,
                ProjectileType<ToyosatomimiNoMiko_Plushie_LaserBeam>(),
                Player.HeldItem.damage,
                Player.HeldItem.knockBack,
                Main.myPlayer,
                0f,
                crit ? 1f : 0f
            );
        }

        private void ChenPlushie_OnHit(NPC n, Player p)
        {
            if (n != null && n.life <= 0 && !n.friendly && n.lifeMax > 5)
            {
                // On kill gain rapid healing, well fed and 25 health
                Player.AddBuff(BuffID.RapidHealing, 720);
                Player.AddBuff(BuffID.WellFed, 720);
                Player.statLife += 25;
                Player.HealEffect(25, true);
            }

            if (p != null && (p.statLife <= 0 || p.dead))
            {
                // On kill gain rapid healing, well fed and 25 health
                Player.AddBuff(BuffID.RapidHealing, 720);
                Player.AddBuff(BuffID.WellFed, 720);
                Player.statLife += 25;
                Player.HealEffect(25, true);
            }
        }

        private void RanYakumoPlushie_OnHit(NPC n, Player p)
        {
            if (n != null && n.life <= 0 && !n.friendly && n.lifeMax > 5)
            {
                // Increase kill counter
                if (RanPlushie_Stacks < 8)
                {
                    RanPlushie_EnemieKillCounter++;
                }

                // Increase stacks
                if (RanPlushie_EnemieKillCounter >= 10 && RanPlushie_Stacks < 8)
                {
                    RanPlushie_Stacks++;
                    RanPlushie_EnemieKillCounter = 0;

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        ModPacket packet = Mod.GetPacket();
                        packet.Write((byte)KourindouMessageType.RanPlushieStacks);
                        packet.Write((byte)Main.myPlayer);
                        packet.Write((byte)RanPlushie_Stacks);
                        packet.Send(-1, Main.myPlayer);
                    }
                }
            }

            if (p != null && (p.statLife <= 0 || p.dead))
            {
                // Increase kill counter
                if (RanPlushie_Stacks < 8)
                {
                    RanPlushie_EnemieKillCounter++;
                }

                // Increase stacks
                if (RanPlushie_EnemieKillCounter >= 10 && RanPlushie_Stacks < 8)
                {
                    RanPlushie_Stacks++;
                    RanPlushie_EnemieKillCounter = 0;

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        ModPacket packet = Mod.GetPacket();
                        packet.Write((byte)KourindouMessageType.RanPlushieStacks);
                        packet.Write((byte)Main.myPlayer);
                        packet.Write((byte)RanPlushie_Stacks);
                        packet.Send(-1, Main.myPlayer);
                    }
                }
            }
        }
        #endregion

        #region OnHitME
        public override void OnHitByNPC(NPC npc, int damage, bool crit)
        {
            if (EquippedPlushies.Contains(ItemType<TenshiHinanawi_Plushie_Item>()))
            {
                TenshiHinanawiPlushie_OnHitBy(damage);
            }
        }

        public override void OnHitByProjectile(Projectile proj, int damage, bool crit)
        {
            if (EquippedPlushies.Contains(ItemType<TenshiHinanawi_Plushie_Item>()))
            {
                TenshiHinanawiPlushie_OnHitBy(damage);
            }
        }

        private void TenshiHinanawiPlushie_OnHitBy(int damage)
        {
            TenshiPlushie_Damage = damage;
            TenshiPlushie_Revenge = true;
        }
        #endregion

        #region ModifyHit
        // --------- Hits this player does on NPC --------- //
        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            // Cirno Plushie every 9th hit has 9% chance to deal 9 times dmg
            if (EquippedPlushies.Contains(ItemType<Cirno_Plushie_Item>()) && CirnoPlushie_TimesNine)
            {
                damage *= 9;
                CirnoPlushie_TimesNine = false;
            }

            // Hitting an NPC with Patchouli Knowledge Plushie equipped deals no damage except for magic type...
            if (EquippedPlushies.Contains(ItemType<PatchouliKnowledge_Plushie_Item>()))
            {
                if (item.CountsAsClass(DamageClass.Melee) || item.CountsAsClass(DamageClass.Ranged) || item.CountsAsClass(DamageClass.Throwing))
                {
                    damage = 0;
                }
            }

            // Shion Yorigami random damage increase on NPC hits 0.1% chance
            if (EquippedPlushies.Contains(ItemType<ShionYorigami_Plushie_Item>()) && (int)Main.rand.Next(1, 1000) == 1)
            {
                damage = (int)(damage * Main.rand.NextFloat(1000f, 1000000f));
            }

            // Byakuren equipped = melee crits deal 200% DMG and 3 times more knockback
            if (EquippedPlushies.Contains(ItemType<ByakurenHijiri_Plushie_Item>()) && item.DamageType == DamageClass.Melee)
            {
                if (crit)
                {
                    damage *= 2;
                }
                knockback *= 3;
            }

            // Tenshi Hinanawi revenge effect
            if (EquippedPlushies.Contains(ItemType<TenshiHinanawi_Plushie_Item>()) && TenshiPlushie_Revenge)
            {
                damage += TenshiPlushie_Damage * 5;
                TenshiPlushie_Revenge = false;
            }
        }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            // Cirno Plushie every 9th hit has 9% chance to deal 9 times dmg
            if (EquippedPlushies.Contains(ItemType<Cirno_Plushie_Item>()) && CirnoPlushie_TimesNine)
            {
                damage *= 9;
                CirnoPlushie_TimesNine = false;
            }

            // Hitting an NPC with Patchouli Knowledge Plushie equipped deals no damage except for magic type...
            if (EquippedPlushies.Contains(ItemType<PatchouliKnowledge_Plushie_Item>()))
            {
                if (proj.CountsAsClass(DamageClass.Melee) || proj.CountsAsClass(DamageClass.Ranged) || proj.CountsAsClass(DamageClass.Throwing) || proj.minion)
                {
                    damage = 0;
                }
            }

            // Shion Yorigami random damage increase on NPC hits 0.1% chance
            if (EquippedPlushies.Contains(ItemType<ShionYorigami_Plushie_Item>()) && (int)Main.rand.Next(1, 1000) == 1)
            {
                damage = (int)(damage * Main.rand.NextFloat(1000f, 1000000f));
            }

            // Disable crit for Flandre Scarlet Plushie effect
            if (proj.type == ProjectileType<FlandreScarlet_Plushie_Explosion>())
            {
                crit = false;
            }

            // Byakuren equipped = melee crits deal 200% DMG and 3 times more knockback
            if (EquippedPlushies.Contains(ItemType<ByakurenHijiri_Plushie_Item>()) && proj.DamageType == DamageClass.Melee)
            {
                if (crit)
                {
                    damage *= 2;
                }
                knockback *= 3;
            }

            // Tenshi Hinanawi revenge effect
            if (EquippedPlushies.Contains(ItemType<TenshiHinanawi_Plushie_Item>()) && TenshiPlushie_Revenge)
            {
                damage += TenshiPlushie_Damage * 5;
                TenshiPlushie_Revenge = false;
            }
        }

        // --------- Hits on OTHER player --------- //
        public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
        {
            // Cirno Plushie every 9th hit has 9% chance to deal 9 times dmg
            if (EquippedPlushies.Contains(ItemType<Cirno_Plushie_Item>()) && CirnoPlushie_TimesNine)
            {
                damage *= 9;
                CirnoPlushie_TimesNine = false;
            }

            // Hitting an NPC with Patchouli Knowledge Plushie equipped deals no damage except for magic type...
            if (EquippedPlushies.Contains(ItemType<PatchouliKnowledge_Plushie_Item>()))
            {
                if (item.CountsAsClass(DamageClass.Melee) || item.CountsAsClass(DamageClass.Ranged) || item.CountsAsClass(DamageClass.Throwing))
                {
                    damage = 0;
                }
            }

            // Shion Yorigami random damage increase on Player hits 0.1% chance
            if (EquippedPlushies.Contains(ItemType<ShionYorigami_Plushie_Item>()) && (int)Main.rand.Next(1, 1000) == 1)
            {
                damage = (int)(damage * Main.rand.NextFloat(1000f, 1000000f));
            }

            // Byakuren equipped = melee crits deal 200% DMG and 3 times more knockback
            if (EquippedPlushies.Contains(ItemType<ByakurenHijiri_Plushie_Item>()) && item.DamageType == DamageClass.Melee && crit)
            {
                damage *= 2;
            }

            // Tenshi Hinanawi revenge effect
            if (EquippedPlushies.Contains(ItemType<TenshiHinanawi_Plushie_Item>()) && TenshiPlushie_Revenge)
            {
                damage += TenshiPlushie_Damage * 5;
                TenshiPlushie_Revenge = false;
            }
        }
        public override void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit)
        {
            // Cirno Plushie every 9th hit has 9% chance to deal 9 times dmg
            if (EquippedPlushies.Contains(ItemType<Cirno_Plushie_Item>()) && CirnoPlushie_TimesNine)
            {
                damage *= 9;
                CirnoPlushie_TimesNine = false;
            }

            // Hitting an NPC with Patchouli Knowledge Plushie equipped deals no damage except for magic type...
            if (EquippedPlushies.Contains(ItemType<PatchouliKnowledge_Plushie_Item>()))
            {
                if (proj.CountsAsClass(DamageClass.Melee) || proj.CountsAsClass(DamageClass.Ranged) || proj.CountsAsClass(DamageClass.Throwing) || proj.minion)
                {
                    damage = 0;
                }
            }

            // Shion Yorigami random damage increase on Player hits 0.1% chance
            if (EquippedPlushies.Contains(ItemType<ShionYorigami_Plushie_Item>()) && (int)Main.rand.Next(1, 1000) == 1)
            {
                damage = (int)(damage * Main.rand.NextFloat(1000f, 1000000f));
            }

            // Disable crit for Flandre Scarlet Plushie effect
            if (proj.type == ProjectileType<FlandreScarlet_Plushie_Explosion>())
            {
                crit = false;
            }

            // Byakuren equipped = melee crits deal 200% DMG and 3 times more knockback
            if (EquippedPlushies.Contains(ItemType<ByakurenHijiri_Plushie_Item>()) && proj.DamageType == DamageClass.Melee && crit)
            {
                damage *= 2;
            }

            // Tenshi Hinanawi revenge effect
            if (EquippedPlushies.Contains(ItemType<TenshiHinanawi_Plushie_Item>()) && TenshiPlushie_Revenge)
            {
                damage += TenshiPlushie_Damage * 5;
                TenshiPlushie_Revenge = false;
            }
        }
        #endregion

        #region ModifyHitME
        // --------- Hits on THIS player by NPC --------- //
        public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit)
        {
            if (DebuffMedicineMelancholy)
            {
                damage = (int)((float)damage * (1f + (0.04f * (DebuffMedicineMelancholyStacks + 1))));
            }
        }
        public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit)
        {
            if (DebuffMedicineMelancholy)
            {
                damage = (int)((float)damage * (1f + (0.04f * (DebuffMedicineMelancholyStacks + 1))));
            }
        }
        #endregion

        // --------- Player got hurt --------- //
        public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter)
        {
            if (EquippedPlushies.Contains(ItemType<Chen_Plushie_Item>()))
            {
                Player.AddBuff(BuffID.ShadowDodge, 180);
            }
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            // Kaguya or Mokou Plushie Equipped [Mortality]
            if (EquippedPlushies.Contains(ItemType<KaguyaHouraisan_Plushie_Item>()) || EquippedPlushies.Contains(ItemType<FujiwaraNoMokou_Plushie_Item>()))
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

                    if (EquippedPlushies.Contains(ItemType<FujiwaraNoMokou_Plushie_Item>()))
                    {
                        Player.AddBuff(BuffID.Wrath, 4140);
                        Player.AddBuff(BuffID.Inferno, 4140);
                    }

                    return false;
                }
            }

            return true;
        }

        public override void ModifyWeaponCrit(Item item, ref float crit)
        {
            if (EquippedPlushies.Contains(ItemType<ByakurenHijiri_Plushie_Item>()))
            {
                crit = 4f;
            }
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            // Reset Ran's counter and stacks when the player dies
            if (EquippedPlushies.Contains(ItemType<RanYakumo_Plushie_Item>()))
            {
                RanPlushie_EnemieKillCounter = 0;
                RanPlushie_Stacks = 0;

                if (Main.myPlayer == Player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)KourindouMessageType.RanPlushieStacks);
                    packet.Write((byte)Main.myPlayer);
                    packet.Write((byte)RanPlushie_Stacks);
                    packet.Send(-1, Main.myPlayer);
                }
            }
        }

        public override void UpdateBadLifeRegen()
        {
            // Medicine Melancholy poison tick
            if (DebuffMedicineMelancholy && !Player.buffImmune[BuffID.Poisoned])
            {
                int damagePerSecond = 5;

                if (Player.lifeRegen > 0)
                {
                    Player.lifeRegen = 0;
                }

                Player.lifeRegen -= damagePerSecond * (DebuffMedicineMelancholyStacks + 1) * (Player.HasBuff(BuffID.Venom) ? 2 : 1);
            }
        }

        public override void PostUpdateEquips()
        {
            // Post plushie effects for non-multiplier based stats
            foreach (int i in EquippedPlushies)
            {
                if (new Item(i).ModItem is PlushieItem plushie)
                {
                    plushie.PlushiePostUpdateEquips(Player);
                }
            }
        }

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

        public void SetCooldown(int itemID, int AttackID, int time)
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
                    packet.Write((byte)KourindouMessageType.AlternateFire);
                    packet.Write((byte)Main.myPlayer);
                    packet.Write((bool)UsedAttack);
                    packet.Write((int)AttackID);
                    packet.Write((int)AttackCounter);
                    packet.Send(-1, Main.myPlayer);
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

        public override void PostItemCheck()
        {
            // Fumo Cola animations
            if (FumoColaAnimationTimer > 0
                && Player.itemAnimation > 0
                && Player.ItemAnimationActive)
            {
                float progress = (float)Player.itemAnimation / (float)Player.itemAnimationMax;

                if ((int)((float)Player.itemAnimationMax * FumoCola.DrinkProgress) == (int)(progress * (float)Player.itemAnimationMax))
                {
                    SoundEngine.PlaySound(SoundID.Item3 with { Volume = 0.8f, PitchVariance = 0.1f });
                }

                float FrontArmAngle = 80f;
                float BackArmAngle = 35f;

                if ((int)Player.gravDir == -1)
                {
                    BackArmAngle *= -1;
                    FrontArmAngle *= -1;
                }

                if (progress > 0f && progress < FumoCola.DrinkProgress)
                {
                    BackArmAngle += 50f;
                }

                Player.compositeFrontArm = new Player.CompositeArmData(true, progress <= FumoCola.OpeningProgress ? Player.CompositeArmStretchAmount.ThreeQuarters : Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(Player.direction * -FrontArmAngle));
                Player.compositeBackArm = new Player.CompositeArmData(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(Player.direction * -BackArmAngle));

                Player.compositeBackArm.enabled = true;
                Player.compositeFrontArm.enabled = progress > FumoCola.DrinkProgress;
            }
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
            return Player.GetModPlayer<KourindouPlayer>().plushiePower;
        }

        public override bool IsVisibleWhenNotEnabled()
        {
            return FunctionalItem.type != ItemID.None;
        }

        public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
        {
            // cannot place an item in the slot if the power mode is not active
            if (!Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushiePower)
            {
                return false;
            }

            if (Main.player[Main.myPlayer].wingsLogic > 0 && checkItem.type == ItemType<AyaShameimaru_Plushie_Item>())
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
            if (!Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushiePower)
            {
                return false;
            }

            if (Main.player[Main.myPlayer].wingsLogic > 0 && item.type == ItemType<AyaShameimaru_Plushie_Item>())
            {
                return false;
            }

            if (item.ModItem is PlushieItem plushie)
            {
                return true;
            }

            return false;
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
        public Texture2D HeldItemTexture;
        public Texture2D HeldItemTexture2;

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.itemAnimation > 0
                && !drawInfo.drawPlayer.dead
                && !drawInfo.drawPlayer.noItems
                && !drawInfo.drawPlayer.CCed
                && !drawInfo.headOnlyRender;
        }

        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            Item item = player.HeldItem;

            if (player.GetModPlayer<KourindouPlayer>().FumoColaAnimationTimer > 0)
            {
                // Get the textures
                HeldItemTexture = TextureAssets.Item[ItemType<FumoCola>()].Value;
                HeldItemTexture2 = player.GetModPlayer<KourindouPlayer>().FumoColaTurnIntoPlushieID != -1 ? TextureAssets.Item[player.GetModPlayer<KourindouPlayer>().FumoColaTurnIntoPlushieID].Value : null;

                int drawX = (int)(drawInfo.Position.X - Main.screenPosition.X - player.bodyFrame.Width / 2f + player.width / 2f);
                int drawY = (int)(drawInfo.Position.Y - Main.screenPosition.Y + player.height - player.bodyFrame.Height + 4);
                Vector2 position = new Vector2(drawX, drawY) + player.bodyPosition + drawInfo.bodyVect;

                Vector2 offset = new Vector2(10, 14);
                SpriteEffects spriteEffects = SpriteEffects.None;
                float rotation = 0f;
                float Opacity = 1f;
                float Opacity2 = 0f;

                // calculate the progress of the animation
                float progress = (float)player.itemAnimation / (float)player.itemAnimationMax;

                // Drink animation
                if (progress < FumoCola.DrinkProgress)
                {
                    offset += new Vector2(4, -18);
                    rotation += -100f;
                }

                // Flip Visibility if a plushie is equipped
                if (player.GetModPlayer<KourindouPlayer>().FumoColaTurnIntoPlushieID != -1)
                {
                    Opacity = progress > FumoCola.OpeningProgress ? 1f : 1f + ((float)player.itemAnimation - (float)player.itemAnimationMax * FumoCola.OpeningProgress) / ((float)player.itemAnimationMax * (1f - FumoCola.OpeningProgress));
                    Opacity2 = progress > FumoCola.OpeningProgress ? 0f : -((float)player.itemAnimation - (float)player.itemAnimationMax * FumoCola.OpeningProgress) / ((float)player.itemAnimationMax * (1f - FumoCola.OpeningProgress));
                }

                if (player.bodyFrame.Y == 392 || player.bodyFrame.Y == 448 || player.bodyFrame.Y == 504
                    || player.bodyFrame.Y == 784 || player.bodyFrame.Y == 840 || player.bodyFrame.Y == 896)
                {
                    offset.Y -= 2;
                }
                
                if (player.direction == -1)
                {
                    spriteEffects |= SpriteEffects.FlipHorizontally;
                    offset.X *= -1;
                    rotation *= -1;
                }

                if ((int)player.gravDir == -1)
                {
                    spriteEffects |= SpriteEffects.FlipVertically;
                    offset.Y *= -1;
                    rotation *= -1;
                }

                // Can
                drawInfo.DrawDataCache.Add(new DrawData(
                    HeldItemTexture,
                    position + offset,
                    new Rectangle(0, 0, HeldItemTexture.Width, HeldItemTexture.Height),
                    drawInfo.itemColor * Opacity,
                    MathHelper.ToRadians(rotation),
                    new Vector2(HeldItemTexture.Width, HeldItemTexture.Height) * 0.5f,
                    0.8f,
                    spriteEffects,
                    0
                ));

                // Plushie
                if (player.GetModPlayer<KourindouPlayer>().FumoColaTurnIntoPlushieID != -1)
                {
                    drawInfo.DrawDataCache.Add(new DrawData(
                        HeldItemTexture2,
                        position + offset,
                        new Rectangle(0, 0, HeldItemTexture2.Width, HeldItemTexture2.Height),
                        drawInfo.itemColor * Opacity2,
                        MathHelper.ToRadians(rotation),
                        new Vector2(HeldItemTexture2.Width, HeldItemTexture2.Height) * 0.5f,
                        0.8f,
                        spriteEffects,
                        0
                    ));
                }
            }
        }
    }
}

//    public class HeldItemLayer : PlayerDrawLayer
//    {
//        Texture2D texture;
//        int itemTexture;
//        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
//        {
//            return drawInfo.drawPlayer.HeldItem.type == ItemType<KoninginDerNacht>()
//                && drawInfo.drawPlayer.itemAnimation > 0
//                && !drawInfo.drawPlayer.dead
//                && !drawInfo.drawPlayer.noItems
//                && !drawInfo.drawPlayer.CCed;
//        }
//        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);
//
//        protected override void Draw(ref PlayerDrawSet drawInfo)
//        {
//            const float StartAngle = -140f;
//            const float SwingAngle = 180f;
//            const float HeldOffset = 6f;
//
//            if (drawInfo.drawPlayer.HeldItem.type == ItemType<KoninginDerNacht>())
//            {
//                if (itemTexture != drawInfo.drawPlayer.HeldItem.type)
//                {
//                    itemTexture = drawInfo.drawPlayer.HeldItem.type;
//                    texture = TextureAssets.Item[drawInfo.drawPlayer.HeldItem.type].Value;
//                }
//            }
//
//            int drawX = (int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2f + drawInfo.drawPlayer.width / 2f);
//            int drawY = (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4);
//            Vector2 position = new Vector2(drawX, drawY) + drawInfo.drawPlayer.bodyPosition + drawInfo.bodyVect;
//
//            float rotation;
//
//            if (drawInfo.drawPlayer.direction == 1)
//            {
//                rotation = StartAngle + (SwingAngle / drawInfo.drawPlayer.itemAnimationMax * (drawInfo.drawPlayer.itemAnimationMax - drawInfo.drawPlayer.itemAnimation)) + 45f;
//                position += new Vector2(HeldOffset, 0f).RotatedBy(MathHelper.ToRadians(rotation - 45f));
//            }
//            else
//            {
//                rotation = StartAngle + 100f - (SwingAngle / drawInfo.drawPlayer.itemAnimationMax * (drawInfo.drawPlayer.itemAnimationMax - drawInfo.drawPlayer.itemAnimation)) - 45f + 180f;
//                position += new Vector2(HeldOffset, 0f).RotatedBy(MathHelper.ToRadians(rotation - 135f));
//            }
//
//            Vector2 offset = new Vector2(0, 0);
//
//            // Bodyframe Y values that indicate the player is at a position in the step cycle
//            // where the sprite shifts.
//            if (drawInfo.drawPlayer.bodyFrame.Y == 392 || drawInfo.drawPlayer.bodyFrame.Y == 448 || drawInfo.drawPlayer.bodyFrame.Y == 504
//                || drawInfo.drawPlayer.bodyFrame.Y == 784 || drawInfo.drawPlayer.bodyFrame.Y == 840 || drawInfo.drawPlayer.bodyFrame.Y == 896)
//            {
//                offset.Y += 2;
//            }
//
//            SpriteEffects spriteEffects = SpriteEffects.None;
//            if (drawInfo.drawPlayer.direction == -1)
//            {
//                spriteEffects |= SpriteEffects.FlipHorizontally;
//            }
//            if ((int)drawInfo.drawPlayer.gravDir == -1)
//            {
//                spriteEffects |= SpriteEffects.FlipVertically;
//                offset.Y *= -1;
//            }
//
//            drawInfo.DrawDataCache.Add(new DrawData(
//                texture,
//                position,
//                new Rectangle(0, 0, texture.Width, texture.Height),
//                drawInfo.itemColor,
//                MathHelper.ToRadians(rotation),
//                new Vector2(spriteEffects.HasFlag((Enum)(object)(SpriteEffects)1) ? texture.Width : 0, texture.Height) + offset,
//                1f,
//                spriteEffects,
//                0
//            ));
//        }
//    }
