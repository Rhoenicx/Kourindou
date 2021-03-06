using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Items
{
    public abstract class PlushieItem : ModItem
    {
        public short plushieDirtWater
        {
            get { return (short)((DirtAmount << 8) + WaterAmount); }
            set { DirtAmount = (byte)(value >> 8); WaterAmount = (byte)(value & 255); }
        }

        public byte DirtAmount = 0;
        public byte WaterAmount = 0;

        public float shootSpeed = 8f;
        public int projectileType = 0;

        public override void SaveData(TagCompound tag)
        {
            tag.Add("plushieDirtWater", plushieDirtWater);
        }

        public override void LoadData(TagCompound tag)
        {
            plushieDirtWater = tag.GetShort("plushieDirtWater");
        }

        // Re-center item texture
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;

            spriteBatch.Draw(
                texture,
                Item.Center - Main.screenPosition,
                texture.Bounds,
                lightColor,
                rotation,
                texture.Size() * 0.5f,
                scale,
                SpriteEffects.None,
                0);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);

            // Remove "Equipable" line if the power mode is not 2
            if (!Kourindou.KourindouConfigClient.plushiePower)
            {
                TooltipLine equipmentLine = tooltips.Find(x => x.Text.Contains("Equipable"));
                tooltips.Remove(equipmentLine);
            }
            else
            {
                TooltipLine EffectLine = new TooltipLine(Mod, "PlushieEffect", "Effect: " + AddEffectTooltip());
                EffectLine.OverrideColor = new Color(255, 255, 0);

                // Add Custom line "Can be Thrown using Right mouse button"
                TooltipLine line = new TooltipLine(Mod, "CanBeThrown", "Right Click: Throw plushie");
                line.OverrideColor = new Color(255, 255, 0);

                for (int index = 0; index < tooltips.Count; ++index)
                {
                    if (tooltips[index].Name == "SpecialPrice")
                    { 
                        tooltips.Insert(index, EffectLine);
                        break;
                    }

                    if (tooltips[index].Name == "Price")
                    {
                        tooltips.Insert(index, EffectLine);
                        break;
                    }

                    if (index == tooltips.Count - 1)
                    {
                        tooltips.Insert(tooltips.Count, EffectLine);
                        break;
                    }
                }
            }
        }

        public virtual string AddEffectTooltip()
        {
            return "None... :(";
        }

        // Execute custom equip effects
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.GetModPlayer<KourindouPlayer>().plushiePower)
            {
                player.GetModPlayer<KourindouPlayer>().EquippedPlushies.Add(Item.type);
                PlushieEquipEffects(player);
            }

            base.UpdateAccessory(player, hideVisual);
        }

        // Determine if this accessory can be equipped in the equipment slots
        // Cannot be placed in normal equipment slots, only the plushie slot
        public override bool CanEquipAccessory(Player player, int slot, bool modded)
        {
            if (!modded)
            {
                return false;   
            }

            if (slot != GetInstance<PlushieEquipSlot>().Type)
            {
                return false;
            }

            return player.GetModPlayer<KourindouPlayer>().plushiePower;
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

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.noUseGraphic = true;
                Vector2 speed = player.velocity + Vector2.Normalize(Main.MouseWorld - player.Center) * shootSpeed;

                // Singeplayer
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(
                        player.GetSource_ItemUse(Item),
                        new Vector2(player.Center.X, player.Center.Y - 16f),
                        speed,
                        projectileType,
                        Item.damage,
                        Item.knockBack,
                        player.whoAmI,
                        30f,
                        plushieDirtWater);
                }
                // Multiplayer
                else
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)KourindouMessageType.ThrowPlushie);
                    packet.Write((byte)player.whoAmI);
                    packet.WriteVector2(speed);
                    packet.Write((int)projectileType);
                    packet.Write((int)Item.damage);
                    packet.Write((float)Item.knockBack);
                    packet.Send();
                }
                return true;
            }
            else
            {
                Item.noUseGraphic = false;
            }

            player.lastVisualizedSelectedItem = Item;

            return null;
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(plushieDirtWater);
        }

        public override void NetReceive(BinaryReader reader)
        {
            plushieDirtWater = reader.ReadInt16();
        }

        // Execute custom effects when this Plushie is equipped
        public virtual void PlushieEquipEffects(Player player)
        {

        }
    }

}