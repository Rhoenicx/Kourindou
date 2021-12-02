using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Kourindou.Items.Plushies
{
    public abstract class PlushieItem : ModItem
    {
        public override bool CanRightClick() 
        { 
            return (!Kourindou.OverrideRightClick() && Main.LocalPlayer.GetModPlayer<KourindouPlayer>().plushiePower == 2);  
        }

        public override void RightClick(Player player) 
        {
            if (!CanRightClick())
            {
                return;
            }
            
            player.GetModPlayer<KourindouPlayer>().EquipPlushie(false, item);
        }

        // Decides what happens when the player uses this item
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                
            }

            return true;
        }

        // Execute custom equip effects
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.GetModPlayer<KourindouPlayer>().plushiePower == 2)
            {
                PlushieEquipEffects(player);
            }

            base.UpdateAccessory(player, hideVisual);
        }

        // Determine if this accessory can be equipped in the equipment slots
        // Cannot be placed in normal equipment slots, only the plushie slot
        public override bool CanEquipAccessory(Player player, int slot)
        {
            if (slot > 0)
            {
                return false;
            }

            if (player.GetModPlayer<KourindouPlayer>().plushiePower != 2)
            {
                return false;
            }

            return true;
        }

        // Prevent the player from putting this accessory in the tinkerer slot
        public override bool? PrefixChance(int pre, UnifiedRandom rand)
        {
            pre = -3;
            return false;
        }

        // Execute custom effects when this Plushie is equipped
        public virtual void PlushieEquipEffects(Player player)
        {

        }
    }
}