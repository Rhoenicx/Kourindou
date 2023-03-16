using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Kourindou.KourindouSpellcardSystem;
using System.Collections.Generic;
using Kourindou.Items.Spellcards;
using Terraria.ModLoader.IO;

namespace Kourindou.Items.Catalysts
{
    public abstract class CatalystItem : ModItem
    {
        #region Catalyst_Properties
        public bool HasAlwaysCastCard = false;
        public bool ShufflingCatalyst = false;
        public int CastAmount = 1;
        public int NextCastStartIndex = 0;
        public int CardSlotAmount = 1;

        public List<CardItem> CardItemsOnCatalyst = new List<CardItem>();
        public CardItem AlwaysCastCard;
        #endregion

        //----- Cloning of this catalyst -----//
        public override ModItem Clone(Item newEntity)
        {
            if (newEntity.ModItem is CatalystItem newCatalyst)
            {
                newCatalyst.CardSlotAmount = CardSlotAmount;
                newCatalyst.CardItemsOnCatalyst = new List<CardItem>(CardItemsOnCatalyst);
                newCatalyst.HasAlwaysCastCard = HasAlwaysCastCard;
                newCatalyst.AlwaysCastCard = AlwaysCastCard;
                newCatalyst.ShufflingCatalyst = ShufflingCatalyst;
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
            SetCatalystDefaults();

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

        public override bool CanUseItem(Player player)
        {
            /*
            CardItemsOnCatalyst[1] = GetCardItem((byte)Groups.ProjectileModifier, (byte)ProjectileModifier.Acceleration);
            return true;

            List<CardItem> CatalystCards = new List<CardItem>()
            {
                GetCardItem((byte)Groups.ProjectileModifier,    (byte)ProjectileModifier.Acceleration               ),
                GetCardItem((byte)Groups.Trigger,               (byte)Trigger.Trigger                               ),
                GetCardItem((byte)Groups.Empty,                 0                                                   ),
                GetCardItem((byte)Groups.Multicast,             (byte)Multicast.DoubleCast                          ),
                GetCardItem((byte)Groups.Multicast,             (byte)Multicast.DoubleCast                          ),
                GetCardItem((byte)Groups.Empty,                 0                                                   ),
                GetCardItem((byte)Groups.Formation,             (byte)Formation.Octagon                             ),
                GetCardItem((byte)Groups.Projectile,            (byte)KourindouSpellcardSystem.Projectile.ShotBlue  ),
                GetCardItem((byte)Groups.Projectile,            (byte)KourindouSpellcardSystem.Projectile.ShotBlue  ),
                GetCardItem((byte)Groups.ProjectileModifier,    (byte)ProjectileModifier.Explosion                  ),
                GetCardItem((byte)Groups.Projectile,            (byte)KourindouSpellcardSystem.Projectile.ShotBlue  ),
                GetCardItem((byte)Groups.Projectile,            (byte)KourindouSpellcardSystem.Projectile.ShotBlue  ),
                GetCardItem((byte)Groups.Empty,                 0                                                   ),
                GetCardItem((byte)Groups.Empty,                 0                                                   ),
                GetCardItem((byte)Groups.ProjectileModifier,    (byte)ProjectileModifier.BounceDown                 ),
                GetCardItem((byte)Groups.Projectile,            (byte)KourindouSpellcardSystem.Projectile.ShotBlue  ),
                GetCardItem((byte)Groups.ProjectileModifier,    (byte)ProjectileModifier.LifetimeUp                 ),
                GetCardItem((byte)Groups.Projectile,            (byte)KourindouSpellcardSystem.Projectile.ShotBlue  ),
                GetCardItem((byte)Groups.Empty,                 0                                                   ),
                GetCardItem((byte)Groups.Projectile,            (byte)KourindouSpellcardSystem.Projectile.ShotBlue  ),
                GetCardItem((byte)Groups.Empty,                 0                                                   ),
            };
            */
            SetSlotPositions(ref CardItemsOnCatalyst);

            Cast cast = GenerateCast(
                CardItemsOnCatalyst,
                HasAlwaysCastCard ? AlwaysCastCard : null,
                this is CatalystItem,
                NextCastStartIndex,
                CastAmount,
                true
            );

            //----- DEBUG -----//

            Kourindou.Instance.Logger.Debug("FailedToCast - " + cast.FailedToCast);
            Kourindou.Instance.Logger.Debug("NextCastStartIndex - " + cast.NextCastStartIndex);
            Kourindou.Instance.Logger.Debug("MustGoOnCooldown - " + cast.MustGoOnCooldown);
            Kourindou.Instance.Logger.Debug("CooldownOverride - " + cast.CooldownOverride);
            Kourindou.Instance.Logger.Debug("CooldownTime - " + cast.CooldownTime);
            Kourindou.Instance.Logger.Debug("CooldownTimePercentage - " + cast.CooldownTimePercentage);
            Kourindou.Instance.Logger.Debug("RechargeOverride - " + cast.RechargeOverride);
            Kourindou.Instance.Logger.Debug("RechargeTime - " + cast.RechargeTime);
            Kourindou.Instance.Logger.Debug("RechargeTimePercentage - " + cast.RechargeTimePercentage);
            Kourindou.Instance.Logger.Debug("AddUseTime - " + cast.AddUseTime);
            Kourindou.Instance.Logger.Debug("MinimumUseTime - " + cast.MinimumUseTime);
            Kourindou.Instance.Logger.Debug("ChanceNoConsumeCard - " + cast.ChanceNoConsumeCard);

            for (int i = 0; i < cast.Casts.Count; i++)
            {
                Kourindou.Instance.Logger.Debug("//----- Cast " + i + "-----//");

                for (int a = 0; a < cast.Casts[i].Blocks.Count; a++)
                {
                    Kourindou.Instance.Logger.Debug("   //----- Block " + a + " -----//");
                    Kourindou.Instance.Logger.Debug("   Repeat - " + cast.Casts[i].Blocks[a].Repeat);
                    Kourindou.Instance.Logger.Debug("   Delay - " + cast.Casts[i].Blocks[a].Delay);
                    Kourindou.Instance.Logger.Debug("   InitialDelay - " + cast.Casts[i].Blocks[a].InitialDelay);
                    Kourindou.Instance.Logger.Debug("   IsDisabled - " + cast.Casts[i].Blocks[a].IsDisabled);
                    Kourindou.Instance.Logger.Debug("       //----- Cards: -----//");
                    for (int c = 0; c < cast.Casts[i].Blocks[a].CardItems.Count; c++)
                    {
                        Kourindou.Instance.Logger.Debug("       [" + c + "] " + cast.Casts[i].Blocks[a].CardItems[c].Name);
                        Kourindou.Instance.Logger.Debug("           Amount - " + cast.Casts[i].Blocks[a].CardItems[c].Amount);
                        Kourindou.Instance.Logger.Debug("           IsInsertedCard - " + cast.Casts[i].Blocks[a].CardItems[c].IsInsertedCard);
                        Kourindou.Instance.Logger.Debug("           IsWrapped - " + cast.Casts[i].Blocks[a].CardItems[c].IsWrapped);
                        Kourindou.Instance.Logger.Debug("           IsAlwaysCast - " + cast.Casts[i].Blocks[a].CardItems[c].IsAlwaysCast);
                        Kourindou.Instance.Logger.Debug("           IsPayload - " + cast.Casts[i].Blocks[a].CardItems[c].IsPayload);
                        Kourindou.Instance.Logger.Debug("           IsMulticasted - " + cast.Casts[i].Blocks[a].CardItems[c].IsMulticasted);
                        Kourindou.Instance.Logger.Debug("           SlotPosition - " + cast.Casts[i].Blocks[a].CardItems[c].SlotPosition);
                    }
                }
            }

            NextCastStartIndex = cast.NextCastStartIndex;

            return !cast.FailedToCast;
        }

    }
}