using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Items
{
    public abstract class PlushieItem : ModItem
    {
        public short PlushieDirtWater
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
            tag.Add("PlushieDirtWater", PlushieDirtWater);
        }

        public override void LoadData(TagCompound tag)
        {
            PlushieDirtWater = tag.GetShort("PlushieDirtWater");
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
                TooltipLine EffectLine = new(Mod, "PlushieEffect", "Effect: " + AddEffectTooltip());
                EffectLine.OverrideColor = new Color(255, 255, 0);

                // Add Custom line "Can be Thrown using Right mouse button"
                TooltipLine line = new(Mod, "CanBeThrown", "Right Click: Throw plushie");
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
                if (!player.GetModPlayer<KourindouPlayer>().EquippedPlushies.Any(kvp => kvp.Key.Type == Item.type) && Item.ModItem is PlushieItem plushie)
                {
                    player.GetModPlayer<KourindouPlayer>().EquippedPlushies.Add(plushie, 1);
                }
                else
                {
                    foreach (KeyValuePair<PlushieItem, int> kvp in player.GetModPlayer<KourindouPlayer>().EquippedPlushies)
                    {
                        if (kvp.Key.Type == Item.type)
                        {
                            player.GetModPlayer<KourindouPlayer>().EquippedPlushies[kvp.Key]++;
                        }
                    }
                }
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
                        PlushieDirtWater);
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
            writer.Write(PlushieDirtWater);
        }

        public override void NetReceive(BinaryReader reader)
        {
            PlushieDirtWater = reader.ReadInt16();
        }

        // Execute custom effects when this Plushie is equipped
        public virtual void PlushieUpdateEquips(Player player, int amountEquipped)
        {

        }

        public virtual void PlushiePostUpdateEquips(Player player, int amountEquipped)
        {

        }

        public virtual bool PlushieCanbeHitByNPC(Player myPlayer, NPC npc, ref int cooldownSlot, int amountEquipped)
        {
            return true;
        }

        public virtual bool? PlushieCanHit(Player myPlayer, Item item, Projectile proj, NPC npc, Player player, int amountEquipped)
        {
            return true;
        }

        public virtual void PlushieOnHit(Player myPlayer, Item item, Projectile proj, NPC npc, Player player, int damage, float knockback, bool crit, int amountEquipped)
        { 
        
        }

        public virtual void PlushieOnHitBy(Player myPlayer, Projectile proj, NPC npc, int damage, bool crit, int amountEquipped)
        { 
        
        }

        public virtual void PlushieModifyHit(Player myPlayer, Item item, Projectile proj, NPC npc, Player player, ref int damage, ref float knockback, ref bool crit, int amountEquipped)
        { 
        
        }
        public virtual void PlushieHurt(Player myPlayer, bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter, int amountEquipped)
        { 

        }

        public virtual bool PlushiePreKill(Player myPlayer, double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, int amountEquipped)
        {
            return true;
        }

        public virtual void PlushieModifyWeaponCrit(Player myPlayer, Item item, ref float crit, int amountEquipped)
        { 
        
        }

        public virtual void PlushieModifyItemScale(Player myPlayer, Item item, ref float scale, int amountEquipped)
        { 
        
        }

        public virtual void PlushieKill(Player myPlayer, double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource, int amountEquipped)
        { 
        
        }

        public virtual void PlushieUpdateBadLifeRegen(Player myPlayer, int amountEquipped)
        { 
        
        }
    }
}