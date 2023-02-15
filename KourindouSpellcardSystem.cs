// TODO
// => Convert CardInfo to regular CardItems
// => DiggingBolt nullify cooldown effect when last projectile => Move to actual cast code, keep track which projectile card has fired last
// => Consumable cards

using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Kourindou.Items.Spellcards;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou
{
    // Individual card, this is how the cards will look like when they get passed around on the network
    // And how they are saved on the player and catalyst
    public class Card
    {
        public byte Group { get; set; } = (byte)Groups.Empty;
        public byte Spell { get; set; } = 0;
        public short Amount { get; set; } = 1;
    }

    // Cast block info
    public class CastBlock
    {
        public int Repeat { get; set; } = 0;
        public int Delay { get; set; } = 0;
        public List<CardItem> CardItems { get; set; }
    }

    // CastInfo 
    public class CastInfo
    {
        public List<CastBlock> Blocks { get; set; }
    }

    // Cast Properties
    public class Cast
    {
        // Catalyst stats after cast
        public bool MustGoOnCooldown { get; set; } = false;
        public bool FailedToCast { get; set; } = false;
        public int NextCastStartIndex { get; set; } = 0;

        // Cooldown after everything has fired => Catalyst reset time
        public bool CooldownOverride { get; set; } = false;
        public int CooldownTime { get; set; } = 0;
        public float CooldownTimePercentage { get; set; } = 1f;

        // Recharge time between casts
        public bool RechargeOverride { get; set; } = false;
        public int RechargeTime { get; set; } = 0;
        public float RechargeTimePercentage { get; set; } = 1f;

        // Additional UseTime, might be handy?
        public int AddUseTime { get; set; } = 0;

        // Minimum time the catalyst needs to be used
        // for repeats+cast delays
        public int MinimumUseTime { get; set; } = 0;

        // The actual cast info
        public List<CastInfo> Casts { get; set; }
        public List<int> ConsumedCards { get; set; }
    }

    public class KourindouSpellcardSystem
    {
        private static Dictionary<byte, Dictionary<byte, int>> CardTable = new();
        private static List<Card> EntireCardPool = new();

        public static void Load()
        {
            CardTable = new();
            EntireCardPool = new();
        }

        public static void Unload()
        {
            CardTable.Clear();
            CardTable = null;
            EntireCardPool.Clear();
            EntireCardPool = null;
        }

        public static bool RegisterCardItem(byte group, byte spell, int CardItemID)
        {
            if (!CardTable.ContainsKey(group))
            {
                CardTable.Add(group, new Dictionary<byte, int>());
            }

            if (!CardTable[group].ContainsKey(spell))
            {
                CardTable[group].Add(spell, CardItemID);
                EntireCardPool.Add(new Card() { Group = group, Spell = spell });
                return true;
            }

            return false;
        }

        public static CardItem GetCardItem(Card card)
        {

            if (CardTable.ContainsKey(card.Group)
                && CardTable[card.Group].ContainsKey(card.Spell)
                && new Item(CardTable[card.Group][card.Spell]).ModItem is CardItem item)
            {
                return item;
            }
            return null;
        }

        public static CardItem GetCardItem(byte _Group, byte _Spell)
        {
            return GetCardItem(new Card() { Group = _Group, Spell = _Spell });
        }

        public static void ShuffleCardItems(ref List<CardItem> CardItemList)
        {
            // Create a new Dictionary to store the cards that are not empty cards
            Dictionary<int, CardItem> ShuffleCardList = new();
            for (int i = 0; i < CardItemList.Count; i++)
            {
                if (CardItemList[i].Group != (byte)Groups.Empty)
                {
                    ShuffleCardList.Add(i, CardItemList[i]);
                }
            }

            // Create list of the keys that we can shuffle
            List<int> Keys = new List<int>(ShuffleCardList.Keys);

            // Now shuffle the dictionary values using the list with keys
            int c = Keys.Count;
            while (c > 1)
            {
                c--;
                int r = Main.rand.Next(0, c + 1);
                CardItem value = ShuffleCardList[Keys[r]];
                ShuffleCardList[Keys[r]] = ShuffleCardList[Keys[c]];
                ShuffleCardList[Keys[c]] = value;
            }

            // put the shuffled values back into the Card list
            foreach (int i in ShuffleCardList.Keys)
            {
                CardItemList[i] = ShuffleCardList[i];
            }
        }

        public static List<CardItem> CardToCardItemList(List<Card> Cards)
        {
            List<CardItem> CardItemList = new();
            for (int i = 0; i < Cards.Count; i++)
            {
                CardItem item = GetCardItem(Cards[i].Group, Cards[i].Spell);
                if (item != null)
                {
                    item.Item.stack = Cards[i].Amount;
                    CardItemList.Add(item);
                }
            }

            return CardItemList;
        }

        public static void SetSlotPositions(ref List<CardItem> Cards)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                Cards[i].SlotPosition = i;
            }
        }

        public static Cast GenerateCast(List<Card> Cards, Card AlwaysCastCard, bool IsCatalyst, int CatalystStartIndex = 0, int CatalystCastAmount = 1, bool Shuffle = false)
        {
            return GenerateCast(CardToCardItemList(Cards), GetCardItem(AlwaysCastCard), IsCatalyst, CatalystStartIndex, CatalystCastAmount, Shuffle);
        }

        public static Cast GenerateCast(List<CardItem> Cards, CardItem AlwaysCastCard, bool IsCatalyst, int CatalystStartIndex = 0, int CatalystCastAmount = 1, bool Shuffle = false)
        {
            // Here we generate the cast info which is passed to the execution part
            // => If this catalyst is a shuffle, the cards should first be shuffled.
            // => Be sure to use SetSlotPositions BEFORE casting and/or shuffling!!!

            // Cast List for storing the Cast blocks
            Cast CastProperties = new();

            // Save wrap-around cards
            List<CardItem> CardWrapReady = Cards;
            if (Shuffle)
            {
                ShuffleCardItems(ref CardWrapReady);
            }
            CardWrapReady.RemoveRange(CatalystStartIndex, Cards.Count - CatalystStartIndex);

            // Check the AlwaysCastCards
            if (AlwaysCastCard != null && AlwaysCastCard.Group != (byte)Groups.Empty)
            {
                AlwaysCastCard.IsInsertedCard = true;
            }

            // --- Variables needed for loops ---

            // The start index of this cast
            int Index = CatalystStartIndex;

            // Current cast and block, set to zero
            int CurrentCast = 0;
            int CurrentBlock = 0;

            // Triggers
            int Triggers = 0;
            bool TriggerActive = false;

            // Multicasts
            int MulticastAmount = 0;

            // Amount of additional inserted cards counter, 
            // used to determine end index for a next cast
            int InsertedCards = 0;

            // Card that needs to be inserted to the beginning of a wrap around
            // used for cards that require cards after it to work
            CardItem InsertThisCardWrapAround = GetCardItem((byte)Groups.Empty, 0);

            // If we currently are wrapping around
            bool IsWrappingAround = false;

            // Check if there is an unended cast
            int CastUnEnded = 0;

            // Save the amount of slots on the catalyst
            int TypicalCatalystCardAmount = Cards.Count;

            // --- Start Casting ---
            while (Index < Cards.Count && CurrentCast < CatalystCastAmount)
            {
                // Create a new cast, for when the catalyst has multiple casts
                if (CastProperties.Casts.ElementAtOrDefault(CurrentCast) == null)
                {
                    CastProperties.Casts.Add(new CastInfo());
                    // If this catalyst has a Always-Cast insert it
                    if (AlwaysCastCard.Group != (byte)Groups.Empty)
                    {
                        if (AlwaysCastCard.Group == (byte)Groups.Projectile)
                        {
                            // Projectiles end casts, we don't want this to happen with a projectile as alwayscast
                            MulticastAmount++;
                        }

                        // Insert the card
                        Cards.Insert(Index, AlwaysCastCard);

                        // Since we have inserted a card, we should use continue here,
                        // we want the catalyst to execute this index again.
                        continue;
                    }
                }

                if (CastProperties.Casts[CurrentCast].Blocks.ElementAtOrDefault(CurrentBlock) == null)
                {
                    CastProperties.Casts[CurrentCast].Blocks.Add(new CastBlock());
                }

                // Count the amount of cards encountered that have been inserted,
                // This is to determine the ending index for the next cast.
                if (!IsWrappingAround && Cards[Index].IsInsertedCard)
                {
                    InsertedCards++;
                }

                // Get random values for random cards
                if (Cards[Index].IsRandomCard)
                {
                    byte NewSpell = Cards[Index].ApplyRandomCard();
                    if (NewSpell != 255)
                    {
                        int pos = Cards[Index].SlotPosition;
                        Cards[Index] = GetCardItem(Cards[Index].Group, NewSpell);
                        Cards[Index].SlotPosition = pos;
                    }
                }

                switch ((Groups)Cards[Index].Group)
                {
                    case Groups.Empty:
                        {
                            // Do nothing
                        }
                        break;

                    case Groups.Projectile:
                        {
                            if (Triggers > 0)
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                                Triggers--;
                                TriggerActive = true;
                            }
                            else
                            {
                                TriggerActive = false;

                                if (MulticastAmount > 0)
                                {
                                    CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                                    MulticastAmount--;
                                    CurrentBlock++;
                                }
                                else
                                {
                                    CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                                    CurrentCast++;
                                    CastUnEnded = 0;
                                }
                            }
                        }
                        break;

                    case Groups.Multicast:
                        {
                            if (TriggerActive)
                            {
                                Triggers += (int)Math.Ceiling(Cards[Index].Strength) - 1;
                            }
                            else
                            {
                                for (int i = 1; i < (int)Math.Ceiling(Cards[Index].Strength); i++)
                                {
                                    if (CastProperties.Casts[CurrentCast].Blocks.ElementAtOrDefault(CurrentBlock + i) == null)
                                    {
                                        CastProperties.Casts[CurrentCast].Blocks.Add(new CastBlock());
                                    }

                                    CastProperties.Casts[CurrentCast].Blocks[CurrentBlock + i] = CastProperties.Casts[CurrentCast].Blocks[CurrentBlock];
                                }

                                MulticastAmount += (int)Math.Ceiling(Cards[Index].Strength) - 1;
                            }
                        }
                        break;

                    case Groups.Trigger:
                        {
                            // If a trigger is active, this card should get send as payload
                            if (TriggerActive)
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                            }
                            // No trigger active: During wrap-around we do not want to add triggers.
                            else if (!IsWrappingAround)
                            {
                                Triggers += (int)Math.Ceiling(Cards[Index].Strength);
                            }
                        }
                        break;

                    case Groups.Multiplication:
                        {
                            // If a trigger is active, this card should get send as payload
                            if (TriggerActive)
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                            }
                            // Otherwise execute its effect on next card
                            else
                            {
                                // Multiplication is at the end of the catalyst's slots/cards
                                if (Index >= Cards.Count - 1)
                                {
                                    InsertThisCardWrapAround = Cards[Index];
                                }
                                // Multiplication card is not on the end
                                else
                                {
                                    // Check again if its the last card
                                    for (int i = Index + 1; i < Cards.Count; i++)
                                    {
                                        // If the Card is not empty try to apply multiplication effect to it
                                        if (Cards[i].Group != (byte)Groups.Empty)
                                        {
                                            // The card encountered is a random card
                                            bool consume = Cards[i].IsConsumable;
                                            int stack = Cards[i].SlotPosition;
                                            int pos = Cards[i].SlotPosition;
                                            if (Cards[i].Group == (byte)Groups.Special && Cards[i].Spell == (byte)Special.RandomCard)
                                            {
                                                Cards[i] = GetRandomCard(Cards[i]);
                                                Cards[i].SlotPosition = pos;
                                            }

                                            // Now try to apply the multiplication effect
                                            switch ((Groups)Cards[i].Group)
                                            {
                                                case Groups.Projectile:
                                                case Groups.Special:
                                                case Groups.Multicast:
                                                    {
                                                        // If the cards found are of the type: Projectile, Special or Multicast:
                                                        // We cannot apply multiplication effects with the strength property.
                                                        // Insert a copy of the same card instead
                                                        for (int y = 1; y < (int)Cards[Index].Strength; y++)
                                                        {
                                                            if (consume && stack-- <= 0)
                                                            {
                                                                break;
                                                            }

                                                            if (consume)
                                                            {
                                                                CastProperties.ConsumedCards.Add(pos);
                                                            }

                                                            // Insert a copy of the card on the same index,
                                                            // the original one will get moved +1 index
                                                            Cards.Insert(i, (CardItem)Cards[i].Item.Clone().ModItem);

                                                            // Although the copy has been placed on the same index,
                                                            // we can now safely alter the 'inserted' card
                                                            Cards[i + 1].IsInsertedCard = true;
                                                        }
                                                    }
                                                    break;
                                                /*
                                                case Groups.Multiplication:
                                                    {
                                                        Cards[i].Strength *= Cards[Index].Strength;
                                                        if (Cards[i].Strength > 5f)
                                                        {
                                                            Cards[i].Strength = 5f;
                                                        }
                                                    }
                                                    break;

                                                case Groups.CatalystModifier:
                                                    {
                                                        switch ((CatalystModifierVariant)Cards[i].Variant)
                                                        {
                                                            case CatalystModifierVariant.ReduceCooldownPercent:
                                                            case CatalystModifierVariant.ReduceRechargePercent:
                                                                {
                                                                    Cards[i].Strength = (float)Math.Pow(Cards[i].Strength, Cards[Index].Strength);
                                                                }
                                                                break;

                                                            default:
                                                                {
                                                                    Cards[i].Strength *= Cards[Index].Strength;
                                                                }
                                                                break;

                                                        }
                                                    }
                                                    break;
                                                */
                                                default:
                                                    {
                                                        float strength = Cards[Index].Strength;

                                                        if (consume)
                                                        {
                                                            if ((int)strength > stack)
                                                            {
                                                                strength = (float)stack;
                                                            }

                                                            for (int y = 1; y < (int)strength; y++)
                                                            {
                                                                CastProperties.ConsumedCards.Add(Cards[i].SlotPosition);
                                                            }
                                                        }

                                                        Cards[i].ApplyMultiplication(strength);
                                                    }
                                                    break;
                                            }

                                            // Stop the for loop because this card has executed its effect
                                            break;
                                        }
                                        // Check if the search has ended to the last card of the catalyst,
                                        // In this case 
                                        else if (i == Cards.Count - 1)
                                        {
                                            InsertThisCardWrapAround = Cards[Index];
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case Groups.CatalystModifier:
                        {
                            if (TriggerActive)
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                            }
                            else
                            {
                                switch ((CatalystModifierVariant)Cards[Index].Variant)
                                {
                                    case CatalystModifierVariant.None:
                                        {
                                            switch ((CatalystModifier)Cards[Index].Spell)
                                            {
                                                case CatalystModifier.EliminateCooldown:
                                                    {
                                                        CastProperties.CooldownOverride = true;
                                                    }
                                                    break;

                                                case CatalystModifier.EliminateRecharge:
                                                    {
                                                        CastProperties.RechargeOverride = true;
                                                    }
                                                    break;
                                                default:
                                                    {
                                                        // failsafe
                                                        CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                                                    }
                                                    break;
                                            }
                                        }
                                        break;

                                    case CatalystModifierVariant.ReduceRechargePercent:
                                        {
                                            CastProperties.RechargeTimePercentage *= Cards[Index].Strength;
                                        }
                                        break;

                                    case CatalystModifierVariant.ReduceCooldownPercent:
                                        {
                                            // Notes:   [CooldownTimePercentage] is by default 1f
                                            //          [ReduceCooldownPercent] 10% card has 0.9f
                                            //
                                            //          Multiplications on percent cards should be quadratic
                                            //          Math.Pow(0.5f, 2) => 0.25f
                                            CastProperties.CooldownTimePercentage *= Cards[Index].Strength;
                                        }
                                        break;

                                    case CatalystModifierVariant.Repeat:
                                        {
                                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].Repeat += (int)Math.Ceiling(Cards[Index].Strength);
                                        }
                                        break;

                                    case CatalystModifierVariant.Delay:
                                        {
                                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].Delay += (int)Math.Ceiling(Cards[Index].Strength);
                                        }
                                        break;
                                    default:
                                        {
                                            //failsafe
                                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case Groups.Special:
                        {
                            if (TriggerActive)
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                            }
                            else
                            {
                                switch ((Special)Cards[Index].Spell)
                                {
                                    case Special.RandomCard:
                                        {
                                            int pos = Cards[Index].SlotPosition;
                                            Cards[Index] = GetRandomCard(Cards[Index]);
                                            Cards[Index].SlotPosition = pos;
                                            continue;
                                        }

                                    case Special.CastEverything:
                                        {
                                            CatalystCastAmount = 100;
                                        }
                                        break;

                                    default:
                                        {
                                            //failsafe
                                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    default:
                        {
                            // Any other card 
                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                        }
                        break;
                }

                // Apply the card's Cooldown, Recharge and Use times
                CastProperties.CooldownTime += Cards[Index].AddCooldown;
                CastProperties.RechargeTime += Cards[Index].AddRecharge;
                CastProperties.AddUseTime += Cards[Index].AddUseTime;

                // Catalyst only, does not work on projectiles
                if (IsCatalyst)
                {
                    // Increase UnEnded if this card needs a projectile to work
                    // The cast is 'Unended' which means it can trigger a wrap-around.
                    if (Cards[Index].NeedsProjectileCard)
                    {
                        CastUnEnded++;
                    }

                    // If this card is a consumable save the consumption
                    if (!Cards[Index].IsInsertedCard && Cards[Index].IsConsumable)
                    {
                        CastProperties.ConsumedCards.Add(Cards[Index].SlotPosition);
                    }

                    if (!IsWrappingAround)
                    {
                        // Check if a wrap-around is needed
                        if (Index == Cards.Count - 1 && (TriggerActive || Triggers > 0 || MulticastAmount > 0 || CastUnEnded > 0))
                        {
                            // Add the previously saved card which needs another card to work
                            if (InsertThisCardWrapAround.Group != (byte)Groups.Empty)
                            {
                                Cards.Add(InsertThisCardWrapAround);
                            }

                            // Now add the wrap-around cards
                            for (int i = 0; i < CardWrapReady.Count; i++)
                            {
                                Cards.Add(CardWrapReady[i]);
                            }

                            // Since we are wrapping-around, the catalyst needs to go on cooldown
                            CastProperties.MustGoOnCooldown = true;

                            // Make it so that this catalyst cannot fire more in wrap-around if it has multiple casts
                            CatalystCastAmount = CurrentCast + 1;

                            // If there is a trigger active, finish it. But we cannot start new ones during wrap-around.
                            if (TriggerActive)
                            {
                                Triggers = 0;
                            }

                            // Wrapping around is true
                            IsWrappingAround = true;
                        }
                    }
                }
                Index++;
            }

            // Determine the start index of the next potential cast
            CastProperties.NextCastStartIndex = Index - InsertedCards;

            // If the catalyst is not set on cooldown yet by the wrap-around,
            // check right now if we're done
            if (!CastProperties.MustGoOnCooldown)
            {
                // Just in case check if this position is beyond the typical amount of cards
                if (CastProperties.NextCastStartIndex >= TypicalCatalystCardAmount)
                {
                    CastProperties.MustGoOnCooldown = true;
                }

                // if its within the cards on the catalyst, 
                // check if there are cards for the next cast
                else
                {
                    // Check if there are cards that can be fired next time
                    bool Valid = false;
                    for (int i = Index; i < Cards.Count; i++)
                    {
                        if (Cards[i].Group != (byte)Groups.Empty)
                        {
                            Valid = true;
                            break;
                        }
                    }

                    // No cards found => cooldown
                    if (!Valid)
                    {
                        CastProperties.MustGoOnCooldown = true;
                    }
                }
            }

            // Reset the start position for the next cast
            if (CastProperties.MustGoOnCooldown)
            {
                CastProperties.NextCastStartIndex = 0;
            }

            // If an recharge or cooldown override has been executed:
            // Set the recharge or cooldown to zero.
            if (CastProperties.CooldownOverride)
            {
                CastProperties.CooldownTime = 0;
            }

            if (CastProperties.RechargeOverride)
            {
                CastProperties.RechargeTime = 0;
            }

            // Remove CastInfo and CastBlocks which don't have projectiles.
            for (int _CastInfo = CastProperties.Casts.Count; _CastInfo >= 0; _CastInfo--)
            {
                for (int _CastBlock = CastProperties.Casts[_CastInfo].Blocks.Count; _CastBlock >= 0; _CastBlock++)
                {
                    // Remove CastBlocks if it has no projectile that can be fired by the catalyst
                    if (IsCatalyst)
                    {
                        if (!CastProperties.Casts[_CastInfo].Blocks[_CastBlock].CardItems.Any(_CardItem => _CardItem.Group == (byte)Groups.Projectile && _CardItem.Spell != (byte)Projectile.MyOwnProjectileInstance))
                        {
                            CastProperties.Casts[_CastInfo].Blocks.RemoveAt(_CastBlock);
                        }
                    }
                    else
                    {
                        if (!CastProperties.Casts[_CastInfo].Blocks[_CastBlock].CardItems.Any(_CardItem => _CardItem.Group == (byte)Groups.Projectile))
                        {
                            CastProperties.Casts[_CastInfo].Blocks.RemoveAt(_CastBlock);
                        }
                    }
                }

                // Remove CastInfo if there are no CastBlocks anymore
                if (CastProperties.Casts[_CastInfo].Blocks.Count <= 0)
                {
                    CastProperties.Casts.RemoveAt(_CastInfo);
                }
            }

            // Check if we have something we can cast, if not the cast fails.
            if (CastProperties.Casts.Count <= 0)
            {
                CastProperties.FailedToCast = true;
                CastProperties.ConsumedCards.Clear();
            }

            // Determine the MinimumUseTime
            if (!CastProperties.FailedToCast)
            {
                foreach (CastInfo _CastInfo in CastProperties.Casts)
                {
                    foreach (CastBlock _CastBlock in _CastInfo.Blocks)
                    {
                        if (_CastBlock.Repeat * _CastBlock.Delay > CastProperties.MinimumUseTime)
                        {
                            CastProperties.MinimumUseTime = _CastBlock.Repeat * _CastBlock.Delay;
                        }
                    }
                }
            }

            // Cast properties done!
            return CastProperties;
        }

        public static void ConsumedCards(ref List<CardItem> Cards, List<int> Consumed)
        {
            foreach (int i in Consumed)
            {
                if (i >= 0 && i < Cards.Count && Cards[i].IsConsumable && !Cards[i].IsInsertedCard)
                {
                    if (Cards[i].Item.stack > 1)
                    {
                        Cards[i].Item.stack -= 1;
                    }
                    else
                    {
                        Cards[i] = GetCardItem(new Card() { Group = (byte)Groups.Empty, Spell = 0 });
                    }
                }
            }
        }

        #region Random
        public static CardItem GetRandomCard(CardItem ToBeReplacedCard)
        {
            if (ToBeReplacedCard.Group == (byte)Groups.Special && ToBeReplacedCard.Spell == (byte)Special.RandomCard)
            {
                int RetryCounter = 0;
                while (RetryCounter > 1000)
                {
                    CardItem NewCard = GetCardItem(EntireCardPool[Main.rand.Next(0, EntireCardPool.Count)]);
                    if (NewCard != null && NewCard.Group != (byte)Groups.Special && NewCard.Spell != (byte)Special.RandomCard)
                    {
                        return NewCard;
                    }

                    RetryCounter++;
                }
            }

            return ToBeReplacedCard;
        }
        #endregion

        #region Enumerators
        public enum Groups : byte
        {
            Empty,
            Projectile,
            Formation,
            Trajectory,
            ProjectileModifier,
            CatalystModifier,
            Element,
            Multicast,
            Trigger,
            Multiplication,
            Special
        }

        public enum Projectile : byte
        {
            Shot,
            DiggingBolt,
            MyOwnProjectileInstance
        }

        public enum Formation : byte
        {
            // Default straight line
            // Variant 0
            Straight,
            ForwardAndSide,

            // Duplicates projectile and add spread, like a shotgun
            // Variant 1
            DoubleScatter, // 2
            TripleScatter, // 3
            QuadrupleScatter, // 4
            QuintupleScatter, // 5
            RandomScatter, // Main.rand.Next(2, 6); (is not random card)

            // Duplicates projectile and place them side-by-side
            // Variant 2
            DoubleFork, // 2
            TripleFork, // 3
            QuadrupleFork, // 4
            QuintupleFork, // 5
            RandomFork, // Main.rand.Next(2, 6); (is not random card)

            // Duplicates projectile and cast it in a circle around the player
            // Variant 3
            Quadragon, // 4
            Pentagon, // 5
            Hexagon, // 6
            Heptagon, // 7
            Octagon, // 8
            Randagon // Main.rand.Next(4, 9); (is not random card)
        }

        public enum FormationVariant : byte
        {
            None,
            Scatter,
            Fork,
            SomethingGon
        }

        public enum Trajectory : byte
        {
            ArcLeft,
            ArcRight,
            ZigZag,
            PingPong,
            Snake,
            Uncontrolled,
            Orbit,
            Spiral,
            Boomerang,
            Aiming,
            Daedalus,
            RandomTrajectory
        }

        public enum ProjectileModifier : byte
        {
            // Acceleration
            Acceleration, // 1f (percent)
            Deceleration, // 1f (percent)

            // Speed
            IncreaseSpeed, // 1f (percent)
            DecreaseSpeed, // 1f (percent)
            Stationary, // 0f

            // Spread - not including wand spread, if negative it does counter wand spread
            DecreaseSpread, //-30f
            IncreaseSpread, // +30f
            AccurateAF, // set to 0

            // Tile bounces
            BounceUp, //+1
            BounceDown, //-1
            FallingFlat, //0
            TerribleIdea, // Infinite bounces

            // Penetration stats
            PenetrationUp, //+1
            PenetrationDown, // > 1 : -1
            PenetrationZero, //0
            PenetrationAll, //-1

            // Gravity
            Gravity,
            NoGravity,
            AntiGravity,

            // Homing 
            Homing,
            RotateToEnemy,

            // Tile collision - Strength=ON(1)/Off(0) 
            Ghosting,
            Collide,
            Phasing,

            // Lifetime
            LifetimeUp, // 0.5f
            LifetimeDown, // -0.5f
            InstantKill, // set to 0

            // Knockback
            IncreaseKnockback,
            DecreaseKnockback,
            NoKnockback,

            // Rotation
            RotateLeft90, // -90f
            RotateRight90, // -90f
            RotateLeft45, // -45f
            RotateRight45, // 45f
            RotateLeft22_5, // -22.5f
            RotateRight22_5, // 22.5f
            RandomRotation,

            // Size
            Snowball, // Projectile increase in size the longer it travels
            IncreaseSize, // 0.25f - Saki effect
            DecreaseSize, // -0.25f

            // Confined
            BounceOnScreenEdge, // Projectile bounces at the side of the screen
            WarpScreenEdges, // Yukari Screen border

            // Seija
            VectorReversal, // reverses velocity, also flips team

            // Cirno
            Shatter,

            // Keiki
            ProjectileEater,

            // Patchouli
            MagicCircleAura,

            // Suwako
            ChanceExploding,

            // Other
            AngryBullets,

            // Random
            RandomProjectileModifier
        }

        public enum CatalystModifier : byte
        {
            // Position where the projectiles are spawned
            DistantCast, // distance
            TeleportCast, // On/Off
            ReverseCast, // long distance + 180 degrees rotated

            // Eliminates waiting time
            // Variant 0
            EliminateCooldown, //cooldown=0
            EliminateRecharge, //recharge=0
            NextCardNoCooldown, //TODO
            NextCardNoRecharge, //TODO
            ChanceNoConsume, //TODO

            // reduces cooldown percentage-based (Clownpiece - Life-Burning torch)
            // Variant 1
            ReduceCooldown10Percent, // 0.9f
            ReduceCooldown25Percent, // 0.75f
            ReduceCooldown50Percent, // 0.5f
            RandomCooldownPrecent, // Main.rand.NextFloat();

            // reduces recharge percentage-based
            // Variant 2
            ReduceRecharge10Percent, // 0.9f
            ReduceRecharge25Percent, // 0.75f
            ReduceRecharge50Percent, // 0.5f
            RandomRechargePrecent, // Main.rand.NextFloat();

            // Repeats
            // Variant 3
            Repeat2, //2
            Repeat3, //3
            Repeat4, //4
            Repeat5, //5
            RepeatRandom, // Main.rand.Next();

            // Delays in ticks
            // Variant 4
            DelayCastBy1, //60
            DelayCastBy2, //120
            DelayCastBy3, //180
            DelayCastBy4, //240
            DelayCastBy5, //300
            DelayCastRandom // Main.rand.Next();
        }

        public enum CatalystModifierVariant : byte
        {
            None,
            ReduceCooldownPercent,
            ReduceRechargePercent,
            Repeat,
            Delay
        }

        public enum Element : byte
        {
            // Patchouli's elements
            Sun,
            Moon,
            Fire,
            Water,
            Wood,
            Metal,
            Earth,

            // Global additional elements
            Electricity,
            Frost,
            MightyWind
        }

        public enum Multicast : byte
        {
            DoubleSpell,
            TrippleSpell,
            QuadrupleSpell
        }

        public enum Trigger : byte
        {
            ProjectileKill,
            OnTileCollide,
            Timer1,
            Timer2,
            Timer3,
            Timer4,
            Timer5
        }

        public enum Special : byte
        {
            CastEverything,
            RandomCard
        }

        public enum Multiplication : byte
        {
            MultiplyBy2,
            MultiplyBy3,
            MultiplyBy4,
            MultiplyBy5,
            DivideBy2,
            DivideBy3,
            DivideBy4,
            DivideBy5,
            MultiplyDivideRandom // Main.rand.Next();
        }
        #endregion
    }
}