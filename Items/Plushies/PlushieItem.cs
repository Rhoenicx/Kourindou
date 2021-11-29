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

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = Main.itemTexture[item.type];
            Texture2D textureOld = mod.GetTexture((texture.ToString() + "_Old").Replace("Kourindou/", ""));

            spriteBatch.Draw(
                Kourindou.KourindouConfigClient.UseOldTextures ? textureOld : texture,
                position,
                Kourindou.KourindouConfigClient.UseOldTextures ? textureOld.Bounds : texture.Bounds,
                Color.White,
                0f,
                new Vector2(0,0),
                scale,
                SpriteEffects.None,
                0);

            return false;
        }
/*
        public override bool PreDrawInWorld (SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = Main.itemTexture[item.type];

            spriteBatch.Draw(
                texture,
                item.position,
                new Rectangle(texture.Width / 2 * (Kourindou.KourindouConfigClient.UseOldTextures ? 1 : 0), 0, texture.Width / 2, texture.Height),
                Color.White,
                rotation,
                new Vector2(0,0),
                scale,
                SpriteEffects.None,
                0);

            return false;
        }
*/
        // Execute custom effects when this Plushie is equipped
        public virtual void PlushieEquipEffects(Player player)
        {

        }
    }
}