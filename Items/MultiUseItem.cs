using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace Kourindou.Items
{
    public abstract class MultiUseItem : ModItem
    {
        // Attack IDs
        public int NormalAttackID;
        public int SkillAttackID;
        public int UltimateAttackID;

        // Attack Counters
        public int NormalAttackCounter;
        public int SkillAttackCounter;
        public int UltimateAttackCounter;

        // Cooldown timer
        public int AttackCooldownTime;

        // Current Attack
        public int _AttackID;
        public int _AttackCounter;

        // Hovering
        private bool _hoveringOnTile;
        private bool _hoveringOnSign;

        // Normal Attack
        public virtual int GetNormalID(Player player)
        {
            return NormalAttackID;
        }

        public virtual int GetNormalCounter(Player player)
        {
            return NormalAttackCounter;
        }

        public virtual bool HasNormal(Player player)
        {
            return true;
        }

        // Skill
        public virtual int GetSkillID(Player player)
        {
            return SkillAttackID;
        }

        public virtual int GetSkillCounter(Player player)
        {
            return SkillAttackCounter;
        }

        public virtual bool HasSkill(Player player)
        {
            return false;
        }

        // Ultimate
        public virtual int GetUltimateID(Player player)
        {
            return UltimateAttackID;
        }

        public virtual int GetUltimateCounter(Player player)
        {
            return UltimateAttackCounter;
        }

        public virtual bool HasUltimate(Player player)
        {
            return false;
        }

        protected virtual int GetCooldownTime()
        {
            return AttackCooldownTime;
        }

        public virtual int GetItemID()
        {
            return Item.type;
        }

        protected virtual bool ShowCooldownBar()
        {
            return true;
        }

        protected virtual bool ShowCooldownUnderItem()
        {
            return true;
        }

        public int ModifyCooldownTime(int AttackID)
        {
            KourindouPlayer player = Main.LocalPlayer.GetModPlayer<KourindouPlayer>();
            return (int)Math.Round(GetCooldownTime() * player.CooldownTimeMultiplier) + player.CooldownTimeAdditive;
        }

        public void SetCooldown(int AttackID)
        {
            KourindouPlayer player = Main.LocalPlayer.GetModPlayer<KourindouPlayer>();
            player.SetCooldown(GetItemID(), AttackID, ModifyCooldownTime(AttackID));
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
            Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Main.gameMenu || !ShowCooldownUnderItem())
            {
                return true;
            }

            KourindouPlayer player = Main.LocalPlayer.GetModPlayer<KourindouPlayer>();

            // SKILL cooldown bar 
            if (player.OnCooldown(Item.type, SkillAttackID))
            {
                if (Main.mouseItem != Item && Main.LocalPlayer.HeldItem != Item)
                {
                    Vector2 slotCenter = position + new Vector2(frame.Width / 2f, frame.Height / 2f) * scale;
                    float quotient = player.Cooldowns[Item.type][SkillAttackID].CurrentTime / (float)player.Cooldowns[Item.type][SkillAttackID].MaximumTime;

                    spriteBatch.Draw(
                        TextureAssets.MagicPixel.Value,
                        slotCenter + new Vector2(-14f, 14f),
                        new Rectangle(0, 0, (int)(28 * quotient), 2),
                        Color.Yellow,
                        0f,
                        Vector2.Zero,
                        1f,
                        SpriteEffects.None,
                        0);
                }
            }

            // ULTIMATE cooldown bar
            if (player.OnCooldown(Item.type, UltimateAttackID))
            {
                if (Main.mouseItem != Item && Main.LocalPlayer.HeldItem != Item)
                {
                    Vector2 slotCenter = position + new Vector2(frame.Width / 2f, frame.Height / 2f) * scale;
                    float quotient = player.Cooldowns[Item.type][UltimateAttackID].CurrentTime / (float)player.Cooldowns[Item.type][UltimateAttackID].MaximumTime;

                    spriteBatch.Draw(
                        TextureAssets.MagicPixel.Value,
                        slotCenter + new Vector2(-14f, 12f),
                        new Rectangle(0, 0, (int)(28 * quotient), 2),
                        Color.Red,
                        0f,
                        Vector2.Zero,
                        1f,
                        SpriteEffects.None,
                        0);
                }
            }

            // Draw the item sprite.
            return true;
        }

        public override bool AltFunctionUse(Player player)
        {
            bool conflictsWithDefaultRightClick = false;
            List<string> skillkeys = Kourindou.SkillKey.GetAssignedKeys();
            List<string> ultimatekeys = Kourindou.UltimateKey.GetAssignedKeys();

            if ((skillkeys.Count > 0 && skillkeys[0] == "Mouse2" && (player.GetModPlayer<KourindouPlayer>().SkillKeyPressed || player.GetModPlayer<KourindouPlayer>().SkillKeyHeld))
                || (ultimatekeys.Count > 0 && ultimatekeys[0] == "Mouse2" && (player.GetModPlayer<KourindouPlayer>().UltimateKeyPressed || player.GetModPlayer<KourindouPlayer>().UltimateKeyHeld)))
            {
                if (_hoveringOnTile || _hoveringOnSign || Main.instance.currentNPCShowingChatBubble != -1)
                {
                    conflictsWithDefaultRightClick = true;
                }
            }

            return ((HasSkill(player) || HasUltimate(player)) && !conflictsWithDefaultRightClick && (player.GetModPlayer<KourindouPlayer>().SkillKeyPressed || player.GetModPlayer<KourindouPlayer>().UltimateKeyPressed || player.GetModPlayer<KourindouPlayer>().SkillKeyHeld || player.GetModPlayer<KourindouPlayer>().UltimateKeyHeld));
        }

        public override void HoldItem(Player player)
        {
            // The variables in Player / Main are reset before AltFunctionUse is executed, and set to their correct
            // value only afterwards. Thus, we save the previous tick's value and use that for our checks.
            _hoveringOnTile = player.cursorItemIconEnabled;
            _hoveringOnSign = Main.signBubble;
        }


        public virtual void SetItemStats(Player player, int AttackID, int AttackCounter)
        { 
            
        }
    }
}
