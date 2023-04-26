// TODO
// => DiggingBolt nullify cooldown effect when last projectile => Move to actual cast code, keep track which projectile card has fired last

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Kourindou.Items.Spellcards;
using Kourindou.Items.Spellcards.Empty;
using Kourindou.Projectiles;
using Kourindou.Items.Catalysts;
using Kourindou.Items.Spellcards.CatalystModifiers;
using Kourindou.Items.Spellcards.Formations;
using System.Reflection;

namespace Kourindou
{
    #region CastProperties
    // Individual card, this is how the cards will look like when they get passed around on the network
    // And how they are saved on the player and catalyst
    // Cast block info
    public class CastBlock
    {
        public CastBlock(bool root = false)
        {
            if (root)
            { 
                IsRoot = root;
            }
        }

        public CastBlock Clone(bool MultiCast = false)
        { 
            CastBlock block = new CastBlock();
            block.Parent = this.Parent;
            block.RepeatAmount = this.RepeatAmount;
            block.TriggerAmount = this.TriggerAmount;
            block.ProjectileCounter = this.ProjectileCounter;
            block.SkipCounting = this.SkipCounting;
            block.Delay = this.Delay;
            block.Timer = this.Timer;
            block.IsPayload = this.IsPayload;
            block.TriggerID = this.TriggerID;

            for (int i = 0; i < TriggerCards.Count; i++)
            {
                block.TriggerCards.Add((CardItem)TriggerCards[i].Item.Clone().ModItem);
            }

            for (int i = 0; i < Cards.Count; i++)
            {
                block.AddCard((CardItem)Cards[i].Item.Clone().ModItem);
                if (MultiCast)
                {
                    block.Cards[^1].IsMulticasted = true;
                }
            }

            return block;
        }

        public void AddChild(CastBlock block, bool Payload = false)
        {
            if (Children == null)
            {
                Children = new();
            }

            block.Parent = this;

            if (Payload)
            {
                for (int i = 0; i < block.Cards.Count; i++)
                {
                    block.Cards[i].IsPayload = true;
                }

                block.IsPayload = Payload;
            }

            Children.Add(block);
        }

        public void AddCard(CardItem item)
        {
            if (IsPayload)
            {
                item.IsPayload = true;
            }

            Cards.Add(item);
        }

        // Parent
        public CastBlock Parent;
        public bool HasParent => Parent != null;

        // Children
        public List<CastBlock> Children;
        public bool HasChildren => Children != null && Children.Count > 0;

        // Cards
        public List<CardItem> Cards { get; set; } = new List<CardItem>();

        // Flags
        public bool IsPayload = false;

        // Repeat Amount
        private int _RepeatAmount = 0;
        public int RepeatAmount
        { 
            get => _RepeatAmount;
            set 
            { 
                _RepeatAmount = value;
                SkipCounting = false;
            }
        }

        // Trigger amount
        private int _TriggerAmount = 0;
        public int TriggerAmount
        {
            get => _TriggerAmount;
            set
            {
                _TriggerAmount = value;
                SkipCounting = false;
            }
        }
        public List<CardItem> TriggerCards = new List<CardItem>();

        // Projectile Counter
        private int _ProjectileCounter = 1;
        public int ProjectileCounter
        {
            get => _ProjectileCounter;
            set
            {
                _ProjectileCounter = value;
                SkipCounting = false;
            }
        }

        // Counting mechanism
        public bool SkipCounting = true;
        public bool UsedForCounting = false;
        public bool ExecutedCalculation = false;

        // Disabled
        public bool IsDisabled = false;

        // Finished
        public bool Finished = false;

        // Other
        public int Delay = 0;
        public int Timer = 0;
        public bool IsRoot = false;
        public int TriggerID = -1;
        public bool TriggerActivated = false;
    }

    // Cast Properties
    public class Cast
    {
        // Catalyst stats after cast
        public bool MustGoOnCooldown = false;
        public bool FailedToCast = false;
        public int NextCastStartIndex = 0;

        // Cooldown after everything has fired => Catalyst reset time
        public bool CooldownOverride = false;
        public int CooldownTime = 0;
        public float CooldownTimePercentage = 1f;

        // Recharge time between casts
        public bool RechargeOverride = false;
        public int RechargeTime = 0;
        public float RechargeTimePercentage = 1f;

        // Additional UseTime, might be handy?
        public int AddUseTime = 0;

        // Minimum time the catalyst needs to be used
        // for repeats+cast delays
        public int MinimumUseTime = 0;

        // Chance to not consume cards
        public float ChanceNoConsumeCard = 1f;

        // Amount of projectiles inside this Cast
        public int ProjectileAmount = 0;

        // The actual cast info
        public CastBlock RootBlock = new(true);
        public List<int> ConsumedCards = new List<int>();
    }
    #endregion

    public class KourindouSpellcardSystem
    {
        private static Dictionary<byte, Dictionary<byte, int>> CardTable = new Dictionary<byte, Dictionary<byte, int>>();
        private static List<byte[]> EntireCardPool = new List<byte[]>();

        public static void Load()
        {
            CardTable ??= new Dictionary<byte, Dictionary<byte, int>>();
            EntireCardPool ??= new List<byte[]>();
        }

        public static void Unload()
        {
            CardTable.Clear();
            CardTable = null;
            EntireCardPool.Clear();
            EntireCardPool = null;
        }

        public static bool RegisterCardItem(byte Group, byte Spell, int CardItemID)
        {
            CardTable ??= new Dictionary<byte, Dictionary<byte, int>>();
            EntireCardPool ??= new List<byte[]>();

            if (!CardTable.ContainsKey(Group))
            {
                CardTable.Add(Group, new Dictionary<byte, int>());
            }

            if (!CardTable[Group].ContainsKey(Spell))
            {
                CardTable[Group].Add(Spell, CardItemID);
                if (CardItemID == 0)
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Camera);
                }

                EntireCardPool.Add(new byte[2] { Group, Spell });
                return true;
            }

            return false;
        }

        public static CardItem GetCardItem(byte _Group, byte _Spell, int Stack = 0)
        {
            if (CardTable.ContainsKey(_Group)
                && CardTable[_Group].ContainsKey(_Spell)
                && new Item(CardTable[_Group][_Spell]).ModItem is CardItem item)
            {
                // Only if there is a stack given assign it as a consumable card.
                if (Stack > 0 && item.IsConsumable)
                {
                    item.Item.stack = Stack;
                }
                else
                {
                    item.IsConsumable = false;
                }

                return item;
            }

            return new EmptyCard();
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

        public static void SetSlotPositions(ref List<CardItem> Cards)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                Cards[i].SlotPosition = i;
            }
        }

        public static Cast GenerateCast(List<CardItem> Cards, CardItem AlwaysCastCard, bool IsCatalyst, int CatalystStartIndex = 0, int CatalystCastAmount = 1)
        {
            // Here we generate the cast info which is passed to the execution part
            // => If this catalyst is a shuffle, the cards should first be shuffled.
            // => Be sure to use SetSlotPositions BEFORE casting and/or shuffling!!!

            // Cast List for storing the Cast blocks
            Cast CastProperties = new();

            // Create a new List with Cards and reset them to defaults, only when this is not the catalyst
            if (IsCatalyst)
            {
                for (int i = 0; i < Cards.Count; i++)
                {
                    Cards[i].SetDefaults();
                }
            }

            // Save wrap-around cards
            List<CardItem> CardsWrapReady = new();
            for (int i = 0; i < CatalystStartIndex; i++)
            {
                CardsWrapReady.Add(Cards[i]);
                CardsWrapReady[^1].IsWrapped = true;
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

            // Current cast and block
            bool AddNewBlock = true;
            int CurrentCast = 0;
            CastBlock CurrentBlock = new CastBlock();
            CurrentBlock = CastProperties.RootBlock;

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

            // --- Start Casting --- //
            while (Index < Cards.Count && CurrentCast < CatalystCastAmount)
            {
                if (AddNewBlock)
                {
                    CastBlock NewBlock = new CastBlock();
                    CurrentBlock.AddChild(NewBlock);
                    CurrentBlock = NewBlock;

                    if (AlwaysCastCard != null && AlwaysCastCard.Group != (byte)Groups.Empty)
                    {
                        if (AlwaysCastCard.Group == (byte)Groups.Projectile && CurrentBlock.HasParent)
                        {
                            CurrentBlock.Parent.AddChild(CurrentBlock.Clone());
                        }

                        Cards.Insert(Index, AlwaysCastCard);
                    }

                    AddNewBlock = false;
                }

                // Multiply this card if needed
                if (Cards[Index].Group != (byte)Groups.Empty && Multiplications > 0f)
                {
                    // Save some comsume stats of this card
                    bool consume = Cards[Index].IsConsumable;
                    int stack = Cards[Index].Item.stack;

                    // Now try to apply the multiplication effect
                    if (!Cards[Index].CanBeMultiplied)
                    {
                        // If the cards found are of the type: Projectile, Special or Multicast:
                        // We cannot apply multiplication effects with the amount property.
                        // Insert a copy of the same card instead
                        for (int y = 1; y < GetFlooredValue(Multiplications); y++)
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
                                Cards.Add((CardItem)Cards[Index].Item.Clone().ModItem);
                            }
                            else
                            {
                                // Otherwise insert the card
                                Cards.Insert(Index + y, (CardItem)Cards[Index].Item.Clone().ModItem);
                            }

                            Cards[Index + y].IsInsertedCard = true;
                        }
                    }
                    else
                    {
                        if (consume && GetFlooredValue(Multiplications) > stack)
                        {
                            Multiplications = (float)stack;
                        }

                        Cards[Index].ApplyMultiplication(Multiplications);
                    }

                    MultiplicationCard = GetCardItem((byte)Groups.Empty, 0);
                    Multiplications = 0f;
                }

                // Replace this random card with a new one
                if (Cards[Index].IsRandomCard)
                {
                    Cards[Index] = GetRandomCard(Cards[Index]);
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
                            // Cast generation
                            CurrentBlock.AddCard(Cards[Index]);

                            if (!CurrentBlock.ExecutedCalculation)
                            {
                                if (CurrentBlock.RepeatAmount > 0)
                                {
                                    CurrentBlock.ProjectileCounter *= CurrentBlock.RepeatAmount + 1;
                                }

                                if (CurrentBlock.TriggerAmount > 0)
                                {
                                    for (int i = 0; i < CurrentBlock.TriggerAmount; i++)
                                    {
                                        CurrentBlock.AddChild(new CastBlock() { ProjectileCounter = CurrentBlock.ProjectileCounter, TriggerID = i }, true);
                                    }
                                }

                                CurrentBlock.ExecutedCalculation = true;
                            }

                            // Flag as non-skip
                            CurrentBlock.SkipCounting = false;

                            // If there is an unfished block connected to this block
                            // Move to the first connected block
                            int MaxSteps = 0;
                            while (MaxSteps++ < 100)
                            {
                                if (CurrentBlock.Children != null && CurrentBlock.Children.Any(b => !b.Finished))
                                {
                                    // Move to the first unfinished block
                                    CurrentBlock = CurrentBlock.Children.First(b => !b.Finished);
                                    break;
                                }

                                // Mark this block as finished
                                CurrentBlock.Finished = true;

                                // Now check if we're going back to the root block,
                                // if this is the case, break instead.
                                if (CurrentBlock.HasParent)
                                {
                                    CurrentBlock = CurrentBlock.Parent;
                                }
                                else
                                {
                                    AddNewBlock = true;
                                    CurrentCast++;
                                    CastUnEnded = 0;
                                    break;
                                }
                            }
                        }
                        break;

                    case Groups.Multicast:
                        {
                            if (CurrentBlock.HasParent)
                            {
                                for (int i = 1; i < GetFlooredValue(Cards[Index].GetValue(), 1); i++)
                                {
                                    CurrentBlock.Parent.AddChild(CurrentBlock.Clone(true));
                                }
                            }
                        }
                        break;

                    case Groups.Trigger:
                        {
                            if (!IsWrappingAround)
                            {
                                if (Cards[Index].Spell == (byte)Trigger.Trigger)
                                {
                                    CurrentBlock.TriggerAmount += GetFlooredValue(Cards[Index].GetValue(), 1);
                                    for (int i = 0; i < GetFlooredValue(Cards[Index].GetValue(), 1); i++)
                                    {
                                        CurrentBlock.TriggerCards.Add(Cards[Index]);
                                    }
                                }
                                else
                                {
                                    CurrentBlock.TriggerAmount += 1;
                                    CurrentBlock.TriggerCards.Add(Cards[Index]);
                                    CurrentBlock.UsedForCounting = true;
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
                                        CurrentBlock.RepeatAmount += GetRoundedValue(Cards[Index].GetValue(), 1);
                                    }
                                    break;

                                case CatalystModifierVariant.Delay:
                                    {
                                        int value = GetRoundedValue(Cards[Index].GetValue(), 0);

                                        if (CurrentBlock.RepeatAmount <= 0)
                                        {
                                            CurrentBlock.Timer += value;
                                        }

                                        CurrentBlock.Delay += value;
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
                                                    NoCooldownCardAmount += GetRoundedValue(Cards[Index].GetValue(), 1);
                                                }
                                                break;
                                            case CatalystModifier.NextCardNoRecharge:
                                                {
                                                    CastProperties.RechargeTime += Cards[Index].AddRecharge;
                                                    NoRechargeCardAmount += GetRoundedValue(Cards[Index].GetValue(), 1);
                                                }
                                                break;

                                            case CatalystModifier.ChanceNoConsume:
                                                {
                                                    CastProperties.ChanceNoConsumeCard *= Cards[Index].GetValue();
                                                }
                                                break;

                                            default:
                                                {
                                                    CurrentBlock.AddCard(Cards[Index]);
                                                }
                                                break;
                                        }
                                    }
                                    break;

                                default:
                                    {
                                        CurrentBlock.AddCard(Cards[Index]);
                                    }
                                    break;
                            }
                        }
                        break;

                    case Groups.Special:
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
                                        CurrentBlock.AddCard(Cards[Index]);
                                    }
                                    break;
                            }
                        }
                        break;


                    case Groups.Trajectory:
                        {
                            CurrentBlock.AddCard(Cards[Index]);
                            CurrentBlock.ProjectileCounter *= GetFlooredValue(Cards[Index].GetValue(), 1);
                        }
                        break;

                    default:
                        {
                            // Any other card
                           CurrentBlock.AddCard(Cards[Index]);
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
                            if (NoCooldownCardAmount >= GetRoundedValue(Cards[Index].Amount, 1))
                            {
                                NoCooldownCardAmount -= GetRoundedValue(Cards[Index].Amount, 1);
                            }
                            else
                            {
                                int Apply = GetRoundedValue(Cards[Index].Amount, 1) - NoCooldownCardAmount;
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
                            if (NoRechargeCardAmount >= GetRoundedValue(Cards[Index].Amount, 1))
                            {
                                NoRechargeCardAmount -= GetRoundedValue(Cards[Index].Amount, 1);
                            }
                            else
                            {
                                int Apply = GetRoundedValue(Cards[Index].Amount, 1) - NoRechargeCardAmount;
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
                        for (int i = 0; i < GetFlooredValue(Cards[Index].Amount, 1); i++)
                        {
                            CastProperties.ConsumedCards.Add(Cards[Index].SlotPosition);
                        }
                    }

                    // Check if a wrap-around is needed
                    if (!IsWrappingAround && Index == Cards.Count - 1 && (!CurrentBlock.Finished || CastUnEnded > 0))
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
                        if (CurrentBlock.TriggerAmount > 0)
                        {
                            CurrentBlock.TriggerAmount = 0;
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

            // Projectile amounts
            CastProperties.ProjectileAmount += CountProjectiles(CastProperties.RootBlock);

            // Disable Child blocks which have invalid starting projectiles
            if (CastProperties.RootBlock.HasChildren)
            {
                foreach (CastBlock block in CastProperties.RootBlock.Children)
                {
                    if (!block.Cards.Any(x => x.Group == (byte)Groups.Projectile)
                        || (block.Cards.Any(x => x.Group == (byte)Groups.Projectile)
                        && block.Cards.First(x => x.Group == (byte)Groups.Projectile).Spell == (byte)Projectile.MyOwnProjectileInstance))
                    {
                        block.IsDisabled = true;
                    }

                    if ((block.RepeatAmount * block.Delay) + block.Timer > CastProperties.MinimumUseTime)
                    {
                        CastProperties.MinimumUseTime = (block.RepeatAmount * block.Delay) + block.Timer;
                    }
                }

                if (!CastProperties.RootBlock.Children.Any(x => !x.IsDisabled))
                {
                    CastProperties.FailedToCast = true;
                    CastProperties.MinimumUseTime = 0;
                }
            }
            else
            { 
                CastProperties.FailedToCast = true;
            }

            // Cast properties done!
            return CastProperties;
        }

        public static void ExecuteCards(
            Terraria.Projectile owner,
            CastBlock Block,
            Vector2 Position,
            Vector2 HeldOffset,
            Vector2 Direction,
            float DamageMultiplier,
            float KnockbackMultiplier,
            float VelocityMultiplier,
            float Spread,
            int Crit)
        {
            int Type = -999;

            if (Block.Cards.Any(x => x.Group == (byte)Groups.Projectile))
            {
                Type = GetFlooredValue(Block.Cards.First(x => x.Group == (byte)Groups.Projectile).GetValue(), 0);
            }

            if (Type < 0)
            {
                return;
            }

            int NewID = SpawnSpellCardProjectile(
                owner.GetSource_FromThis(),
                Type,
                Position,
                Direction,
                DamageMultiplier,
                KnockbackMultiplier,
                VelocityMultiplier,
                Crit,
                Main.myPlayer);

            if (Main.projectile[NewID].ModProjectile is not SpellCardProjectile)
            {
                return;
            }

            if (Main.projectile[NewID].ModProjectile is SpellCardProjectile SPproj)
            {
                SPproj.SpawnSpread = Spread;
            }

            List<int> Projectiles = new() { NewID };
            bool EncounteredProjectile = false;
            bool NoSpread = false;

            

            // Loop throught the cards and setup projectiles
            for (int Index = 0; Index < Block.Cards.Count; Index++)
            {
                for (int k = Projectiles.Count - 1; k >= 0; k--)
                {
                    Main.NewText(Block.Cards[Index].Name + " - " + Block.Cards[Index].Group + " " + Block.Cards[Index].Spell + " " + Block.Cards[Index].Variant);

                    if (Main.projectile[Projectiles[k]].ModProjectile is SpellCardProjectile proj)
                    {
                        switch (Block.Cards[Index].Group)
                        {
                            case (byte)Groups.CatalystModifier:
                                {
                                    switch (Block.Cards[Index].Spell)
                                    {
                                        case (byte)CatalystModifier.DistantCast:
                                            {
                                                proj.Projectile.position += new Vector2(240f * Block.Cards[Index].GetValue(), 0f).RotatedBy(proj.Projectile.velocity.ToRotation());
                                            }
                                            break;

                                        case (byte)CatalystModifier.TeleportCast:
                                            {
                                                proj.Projectile.Center = Main.MouseWorld;
                                            }
                                            break;

                                        case (byte)CatalystModifier.ReverseCast:
                                            {
                                                proj.Projectile.position += new Vector2(1024f, 0f).RotatedBy(proj.Projectile.velocity.ToRotation());
                                                proj.Projectile.velocity = proj.Projectile.velocity.RotatedBy(MathHelper.ToRadians(180));
                                                proj.SpawnOffset = proj.SpawnOffset.RotatedBy(MathHelper.ToRadians(180));
                                            }
                                            break;

                                        case (byte)CatalystModifier.Sniper:
                                            {
                                                NoSpread = true;
                                            }
                                            break;

                                        default:
                                            {
                                                Block.Cards[Index].ExecuteCard(ref proj);
                                            }
                                            break;
                                    }
                                }
                                break;

                            case (byte)Groups.ProjectileModifier:
                                {
                                    switch (Block.Cards[Index].Spell)
                                    {
                                        case (byte)ProjectileModifier.RotateLeft22_5:
                                        case (byte)ProjectileModifier.RotateLeft45:
                                        case (byte)ProjectileModifier.RotateLeft90:
                                        case (byte)ProjectileModifier.RotateRight22_5:
                                        case (byte)ProjectileModifier.RotateRight45:
                                        case (byte)ProjectileModifier.RotateRight90:
                                        case (byte)ProjectileModifier.RandomRotation:
                                            {
                                                proj.Projectile.velocity = proj.Projectile.velocity.RotatedBy(MathHelper.ToRadians(Block.Cards[Index].GetValue()));
                                                proj.SpawnOffset = proj.SpawnOffset.RotatedBy(MathHelper.ToRadians(Block.Cards[Index].GetValue()));
                                            }
                                            break;

                                        default:
                                            {
                                                Block.Cards[Index].ExecuteCard(ref proj);
                                            }
                                            break;
                                    }
                                }
                                break;

                            case (byte)Groups.Formation:
                                {
                                    switch (Block.Cards[Index].Variant)
                                    {
                                        case (byte)FormationVariant.None:
                                            {
                                                switch (Block.Cards[Index].Spell)
                                                {
                                                    case (byte)Formation.ForwardAndBack:
                                                        {
                                                            SpellCardProjectile back = proj.Clone();
                                                            if (back != null)
                                                            {
                                                                back.Projectile.velocity = back.Projectile.velocity.RotatedBy(MathHelper.ToRadians(+180));
                                                                back.SpawnOffset = back.SpawnOffset.RotatedBy(MathHelper.ToRadians(+180));
                                                                Projectiles.Add(back.Projectile.whoAmI);
                                                            }
                                                        }
                                                        break;

                                                    case (byte)Formation.ForwardAndSide:
                                                        {
                                                            SpellCardProjectile right = proj.Clone();
                                                            if (right != null)
                                                            {
                                                                right.Projectile.velocity = right.Projectile.velocity.RotatedBy(MathHelper.ToRadians(+90));
                                                                right.SpawnOffset = right.SpawnOffset.RotatedBy(MathHelper.ToRadians(+90));
                                                                Projectiles.Add(right.Projectile.whoAmI);
                                                            }

                                                            SpellCardProjectile left = proj.Clone();
                                                            if (left != null)
                                                            {
                                                                left.Projectile.velocity = left.Projectile.velocity.RotatedBy(MathHelper.ToRadians(-90));
                                                                left.SpawnOffset = left.SpawnOffset.RotatedBy(MathHelper.ToRadians(-90));
                                                                Projectiles.Add(left.Projectile.whoAmI);
                                                            }
                                                        }
                                                        break;

                                                    case (byte)Formation.OnlySides:
                                                        {
                                                            proj.Projectile.velocity.RotatedBy(MathHelper.ToRadians(+90));
                                                            proj.SpawnOffset.RotatedBy(MathHelper.ToRadians(+90));

                                                            SpellCardProjectile side = proj.Clone();
                                                            if (side != null)
                                                            {
                                                                side.Projectile.velocity = side.Projectile.velocity.RotatedBy(MathHelper.ToRadians(+180));
                                                                side.SpawnOffset = side.SpawnOffset.RotatedBy(MathHelper.ToRadians(+180));
                                                                Projectiles.Add(side.Projectile.whoAmI);
                                                            }
                                                        }
                                                        break;

                                                    case (byte)Formation.Daedalus:
                                                        {
                                                            // TODO
                                                        }
                                                        break;
                                                }
                                            }
                                            break;

                                        case (byte)FormationVariant.Fork:
                                            {
                                                int amount = GetFlooredValue(Block.Cards[Index].GetValue(), 0);
                                                if (amount > 1)
                                                {
                                                    float SizeSQ = proj.Projectile.Size.Length();

                                                    float rotation = proj.Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                                                    float StartOffset = (SizeSQ * (amount - 1)) - (amount % 2 == 0 ? SizeSQ / 2f : 0f);
                                                    proj.SpawnOffset -= new Vector2(StartOffset / 2f, 0f).RotatedBy(rotation);

                                                    for (int f = 1; f < amount; f++)
                                                    {
                                                        SpellCardProjectile copy = proj.Clone();
                                                        if (copy != null)
                                                        {
                                                            copy.SpawnOffset += new Vector2(SizeSQ * f, 0f).RotatedBy(rotation);
                                                            Projectiles.Add(copy.Projectile.whoAmI);
                                                        }
                                                    }
                                                }
                                            }
                                            break;

                                        case (byte)FormationVariant.Scatter:
                                            {
                                                int amount = GetFlooredValue(Block.Cards[Index].GetValue(), 0);
                                                if (amount > 1)
                                                {
                                                    float TotalAngle = 90f;
                                                    float AddedAngle = TotalAngle / (amount - 1);
                                                    float Angle = -TotalAngle / 2f;
                                                    proj.Projectile.velocity = proj.Projectile.velocity.RotatedBy(MathHelper.ToRadians(Angle));

                                                    for (int f = 1; f < amount; f++)
                                                    {
                                                        SpellCardProjectile copy = proj.Clone();
                                                        if (copy != null)
                                                        {
                                                            copy.Projectile.velocity = copy.Projectile.velocity.RotatedBy(MathHelper.ToRadians(AddedAngle * f));
                                                            copy.SpawnOffset = copy.SpawnOffset.RotatedBy(MathHelper.ToRadians(AddedAngle * f));
                                                            Projectiles.Add(copy.Projectile.whoAmI);
                                                        }
                                                    }
                                                }
                                            }
                                            break;

                                        case (byte)FormationVariant.SomethingGon:
                                            {
                                                int amount = GetFlooredValue(Block.Cards[Index].GetValue(), 0);
                                                if (amount > 1)
                                                {
                                                    float TotalAngle = 360f;
                                                    float AddedAngle = TotalAngle / amount;

                                                    for (int f = 1; f < amount; f++)
                                                    {
                                                        SpellCardProjectile copy = proj.Clone();
                                                        if (copy != null)
                                                        {
                                                            copy.Projectile.velocity = copy.Projectile.velocity.RotatedBy(MathHelper.ToRadians(AddedAngle * f));
                                                            copy.SpawnOffset = copy.SpawnOffset.RotatedBy(MathHelper.ToRadians(AddedAngle * f));
                                                            Projectiles.Add(copy.Projectile.whoAmI);
                                                        }
                                                    }
                                                }
                                            }
                                            break;

                                    }
                                }
                                break;

                            case (byte)Groups.Projectile:
                                {
                                    Block.Cards[Index].ExecuteCard(ref proj);
                                    EncounteredProjectile = true;
                                }
                                break;

                            default:
                                {
                                    Block.Cards[Index].ExecuteCard(ref proj);
                                }
                                break;
                        }

                        if (EncounteredProjectile)
                        {
                            break;
                        }
                    }
                }
            }

            // Apply last stats
            foreach (int i in Projectiles)
            {
                if (Main.projectile[i].ModProjectile is SpellCardProjectile proj)
                {
                    // Apply spawnoffset
                    proj.Projectile.position += proj.SpawnOffset;
                    if (!NoSpread)
                    {
                        proj.Projectile.velocity.RotateRandom(MathHelper.ToRadians(proj.SpawnSpread));
                    }

                    // Apply triggers
                    if (Block.HasChildren && Block.TriggerCards != null && Block.TriggerCards.Count > 0 && Block.TriggerAmount > 0)
                    {
                        foreach (CastBlock child in Block.Children)
                        {
                            proj.Payload.Add(child.Clone());
                        }

                        proj.TriggerAmount = Block.TriggerAmount;
                        proj.TriggerCards = Block.TriggerCards;
                    }

                    // Apply stats
                    proj.DamageMultiplier = DamageMultiplier;
                    proj.KnockbackMultiplier = KnockbackMultiplier;
                    proj.VelocityMultiplier = VelocityMultiplier;
                    proj.Spread = Spread;
                    proj.Crit = Crit;

                    // Manually send the net update for this projectile
                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        NetMessage.TrySendData(27, number: proj.Projectile.whoAmI);
                    }
                }
            }
        }

        public static int SpawnSpellCardProjectile(
            IEntitySource spawnSource,
            int Type,
            Vector2 position,
            Vector2 direction,
            float DamageMultiplier,
            float KnockBackMultiplier,
            float VelocityMultiplier,
            int CritChance,
            int Owner = 255,
            float ai0 = 0.0f,
            float ai1 = 0.0f)
        {
            // ----- Mimic vanilla code -----//

            // Search for a free slot
            int FreeSlot = 1000;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (!Main.projectile[i].active)
                {
                    FreeSlot = i;
                    break;
                }
            }

            // No free slot, grab the projectile with the lowest lifetime instead
            if (FreeSlot >= Main.maxProjectiles)
            {
                FreeSlot = Terraria.Projectile.FindOldestProjectile();
            }

            // Grab the projectile instance
            Terraria.Projectile projectile = Main.projectile[FreeSlot];

            // Reset Defaults
            projectile.SetDefaults(Type);

            // Setup projectile - Apply catalyst modifiers directly
            projectile.position = position - new Vector2(projectile.width, projectile.height) * 0.5f;
            projectile.velocity = direction * projectile.velocity.Length() * VelocityMultiplier;
            projectile.owner = Owner;
            projectile.damage = (int)(projectile.damage * DamageMultiplier);
            projectile.knockBack *= KnockBackMultiplier;
            projectile.CritChance += CritChance;
            projectile.identity = FreeSlot;
            projectile.gfxOffY = 0f;
            projectile.stepSpeed = 1f;
            projectile.wet = projectile.ignoreWater ? false : Collision.WetCollision(projectile.position, projectile.width, projectile.height);
            projectile.honeyWet = Collision.honey;
            projectile.ai[0] = ai0;
            projectile.ai[1] = ai1;

            // Set Identity
            Main.projectileIdentity[Owner, FreeSlot] = FreeSlot;

            // Banner
            FindBannerToAssociateTo(spawnSource, projectile);

            // Player stats
            HandlePlayerStatModifiers(spawnSource, projectile);

            // Call ProjectileLoader OnSpawn()
            typeof(ProjectileLoader).GetMethod("OnSpawn", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { projectile, spawnSource });

            // Done! Don't forget to manually use SendData to sync the projectile!
            return FreeSlot;
        }

        private static void FindBannerToAssociateTo(IEntitySource spawnSource, Terraria.Projectile next)
        {
            if (!(spawnSource is EntitySource_Parent entitySourceParent))
                return;
            if (entitySourceParent.Entity is Terraria.Projectile entity2)
            {
                next.bannerIdToRespondTo = entity2.bannerIdToRespondTo;
            }
            else
            {
                if (!(entitySourceParent.Entity is NPC entity3))
                    return;
                next.bannerIdToRespondTo = Item.NPCtoBanner(entity3.BannerID());
            }
        }

        private static void HandlePlayerStatModifiers(IEntitySource spawnSource, Terraria.Projectile projectile)
        {
            switch (spawnSource)
            {
                case EntitySource_ItemUse entitySourceItemUse when entitySourceItemUse.Entity is Player entity1:
                    projectile.CritChance += entity1.GetWeaponCrit(entitySourceItemUse.Item);
                    projectile.ArmorPenetration += entity1.GetWeaponArmorPenetration(entitySourceItemUse.Item);
                    break;
                case EntitySource_Parent entitySourceParent when entitySourceParent.Entity is Terraria.Projectile entity2:
                    projectile.CritChance += entity2.CritChance;
                    projectile.ArmorPenetration += entity2.ArmorPenetration;
                    break;
            }
        }

        private static int CountProjectiles(CastBlock block)
        {
            int amount = 0;

            // Execute pending calculations
            if (!block.ExecutedCalculation)
            {
                block.ProjectileCounter *= block.RepeatAmount + 1;
            }

            // Loop recursive through all child nodes
            if (block.HasChildren)
            {
                foreach (CastBlock b in block.Children)
                {
                    amount += CountProjectiles(b);
                }

                // Timer trigger have a chance to fire even when the parent
                // projectile is still alive, so we gotta count the parent.
                if (!block.SkipCounting && block.UsedForCounting)
                {
                    amount += block.ProjectileCounter;
                }
            }
            else if (!block.SkipCounting)
            {
                amount += block.ProjectileCounter;
            }
            
            return amount;
        }

        public static void ConsumedCards(ref List<CardItem> Cards, List<int> Consumed, float chance)
        {
            foreach (int i in Consumed)
            {
                if (i >= 0 && i < Cards.Count && Cards[i].IsConsumable && !Cards[i].IsInsertedCard && Main.rand.NextFloat(0f, 1f) < chance)
                {
                    if (Cards[i].Item.stack > 1)
                    {
                        Cards[i].Item.stack -= 1;
                    }
                    else
                    {
                        Cards[i] = GetCardItem((byte)Groups.Empty, 0);
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
                    byte[] GroupSpellPair = EntireCardPool[Main.rand.Next(0, EntireCardPool.Count)];
                    NewCard = GetCardItem(GroupSpellPair[0], GroupSpellPair[1]);
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
                NewCard.Item.stack = ToBeReplacedCard.Item.stack;
                NewCard.IsConsumable = ToBeReplacedCard.IsConsumable;
                NewCard.IsInsertedCard = ToBeReplacedCard.IsInsertedCard;
                NewCard.SlotPosition = ToBeReplacedCard.SlotPosition;
                NewCard.IsWrapped = ToBeReplacedCard.IsWrapped;
                NewCard.IsPayload = ToBeReplacedCard.IsPayload;
                NewCard.IsMulticasted = ToBeReplacedCard.IsMulticasted;
            }

            return NewCard;
        }

        public static int GetFlooredValue(float value, int lowerlimit = 1)
        {
            int output;

            if ((int)Math.Floor(value) < lowerlimit)
            {
                output = lowerlimit;
            }
            else
            {
                output = (int)Math.Floor(value);
            }

            return output;
        }

        public static int GetRoundedValue(float value, int lowerlimit = 1)
        {
            int output;

            if ((int)Math.Round(value) < lowerlimit)
            {
                output = lowerlimit;
            }
            else
            {
                output = (int)Math.Round(value);
            }

            return output;
        }

        public static void DebugCast(Cast cast)
        {
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
            Kourindou.Instance.Logger.Debug("ProjectileAmount - " + cast.ProjectileAmount);
            Main.NewText("ProjectileAmount - " + cast.ProjectileAmount);

            DebugRecursive(cast.RootBlock, 0);
        }

        private static void DebugRecursive(CastBlock block, int depth)
        {
            string space = "";
            for (int i = 0; i <= depth; i++)
            {
                space += " ";
            }

            Kourindou.Instance.Logger.Debug(space + "//----- Block -----//");
            Kourindou.Instance.Logger.Debug(space + "Repeat - " + block.RepeatAmount);
            Kourindou.Instance.Logger.Debug(space + "Trigger - " + block.TriggerAmount);
            Kourindou.Instance.Logger.Debug(space + "Delay - " + block.Delay);
            Kourindou.Instance.Logger.Debug(space + "Timer - " + block.Timer);
            Kourindou.Instance.Logger.Debug(space + "IsDisabled - " + block.IsDisabled);
            Kourindou.Instance.Logger.Debug(space + "    //----- Cards -----//");
            for (int c = 0; c < block.Cards.Count; c++)
            {
                Kourindou.Instance.Logger.Debug(space + "    [" + c + "] " + block.Cards[c].Name);
                Kourindou.Instance.Logger.Debug(space + "    Amount - " + block.Cards[c].Amount);
                Kourindou.Instance.Logger.Debug(space + "    IsInsertedCard - " + block.Cards[c].IsInsertedCard);
                Kourindou.Instance.Logger.Debug(space + "    IsWrapped - " + block.Cards[c].IsWrapped);
                Kourindou.Instance.Logger.Debug(space + "    IsAlwaysCast - " + block.Cards[c].IsAlwaysCast);
                Kourindou.Instance.Logger.Debug(space + "    IsPayload - " + block.Cards[c].IsPayload);
                Kourindou.Instance.Logger.Debug(space + "    IsMulticasted - " + block.Cards[c].IsMulticasted);
                Kourindou.Instance.Logger.Debug(space + "    SlotPosition - " + block.Cards[c].SlotPosition);
            }

            if (block.HasChildren)
            {
                depth++;
                foreach (CastBlock b in block.Children)
                { 
                    DebugRecursive(b, depth);
                }
            }
        }

        #region Enumerators
        #region Card_Types
        public enum Groups : byte
        {
            Empty,
            Projectile,
            Trajectory,
            Formation,
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
            ForwardAndSide,                                                                             // 1f (Cannot be multiplied)
            ForwardAndBack,
            OnlySides,
            Daedalus,                                                                                   // 1f * AMOUNT// 

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
            Shimmer,                                                                                    // 
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
            // Patchouli's elements
            Generic, 
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

        public enum SpawnOrderStats : byte
        {
            ForwardAndSide,
            ForwardAndBack,
            OnlySides,
            Scatter,
            Fork,
            SomethingGon,
            Daedalus,
            DistantCast,
            TeleportCast,
            ReverseCast,
            Rotation
        }
        public enum ProjectileStats : byte
        {
            Arc,
            ZigZag,
            PingPong,
            Snake,
            Uncontrolled,
            Orbit,
            Spiral,
            Boomerang,
            Aiming,
            Acceleration,
            AccelerationMultiplier,
            Bounce,
            Penetrate,
            Gravity,
            Homing,
            RotateToEnemy,
            Collide,
            Shimmer,
            LifeTime,
            //KnockBack, => Auto-synched if != 0
            //Damage, => Auto-synched if != 0
            Snowball,
            Scale,
            EdgeType,
            Shatter,
            Eater,
            Forcefield,
            Explosion,
            Hostile,
            Friendly,
            Light,
            CritChance,
            ArmorPenetration,
            Element
        }

        #endregion
        #endregion
    }
}