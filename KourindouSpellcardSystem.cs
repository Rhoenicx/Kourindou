using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Kourindou.Items.Spellcards;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou
{
    // Individual card, this is how the cards will look like when they get passed around on the network
    public class Card
    {
        public byte Group { get; set; } = (byte)Groups.Empty;
        public byte Spell { get; set; } = 0;
    }

    public class CardInfo
    {
        public byte Group { get; set; } = (byte)Groups.Empty;
        public byte Spell { get; set; } = 0;
        public float Strength { get; set; } = 1f;
        public byte Variant { get; set; } = 0;
        public int AddUseTime { get; set; } = 0;
        public int AddCooldown { get; set; } = 0;
        public int AddRecharge { get; set; } = 0;
        public float AddSpread { get; set; } = 0f;
        public float FixedAngle { get; set; } = 0;
        public bool IsRandomCard { get; set; } = false;
        public bool IsInsertedCard { get; set; } = false;
    }

    // Cast block info
    public class CastBlock
    {
        public int Repeat { get; set; } = 0;
        public int Delay { get; set; } = 0;
        public List<CardInfo> CardInfo { get; set; }
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
    }

    public class KourindouSpellcardSystem
    {
        private static Dictionary<byte, Dictionary<byte, CardItem>> CardItems = new();
        private static Dictionary<int, Card> EntireCardPool = new();
        public static void Load()
        {
            CardItems = new();
            EntireCardPool = new();
        }

        public static void Unload()
        {
            CardItems.Clear();
            CardItems = null;
            EntireCardPool.Clear();
            EntireCardPool = null;
        }

        public static void PostSetupContent()
        {
            SetupEntireCardPool();
        }

        public static bool AddCardItem(byte group, byte spell, CardItem cardItem)
        {
            if (!CardItems.ContainsKey(group))
            {
                CardItems.Add(group, new Dictionary<byte, CardItem>());
            }

            if (!CardItems[group].ContainsKey(spell))
            {
                CardItems[group].Add(spell, cardItem);

                return true;
            }

            return false;
        }

        public static CardInfo GetCardInfo(Card card)
        {
            if (CardItems.ContainsKey(card.Group) && CardItems[card.Group].ContainsKey(card.Spell))
            {
                return CardItems[card.Group][card.Spell].GetCardInfo();
            }

            return null;
        }

        public static CardInfo GetCardInfo(byte Group, byte Spell)
        {
            if (CardItems.ContainsKey(Group) && CardItems[Group].ContainsKey(Spell))
            {
                return CardItems[Group][Spell].GetCardInfo();
            }

            return null;
        }

        public static void SetupEntireCardPool()
        {
            int Index = 0;
            foreach (byte b in CardItems.Keys)
            {
                foreach (byte s in CardItems[b].Keys)
                {
                    EntireCardPool.Add(Index, new Card() { Group = b, Spell = s });
                    Index++;
                }
            }
        }

        public static void ShuffleCards(ref List<Card> CatalystCards)
        {
            // Create a new Dictionary to store the cards that are not empty cards
            Dictionary<int, Card> Cards = new();
            for (int i = 0; i < CatalystCards.Count; i++)
            {
                if (CatalystCards[i].Group != (byte)Groups.Empty)
                {
                    Cards.Add(i, CatalystCards[i]);
                }
            }

            // Create list of the keys that we can shuffle
            List<int> Keys = new List<int>(Cards.Keys);

            // Now shuffle the dictionary values using the list with keys
            int c = Keys.Count;
            while (c > 1)
            {
                c--;
                int r = Main.rand.Next(0, c + 1);
                Card value = Cards[Keys[r]];
                Cards[Keys[r]] = Cards[Keys[c]];
                Cards[Keys[c]] = value;
            }

            // put the shuffled values back into the Card list
            foreach (int i in Cards.Keys)
            {
                CatalystCards[i] = Cards[i];
            }
        }

        public static void ShuffleCards(ref List<CardInfo> CardInfos)
        {
            // Create a new Dictionary to store the cards that are not empty cards
            Dictionary<int, CardInfo> Cards = new();
            for (int i = 0; i < CardInfos.Count; i++)
            {
                if (CardInfos[i].Group != (byte)Groups.Empty)
                {
                    Cards.Add(i, CardInfos[i]);
                }
            }

            // Create list of the keys that we can shuffle
            List<int> Keys = new List<int>(Cards.Keys);

            // Now shuffle the dictionary values using the list with keys
            int c = Keys.Count;
            while (c > 1)
            {
                c--;
                int r = Main.rand.Next(0, c + 1);
                CardInfo value = Cards[Keys[r]];
                Cards[Keys[r]] = Cards[Keys[c]];
                Cards[Keys[c]] = value;
            }

            // put the shuffled values back into the Card list
            foreach (int i in Cards.Keys)
            {
                CardInfos[i] = Cards[i];
            }
        }

        public static List<CardInfo> CardToCardInfo(List<Card> Cards)
        {
            List<CardInfo> _CardInfo = new();
            foreach (Card c in Cards)
            {
                if (c.Group != (byte)Groups.Empty && CardItems.ContainsKey(c.Group) && CardItems[c.Group].ContainsKey(c.Spell))
                {
                    _CardInfo.Add(CardItems[c.Group][c.Spell].GetCardInfo());
                }
                else
                {
                    _CardInfo.Add(new CardInfo());
                }
            }
            return _CardInfo;
        }

        public static Cast GenerateCast(List<CardInfo> Cards, bool IsCatalyst, int CatalystStartIndex = 0, int CatalystCastAmount = 1, bool shuffle = false, Card AlwaysCastCard = null)
        {
            // Here we generate the cast info which is passed to the execution part
            // => If this catalyst is a shuffle, the cards should already be shuffled.

            // Cast List for storing the Cast blocks
            Cast CastProperties = new();

            // Save wrap-around cards
            List<CardInfo> CardWrapReady = Cards;
            if (shuffle)
            {
                ShuffleCards(ref CardWrapReady);
            }
            CardWrapReady.RemoveRange(CatalystStartIndex, Cards.Count - CatalystStartIndex);

            // Check the AlwaysCastCard
            CardInfo AlwaysCastCardInfo = new CardInfo() { Group = (byte)Groups.Empty };
            if (AlwaysCastCard != null && AlwaysCastCard.Group != (byte)Groups.Empty)
            {
                AlwaysCastCardInfo = GetCardInfo(AlwaysCastCard);
                AlwaysCastCardInfo.IsInsertedCard = true;
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
            CardInfo InsertThisCardWrapAround = new CardInfo();

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
                    if (AlwaysCastCardInfo.Group != (byte)Groups.Empty)
                    {
                        switch ((Groups)AlwaysCastCardInfo.Group)
                        {
                            // Projectiles end casts, we don't want this to happen with a multicast
                            // so increase the multicast amount instead.
                            case Groups.Projectile:
                                {
                                    MulticastAmount++;
                                }
                                break;

                            // if the always cast is a multicast, set the multicast amount
                            case Groups.Multicast:
                                {
                                    MulticastAmount += (int)Math.Ceiling(AlwaysCastCardInfo.Strength) - 1;
                                }
                                break;
                        }

                        // Insert the card
                        Cards.Insert(Index, AlwaysCastCardInfo);

                        // Since we have inserted a card, we should use continue here,
                        // we want the catalyst to execute this index again.
                        continue;
                    }
                }

                if (CastProperties.Casts[CurrentCast].Blocks.ElementAtOrDefault(CurrentBlock) == null)
                {
                    CastProperties.Casts[CurrentCast].Blocks.Add(new CastBlock());
                }

                // Count the amount of cards encoutered that have been inserted,
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
                            if (Triggers > 0)
                            {
                                Triggers--;
                                TriggerActive = true;
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                            }
                            else
                            {
                                TriggerActive = false;

                                if (MulticastAmount > 0)
                                {
                                    MulticastAmount--;
                                    CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                                    CurrentBlock++;
                                }
                                else
                                {
                                    CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                                    CurrentCast++;

                                    // If the DiggingBolt is the last card on the catalyst, nullify cooldown
                                    if (CurrentCast == CatalystCastAmount && Cards[Index].Spell == (byte)Projectile.DiggingBolt)
                                    {
                                        CastProperties.CooldownOverride = true;
                                    }

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
                            if (TriggerActive)
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                            }

                            if (!IsWrappingAround)
                            {
                                Triggers += (int)Math.Ceiling(Cards[Index].Strength);
                            }
                        }
                        break;

                    case Groups.Multiplication:
                        {
                            if (TriggerActive)
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                            }
                            else
                            {
                                // Multiplication is at the end of the catalyst
                                if (Index >= Cards.Count - 1)
                                {
                                    InsertThisCardWrapAround = Cards[Index];
                                }
                                // Multiplication card is not on the end
                                else
                                {
                                    // Check if there is a not empty card after the multiplication
                                    for (int i = Index + 1; i < Cards.Count; i++)
                                    {
                                        if (Cards[i].Group != (byte)Groups.Empty)
                                        {
                                            switch ((Groups)Cards[i].Group)
                                            {
                                                case Groups.Projectile:
                                                case Groups.Special:
                                                case Groups.Multicast:
                                                    {
                                                        // Insert new cards if they are projectile, special or multicast variants
                                                        for (int y = 1; y < (int)Cards[Index].Strength; y++)
                                                        {
                                                            Cards.Insert(i, Cards[i]);
                                                            Cards[i + 1].IsInsertedCard = true;
                                                        }
                                                    }
                                                    break;

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

                                                default:
                                                    {
                                                        Cards[i].Strength *= Cards[Index].Strength;
                                                    }
                                                    break;
                                            }

                                            // Stop the for loop because this card has executed its effect
                                            break;
                                        }
                                        // Check if the search has ended to the last card of catalyst
                                        else if (i == Cards.Count - 1)
                                        {
                                            InsertThisCardWrapAround = Cards[Index];
                                        }
                                    }
                                }
                            }
                            CastUnEnded++;
                        }
                        break;

                    case Groups.CatalystModifier:
                        {
                            switch ((CatalystModifierVariant)Cards[Index].Variant)
                            {
                                case CatalystModifierVariant.None:
                                    {
                                        if (!IsCatalyst)
                                        {
                                            break;
                                        }

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
                                        }
                                    }
                                    break;

                                case CatalystModifierVariant.ReduceRechargePercent:
                                    {
                                        if (!TriggerActive && IsCatalyst)
                                        {
                                            CastProperties.RechargeTimePercentage *= Cards[Index].Strength;
                                        }
                                    }
                                    break;

                                case CatalystModifierVariant.ReduceCooldownPercent:
                                    {
                                        if (!TriggerActive && IsCatalyst)
                                        {
                                            CastProperties.CooldownTimePercentage *= Cards[Index].Strength;
                                        }
                                    }
                                    break;

                                case CatalystModifierVariant.Repeat:
                                    {
                                        if (TriggerActive)
                                        {
                                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                                        }
                                        else
                                        {
                                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].Repeat += (int)Math.Ceiling(Cards[Index].Strength);
                                        }
                                    }
                                    break;

                                case CatalystModifierVariant.Delay: 
                                    {
                                        if (TriggerActive)
                                        {
                                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                                        }
                                        else
                                        {
                                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].Delay += (int)Math.Ceiling(Cards[Index].Strength);
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                    
                    case Groups.Special:
                        {
                            if (TriggerActive)
                            {
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                            }
                            else
                            {
                                switch ((Special)Cards[Index].Spell)
                                {
                                    case Special.RandomCard:
                                        {
                                            Cards[Index] = GetRandomCard(Cards[Index]);
                                            continue;
                                        }

                                    case Special.CastEverything:
                                        {
                                            if (IsCatalyst)
                                            {
                                                CatalystCastAmount = 1000;
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                        }
                        

                    default:
                        {
                            CastUnEnded++;
                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                        }
                        break;
                }

                if (Cards[Index].Group != (byte)Groups.Empty)
                {
                    if (!CastProperties.CooldownOverride)
                    {
                        CastProperties.CooldownTime += Cards[Index].AddCooldown;
                    }
                    if (!CastProperties.RechargeOverride)
                    {
                        CastProperties.RechargeTime += Cards[Index].AddRecharge;
                    }

                    CastProperties.RechargeTime += Cards[Index].AddUseTime;
                }

                // Check if we need to wrap around:
                // => If we are on the last card and are currently not wrapping-around
                //    - Check if there are trigger active -> Yes Wrap Around
                //    - Check if a Multicast is active -> Yes Wrap Around
                //    - Check if a Cast is currently not ended -> Yes Wrap Around
                if (IsCatalyst)
                {
                    if (!IsWrappingAround && Index == Cards.Count - 1)
                    {
                        if (TriggerActive || Triggers > 0 || MulticastAmount > 0 || CastUnEnded > 0)
                        {
                            // Add the previously saved Multiplication card
                            if (InsertThisCardWrapAround.Group != (byte)Groups.Empty)
                            {
                                Cards.Add(InsertThisCardWrapAround);
                            }

                            // Now add the wrap-around cards
                            foreach (CardInfo cardInfo in CardWrapReady)
                            {
                                Cards.Add(cardInfo);
                            }

                            // Since we are wrapping-around, the catalyst needs to go on cooldown
                            CastProperties.MustGoOnCooldown = true;

                            // Save the end index
                            CastProperties.NextCastStartIndex = Index - InsertedCards;

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
                    else if (IsWrappingAround && Index == Cards.Count - 1)
                    {
                        break;
                    }
                }

                Index++;
            }

            #region AfterGeneration
            // Execute catalyst specific properties
            if (IsCatalyst)
            {
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
                        for (int i = Index; i<Cards.Count; i++)
                        {
                            if (Cards[i].Group != (byte) Groups.Empty)
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
            }
            else
            {
                // GenerateCast not run by a catalyst,
                // this must be a projectile with a trigger
                // projectiles don't have cooldowns, they simply die
                CastProperties.MustGoOnCooldown = true;
                CastProperties.NextCastStartIndex = 0;
            }

            // Remove CastInfo and CastBlocks which don't have projectiles.
            for (int _CastInfo = CastProperties.Casts.Count; _CastInfo >= 0; _CastInfo--)
            {
                for (int _CastBlock = CastProperties.Casts[_CastInfo].Blocks.Count; _CastBlock >= 0; _CastBlock++)
                {
                    // Remove CastBlocks if it has no projectile
                    if (!CastProperties.Casts[_CastInfo].Blocks[_CastBlock].CardInfo.Any(_CardInfo => _CardInfo.Group == (byte)Groups.Projectile && _CardInfo.Spell != (byte)Projectile.MyOwnProjectileInstance))
                    {
                        CastProperties.Casts[_CastInfo].Blocks.RemoveAt(_CastBlock);
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
            }
        
            // Determine the MinimumUseTime
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
            #endregion
            // Cast properties done!
            return CastProperties;
        }
        
        #region Random
        public static CardInfo GetRandomCard(CardInfo _cardInfo)
        {
            if (_cardInfo.IsRandomCard)
            {
                if (_cardInfo.Group == (byte)Groups.Special && _cardInfo.Spell == (byte)Special.RandomCard)
                {
                    // Grab a random card from the card pool
                    bool Valid = false;
                    while (!Valid)
                    {
                        int CardNumber = Main.rand.Next(0, EntireCardPool.Count);
                        _cardInfo = GetCardInfo(EntireCardPool[CardNumber]);
                        if (!_cardInfo.IsRandomCard)
                        { 
                            Valid = true;
                        }
                    }
                }
                else
                {
                    // Get a random card in the same group and which has the same variant
                    CardInfo info = GetCardInfo(_cardInfo.Group, GetRandomSpell(_cardInfo));
                    if (info != null)
                    {
                        return info;
                    }
                }
            }
        
            return _cardInfo;
        }

        public static byte GetRandomSpell(CardInfo _cardInfo)
        {
            // Invalid new spell
            byte NewSpell = 0;
            bool Valid = false;
        
            while (!Valid)
            {
                switch ((Groups)_cardInfo.Group)
                {
                    case Groups.Projectile:
                        {
                            NewSpell = (byte)Main.rand.Next(0, Enum.GetNames(typeof(Formation)).Length);
                        }
                        break;
                    case Groups.Formation:
                        {
                            NewSpell = (byte)Main.rand.Next(0, Enum.GetNames(typeof(Formation)).Length);
                        }
                        break;
                    case Groups.Trajectory:
                        {
                            NewSpell = (byte)Main.rand.Next(0, Enum.GetNames(typeof(Trajectory)).Length);
                        }
                        break;
                    case Groups.ProjectileModifier:
                        {
                            NewSpell = (byte)Main.rand.Next(0, Enum.GetNames(typeof(ProjectileModifier)).Length);
                        }
                        break;
                    case Groups.CatalystModifier:
                        {
                            NewSpell = (byte)Main.rand.Next(0, Enum.GetNames(typeof(CatalystModifier)).Length);
                        }
                        break;
                    case Groups.Element:
                        {
                            NewSpell = (byte)Main.rand.Next(0, Enum.GetNames(typeof(Element)).Length);
                        }
                        break;
                    case Groups.Multicast:
                        {
                            NewSpell = (byte)Main.rand.Next(0, Enum.GetNames(typeof(Multicast)).Length);
                        }
                        break;
                    case Groups.Trigger:
                        {
                            NewSpell = (byte)Main.rand.Next(0, Enum.GetNames(typeof(Trigger)).Length);
                        }
                        break;
                    case Groups.Multiplication:
                        {
                            NewSpell = (byte)Main.rand.Next(0, Enum.GetNames(typeof(Multiplication)).Length);
                        }
                        break;
                    default:
                        {
                            return NewSpell;
                        }
                }
        
                if (!GetCardInfo(_cardInfo.Group, NewSpell).IsRandomCard && (GetCardInfo(_cardInfo.Group, NewSpell).Variant == _cardInfo.Variant || _cardInfo.Variant == 0))
                {
                    Valid = true;
                }
            }
        
            return NewSpell;
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

            // Duplicates projectile and add spread, like a shotgun
            // Variant 1
            DoubleScatter, // 2
            TripleScatter, // 3
            QuadrupleScatter, // 4
            QuintupleScatter, // 5
            RandomScatter, // one of above

            // Duplicates projectile and place them side-by-side
            // Variant 2
            DoubleFork, // 2
            TripleFork, // 3
            QuadrupleFork, // 4
            QuintupleFork, // 5
            RandomFork, // one of above

            // Duplicates projectile and cast it in a circle around the player
            // Variant 3
            Quadragon, // 4
            Pentagon, // 5
            Hexagon, // 6
            Heptagon, // 7
            Octagon, // 8
            Randagon // one of above
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
            Wave,
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
            // Changes speed over time
            Acceleration, // +0.1f
            Deceleration, // -0.1f
        
            // Change base speed
            SpeedUp, // +1f
            SpeedDown, // -1f
            Stationary, // 0f

            // Change base spread - not including wand spread, if negative it does counter wand spread
            SpreadUp, //+5
            SpreadDown, // >= 5 ? -5 :  
            AccurateAF, // 0

            // Change the amount of tile bounces this projectile can do - Strength=Amount
            BounceUp, //+1
            BounceDown, //-1
            FallingFlat, //0
            TerribleIdea, // infinite bounces

            // Change penetration stats - Strength=Amount
            PenetrationUp, //+1
            PenetrationDown, // > 1 : -1
            PenetrationZero, //0
            PenetrationAll, //-1

            // Change gravity - Strength=ON(1)/Off(0) 
            Gravity,
            NoGravity,
            AntiGravity,
        
            // Change homing 
            Homing,
            RotateToEnemy,

            // Tile collision - Strength=ON(1)/Off(0) 
            Ghosting,
            Collide,

            // Lifetime
            LifetimeUp, // 50f (percent)
            LifetimeDown, // 50f (percent)
            Instant,
        
            // Rotation
            RotateLeft90, // -90f
            RotateRight90, // -90f
            RotateLeft45, // -45f
            RotateRight45, // 45f
        
            // Random
            RandomProjectileModifier
        }
        
        public enum CatalystModifier : byte
        {
            // reduces cooldown percentage-based
            // Variant 1
            ReduceCooldown10Percent, // 0.9f
            ReduceCooldown25Percent, // 0.75f
            ReduceCooldown50Percent, // 0.5f
            RandomCooldownPrecent, // one of above

            // reduces recharge percentage-based
            // Variant 2
            ReduceRecharge10Percent, // 0.9f
            ReduceRecharge25Percent, // 0.75f
            ReduceRecharge50Percent, // 0.5f
            RandomRechargePrecent, // one of above

            // Eliminates waiting time
            // Variant 0
            EliminateCooldown, //cooldown=0
            EliminateRecharge, //recharge=0
        
            // Repeats
            // Variant 3
            Repeat2, //2
            Repeat3, //3
            Repeat4, //4
            Repeat5, //5
            RepeatRandom, //one of above

            // Delays in ticks
            // Variant 4
            DelayCastBy1, //60
            DelayCastBy2, //120
            DelayCastBy3, //180
            DelayCastBy4, //240
            DelayCastBy5, //300
            DelayCastRandom, //one of above

            // Position where the projectiles are spawned
            DistantCast, // distance
            TeleportCast, // On/Off
            ReverseCast // long distance + 180 degrees rotated
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
            Sun,
            Moon,
            Fire,
            Water,
            Wood,
            Metal,
            Earth
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
            Timer
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
            MultiplyDivideRandom
        }
        #endregion
    }
}