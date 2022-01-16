using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Items.Plushies
{
    public abstract class PlushieItem : ModItem
    {
        public float shootSpeed = 8f;
        public int projectileType = 0;

        // Re-center item texture
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = Main.itemTexture[item.type];

            spriteBatch.Draw(
                texture,
                item.Center - Main.screenPosition,
                texture.Bounds,
                lightColor,
                rotation,
                texture.Size() * 0.5f,
                scale,
                SpriteEffects.None,
                0);

            return false;
        }

        // Make item right clickable in inventory
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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // Remove "Equipable" line if the power mode is not 2
            if (Kourindou.KourindouConfigClient.plushiePower != 2)
            {
                TooltipLine equipmentLine = tooltips.Find(x => x.text.Contains("Equipable"));
                tooltips.Remove(equipmentLine);
            }

            // Add Custom line "Can be Thrown using Right mouse button"
            tooltips.Add(new TooltipLine(mod, "CanBeThrown", "Right Click: Throw plushie"));
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

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Vector2 speed = player.velocity + Vector2.Normalize(Main.MouseWorld - player.Center) * shootSpeed;

                // Singeplayer
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {                    
                    Projectile.NewProjectile(
                        new Vector2(player.Center.X, player.Center.Y - 16f),
                        speed,
                        projectileType,
                        item.damage,
                        item.knockBack,
                        player.whoAmI,
                        30f,
                        0f);
                }
                // Multiplayer
                else
                {
                    ModPacket packet = mod.GetPacket();
                    packet.Write((byte) KourindouMessageType.ThrowPlushie);
                    packet.Write((byte) player.whoAmI);
                    packet.WriteVector2(speed);
                    packet.Write((int) projectileType);
                    packet.Write((int) item.damage);
                    packet.Write((float) item.knockBack);
                    packet.Send();
                }

                return true;
            }
            return false;
        }

        // Execute custom effects when this Plushie is equipped
        public virtual void PlushieEquipEffects(Player player)
        {

        }
    }
}