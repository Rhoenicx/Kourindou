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
using Kourindou.Items.Plushies;
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

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)KourindouMessageType.PlushieSlot);
            packet.Write((byte)player.whoAmI);
            ItemIO.Send(plushieEquipSlot.Item, packet);
            packet.Send(toWho, fromWho);
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

        // Update player with the equipped plushie
        public override void UpdateEquips(ref bool wallSpeedBuff, ref bool tileSpeedBuff, ref bool tileRangeBuff)
        {
            // When the plushie power setting is changed to 0 or 1 clear the slot 
            // and place the item on top of the player
            if (Main.LocalPlayer.GetModPlayer<KourindouPlayer>().plushiePower != 2)
            {
                if (plushieEquipSlot.Item.stack > 0)
                {
                    Item.NewItem(
                        Main.LocalPlayer.Center,
                        new Vector2(Main.LocalPlayer.width, Main.LocalPlayer.height),
                        plushieEquipSlot.Item.type, 
                        1
                    );

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

        // When a player joins a world
        public override void OnEnterWorld(Player player)
        {
            // PlushiePower Client Config
            plushiePower = (byte)Kourindou.KourindouConfigClient.plushiePower;

            // If joining a server, also send a packet
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)KourindouMessageType.ClientConfig);
                packet.Write((byte)player.whoAmI);
                packet.Write((byte)plushiePower);
                packet.Send();
            }

            base.OnEnterWorld(player);
        }

        public override TagCompound Save() 
        {
            return new TagCompound 
            {
                { "plushieEquipSlot", ItemIO.Save(plushieEquipSlot.Item) },
                { "plushiePowerMode", plushiePower}
            };
        }

        public override void Load(TagCompound tag) 
        {
            SetPlushie(ItemIO.Load(tag.GetCompound("plushieEquipSlot")));
            plushiePower = tag.GetByte("plushiePowerMode");
            base.Load(tag);
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