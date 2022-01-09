using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Graphics.Effects;
using Terraria.UI;
using TerraUI.Objects;
using Kourindou.Items;
using Kourindou.Items.Plushies;
using Kourindou.Projectiles.Plushies.PlushieEffects;
using static Terraria.ModLoader.ModContent;

namespace Kourindou
{
    public class KourindouPlayer : ModPlayer
    {
//--------------------------------------------------------------------------------
        // Determines the power mode of all the plushies
        public byte plushiePower;

        // Dedicated Plushie slot
        public UIItemSlot plushieEquipSlot;
        
        // Cirno Plushie Effect Attack Counter
        public byte CirnoPlushie_Attack_Counter;
        public bool CirnoPlushie_TimesNine;

//--------------------------------------------------------------------------------
        public override void clientClone(ModPlayer clientClone)
        {
            KourindouPlayer clone = clientClone as KourindouPlayer;

            if (clone == null)
            {
                return;
            }

            clone.plushieEquipSlot.Item = plushieEquipSlot.Item.Clone();
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            KourindouPlayer oldClone = clientPlayer as KourindouPlayer;

            if (oldClone == null)
            {
                return;
            }

            //Detect changes in plushie equip slot
            if (oldClone.plushieEquipSlot.Item.IsNotTheSameAs(plushieEquipSlot.Item))
            {
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)KourindouMessageType.PlushieSlot);
                packet.Write((byte)player.whoAmI);
                ItemIO.Send(plushieEquipSlot.Item, packet);
                packet.Send(-1, player.whoAmI);
            }

        }

        // Init
        public override void Initialize() 
        {
            plushieEquipSlot = new UIItemSlot(
                Vector2.Zero,
                context: ItemSlot.Context.EquipAccessory,
                hoverText: "Plushie",
                conditions: Slot_Conditions,
                drawBackground: Slot_DrawBackground,
                scaleToInventory: true
                );

            plushieEquipSlot.BackOpacity = .8f;

            plushieEquipSlot.Item = new Item();
            plushieEquipSlot.Item.SetDefaults(0, true);
        }

        public override TagCompound Save() 
        {
            return new TagCompound 
            {
                { "plushieEquipSlot", ItemIO.Save(plushieEquipSlot.Item) },
                { "plushiePowerMode", plushiePower},
                { "cirnoPlushieAttackCounter", CirnoPlushie_Attack_Counter},
                { "cirnoPlushieTimesNine", CirnoPlushie_TimesNine}
            };
        }

        public override void Load(TagCompound tag) 
        {
            SetPlushie(ItemIO.Load(tag.GetCompound("plushieEquipSlot")));
            plushiePower = tag.GetByte("plushiePowerMode");
            CirnoPlushie_Attack_Counter = tag.GetByte("cirnoPlushieAttackCounter");
            CirnoPlushie_TimesNine = tag.GetBool("cirnoPlushieTimesNine");
            base.Load(tag);
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
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)KourindouMessageType.ClientConfig);
                packet.Write((byte)Main.myPlayer);
                packet.Write((byte)plushiePower);
                packet.Send();

                packet = mod.GetPacket();
                packet.Write((byte)KourindouMessageType.PlushieSlot);
                packet.Write((byte)Main.myPlayer);
                ItemIO.Send(Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushieEquipSlot.Item, packet);
                packet.Send();
            }
        }

        // Update player with the equipped plushie
        public override void UpdateEquips(ref bool wallSpeedBuff, ref bool tileSpeedBuff, ref bool tileRangeBuff)
        {
            // When the plushie power setting is changed to 0 or 1 clear the slot 
            // and place the item on top of the player
            if (Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushiePower != 2 && player.whoAmI == Main.myPlayer)
            {
                if (plushieEquipSlot.Item.stack > 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Item.NewItem(
                            Main.player[Main.myPlayer].Center,
                            new Vector2(Main.player[Main.myPlayer].width, Main.player[Main.myPlayer].height),
                            plushieEquipSlot.Item.type, 
                            1
                        );
                    }
                    else
                    {
                        ModPacket packet = mod.GetPacket();
                        packet.Write((byte)KourindouMessageType.ForceUnequipPlushie);
                        packet.Write((byte)Main.myPlayer);
                        ItemIO.Send(Main.player[Main.myPlayer].GetModPlayer<KourindouPlayer>().plushieEquipSlot.Item, packet);
                        packet.Send();
                    }

                    plushieEquipSlot.Item = new Item();
                    plushieEquipSlot.Item.SetDefaults(0, true);
                }
            }

            if (plushieEquipSlot.Item.stack > 0)
            {
                player.VanillaUpdateAccessory(player.whoAmI, plushieEquipSlot.Item, !plushieEquipSlot.ItemVisible, ref wallSpeedBuff,
                    ref tileSpeedBuff, ref tileRangeBuff);

                player.VanillaUpdateEquip(plushieEquipSlot.Item);
            }
        }

        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            // Plushie Power 2
            if (plushiePower == 2)
            {   
                // Cirno Plushie Equipped
                if (plushieEquipSlot.Item.type == ItemType<Cirno_Plushie_Item>())
                {
                    CirnoPlushie_OnHit(null, target, crit);
                }

                // FlandreScarlet Plushie Equipped
                if (plushieEquipSlot.Item.type == ItemType<FlandreScarlet_Plushie_Item>())
                {
                    FlandreScarletPlushie_OnHit(target, null, damage, crit, item.useAnimation);
                }
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockBack, bool crit)
        {
            // Plushie Power 2
            if (plushiePower == 2)
            {
                // Cirno Plushie Equipped
                if (plushieEquipSlot.Item.type == ItemType<Cirno_Plushie_Item>())
                {
                    CirnoPlushie_OnHit(null, target, crit);
                }
                
                // FlandreScarlet Plushie Equipped
                if (plushieEquipSlot.Item.type == ItemType<FlandreScarlet_Plushie_Item>())
                {
                    FlandreScarletPlushie_OnHit(target, null, damage, crit, target.immune[proj.owner]);
                    if (crit)
                    {
                        target.immune[proj.owner] = 0;
                    }      
                }
            }
        }

        public override void OnHitPvp(Item item, Player target, int damage, bool crit)
        {
            // Plushie Power 2
            if (plushiePower == 2)
            {
                // Cirno Plushie Equipped
                if (plushieEquipSlot.Item.type == ItemType<Cirno_Plushie_Item>())
                {
                    CirnoPlushie_OnHit(target, null, crit);
                }

                // FlandreScarlet Plushie Equipped
                if (plushieEquipSlot.Item.type == ItemType<FlandreScarlet_Plushie_Item>())
                {
                    FlandreScarletPlushie_OnHit(null, target, damage, crit, item.useAnimation);
                }
            }
        }

        public override void OnHitPvpWithProj (Projectile proj, Player target, int damage, bool crit)
        {
            // Plushie Power 2
            if (plushiePower == 2)
            { 
                // Cirno Plushie Equipped
                if (plushieEquipSlot.Item.type == ItemType<Cirno_Plushie_Item>())
                {
                    CirnoPlushie_OnHit(target, null, crit);
                }

                // FlandreScarlet Plushie Equipped
                if (plushieEquipSlot.Item.type == ItemType<FlandreScarlet_Plushie_Item>())
                {
                    FlandreScarletPlushie_OnHit(null, target, damage, crit, target.immuneTime);
                    if (crit)
                    {
                        target.immuneTime = 0;
                    }
                }
            }
        }

        private void CirnoPlushie_OnHit(Player p, NPC n, bool crit)
        {
            CirnoPlushie_Attack_Counter++;
            
            if (CirnoPlushie_Attack_Counter == 8 && (int)Main.rand.Next(0,10) == 9)
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



                Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode.SoundId, (int)position.X, (int)position.Y, SoundID.DD2_ExplosiveTrapExplode.Style, .8f, 1f);

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Send sound packet for other clients
                    ModPacket packet2 = mod.GetPacket();
                    packet2.Write((byte) KourindouMessageType.PlaySound);
                    packet2.Write((byte) SoundID.DD2_ExplosiveTrapExplode.SoundId);
                    packet2.Write((short) SoundID.DD2_ExplosiveTrapExplode.Style);
                    packet2.Write((float) 0.8f);
                    packet2.Write((float) 1f);
                    packet2.Write((int)position.X);
                    packet2.Write((int)position.Y);
                    packet2.Send();
                }
            }
        }

        //Draw Plushie slot
        private void Slot_DrawBackground(UIObject sender, SpriteBatch spriteBatch)
        {
            UIItemSlot slot = (UIItemSlot)sender;

            if (ShouldDrawSlots())
            {
                slot.OnDrawBackground(spriteBatch);

                if(slot.Item.stack == 0)
                {
                    Texture2D tex = mod.GetTexture(Kourindou.PlushieSlotBackTex);
                    Vector2 origin = tex.Size() / 2f * Main.inventoryScale;
                    Vector2 position = slot.Rectangle.TopLeft();

                    spriteBatch.Draw(
                        tex,
                        position + (slot.Rectangle.Size() / 2f) - (origin / 2f),
                        null,
                        Color.White * 0.35f,
                        0f,
                        origin,
                        Main.inventoryScale,
                        SpriteEffects.None,
                        0f); // layer depth 0 = front
                }
            }
        }

        private bool Slot_Conditions(Item item)
        {
            // Prevent Nasty NullReferenceException - Crashes Game 
            if (item.modItem != null)
            {
                // Check whether this item can be placed in the Plushie Slot
                if (item.Clone().modItem.GetType().IsSubclassOf(typeof(PlushieItem)) && Main.LocalPlayer.GetModPlayer<KourindouPlayer>().plushiePower == 2)
                {
                    return true;
                }
            }   

            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //Draw the custom inventory slot
            if (!ShouldDrawSlots())
            {
                return;
            }

            int mapH = 0;

            int rX;
            int rY;
            float origScale = Main.inventoryScale;

            Main.inventoryScale = 0.85f;

            if(Main.mapEnabled) {
                if(!Main.mapFullscreen && Main.mapStyle == 1) {
                    mapH = 256;
                }
            }

            if(Main.mapEnabled) {
                int adjustY = 600;
                if(Main.player[Main.myPlayer].ExtraAccessorySlotsShouldShow) {
                    adjustY = 610 + PlayerInput.UsingGamepad.ToInt() * 30;
                }
                if((mapH + adjustY) > Main.screenHeight) {
                    mapH = Main.screenHeight - adjustY;
                }
            }
            int slotCount = 7 + Main.player[Main.myPlayer].extraAccessorySlots;
            if((Main.screenHeight < 900) && (slotCount >= 8)) {
                slotCount = 7;
            }
            rX = Main.screenWidth - 92 - 14 - (47 * 3) - (int)(Main.extraTexture[58].Width * Main.inventoryScale);
            rY = (int)(mapH + 174 + 4 + slotCount * 56 * Main.inventoryScale);

            //if Wingslot is also installed move up
            Mod Wingslot = ModLoader.GetMod("WingSlot");
            if (Wingslot != null && Wingslot.Version >= new Version(1,7,3))
            {
                if (WingSlotNextToAccessories)
                {
                    rY -= 47;
                }
            }

            plushieEquipSlot.Position = new Vector2(rX, rY);
            plushieEquipSlot.Draw(spriteBatch);

            Main.inventoryScale = origScale;

            plushieEquipSlot.Update();
        }
        
        //Check WingSlot Mod Settings
        public bool WingSlotNextToAccessories
        {
            get { return WingSlot.WingSlot.SlotsNextToAccessories; }
        }

        // Determine if the slot should be drawn
        public static bool ShouldDrawSlots()
        {
            if (Main.playerInventory == true && Main.EquipPage == 0 && Main.LocalPlayer.GetModPlayer<KourindouPlayer>().plushiePower == 2)
            {
                return true;
            }

            return false;
        }

        public void EquipPlushie(bool isVanity, Item item)
        {
            if (player.GetModPlayer<KourindouPlayer>().plushiePower != 2)
            {
                return;
            }

            UIItemSlot slot = plushieEquipSlot;
            int fromSlot = Array.FindIndex(player.inventory, i => i == item);

            if (fromSlot < 0)
            {
                return;
            }

            item.favorited = false;
            player.inventory[fromSlot] = slot.Item.Clone();
            Main.PlaySound(SoundID.Grab);
            Recipe.FindRecipes();
            SetPlushie(item);
        }

        private void SetPlushie(Item item)
        {
            plushieEquipSlot.Item = item.Clone();
        }

        public void ClearPlushie()
        {
            plushieEquipSlot.Item = new Item();
            plushieEquipSlot.Item.SetDefaults();
        }
    }
}