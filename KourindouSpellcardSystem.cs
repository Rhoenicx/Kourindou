// TODO
// => DiggingBolt nullify cooldown effect when last projectile => Move to actual cast code, keep track which projectile card has fired last

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Kourindou.Items.Spellcards;
using static Kourindou.KourindouSpellcardSystem;
using Terraria.ID;

namespace Kourindou
{
    #region CastProperties
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
        public CastBlock(CastBlock castBlock = null)
        {
            if (castBlock != null)
            { 
                this.Repeat = castBlock.Repeat;
                this.Delay = castBlock.Delay;
                this.InitialDelay = castBlock.InitialDelay;
                this.TriggerActive = castBlock.TriggerActive;
                this.TriggerAmount = castBlock.TriggerAmount;
                for (int i = 0; i < castBlock.TriggerInOrder.Count; i++)
                {
                    this.TriggerInOrder.Add(GetCardItem(castBlock.CardItems[i].Group, castBlock.CardItems[i].Spell).Copy(castBlock.CardItems[i]));
                }
                for (int i = 0; i < castBlock.CardItems.Count; i++)
                {
                    this.CardItems.Add(GetCardItem(castBlock.CardItems[i].Group, castBlock.CardItems[i].Spell).Copy(castBlock.CardItems[i]));
                }
                this.IsDisabled = castBlock.IsDisabled;
            }
        }

        public int Repeat { get; set; } = 0;
        public int Delay { get; set; } = 0;
        public int InitialDelay { get; set; } = 0;
        public bool TriggerActive { get; set; } = false;
        public int TriggerAmount { get; set; } = 0;
        public List<CardItem> TriggerInOrder { get; set; } = new List<CardItem>();
        public List<CardItem> CardItems { get; set; } = new List<CardItem>();
        public bool IsDisabled { get; set; } = false;
    }

    // CastInfo 
    public class CastInfo
    {
        public List<CastBlock> Blocks { get; set; } = new List<CastBlock>();
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

        // Chance to not consume cards
        public float ChanceNoConsumeCard = 0f;

        // The actual cast info
        public List<CastInfo> Casts { get; set; } = new List<CastInfo>();
        public List<int> ConsumedCards { get; set; } = new List<int>();
    }
    #endregion

    #region ProjectileProperties
    public class ProjectileInfo
    {
        // Projectile type that needs to be spawned
        public int Type { get; set; }

        // Catalyst 
        public Vector2 CastPosition { get; set; }
        public Vector2 CastVelocity { get; set; }
        public Vector2 CatalystOffset { get; set; }
        public float CatalystSpread { get; set; }

        // Altered positions after applying effects/modifiers
        public Vector2 SpawnPosition { get; set; }
        public Vector2 SpawnVelocity { get; set; }
        public float SpawnSpread { get; set; }

        // Applied modifiers to this projectile
        public List<KeyValuePair<byte, int>> FormationInOrder { get; set; }

        // Stats that need to be send to this projectile.
        public Dictionary<byte, float> ProjectileStats { get; set; }
        public List<Card> TriggerInOrder { get; set; }
        public List<Card> TriggerPayload { get; set; }
    }
    #endregion

    public class KourindouSpellcardSystem
    {
        private static Dictionary<byte, Dictionary<byte, int>> CardTable = new Dictionary<byte, Dictionary<byte, int>>();
        private static List<Card> EntireCardPool = new List<Card>();

        public static void Load()
        {
            CardTable ??= new Dictionary<byte, Dictionary<byte, int>>();
            EntireCardPool ??= new List<Card>();
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
            CardTable ??= new Dictionary<byte, Dictionary<byte, int>>();
            EntireCardPool ??= new List<Card>();

            if (!CardTable.ContainsKey(group))
            {
                CardTable.Add(group, new Dictionary<byte, int>());
            }

            if (!CardTable[group].ContainsKey(spell))
            {
                CardTable[group].Add(spell, CardItemID);
                if (CardItemID == 0)
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Camera);
                }

                EntireCardPool.Add(new Card() { Group = group, Spell = spell });
                return true;
            }

            return false;
        }

        public static Card GetCard(CardItem item)
        {
            return GetCard(item.Group, item.Spell);
        }

        public static Card GetCard(byte _Group, byte _Spell)
        {
            return new Card() { Group = _Group, Spell = _Spell };
        }

        public static CardItem GetCardItem(Card card)
        {
            if (card != null && CardTable.ContainsKey(card.Group)
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

        public static Cast GenerateCast(List<Card> Cards, Card AlwaysCastCard, bool IsCatalyst, int CatalystStartIndex = 0, int CatalystCastAmount = 1, bool Shuffle = false, bool IsVisualized = false)
        {
            return GenerateCast(CardToCardItemList(Cards), GetCardItem(AlwaysCastCard), IsCatalyst, CatalystStartIndex, CatalystCastAmount, Shuffle, IsVisualized);
        }

        public static Cast GenerateCast(List<CardItem> Cards, CardItem AlwaysCastCard, bool IsCatalyst, int CatalystStartIndex = 0, int CatalystCastAmount = 1, bool Shuffle = false, bool IsVisualized = false)
        {
            // Here we generate the cast info which is passed to the execution part
            // => If this catalyst is a shuffle, the cards should first be shuffled.
            // => Be sure to use SetSlotPositions BEFORE casting and/or shuffling!!!

            // Cast List for storing the Cast blocks
            Cast CastProperties = new();

            // Save wrap-around cards
            List<CardItem> CardsWrapReady = new();
            for (int i = 0; i < Cards.Count; i++)
            {
                CardsWrapReady.Add(GetCardItem(Cards[i].Group, Cards[i].Spell).Copy(Cards[i]));
            }

            if (Shuffle)
            {
                ShuffleCardItems(ref CardsWrapReady);
            }
            CardsWrapReady.RemoveRange(CatalystStartIndex, Cards.Count - CatalystStartIndex);

            // Mark the cards as wrap-around when the visualization is active
            if (IsVisualized)
            {
                foreach (CardItem item in CardsWrapReady)
                {
                    item.IsWrapped = true;
                }
            }

            // Setup the AlwaysCastCards
            if (AlwaysCastCard != null && AlwaysCastCard.Group != (byte)Groups.Empty)
            {
                AlwaysCastCard.IsInsertedCard = true;
                AlwaysCastCard.IsAlwaysCast = true;
                AlwaysCastCard.AddCooldown = 0;
                AlwaysCastCard.AddRecharge = 0;
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

            // Multiplication
            float Multiplications = 0;
            CardItem MultiplicationCard = GetCardItem((byte)Groups.Empty, 0);

            // Amount of additional inserted cards counter, 
            // used to determine end index for a next cast
            int InsertedCards = 0;

            // If we are currently wrapping around
            bool IsWrappingAround = false;

            // Check if there is an unended cast
            int CastUnEnded = 0;

            // Save the amount of slots on the catalyst
            int TypicalCatalystCardAmount = Cards.Count;

            // Amount of cards where cooldown or recharge need to be ignored
            int NoCooldownCardAmount = 0;
            int NoRechargeCardAmount = 0;

            // --- Start Casting ---
            while (Index < Cards.Count && CurrentCast < CatalystCastAmount)
            {
                // Create a new cast, for when the catalyst has multiple casts
                if (CastProperties.Casts.ElementAtOrDefault(CurrentCast) == null)
                {
                    CastProperties.Casts.Add(new CastInfo());

                    // If this catalyst has a Always-Cast insert the card to the front of this cast
                    if (AlwaysCastCard != null && AlwaysCastCard.Group != (byte)Groups.Empty)
                    {
                        if (AlwaysCastCard.Group == (byte)Groups.Projectile)
                        {
                            // Projectiles end casts, we don't want this to happen with a projectile as Always-Cast
                            // Increase the multicast amount by 1 so it continues after this projectile.
                            MulticastAmount++;
                        }

                        // Insert the card to the front
                        Cards.Insert(Index, GetCardItem(AlwaysCastCard.Group, AlwaysCastCard.Spell).Copy(AlwaysCastCard));

                        // Since we have inserted a card, we should use continue here,
                        // we want the catalyst to execute this index again (has the new card now).
                        continue;
                    }
                }

                // Create a new castblock
                if (CastProperties.Casts[CurrentCast].Blocks.ElementAtOrDefault(CurrentBlock) == null)
                {
                    CastProperties.Casts[CurrentCast].Blocks.Add(new CastBlock());
                }

                // Multiply this card if needed
                if (Cards[Index].Group != (byte)Groups.Empty && Multiplications > 0f)
                {
                    // Save some comsume stats of this card
                    bool consume = Cards[Index].IsConsumable;
                    int stack = Cards[Index].Item.stack;

                    // Now try to apply the multiplication effect
                    switch ((Groups)Cards[Index].Group)
                    {
                        case Groups.Projectile:
                        case Groups.Special:
                        case Groups.Multicast:
                            {
                                // If the cards found are of the type: Projectile, Special or Multicast:
                                // We cannot apply multiplication effects with the amount property.
                                // Insert a copy of the same card instead
                                for (int y = 1; y < (int)Math.Ceiling(Multiplications); y++)
                                {
                                    if (consume && stack-- <= 0)
                                    {
                                        break;
                                    }

                                    // Insert the same card after this one,
                                    // if the location where this card needs to be placed is empty
                                    if (Cards.ElementAtOrDefault(Index + y) == null)
                                    {
                                        // Add the card instead
                                        Cards.Add(GetCardItem(Cards[Index].Group, Cards[Index].Spell).Copy(Cards[Index]));
                                    }
                                    else
                                    {
                                        // Otherwise insert the card
                                        Cards.Insert(Index + y, GetCardItem(Cards[Index].Group, Cards[Index].Spell).Copy(Cards[Index]));
                                    }

                                    Cards[Index + y].IsInsertedCard = true;
                                }
                            }
                            break;

                        default:
                            {
                                if (CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerActive && MultiplicationCard.Group != (byte)Groups.Empty)
                                {
                                    CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(MultiplicationCard);
                                }
                                else
                                {
                                    if (consume)
                                    {
                                        if ((int)Math.Ceiling(Multiplications) > stack)
                                        {
                                            Multiplications = (float)stack;
                                        }

                                        for (int y = 1; y < (int)Multiplications; y++)
                                        {
                                            CastProperties.ConsumedCards.Add(Cards[Index].SlotPosition);
                                        }
                                    }

                                    Cards[Index].ApplyMultiplication(Multiplications);
                                }

                                MultiplicationCard = GetCardItem((byte)Groups.Empty, 0);
                            }
                            break;
                    }

                    Multiplications = 0f;
                }

                // Replace this random card with a new one
                if (Cards[Index].IsRandomCard)
                {
                    Cards[Index] = GetRandomCard(Cards[Index]);
                }

                // If visualization in enabled mark this card as a payload card
                if (IsVisualized && CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerActive)
                {
                    Cards[Index].IsPayload = true;
                }

                // Count the amount of cards encountered that have been inserted,
                // This is to determine the ending index for the next cast.
                if (!IsWrappingAround && Cards[Index].IsInsertedCard)
                {
                    InsertedCards++;
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
                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);

                            if (CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerAmount > 0)
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerAmount--;
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerActive = true;
                            }
                            else
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerActive = false;

                                if (MulticastAmount > 0)
                                {
                                    MulticastAmount--;
                                    CurrentBlock++;
                                }
                                else
                                {
                                    CurrentCast++;
                                    CurrentBlock = 0;
                                    CastUnEnded = 0;
                                }
                            }
                        }
                        break;

                    case Groups.Multicast:
                        {
                            if (CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerActive)
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerAmount += (int)Math.Ceiling(Cards[Index].GetValue()) - 1;
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                            }
                            else
                            {
                                for (int i = 1; i < (int)Math.Ceiling(Cards[Index].GetValue()); i++)
                                {
                                    if (CastProperties.Casts[CurrentCast].Blocks.ElementAtOrDefault(CurrentBlock + MulticastAmount + i) == null)
                                    {
                                        CastProperties.Casts[CurrentCast].Blocks.Add(new CastBlock(CastProperties.Casts[CurrentCast].Blocks[CurrentBlock]));

                                        // When the visualization setting is enabled, like in the UI mark the copied cards
                                        if (IsVisualized)
                                        {
                                            foreach (CardItem item in CastProperties.Casts[CurrentCast].Blocks[CurrentBlock + MulticastAmount + i].CardItems)
                                            {
                                                item.IsMulticasted = true;
                                            }
                                        }
                                    }
                                }

                                MulticastAmount += (int)Cards[Index].GetValue() - 1;
                            }
                        }
                        break;

                    case Groups.Trigger:
                        {
                            // If a trigger is active, this card should get send as payload
                            if (CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerActive)
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerAmount += (int)Cards[Index].GetValue();
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                            }
                            // No trigger active: During wrap-around we do not want to add triggers.
                            else if (!IsWrappingAround)
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerAmount += (int)Cards[Index].GetValue();
                                for (int i = 0; i < (int)Cards[Index].GetValue(); i++)
                                {
                                    CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerInOrder.Add(Cards[Index]);
                                }
                            }
                        }
                        break;

                    case Groups.Multiplication:
                        {
                            Multiplications += Cards[Index].GetValue();
                            MultiplicationCard = Cards[Index];
                        }
                        break;

                    case Groups.CatalystModifier:
                        {
                            if (CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerActive)
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                            }
                            else
                            {
                                switch ((CatalystModifierVariant)Cards[Index].Variant)
                                {
                                    case CatalystModifierVariant.ReduceRechargePercent:
                                        {
                                            CastProperties.RechargeTimePercentage *= Cards[Index].GetValue();
                                        }
                                        break;

                                    case CatalystModifierVariant.ReduceCooldownPercent:
                                        {
                                            CastProperties.CooldownTimePercentage *= Cards[Index].GetValue();
                                        }
                                        break;

                                    case CatalystModifierVariant.Repeat:
                                        {
                                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].Repeat += (int)Math.Ceiling(Cards[Index].GetValue());
                                        }
                                        break;

                                    case CatalystModifierVariant.Delay:
                                        {
                                            if (CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].Repeat > 0)
                                            {
                                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].Delay += (int)Math.Ceiling(Cards[Index].GetValue());
                                            }
                                            else
                                            {
                                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].InitialDelay += (int)Math.Ceiling(Cards[Index].GetValue());
                                            }
                                        }
                                        break;

                                    case CatalystModifierVariant.Special:
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

                                                case CatalystModifier.NextCardNoCooldown:
                                                    {
                                                        CastProperties.CooldownTime += Cards[Index].AddCooldown;
                                                        NoCooldownCardAmount += (int)Cards[Index].GetValue();
                                                    }
                                                    break;
                                                case CatalystModifier.NextCardNoRecharge:
                                                    {
                                                        CastProperties.RechargeTime += Cards[Index].AddRecharge;
                                                        NoRechargeCardAmount += (int)Cards[Index].GetValue();
                                                    }
                                                    break;

                                                case CatalystModifier.ChanceNoConsume:
                                                    {
                                                        CastProperties.ChanceNoConsumeCard += Cards[Index].GetValue();
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

                                    default:
                                        {
                                            // Just variant 0 => add it
                                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case Groups.Special:
                        {
                            if (CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerActive)
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                            }
                            else
                            {
                                switch ((Special)Cards[Index].Spell)
                                {
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

                // Catalyst only, does not work on projectiles
                if (IsCatalyst)
                {
                    // Apply the card's Cooldown, Recharge and Use times
                    if (Cards[Index].Group != (byte)Groups.Empty)
                    {
                        // Cooldown
                        if (NoCooldownCardAmount <= 0 || (Cards[Index].Group == (byte)Groups.CatalystModifier && Cards[Index].Spell == (byte)CatalystModifier.NextCardNoCooldown))
                        {
                            CastProperties.CooldownTime += Cards[Index].AddCooldown;
                        }
                        else
                        {
                            if (NoCooldownCardAmount >= (int)Math.Ceiling(Cards[Index].Amount))
                            {
                                NoCooldownCardAmount -= (int)Math.Ceiling(Cards[Index].Amount);
                            }
                            else
                            {
                                int Apply = (int)Math.Ceiling(Cards[Index].Amount) - NoCooldownCardAmount;
                                CastProperties.CooldownTime += ((int)(Cards[Index].AddCooldown / Cards[Index].Amount) * Apply);
                                NoCooldownCardAmount = 0;
                            }
                        }

                        // Recharge
                        if (NoRechargeCardAmount <= 0 || (Cards[Index].Group == (byte)Groups.CatalystModifier && Cards[Index].Spell == (byte)CatalystModifier.NextCardNoRecharge))
                        {
                            CastProperties.RechargeTime += Cards[Index].AddRecharge;
                        }
                        else
                        {
                            if (NoRechargeCardAmount >= (int)Math.Ceiling(Cards[Index].Amount))
                            {
                                NoRechargeCardAmount -= (int)Math.Ceiling(Cards[Index].Amount);
                            }
                            else
                            {
                                int Apply = (int)Math.Ceiling(Cards[Index].Amount) - NoRechargeCardAmount;
                                CastProperties.RechargeTime += ((int)(Cards[Index].AddRecharge / Cards[Index].Amount) * Apply);
                                NoRechargeCardAmount = 0;
                            }
                        }

                        // UseTime
                        CastProperties.AddUseTime += Cards[Index].AddUseTime;
                    }

                    // Increase UnEnded if this card needs a projectile to work
                    // The cast is 'Unended' which means it can trigger a wrap-around.
                    if (Cards[Index].NeedsProjectileCard)
                    {
                        CastUnEnded++;
                    }

                    // If this card is a consumable save the consumption
                    if (Cards[Index].IsConsumable)
                    {
                        CastProperties.ConsumedCards.Add(Cards[Index].SlotPosition);
                    }

                    // Check if a wrap-around is needed
                    if (!IsWrappingAround && Index == Cards.Count - 1 && (TriggerActive || Triggers > 0 || MulticastAmount > 0 || CastUnEnded > 0))
                    {
                        // Now add the wrap-around cards
                        Cards.AddRange(CardsWrapReady);

                        // Since the only way we're wrapping-around is because the 
                        // end of the cards on the catalyst has been reached,
                        // the catalyst needs to go on cooldown
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
                Index++;
            }

            // Determine the start index of the next potential cast
            CastProperties.NextCastStartIndex = Index - InsertedCards;

            // If the catalyst is not set on cooldown by the wrap-around,
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
            for (int _CastInfo = CastProperties.Casts.Count - 1; _CastInfo >= 0; _CastInfo--)
            {
                for (int _CastBlock = CastProperties.Casts[_CastInfo].Blocks.Count - 1; _CastBlock >= 0; _CastBlock--)
                {
                    // Remove CastBlocks if it has no projectile that can be fired by the catalyst
                    if (IsCatalyst)
                    {
                        if (!CastProperties.Casts[_CastInfo].Blocks[_CastBlock].CardItems.Any(_CardItem => _CardItem.Group == (byte)Groups.Projectile && _CardItem.Spell != (byte)Projectile.MyOwnProjectileInstance))
                        {
                            if (IsVisualized)
                            {
                                CastProperties.Casts[_CastInfo].Blocks[_CastBlock].IsDisabled = true;
                            }
                            else
                            {
                                CastProperties.Casts[_CastInfo].Blocks.RemoveAt(_CastBlock);
                            }
                        }
                    }
                    else
                    {
                        if (!CastProperties.Casts[_CastInfo].Blocks[_CastBlock].CardItems.Any(_CardItem => _CardItem.Group == (byte)Groups.Projectile))
                        {
                            if (IsVisualized)
                            {
                                CastProperties.Casts[_CastInfo].Blocks[_CastBlock].IsDisabled = true;
                            }
                            else
                            {
                                CastProperties.Casts[_CastInfo].Blocks.RemoveAt(_CastBlock);
                            }
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
            if (IsVisualized)
            {
                CastProperties.FailedToCast = true;

                foreach (CastInfo info in CastProperties.Casts)
                {
                    if (info.Blocks.Any(_block => !_block.IsDisabled))
                    {
                        CastProperties.FailedToCast = false;
                        break;
                    }
                }
            }
            else
            {
                if (CastProperties.Casts.Count <= 0)
                {
                    CastProperties.FailedToCast = true;
                }
            }


            // Determine the MinimumUseTime
            if (!CastProperties.FailedToCast)
            {
                foreach (CastInfo _CastInfo in CastProperties.Casts)
                {
                    foreach (CastBlock _CastBlock in _CastInfo.Blocks)
                    {
                        if ((_CastBlock.Repeat * _CastBlock.Delay) + _CastBlock.InitialDelay > CastProperties.MinimumUseTime)
                        {
                            CastProperties.MinimumUseTime = (_CastBlock.Repeat * _CastBlock.Delay) + _CastBlock.InitialDelay;
                        }
                    }
                }
            }

            // Cast properties done!
            return CastProperties;
        }

        public static void ConsumedCards(ref List<CardItem> Cards, List<int> Consumed, float chance)
        {
            foreach (int i in Consumed)
            {
                if (i >= 0 && i < Cards.Count && Cards[i].IsConsumable && !Cards[i].IsInsertedCard && Main.rand.NextFloat(0f, 1f) > chance)
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

        public static CardItem GetRandomCard(CardItem ToBeReplacedCard)
        {
            int RetryCounter = 0;
            bool Valid = false;
            CardItem NewCard = GetCardItem((byte)Groups.Empty, 0);

            while (RetryCounter < 1000 && !Valid)
            {
                if (ToBeReplacedCard.Group == (byte)Groups.Special && ToBeReplacedCard.Spell == (byte)Special.RandomCard)
                {
                    NewCard = GetCardItem(EntireCardPool[Main.rand.Next(0, EntireCardPool.Count)]);
                    if (NewCard != null && !NewCard.IsRandomCard && NewCard.Group != (byte)Groups.Special && NewCard.Spell != (byte)Special.RandomCard)
                    {
                        Valid = true;
                    }
                }
                else
                {
                    NewCard = GetCardItem(ToBeReplacedCard.Group, (byte)ToBeReplacedCard.GetValue());
                    if (NewCard != null && !NewCard.IsRandomCard && NewCard.Spell != ToBeReplacedCard.Spell)
                    {
                        Valid = true;
                    }
                }
                RetryCounter++;
            }

            if (Valid)
            {
                NewCard.Amount = ToBeReplacedCard.Amount;
                NewCard.IsConsumable = ToBeReplacedCard.IsConsumable;
                NewCard.IsInsertedCard = ToBeReplacedCard.IsInsertedCard;
                NewCard.SlotPosition = ToBeReplacedCard.SlotPosition;
                NewCard.IsWrapped = ToBeReplacedCard.IsWrapped;
                NewCard.IsPayload = ToBeReplacedCard.IsPayload;
                NewCard.IsMulticasted = ToBeReplacedCard.IsMulticasted;
            }

            return NewCard;
        }

        #region Enumerators
        #region Card_Types
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
            ShotBlue,
            //DiggingBolt,
            MyOwnProjectileInstance
        }

        public enum Formation : byte                                                                    // GETVALUE
        {                                                                                               // 
            // Default straight line                                                                    // 
            OnlyForward,                                                                                // 1f (Cannot be multiplied)
            ForwardAndSide,                                                                             // 1f (Cannot be multiplied)
            ForwardAndBack,
            OnlySides,                                                                                  // 
                                                                                                        // Duplicates projectile and add spread, like a shotgun                                     // 
            DoubleScatter, // 2                                                                         // 2f * AMOUNT
            TripleScatter, // 3                                                                         // 3f * AMOUNT
            QuadrupleScatter, // 4                                                                      // 4f * AMOUNT
            QuintupleScatter, // 5                                                                      // 5f * AMOUNT
            ShotgunScatter,                                                                             // Main.rand.Next(2, 6) * AMOUNT
                                                                                                        // 
                                                                                                        // Duplicates projectile and place them side-by-side                                        // 
            DoubleFork, // 2                                                                            // 2f * AMOUNT
            TripleFork, // 3                                                                            // 3f * AMOUNT
            QuadrupleFork, // 4                                                                         // 4f * AMOUNT
            QuintupleFork, // 5                                                                         // 5f * AMOUNT
            RandomFork,                                                                                 // Main.rand.Next(2, 6) * AMOUNT
                                                                                                        // 
                                                                                                        // Duplicates projectile and cast it in a circle around the player                          // 
            Quadragon, // 4                                                                             // 4f * AMOUNT
            Pentagon, // 5                                                                              // 5f * AMOUNT
            Hexagon, // 6                                                                               // 6f * AMOUNT
            Heptagon, // 7                                                                              // 7f * AMOUNT
            Octagon, // 8                                                                               // 8f * AMOUNT
            Randagon, // Main.rand.Next(4, 9); (is not random card)                                     // Main.rand.Next(2, 9) * AMOUNT

            RandomFormation
        }                                                                                               // 
                                                                                                        // 
        public enum FormationVariant : byte                                                             // 
        {                                                                                               // 
            None,                                                                                       // 
            Scatter,                                                                                    // 
            Fork,                                                                                       // 
            SomethingGon                                                                                // 
        }                                                                                               // 
                                                                                                        // 
        public enum Trajectory : byte                                                                   // 
        {                                                                                               // 
            Straight,
            ArcLeft,                                                                                    // 1f * AMOUNT
            ArcRight,                                                                                   // 1f * AMOUNT
            ZigZag,                                                                                     // 1f * AMOUNT
            PingPong,                                                                                   // 1f * AMOUNT
            Snake,                                                                                      // 1f * AMOUNT
            Uncontrolled,                                                                               // 1f * AMOUNT
            Orbit,                                                                                      // 1f * AMOUNT
            Spiral,                                                                                     // 1f * AMOUNT
            Boomerang,                                                                                  // 1f * AMOUNT
            Aiming,                                                                                     // 1f * AMOUNT
            Daedalus, //TODO: needs to be a formation                                                   // 1f * AMOUNT
            RandomTrajectory                                                                            // 1f * AMOUNT
        }                                                                                               // 
                                                                                                        // 
        public enum ProjectileModifier : byte                                                           // 
        {                                                                                               // 
            // Acceleration => Amount of percent of base speed added each second                        // 
            Acceleration, // 1f (percent)                                                               // 0.1f * AMOUNT
            Deceleration, // 1f (percent)                                                               // 0.1f * AMOUNT
                          //
                          // Acceleration Multipier => Amount of percent current speed is changed                     //
            AccelerationMultiplier,                                                                     // 0.1f * AMOUNT
            DecelerationMultiplier,                                                                     // 0.1f * AMOUNT
                                                                                                        //
                                                                                                        // Speed => Increases base speed by a percentage                                            // 
            IncreaseSpeed, // 1f (percent)                                                              // 0.1f * AMOUNT
            DecreaseSpeed, // 1f (percent)                                                              // 0.1f * AMOUNT
            Stationary, // 0f                                                                           // Set base speed to 0f
                        //
                        // Tile bounces                                                                             // 
            BounceUp, //+1                                                                              // 
            BounceDown, //-1                                                                            // 
            FallingFlat, //0                                                                            // 
            A_Terrible_Idea, // Infinite bounces                                                         // 
                             // 
                             // Penetration stats                                                                        // 
            PenetrationUp, //+1                                                                         // 
            PenetrationDown, // > 1 : -1                                                                // 
            PenetrationZero, //0                                                                        // 
            PenetrationAll, //-1                                                                        // 
                            // 
                            // Gravity                                                                                  // 
            Gravity,                                                                                    // 
            NoGravity,                                                                                  // 
            AntiGravity,                                                                                // 
                                                                                                        // 
                                                                                                        // Homing                                                                                   // 
            Homing,                                                                                     // 
            RotateToEnemy,                                                                              // 
                                                                                                        // 
                                                                                                        // Tile collision                                                                           // 
            Ghosting,                                                                                   // 
            Collide,                                                                                    // 
            Phasing,                                                                                    // 
                                                                                                        // 
                                                                                                        // Lifetime                                                                                 // 
            LifetimeUp, // 0.5f                                                                         // 
            LifetimeDown, // -0.5f                                                                      // 
            InstantKill, // set to 0                                                                    // 
                         // 
                         // Knockback                                                                                // 
            IncreaseKnockback,                                                                          // 
            DecreaseKnockback,                                                                          // 
            NoKnockback,                                                                                // 
                                                                                                        // 
                                                                                                        // Damage
            DamageUp,
            DamageDown,

            // Rotation                                                                                 // 
            RotateLeft90, // -90f                                                                       // 
            RotateRight90, // -90f                                                                      // 
            RotateLeft45, // -45f                                                                       // 
            RotateRight45, // 45f                                                                       // 
            RotateLeft22_5, // -22.5f                                                                   // 
            RotateRight22_5, // 22.5f                                                                   // 
            RandomRotation,                                                                             // 
                                                                                                        // 
                                                                                                        // Size                                                                                     // 
            Snowball, // Projectile increase in size the longer it travels                              // 
            IncreaseSize, // 0.25f - Saki effect                                                        // 
            DecreaseSize, // -0.25f                                                                     // 
                          // 
                          // Confined                                                                                 // 
            BounceOnScreenEdge, // Projectile bounces at the side of the screen                         // Return 1f
            WarpOnScreenEdge, // Yukari Screen border                                                   // Return 1f
                              // 
                              // Seija                                                                                    // 
            VectorReversal, // reverses velocity, also flips team                                       // Return 1f
                            // 
                            // Cirno                                                                                    // 
            Shatter,                                                                                    // 
                                                                                                        // 
                                                                                                        // Keiki                                                                                    // 
            ProjectileEater,                                                                            // 
                                                                                                        // 
                                                                                                        // Patchouli                                                                                // 
            MagicCircleAura,                                                                            // 
                                                                                                        // 
                                                                                                        // Suwako                                                                                   // 
            Explosion,                                                                                  //
                                                                                                        //
                                                                                                        // Other                                                                                    // 
            AngryBullets,                                                                               // also hostile
            Luminescence,                                                                               // Light intensity
            CriticalPlus,
            //
            // Random                                                                                   // 
            RandomProjectileModifier                                                                    // 
        }                                                                                               // 
                                                                                                        // 
        public enum CatalystModifier : byte                                                             // 
        {                                                                                               //
            // Spread - Reduces spread on the wand                                                      //
            DecreaseSpread, //-30f                                                                      // 
            IncreaseSpread, // +30f                                                                     // 
            Sniper, // no spread for this castblock                                                 //
                    //
                    // Position where the projectiles are spawned                                               // 
            DistantCast,                                                                                // 128f * AMOUNT
            TeleportCast,                                                                               // Cast => OnCursor
            ReverseCast,                                                                                // Cast => Far away + rotated
                                                                                                        // 
                                                                                                        // reduces cooldown percentage-based (Clownpiece - Life-Burning torch)                      // 
                                                                                                        // Variant 1                                                                                // 
            ReduceCooldown10Percent, // 0.9f                                                            // 0.90f ^ AMOUNT
            ReduceCooldown25Percent, // 0.75f                                                           // 0.75f ^ AMOUNT
            ReduceCooldown50Percent, // 0.5f                                                            // 0.50f ^ AMOUNT
            RandomCooldownPercent, // Main.rand.NextFloat();                                            // Main.rand.NextFloat(0.5, 1.0) ^ AMOUNT
                                   // 
                                   // reduces recharge percentage-based                                                        // 
                                   // Variant 2                                                                                // 
            ReduceRecharge10Percent, // 0.9f                                                            // 0.90f ^ AMOUNT
            ReduceRecharge25Percent, // 0.75f                                                           // 0.75f ^ AMOUNT
            ReduceRecharge50Percent, // 0.5f                                                            // 0.50f ^ AMOUNT
            RandomRechargePercent, // Main.rand.NextFloat();                                            // Main.rand.NextFloat(0.5, 1.0) ^ AMOUNT
                                   // 
                                   // Repeats                                                                                  // 
                                   // Variant 3                                                                                // 
            Repeat1, //1
            Repeat2, //2                                                                                // 2f
            Repeat3, //3                                                                                // 3f
            Repeat4, //4                                                                                // 4f
            Repeat5, //5                                                                                // 5f
            RepeatRandom, // Main.rand.Next();                                                          // Main.rand.NextFloat(2, 6)
                          // 
                          // Delays in ticks                                                                          // 
                          // Variant 4                                                                                // 
            DelayCastBy1, //60                                                                          // 
            DelayCastBy2, //120                                                                         // 
            DelayCastBy3, //180                                                                         // 
            DelayCastBy4, //240                                                                         // 
            DelayCastBy5, //300                                                                         // 
            DelayCastRandom, // Main.rand.Next();                                                       // 
                             //
                             // Eliminates waiting time                                                                  // 
                             // Variant 5                                                                                // 
            EliminateCooldown, //cooldown=0                                                             // Cast => CooldownOverride
            EliminateRecharge, //recharge=0                                                             // Cast => RechargeOverride
            NextCardNoCooldown,                                                                         // 1f * AMOUNT
            NextCardNoRecharge,                                                                         // 1f * AMOUNT
            ChanceNoConsume,                                                                            // 0.075f * AMOUNT
                                                                                                        //
        }                                                                                               // 
                                                                                                        // 
        public enum CatalystModifierVariant : byte                                                      // 
        {                                                                                               // 
            None,                                                                                       // 
            ReduceCooldownPercent,                                                                      // 
            ReduceRechargePercent,                                                                      // 
            Repeat,                                                                                     // 
            Delay,                                                                                      //
            Special                                                                                     // 
        }                                                                                               // 
                                                                                                        // 
        public enum Element : byte                                                                      // 
        {                                                                                               // 
            // Patchouli's elements                                                                     // 
            Sun,                                                                                        // 
            Moon,                                                                                       // 
            Fire,                                                                                       // 
            Water,                                                                                      // 
            Wood,                                                                                       // 
            Metal,                                                                                      // 
            Earth,                                                                                      // 
                                                                                                        // 
                                                                                                        // Global additional elements                                                               // 
            Electricity,                                                                                // 
            Frost,                                                                                      // 
            MightyWind,                                                                                 // 
            Poison,
            Magic
        }                                                                                               // 
                                                                                                        // 
        public enum Multicast : byte                                                                    // 
        {                                                                                               // 
            DoubleCast,                                                                                 // 2f
            TripleCast,                                                                                // 3f
            QuadrupleCast                                                                               // 4f
        }                                                                                               // 
                                                                                                        // 
        public enum Trigger : byte                                                                      // 
        {                                                                                               // 
            Trigger,                                                                                    // 
            Timer1,                                                                                     // 1f
            Timer2,                                                                                     // 2f
            Timer3,                                                                                     // 3f
            Timer4,                                                                                     // 4f
            Timer5                                                                                      // 5f
        }                                                                                               // 
                                                                                                        // 
        public enum Special : byte                                                                      // 
        {                                                                                               // 
            CastEverything,                                                                             // 1f
            RandomCard                                                                                  // 1f
        }                                                                                               // 
                                                                                                        // 
        public enum Multiplication : byte                                                               // 
        {                                                                                               // 
            MultiplyBy2,                                                                                // 2f   
            MultiplyBy3,                                                                                // 3f   
            MultiplyBy4,                                                                                // 4f   
            MultiplyBy5,                                                                                // 5f   
            DivideBy2,                                                                                  // 0.5f 
            DivideBy3,                                                                                  // 0.33f
            DivideBy4,                                                                                  // 0.25f
            DivideBy5,                                                                                  // 0.20f
            MultiplyDivideRandom                                                                        // Main.rand.NextFloat(0.2f, 5f);      
        }
        #endregion

        #region Projectile_Stats
        public enum ProjectileStats : byte
        {
            // Trajectories
            StraightTrigger,
            ArcLeftAmount,
            ArcRightAmount,
            ZigZagAmount,
            PingPongAmount,
            SnakeAmount,
            UncontrolledAmount,
            OrbitAmount,
            SpiralAmount,
            BoomerangAmount,
            AimingAmount,

            // Modifiers
            AccelerationAmount,                                 //
            AccelerationMultiplierAmount,                       //
            SpeedAmount,                                        // => One Time SET
            SpeedNone,                                          // => One Time SET
            BounceAmount,                                       //
            BounceNone,                                         //
            BounceInfinite,                                     //
            PenetrationAmount,                                  // => One Time SET in Defaults
            PenetrationNone,                                    // => One Time SET in Defaults
            PenetrationInfinite,                                // => One Time SET in Defaults
            GravityAmount,                                      //
            HomingEnabled,                                      //
            RotateToEnemyEnabled,                               //
            CollisionEnabled,                                   // => One Time SET in Defaults
            PhasingEnabled,                                     //
            LifeTimeAmount,                                     // => One Time SET in Defaults
            LifeTimeNone,                                       // => One Time SET in Defaults
            KnockbackAmount,                                    // => One Time SET in Defaults
            KnockbackNone,                                      // => One Time SET in Defaults
            DamageAmount,                                       // => One Time SET in Defaults
            RotationAmount,                                     // => One Time SET
            SnowballEnabled,                                    //
            SizeAmount,                                         // => One Time SET in Defaults
            OnScreenEdgeType,                                   //
            VectorReversalEnabled,                              // => One Time SET
            ShatterEnabled,                                     //
            ProjectileEaterEnabled,                             //
            MagicCircleAuraEnabled,                             //
            ExplosionEnabled,                                   //
            HostileOrFriendly,                                  // => One Time SET in Defaults
            LuminescenceEnabled,                                //
            CritrateAmount,                                     // => One Time SET in Defaults
            ElementType                                         //
        }

        #endregion
        #endregion
    }
}