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
using Terraria.Localization;

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
        public int tileType = -1;

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
            // Remove "Equipable" line if the power mode is not enabled
            if (Kourindou.KourindouConfigClient.plushiePower <= 0)
            {
                TooltipLine equipmentLine = tooltips.Find(x => x.Name == "Equipable");
                tooltips.Remove(equipmentLine);
            }
            
            TooltipLine plushieEffectLine = tooltips.Find(x => x.Text.Contains("{PlushieEquipEffectLine}"));
            if (plushieEffectLine != null)
            {
                plushieEffectLine.Text =
                    Kourindou.KourindouConfigClient.plushiePower >= 1 ?
                        Language.GetTextValue("Mods.Kourindou.PlushieEffectLines.WhenEquipped")
                            + Language.GetTextValue("Mods.Kourindou.Items." + Name + ".PlushieEquipEffect")
                        : "";
            }
            base.ModifyTooltips(tooltips);
        }

        // Execute custom equip effects
        public sealed override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.GetModPlayer<KourindouPlayer>().plushiePower >= 1)
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

            if (slot != GetInstance<PlushieEquipSlot>().Type
                && slot != GetInstance<PlushieEquipSlot2>().Type
                && slot != GetInstance<PlushieEquipSlot3>().Type)
            {
                return false;
            }

            return player.GetModPlayer<KourindouPlayer>().plushiePower >= 1;
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

        public override bool CanUseItem(Player player)
        {
            // Save the Tile that should be created in a new field
            tileType = Item.createTile;

            // Check if the item is used with the right mouse button
            if (player.altFunctionUse == 2)
            {
                // Prevent tile creation when throwing the plushie
                // with the right mouse button.
                Item.createTile = -1;

                // A projectile will be created, send right click
                if (Main.netMode == NetmodeID.MultiplayerClient
                    && player.whoAmI == Main.myPlayer)
                {
                    // Send right click sync packet
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)KourindouMessageType.RightClick);
                    packet.Write(player.whoAmI);
                    packet.Send();

                    // Send player controls packet
                    NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI);
                }
            }

            return base.CanUseItem(player);
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2 && player.whoAmI == Main.myPlayer)
            {
                Item.noUseGraphic = true;
                Vector2 speed = player.velocity + Vector2.Normalize(Main.MouseWorld - player.Center) * shootSpeed;

                // Singeplayer
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(
                        player.GetSource_ItemUse(Item),
                        player.RotatedRelativePoint(player.MountedCenter),
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

            Item.noUseGraphic = false;

            Item.createTile = tileType;

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

        public virtual bool PlushieCanBeHitByNPC(Player player, NPC npc, ref int cooldownSlot, int amountEquipped)
        {
            return true;
        }

        public virtual bool PlushieCanHitNPC(Player player, NPC target, int amountEquipped)
        {
            return true;
        }

        public virtual bool PlushieCanHitNPCWithProj(Player player, Projectile proj, NPC target, int amountEquipped)
        {
            return true;
        }

        public virtual bool PlushieCanHitPvp(Player player, Item item, Player target, int amountEquipped)
        {
            return true;
        }

        public virtual bool PlushieCanHitPvpWithProj(Player player, Projectile proj, Player target, int amountEquipped)
        {
            return true;
        }

        public virtual void PlushieOnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit, int damageDone, int amountEquipped)
        { 
        
        }
       
        public virtual void PlushieOnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone, int amountEquipped)
        {

        }

        public virtual void PlushieOnHitByNPC(Player player, NPC npc, Player.HurtInfo hurtInfo, int amountEquipped)
        { 
        
        }

        public virtual void PlushieOnHitByProjectile(Player player, Projectile proj, Player.HurtInfo hurtInfo, int amountEquipped)
        {

        }

        public virtual void PlushieModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers, int amountEquipped)
        { 
        
        }

        public virtual void PlushieModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers, int amountEquipped)
        {

        }

        public virtual void PlushieModifyHurt(Player player, ref Player.HurtModifiers modifiers, int amountEquipped)
        {

        }

        public virtual void PlushieOnHurt(Player player, Player.HurtInfo info, int amountEquipped)
        { 

        }

        public virtual bool PlushiePreKill(Player player, double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, int amountEquipped)
        {
            return true;
        }

        public virtual void PlushieModifyWeaponCrit(Player player, Item item, ref float crit, int amountEquipped)
        { 
        
        }

        public virtual void PlushieModifyItemScale(Player player, Item item, ref float scale, int amountEquipped)
        { 
        
        }

        public virtual void PlushieKill(Player player, double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource, int amountEquipped)
        {
        
        }

        public virtual void PlushieUpdateBadLifeRegen(Player player, int amountEquipped)
        { 
        
        }

        public virtual void PlushieModifyWeaponDamage(Player player, Item item, ref StatModifier damage, int amountEquipped)
        { 
        
        }
    }
}