using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Kourindou.KourindouSpellcardSystem;
using Kourindou.Items.Spellcards;
using System.Threading;
using System.Collections;
using System.Text.RegularExpressions;
using Kourindou.Projectiles;

namespace Kourindou.Items.Catalysts
{
    public abstract class CatalystItem : ModItem
    {
        #region Catalyst_Properties
        public int CatalystID = 0;
        public bool HasAlwaysCastCard = false;
        public bool ShufflingCatalyst = false;
        public int CastAmount = 1;
        public int NextCastStartIndex = 0;
        public int CardSlotAmount = 1;
        public int BaseRecharge = 0;
        public int BaseCooldown = 0;
        public float BaseSpread = 0f;
        public int AddedRecharge = 0;
        public int AddedCooldown = 0;
        public int AddedDamage = 0;
        public float AddedSpread = 0;

        public float HeldCatalystOffset = 0f;

        public List<CardItem> CardItemsOnCatalyst = new List<CardItem>();
        public CardItem AlwaysCastCard;
        #endregion

        //----- Cloning of this catalyst -----//
        public override ModItem Clone(Item newEntity)
        {
            if (newEntity.ModItem is CatalystItem newCatalyst)
            {
                newCatalyst.HasAlwaysCastCard = HasAlwaysCastCard;
                newCatalyst.ShufflingCatalyst = ShufflingCatalyst;
                newCatalyst.CastAmount = CastAmount;
                newCatalyst.CardSlotAmount = CardSlotAmount;
                newCatalyst.AddedRecharge = AddedRecharge;
                newCatalyst.AddedCooldown = AddedCooldown;
                newCatalyst.AddedDamage = AddedDamage;
                newCatalyst.AddedSpread = AddedSpread;
                newCatalyst.CardItemsOnCatalyst = new List<CardItem>(CardItemsOnCatalyst);
                newCatalyst.AlwaysCastCard = AlwaysCastCard;
            }

            return base.Clone(newEntity);
        }

        #region DataHandling
        public override void SaveData(TagCompound tag)
        {
            tag.Add("HasAlwaysCastCard", HasAlwaysCastCard);
            tag.Add("ShufflingCatalyst", ShufflingCatalyst);
            tag.Add("CastAmount", CastAmount);
            tag.Add("NextCastStartIndex", NextCastStartIndex);
            tag.Add("CardSlotAmount", CardSlotAmount);
            tag.Add("AddedRecharge", AddedRecharge);
            tag.Add("AddedCooldown", AddedCooldown);
            tag.Add("AddedDamage", AddedDamage);
            tag.Add("AddedSpread", AddedSpread);

            if (HasAlwaysCastCard)
            {
                tag.Add("AlwaysCastCard.Group", AlwaysCastCard.Group);
                tag.Add("AlwaysCastCard.Spell", AlwaysCastCard.Spell);
            }

            for (int i = 0; i < CardSlotAmount; i++)
            {
                if (CardItemsOnCatalyst.ElementAtOrDefault(i) != null)
                {
                    tag.Add("CardSlot[" + i + "].Group", CardItemsOnCatalyst[i].Group);
                    tag.Add("CardSlot[" + i + "].Spell", CardItemsOnCatalyst[i].Spell);
                    tag.Add("CardSlot[" + i + "].Stack", CardItemsOnCatalyst[i].Item.stack);
                }
            }
        }

        public override void LoadData(TagCompound tag)
        {
            HasAlwaysCastCard = tag.GetBool("HasAlwaysCastCard");
            ShufflingCatalyst = tag.GetBool("ShufflingCatalyst");
            CastAmount = tag.GetInt("CastAmount");
            NextCastStartIndex = tag.GetInt("NextCastStartIndex");
            CardSlotAmount = tag.GetInt("CardSlotAmount");
            AddedRecharge = tag.GetInt("AddedRecharge");
            AddedCooldown = tag.GetInt("AddedCooldown");
            AddedDamage = tag.GetInt("AddedDamage");
            AddedSpread = tag.GetInt("AddedSpread");

            if (HasAlwaysCastCard)
            {
                byte Group = tag.GetByte("AlwaysCastCard.Group");
                byte Spell = tag.GetByte("AlwaysCastCard.Spell");
                AlwaysCastCard = GetCardItem(Group, Spell);
            }

            for (int i = 0; i < CardSlotAmount; i++)
            {
                byte Group = tag.GetByte("CardSlot[" + i + "].Group");
                byte Spell = tag.GetByte("CardSlot[" + i + "].Spell");
                int Stack = tag.GetInt("CardSlot[" + i + "].Stack");

                if (CardItemsOnCatalyst.ElementAtOrDefault(i) == null)
                {
                    CardItemsOnCatalyst.Insert(i, GetCardItem(Group, Spell, Stack));
                }
                else
                {
                    CardItemsOnCatalyst[i] = GetCardItem(Group, Spell, Stack);
                }
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(CatalystID);
            writer.Write(HasAlwaysCastCard);
            writer.Write(ShufflingCatalyst);
            writer.Write(CastAmount);
            writer.Write(NextCastStartIndex);
            writer.Write(CardSlotAmount);
            writer.Write(AddedRecharge);
            writer.Write(AddedCooldown);
            writer.Write(AddedDamage);
            writer.Write(AddedSpread);

            if (HasAlwaysCastCard)
            {
                writer.Write(AlwaysCastCard.Group);
                writer.Write(AlwaysCastCard.Spell);
            }

            for (int i = 0; i < CardSlotAmount; i++)
            {
                CheckCardSlot(i);
                writer.Write(CardItemsOnCatalyst[i].Group);
                writer.Write(CardItemsOnCatalyst[i].Spell);
                writer.Write(CardItemsOnCatalyst[i].Item.stack);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            CatalystID = reader.ReadInt32();
            HasAlwaysCastCard = reader.ReadBoolean();
            ShufflingCatalyst = reader.ReadBoolean();
            CastAmount = reader.ReadInt32();
            NextCastStartIndex = reader.ReadInt32();
            CardSlotAmount = reader.ReadInt32();
            AddedRecharge = reader.ReadInt32();
            AddedCooldown = reader.ReadInt32();
            AddedDamage = reader.ReadInt32();
            AddedSpread = reader.ReadSingle();

            if (HasAlwaysCastCard)
            {
                AlwaysCastCard = GetCardItem(reader.ReadByte(), reader.ReadByte());
            }

            for (int i = 0; i < CardSlotAmount; i++)
            {
                byte Group = reader.ReadByte();
                byte Spell = reader.ReadByte();
                int Stack = reader.ReadInt32();

                if (CardItemsOnCatalyst.ElementAtOrDefault(i) == null)
                {
                    CardItemsOnCatalyst.Insert(i, GetCardItem(Group, Spell, Stack));
                }
                else
                {
                    if (CardItemsOnCatalyst[i].Group != Group
                        || CardItemsOnCatalyst[i].Spell != Spell)
                    {
                        CardItemsOnCatalyst[i] = GetCardItem(Group, Spell, Stack);
                    }

                    if (CardItemsOnCatalyst[i].Group == Group
                       && CardItemsOnCatalyst[i].Spell == Spell
                       && CardItemsOnCatalyst[i].Item.stack != Stack)
                    {
                        CardItemsOnCatalyst[i].Item.stack = Stack;
                    }
                }
            }
        }
        #endregion

        #region Tooltip
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);

            String text = "";
            text += "Cast amount: " + CastAmount + "\r\n";
            text += "Shuffle: " + ShufflingCatalyst +"\r\n";
            text += "Cards:\r\n";

            for (byte i = 0; i < CardSlotAmount; i++)
            {
                CheckCardSlot(i);

                text += CardItemsOnCatalyst[i].Item.Name + "\r\n";
            }

            TooltipLine line1 = new TooltipLine(Mod, "CatalystText", text);

            tooltips.Add(line1);
        }

        #endregion

        #region Defaults
        public abstract void SetCatalystDefaults();

        public override void SetDefaults()
        {
            // Item Defaults
            SetCatalystDefaults();

            // ID of this catalyst
            CatalystID = Kourindou.GetNewCatalystID();

            // Initialize card slots
            for (int i = 0; i < CardSlotAmount; i++)
            {
                CheckCardSlot(i);
            }
        }
        #endregion

        #region UI
        // Place this in the UI code file instead!!!!!!
        //----------------------------------------------
        public bool LeftClickCardSlot(byte slot)
        {
            // This slot does not exist on this catalyst => return.
            if (!CheckCardSlot(slot))
            {
                return false;
            }

            // The mouse cursor has no item on it
            if (Main.mouseItem.type == ItemID.None)
            {
                // Check the requested card slot 
                if (CardItemsOnCatalyst[slot].Group != (byte)Groups.Empty)
                {
                    Main.mouseItem = CardItemsOnCatalyst[slot].Item.Clone();
                    CardItemsOnCatalyst[slot] = GetCardItem((byte)Groups.Empty, 0);
                    return true;
                }
            }
            // The mouse cursor has an Item
            else if (Main.mouseItem.ModItem is CardItem mouseItem)
            {
                // The slot in the Card List is still null, we can instantly insert it.
                if (CardItemsOnCatalyst.ElementAtOrDefault(slot) == null)
                {
                    CardItemsOnCatalyst.Insert(slot, (CardItem)mouseItem.Item.Clone().ModItem);
                    Main.mouseItem = new Item(ItemID.None);
                    return true;
                }

                // The slot in the Card List contains an empty card, overwrite it.
                else if (CardItemsOnCatalyst[slot].Group == (byte)Groups.Empty)
                {
                    CardItemsOnCatalyst[slot] = (CardItem)mouseItem.Item.Clone().ModItem;
                    Main.mouseItem = new Item(ItemID.None);
                    return true;
                }

                // The slot in the Card List contains a consumable card,
                // and we have the same card on the mouse, increase stack
                else if (CardItemsOnCatalyst[slot].Group == mouseItem.Group
                    && CardItemsOnCatalyst[slot].Spell == mouseItem.Spell
                    && CardItemsOnCatalyst[slot].IsConsumable
                    && mouseItem.IsConsumable)
                {
                    if (mouseItem.Item.stack + CardItemsOnCatalyst[slot].Item.stack > CardItemsOnCatalyst[slot].Item.maxStack)
                    {
                        int amount = CardItemsOnCatalyst[slot].Item.maxStack - CardItemsOnCatalyst[slot].Item.stack;
                        CardItemsOnCatalyst[slot].Item.stack += amount;
                        mouseItem.Item.stack -= amount;
                        return true;
                    }
                    else
                    {
                        CardItemsOnCatalyst[slot].Item.stack += mouseItem.Item.stack;
                        Main.mouseItem = new Item(ItemID.None);
                        return true;
                    }
                }

                // Swap the cards between slot and mouse
                else
                {
                    CardItem card = CardItemsOnCatalyst[slot];
                    CardItemsOnCatalyst[slot] = mouseItem;
                    Main.mouseItem = card.Item;
                    return true;
                }
            }
            return false;
        }

        public bool RightClickCardSlot(byte slot)
        {
            if (!CheckCardSlot(slot) || CardItemsOnCatalyst[slot].Group == (byte)Groups.Empty)
            {
                return false;
            }

            // Mouse empty
            if (Main.mouseItem.type == ItemID.None)
            {
                // Clone the card item to the mouse cursor
                Main.mouseItem = CardItemsOnCatalyst[slot].Item.Clone();

                // If the stack on the slot is greater than 1, only grab one from the stack
                if (CardItemsOnCatalyst[slot].Item.stack > 1)
                {
                    Main.mouseItem.stack = 1;
                    CardItemsOnCatalyst[slot].Item.stack -= 1;
                    return true;
                }
                // Otherwise set to zero
                else
                {
                    CardItemsOnCatalyst[slot] = GetCardItem((byte)Groups.Empty, 0);
                }
                return true;
            }
            else if (Main.mouseItem.ModItem is CardItem mouseItem)
            {
                // The card on the mouse is the same as the slot, and are both consumable
                if (CardItemsOnCatalyst[slot].Group == mouseItem.Group
                    && CardItemsOnCatalyst[slot].Spell == mouseItem.Spell
                    && CardItemsOnCatalyst[slot].IsConsumable
                    && mouseItem.IsConsumable)
                {
                    // There are cards available on the stack, move the amounts
                    if (CardItemsOnCatalyst[slot].Item.stack > 1)
                    {
                        CardItemsOnCatalyst[slot].Item.stack -= 1;
                        mouseItem.Item.stack += 1;
                        return true;
                    }
                    // When the card slot is on stack = 1 (one left), grab it and then empty the slot
                    else
                    {
                        mouseItem.Item.stack += 1;
                        CardItemsOnCatalyst[slot] = GetCardItem((byte)Groups.Empty, 0);
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
        public bool CheckCardSlot(int slot)
        {
            if (slot >= CardSlotAmount)
            {
                return false;
            }

            if (CardItemsOnCatalyst.ElementAtOrDefault(slot) == null)
            {
                CardItemsOnCatalyst.Insert(slot, GetCardItem((byte)Groups.Empty, 0));
            }

            return true;
        }

        public bool AddCardSlots(int amount)
        {
            CardSlotAmount += amount;

            for (int i = 0; i < amount; i++)
            {
                CardItemsOnCatalyst.Add(GetCardItem((byte)Groups.Empty, 0));
            }
            return true;
        }

        #region RightClick
        public override bool CanRightClick()
        {
            return true;
        }

        public override void RightClick(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (Main.mouseItem.type == ItemID.None)
                {
                    for (int i = CardSlotAmount - 1; i >= 0; i--)
                    {
                        CheckCardSlot(i);

                        if (CardItemsOnCatalyst[i].Group != (byte)Groups.Empty)
                        {
                            Main.mouseItem = CardItemsOnCatalyst[i].Item.Clone();
                            CardItemsOnCatalyst[i] = GetCardItem((byte)Groups.Empty, 0);
                            break;
                        }
                    }
                }
                else if (Main.mouseItem.ModItem is CardItem card)
                {
                    for (int i = 0; i < CardSlotAmount; i++)
                    { 
                        CheckCardSlot(i);

                        if (CardItemsOnCatalyst[i].Group == (byte)Groups.Empty)
                        {
                            CardItemsOnCatalyst[i] = (CardItem)card.Item.Clone().ModItem;
                            Main.mouseItem = new Item(ItemID.None);
                            break;
                        }
                    }
                }
            }
        }

        public override bool ConsumeItem(Player player)
        {
            return false;
        }
        #endregion

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Copy the card list to a new list

            List<CardItem> Cards = CardItemsOnCatalyst.ToList();

            // Set the slot positions
            SetSlotPositions(ref Cards);
            SetSlotPositions(ref CardItemsOnCatalyst);

            // Shuffle the cards at the start
            if (ShufflingCatalyst && NextCastStartIndex == 0)
            {
                ShuffleCardItems(ref Cards);
            }

            // Generate cast
            Cast cast = GenerateCast(Cards, AlwaysCastCard, true, NextCastStartIndex, CastAmount, true);

            // Apply the properties after the cast to this catalyst
            NextCastStartIndex = cast.NextCastStartIndex;

            // Apply consumed cards
            ConsumedCards(ref CardItemsOnCatalyst, cast.ConsumedCards, cast.ChanceNoConsumeCard);

            // Setup projectile spawn parameters
            int LifeTime = Item.useAnimation;

            if (!cast.FailedToCast && cast.MinimumUseTime > Item.useAnimation)
            {
                LifeTime = cast.MinimumUseTime;
            }

            // Spawn the catalyst projectile
            int CatalystProjID = Terraria.Projectile.NewProjectile(
                source,
                position,
                velocity,
                type,
                damage,
                knockback,
                Main.myPlayer,
                LifeTime
            );

            // Pass the cast properties clientsided
            if (Main.projectile[CatalystProjID].ModProjectile is CatalystProjectile catalyst)
            {
                catalyst.Casts = cast.Casts;
            }

            // Calculate Cooldown and recharge
            int Timeout = 0;

            if (cast.MustGoOnCooldown && !cast.CooldownOverride)
            {
                Timeout = (int)((AddedCooldown + AddedCooldown + cast.CooldownTime) * cast.CooldownTimePercentage);
            }

            if (!cast.MustGoOnCooldown && !cast.RechargeOverride)
            {
                Timeout = -(int)((BaseRecharge + AddedRecharge + cast.RechargeTime) * cast.RechargeTimePercentage);
            }

            player.GetModPlayer<KourindouPlayer>().SetCooldown(
                CatalystID,
                LifeTime,
                Timeout
            );

            return false;
        }
    }
}