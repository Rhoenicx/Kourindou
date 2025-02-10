using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.Default;
using Terraria.GameContent;
using static Terraria.ModLoader.ModContent;
using Kourindou.Buffs;
using Kourindou.Items;
using Kourindou.Items.Plushies;
using Kourindou.Items.Consumables;
using Kourindou.Items.Catalysts;


namespace Kourindou
{
    public class KourindouPlayer : ModPlayer
    {
        //--------------------------------------------------------------------------------
        // Determines the power mode of all the plushies
        public byte plushiePower;

        // Item ID of the plushie slot item
        public Dictionary<PlushieItem, int> EquippedPlushies = new();

        // Reimu plushie maximum homing distance
        public float ReimuPlushieMaxDistance = 500f;

        // Ran Plushie kill and stack counter
        public byte RanPlushie_EnemieKillCounter;
        public byte RanPlushie_Stacks;

        // Tenshi Plushie Effect
        public bool TenshiPlushie_Revenge;
        public int TenshiPlushie_Damage;

        // Yukari Plushie effect
        public bool SkillKeyPressed;

        // Items
        public int FumoColaTurnIntoPlushieID = -1;
        public int FumoColaAnimationTimer;
        public int FumoColaBuffStacks;

        // Half Phantom pet active
        public bool HalfPhantomPet;



        // Buffs
        public bool DebuffMedicineMelancholy;
        public int DebuffMedicineMelancholyStacks;

        // Catalyst
        public Dictionary<int, int[]> CatalystCooldown = new();

        // Weapons
        // public Dictionary<int, int> CrescentMoonStaffFlames = new Dictionary<int, int>();


        //--------------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------- Save/Load data --------------------------------------------------//
        //--------------------------------------------------------------------------------------------------------------------//

        public override void SaveData(TagCompound tag)
        {
            tag.Add("plushiePowerMode", plushiePower);
            tag.Add("fumoColaBuffStacks", FumoColaBuffStacks);
            tag.Add("ranPlushieStacks", RanPlushie_Stacks);
        }

        public override void LoadData(TagCompound tag)
        {
            plushiePower = tag.GetByte("plushiePowerMode");
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

        public override void OnEnterWorld()
        {
            Kourindou.KourindouConfigClient.plushiePower = Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushiePower;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Send the configured plushie power setting
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)KourindouMessageType.ClientConfig);
                packet.Write((byte)Main.myPlayer);
                packet.Write((byte)Kourindou.KourindouConfigClient.plushiePower);
                packet.Send(-1, Main.myPlayer);

                // Send the stack amount of the Ran Plushie
                packet = Mod.GetPacket();
                packet.Write((byte)KourindouMessageType.RanPlushieStacks);
                packet.Write((byte)Main.myPlayer);
                packet.Write((byte)Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().RanPlushie_Stacks);
                packet.Send(-1, Main.myPlayer);
            }
        }

        public override void PlayerConnect()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Send the configured plushie power setting
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)KourindouMessageType.ClientConfig);
                packet.Write((byte)Main.myPlayer);
                packet.Write((byte)Kourindou.KourindouConfigClient.plushiePower);
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
            if (EquippedPlushies.Any(kvp => kvp.Key.Type == ItemType<YukariYakumo_Plushie_Item>()))
            {
                SkillKeyPressed = Kourindou.SkillKey.JustPressed;
            }

            if (Player.whoAmI == Main.myPlayer
                && Player.controlUseItem
                && Player.HeldItem.ModItem is CatalystItem catalyst
                && OnCooldown(catalyst.CatalystID))
            {
                Player.controlUseItem = false;
            }
        }

        public override void PreUpdate()
        {
            if (FumoColaAnimationTimer > 0)
            {
                FumoColaAnimationTimer--;
            }

            foreach (int[] array in CatalystCooldown.Values)
            {
                // Reduce times
                if (array[0] > 0)
                {
                    array[0]--;
                }
                else if (array[1] > 0)
                {
                    array[1]--;
                }
            }
        }

        public override void ResetEffects()
        {
            // Reset the plushie item slots
            EquippedPlushies.Clear();

            // Murasa Effect reset breathMax
            Player.breathMax = 200;

            // Reset buff timer visibility
            Main.buffNoTimeDisplay[146] = false;

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

        public override void UpdateEquips()
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                plushie.Key.PlushieUpdateEquips(Player, plushie.Value);
            }
        }

        public override void PostUpdateEquips()
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                plushie.Key.PlushiePostUpdateEquips(Player, plushie.Value);
            }
        }

        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
        {
            bool hit = base.CanBeHitByNPC(npc, ref cooldownSlot);

            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                if (!plushie.Key.PlushieCanBeHitByNPC(Player, npc, ref cooldownSlot, plushie.Value))
                {
                    hit = false;
                }
            }

            return hit;
        }

        public override bool CanHitNPC(NPC target)
        {
            bool hit = base.CanHitNPC(target);

            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                if (!plushie.Key.PlushieCanHitNPC(Player, target, plushie.Value))
                {
                    hit = false;
                }
            }

            return hit;
        }

        public override bool? CanHitNPCWithProj(Projectile proj, NPC target)
        {
            bool? hit = base.CanHitNPCWithProj(proj, target);

            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                if (!plushie.Key.PlushieCanHitNPCWithProj(Player, proj, target, plushie.Value))
                {
                    hit = false;
                }
            }

            return hit;
        }

        public override bool CanHitPvp(Item item, Player target)
        {
            bool hit = base.CanHitPvp(item, target);

            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                if (!plushie.Key.PlushieCanHitPvp(Player, item, target, plushie.Value))
                {
                    hit = false;
                }
            }

            return hit;
        }

        public override bool CanHitPvpWithProj(Projectile proj, Player target)
        {
            bool hit = base.CanHitPvpWithProj(proj, target);

            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                if (!plushie.Key.PlushieCanHitPvpWithProj(Player, proj, target, plushie.Value))
                {
                    hit = false;
                }
            }

            return hit;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                plushie.Key.PlushieOnHitNPCWithItem(Player, item, target, hit, damageDone, plushie.Value);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                plushie.Key.PlushieOnHitNPCWithProj(Player, proj, target, hit, damageDone, plushie.Value);
            }
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                plushie.Key.PlushieOnHitByNPC(Player, npc, hurtInfo, plushie.Value);
            }
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                plushie.Key.PlushieOnHitByProjectile(Player, proj, hurtInfo, plushie.Value);
            }
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                plushie.Key.PlushieModifyHitNPCWithItem(Player, item, target, ref modifiers, plushie.Value);
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                plushie.Key.PlushieModifyHitNPCWithProj(Player, proj, target, ref modifiers, plushie.Value);
            }
        }

        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (DebuffMedicineMelancholy)
            {
                modifiers.FinalDamage *= 1f + (0.04f * (DebuffMedicineMelancholyStacks + 1));
            }
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (DebuffMedicineMelancholy)
            {
                modifiers.FinalDamage *= 1f + (0.04f * (DebuffMedicineMelancholyStacks + 1));
            }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                plushie.Key.PlushieModifyHurt(Player, ref modifiers, plushie.Value);
            }
        }

        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                plushie.Key.PlushieModifyWeaponDamage(Player, item, ref damage, plushie.Value);
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                plushie.Key.PlushieOnHurt(Player, info, plushie.Value);
            }
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                return plushie.Key.PlushiePreKill(Player, damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource, plushie.Value);
            }

            return true;
        }

        public override void ModifyWeaponCrit(Item item, ref float crit)
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                plushie.Key.PlushieModifyWeaponCrit(Player, item, ref crit, plushie.Value);
            }
        }

        public override void ModifyItemScale(Item item, ref float scale)
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                plushie.Key.PlushieModifyItemScale(Player, item, ref scale, plushie.Value);
            }
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            foreach (KeyValuePair<PlushieItem, int> plushie in EquippedPlushies)
            {
                plushie.Key.PlushieKill(Player, damage, hitDirection, pvp, damageSource, plushie.Value);
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

        public bool OnCooldown(int ID)
        {
            if (CatalystCooldown.ContainsKey(ID))
            {
                return CatalystCooldown[ID][1] > 0;
            }

            return false;
        }

        public void SetCooldown(int ID, int AnimationTime, int Time)
        {
            if (!CatalystCooldown.ContainsKey(ID))
            {
                CatalystCooldown.Add(ID, new int[] { AnimationTime, Math.Abs(Time), Math.Abs(Time), Math.Sign(Time)});
            }
            else
            {
                CatalystCooldown[ID][0] = AnimationTime;
                CatalystCooldown[ID][1] = Math.Abs(Time);
                CatalystCooldown[ID][2] = Math.Abs(Time);
                CatalystCooldown[ID][3] = Math.Sign(Time);
            }
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

                Player.compositeFrontArm = new Player.CompositeArmData(true, progress <= FumoCola.OpeningProgress ? Player.CompositeArmStretchAmount.ThreeQuarters : Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(Player.direction * -FrontArmAngle))
                {
                    enabled = progress > FumoCola.DrinkProgress
                };

                Player.compositeBackArm = new Player.CompositeArmData(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(Player.direction * -BackArmAngle))
                {
                    enabled = true
                };
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
            if (Player == null
                || !Player.TryGetModPlayer<KourindouPlayer>(out _))
            {
                return false;
            }

            return Player.GetModPlayer<KourindouPlayer>().plushiePower >= 1;
        }

        public override bool IsVisibleWhenNotEnabled()
        {
            return FunctionalItem.type != ItemID.None;
        }

        public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
        {
            // cannot place an item in the slot if the power mode is not active
            if (Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushiePower <= 0)
            {
                return false;
            }

            if (Main.player[Main.myPlayer].wingsLogic > 0 && checkItem.type == ItemType<AyaShameimaru_Plushie_Item>())
            {
                return false;
            }

            if (checkItem.ModItem is PlushieItem)
            {
                return true;
            }

            return false;
        }

        public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo)
        {
            if (Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushiePower <= 0)
            {
                return false;
            }

            if (Main.player[Main.myPlayer].wingsLogic > 0 && item.type == ItemType<AyaShameimaru_Plushie_Item>())
            {
                return false;
            }

            if (item.ModItem is PlushieItem)
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

    public class PlushieEquipSlot2 : ModAccessorySlot
    {
        public override string Name => "Plushie Slot 2";
        public override bool DrawDyeSlot => false;
        public override bool DrawVanitySlot => false;

        public override bool IsEnabled()
        {
            if (Player == null
                || !Player.TryGetModPlayer<KourindouPlayer>(out _))
            {
                return false;
            }

            return Player.GetModPlayer<KourindouPlayer>().plushiePower >= 2;
        }

        public override bool IsVisibleWhenNotEnabled()
        {
            return FunctionalItem.type != ItemID.None;
        }

        public override Vector2? CustomLocation
        { 
            get
            {
                int posX = 0;
                int posY = 0;
                int VanillaSlots = 5 + (Player.CanDemonHeartAccessoryBeShown() ? 1 : 0) + (Player.CanMasterModeAccessoryBeShown() ? 1 : 0);
                SetDrawLocation(GetInstance<PlushieEquipSlot>().Type + VanillaSlots, 0, ref posX, ref posY);
                return new Vector2(posX - 47, posY);    
            }
        }

        internal bool SetDrawLocation(int trueSlot, int skip, ref int xLoc, ref int yLoc)
        {
            int accessorySlotPerColumn = GetAccessorySlotPerColumn();
            int num1 = trueSlot / accessorySlotPerColumn;
            int num2 = trueSlot % accessorySlotPerColumn;

            FieldInfo fi = typeof(ModAccessorySlotPlayer).GetField("scrollSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            bool scrollSlots = (bool)fi.GetValue(Player.GetModPlayer<ModAccessorySlotPlayer>());

            FieldInfo fi2 = typeof(ModAccessorySlotPlayer).GetField("scrollbarSlotPosition", BindingFlags.NonPublic | BindingFlags.Instance);
            int scrollbarSlotPosition = (int)fi2.GetValue(Player.GetModPlayer<ModAccessorySlotPlayer>());

            if (scrollSlots)
            {
                int num3 = num2 + num1 * accessorySlotPerColumn - scrollbarSlotPosition - skip;
                yLoc = (int)((double)AccessorySlotLoader.DrawVerticalAlignment + (double)((num3 + 3) * 56) * (double)Main.inventoryScale) + 4;
                int num4 = (int)((double)AccessorySlotLoader.DrawVerticalAlignment + 168.0 * (double)Main.inventoryScale) + 4;
                int num5 = (int)((double)AccessorySlotLoader.DrawVerticalAlignment + (double)((accessorySlotPerColumn - 1 + 3) * 56) * (double)Main.inventoryScale) + 4;
                if (yLoc > num5 || yLoc < num4)
                    return false;
                xLoc = Main.screenWidth - 64 - 28;
            }
            else
            {
                int num3 = num2;
                int num4 = trueSlot;
                int num5 = num1;
                if (skip > 0)
                {
                    int num6 = num4 - skip;
                    num3 = num6 % accessorySlotPerColumn;
                    num5 = num6 / accessorySlotPerColumn;
                }
                yLoc = (int)((double)AccessorySlotLoader.DrawVerticalAlignment + (double)((num3 + 3) * 56) * (double)Main.inventoryScale) + 4;
                xLoc = num5 <= 0 ? Main.screenWidth - 64 - 28 - 141 * num5 : Main.screenWidth - 64 - 28 - 141 * num5 - 50;
            }
            return true;
        }

        internal int GetAccessorySlotPerColumn()
        {
            float num = (float)((double)AccessorySlotLoader.DrawVerticalAlignment + 112.0 * (double)Main.inventoryScale + 4.0);
            return (int)(((double)Main.screenHeight - (double)num) / (56.0 * (double)Main.inventoryScale) - 1.79999995231628);
        }

        public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
        {
            // cannot place an item in the slot if the power mode is not active
            if (Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushiePower <= 0)
            {
                return false;
            }

            if (Main.player[Main.myPlayer].wingsLogic > 0 && checkItem.type == ItemType<AyaShameimaru_Plushie_Item>())
            {
                return false;
            }

            if (checkItem.ModItem is PlushieItem)
            {
                return true;
            }

            return false;
        }

        public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo)
        {
            if (Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushiePower <= 0)
            {
                return false;
            }

            if (Main.player[Main.myPlayer].wingsLogic > 0 && item.type == ItemType<AyaShameimaru_Plushie_Item>())
            {
                return false;
            }

            if (item.ModItem is PlushieItem)
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
                    Main.hoverItemName = "Plushie Slot 2";
                    break;
            }
        }
    }

    public class PlushieEquipSlot3 : ModAccessorySlot
    {
        public override string Name => "Plushie Slot 3";
        public override bool DrawDyeSlot => false;
        public override bool DrawVanitySlot => false;

        public override bool IsEnabled()
        {
            if (Player == null
                || !Player.TryGetModPlayer<KourindouPlayer>(out _))
            {
                return false;
            }

            return Player.GetModPlayer<KourindouPlayer>().plushiePower >= 3;
        }

        public override bool IsVisibleWhenNotEnabled()
        {
            return FunctionalItem.type != ItemID.None;
        }

        public override Vector2? CustomLocation
        {
            get
            {
                int posX = 0;
                int posY = 0;
                int VanillaSlots = 5 + (Player.CanDemonHeartAccessoryBeShown() ? 1 : 0) + (Player.CanMasterModeAccessoryBeShown() ? 1 : 0);
                SetDrawLocation(GetInstance<PlushieEquipSlot>().Type + VanillaSlots, 0, ref posX, ref posY);
                return new Vector2(posX - 94, posY);
            }
        }

        internal bool SetDrawLocation(int trueSlot, int skip, ref int xLoc, ref int yLoc)
        {
            int accessorySlotPerColumn = GetAccessorySlotPerColumn();
            int num1 = trueSlot / accessorySlotPerColumn;
            int num2 = trueSlot % accessorySlotPerColumn;

            FieldInfo fi = typeof(ModAccessorySlotPlayer).GetField("scrollSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            bool scrollSlots = (bool)fi.GetValue(Player.GetModPlayer<ModAccessorySlotPlayer>());

            FieldInfo fi2 = typeof(ModAccessorySlotPlayer).GetField("scrollbarSlotPosition", BindingFlags.NonPublic | BindingFlags.Instance);
            int scrollbarSlotPosition = (int)fi2.GetValue(Player.GetModPlayer<ModAccessorySlotPlayer>());

            if (scrollSlots)
            {
                int num3 = num2 + num1 * accessorySlotPerColumn - scrollbarSlotPosition - skip;
                yLoc = (int)((double)AccessorySlotLoader.DrawVerticalAlignment + (double)((num3 + 3) * 56) * (double)Main.inventoryScale) + 4;
                int num4 = (int)((double)AccessorySlotLoader.DrawVerticalAlignment + 168.0 * (double)Main.inventoryScale) + 4;
                int num5 = (int)((double)AccessorySlotLoader.DrawVerticalAlignment + (double)((accessorySlotPerColumn - 1 + 3) * 56) * (double)Main.inventoryScale) + 4;
                if (yLoc > num5 || yLoc < num4)
                    return false;
                xLoc = Main.screenWidth - 64 - 28;
            }
            else
            {
                int num3 = num2;
                int num4 = trueSlot;
                int num5 = num1;
                if (skip > 0)
                {
                    int num6 = num4 - skip;
                    num3 = num6 % accessorySlotPerColumn;
                    num5 = num6 / accessorySlotPerColumn;
                }
                yLoc = (int)((double)AccessorySlotLoader.DrawVerticalAlignment + (double)((num3 + 3) * 56) * (double)Main.inventoryScale) + 4;
                xLoc = num5 <= 0 ? Main.screenWidth - 64 - 28 - 141 * num5 : Main.screenWidth - 64 - 28 - 141 * num5 - 50;
            }
            return true;
        }

        internal int GetAccessorySlotPerColumn()
        {
            float num = (float)((double)AccessorySlotLoader.DrawVerticalAlignment + 112.0 * (double)Main.inventoryScale + 4.0);
            return (int)(((double)Main.screenHeight - (double)num) / (56.0 * (double)Main.inventoryScale) - 1.79999995231628);
        }

        public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
        {
            // cannot place an item in the slot if the power mode is not active
            if (Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushiePower <= 0)
            {
                return false;
            }

            if (Main.player[Main.myPlayer].wingsLogic > 0 && checkItem.type == ItemType<AyaShameimaru_Plushie_Item>())
            {
                return false;
            }

            if (checkItem.ModItem is PlushieItem)
            {
                return true;
            }

            return false;
        }

        public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo)
        {
            if (Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushiePower <= 0)
            {
                return false;
            }

            if (Main.player[Main.myPlayer].wingsLogic > 0 && item.type == ItemType<AyaShameimaru_Plushie_Item>())
            {
                return false;
            }

            if (item.ModItem is PlushieItem)
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
                    Main.hoverItemName = "Plushie Slot 3";
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

            if (player.GetModPlayer<KourindouPlayer>().FumoColaAnimationTimer > 0)
            {
                // Get the textures
                HeldItemTexture = TextureAssets.Item[ItemType<FumoCola>()].Value;
                HeldItemTexture2 = player.GetModPlayer<KourindouPlayer>().FumoColaTurnIntoPlushieID != -1 ? TextureAssets.Item[player.GetModPlayer<KourindouPlayer>().FumoColaTurnIntoPlushieID].Value : null;

                int drawX = (int)(drawInfo.Position.X - Main.screenPosition.X - player.bodyFrame.Width / 2f + player.width / 2f);
                int drawY = (int)(drawInfo.Position.Y - Main.screenPosition.Y + player.height - player.bodyFrame.Height + 4);
                Vector2 position = new Vector2(drawX, drawY) + player.bodyPosition + drawInfo.bodyVect;

                Vector2 offset = new(10f, 14f);
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
