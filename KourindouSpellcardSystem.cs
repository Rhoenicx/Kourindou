using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using ReLogic.Content;
using ReLogic;
using Terraria;
using Terraria.Chat;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.Default;
using Terraria.GameContent;
using static Terraria.ModLoader.ModContent;
using Kourindou.Items.Spellcards;
using System.Text.RegularExpressions;
using static Kourindou.KourindouSpellcardSystem;
using System.Dynamic;

namespace Kourindou
{
    // Individual card's info
    public class Card
    {
        public byte Group { get; set; }
        public short Spell { get; set; }
    }

    public class CardInfo
    { 
        public byte Group { get; set; }
        public short Spell { get; set; }
        public float Strength { get; set; }
        public bool IsRandomCard { get; set; }
    }

    // Cast block info
    public class CastBlock
    {
        public int Repeat { get; set; }
        public int Delay { get; set; }
        public List<CardInfo> CardInfo { get; set;}
    }

    // Cast Info
    public class Cast
    {
        public bool MustGoOnCooldown { get; set; }
        public List<CastBlock> Blocks { get; set; }
    }

    public class KourindouSpellcardSystem
    {
        private static Dictionary<byte, Dictionary<short, CardItem>> CardItems = new();

        public static void Load()
        {
            CardItems = new();
        }

        public static void Unload()
        {
            CardItems.Clear();
            CardItems = null;
        }

        public static bool AddCardItem(byte group, short spell, CardItem cardItem)
        {
            if (!CardItems.ContainsKey(group))
            {
                CardItems.Add(group, new Dictionary<short, CardItem>());
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

        public static CardInfo GetCardInfo(byte Group, short Spell)
        {
            if (CardItems.ContainsKey(Group) && CardItems[Group].ContainsKey(Spell))
            {
                return CardItems[Group][Spell].GetCardInfo();
            }

            return null;
        }

        public static void ShuffleCards(ref List<Card> CatalystCards)
        { 
            int c = CatalystCards.Count;
            while (c > 1)
            {
                c--;
                int r = Main.rand.Next(0, c + 1);
                Card value = CatalystCards[r];
                CatalystCards[r] = CatalystCards[c];
                CatalystCards[c] = value;
            }
        }

        public static void ShuffleCards(ref List<CardInfo> Cards)
        {
            int c = Cards.Count;
            while (c > 1)
            {
                c--;
                int r = Main.rand.Next(0, c + 1);
                CardInfo value = Cards[r];
                Cards[r] = Cards[c];
                Cards[c] = value;
            }
        }

        public static List<CardInfo> CardToCardInfo(List<Card> Cards)
        {
            List<CardInfo> CardInfo = new();
            foreach (Card c in Cards)
            {
                if (CardItems.ContainsKey(c.Group) && CardItems[c.Group].ContainsKey(c.Spell))
                {
                    CardInfo.Add(CardItems[c.Group][c.Spell].GetCardInfo());
                }
            }
            return CardInfo;
        }

        public static Cast GenerateCasts(List<CardInfo> Cards, int CatalystStartIndex = 0, int CatalystCastAmount = 1, bool shuffle = false, List<Card> AlwaysCastCards = null)
        {
            // Here we generate the cast info which is passed to the execution part
            // => If this catalyst is a shuffle, the cards should already be shuffled.

            // Cast List for storing the Cast blocks
            List<Cast> Casts = new();

            List<CardInfo> CardWrapReady = Cards;
            if (shuffle)
            {
                ShuffleCards(ref CardWrapReady);
            }
            CardWrapReady.RemoveRange(CatalystStartIndex, Cards.Count - CatalystStartIndex);

            // Variables needed for loops
            int CurrentCast = 0;
            int CurrentBlock = 0;
            int Index = CatalystStartIndex;

            int Triggers = 0;
            bool TriggerActive = false;

            int MulticastAmount = 0;

            int ExtraCardsInserted = 0;
            CardInfo InsertThisCardWrapAround = null;
            bool IsWrappingAround = false;

            int CatalystEndIndex = 0;

            // Start Casting
            while (Index < Cards.Count)
            {
                if (Casts.ElementAtOrDefault(CurrentCast) == null)
                {
                    Casts.Add(new Cast());
                    if (AlwaysCastCards != null && AlwaysCastCards.Count > 0)
                    {
                        for (int i = 0; i < AlwaysCastCards.Count; i++)
                        {
                            Cards.Insert(Index, GetCardInfo(AlwaysCastCards[i]));
                            ExtraCardsInserted++;
                        }
                        continue;
                    }
                }

                if (Casts[CurrentCast].Blocks.ElementAtOrDefault(CurrentBlock) == null)
                {
                    Casts[CurrentCast].Blocks.Add(new CastBlock());
                }

                switch ((Groups)Cards[Index].Group)
                {
                    case Groups.Projectile:
                        {
                            if (Triggers > 0)
                            {
                                Triggers--;
                                TriggerActive = true;
                                Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                            }
                            else
                            {
                                TriggerActive = false;

                                if (MulticastAmount > 0)
                                {
                                    MulticastAmount--;
                                    Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                                    CurrentBlock++;
                                }
                                else
                                {
                                    Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                                    CurrentCast++;
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
                                    if (Casts[CurrentCast].Blocks.ElementAtOrDefault(CurrentBlock + i) == null)
                                    {
                                        Casts[CurrentCast].Blocks.Add(new CastBlock());
                                    }

                                    Casts[CurrentCast].Blocks[CurrentBlock + i] = Casts[CurrentCast].Blocks[CurrentBlock];
                                }

                                MulticastAmount += (int)Math.Ceiling(Cards[Index].Strength) - 1;
                            }
                        }
                        break;

                    case Groups.Trigger:
                        {
                            if (TriggerActive)
                            {
                                Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                            }

                            Triggers += (int)Math.Ceiling(Cards[Index].Strength);
                        }
                        break;

                    case Groups.Repeat:
                        {
                            if (TriggerActive)
                            {
                                Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                            }
                            else
                            {
                                Casts[CurrentCast].Blocks[CurrentBlock].Repeat += (int)Math.Ceiling(Cards[Index].Strength);
                            }
                        }
                        break;

                    case Groups.CastDelay:
                        {
                            if (TriggerActive)
                            {
                                Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                            }
                            else
                            {
                                Casts[CurrentCast].Blocks[CurrentBlock].Delay += (int)Math.Ceiling(Cards[Index].Strength);
                            }
                        }
                        break;

                    case Groups.Multiplication:
                        {
                            if (Index + 1 >= Cards.Count)
                            {
                                InsertThisCardWrapAround = Cards[Index];
                            }
                            else
                            { 
                                switch ((Groups)Cards[Index + 1].Group)
                                {
                                    case Groups.Projectile:
                                    case Groups.Special:
                                    case Groups.Multicast:
                                        {
                                            // Insert new cards if they are projectiles or special variants
                                            for (int y = 1; y < (int)Cards[Index].Strength; y++)
                                            {
                                                Cards.Insert(Index + 1, Cards[Index + 1]);
                                                ExtraCardsInserted++;
                                            }
                                        }
                                        break;

                                    case Groups.Multiplication:
                                        {
                                            // Do nothing, we don't want to chain multiplication cards
                                        }
                                        break;

                                    default:
                                        {
                                            Cards[Index + 1].Strength *= Cards[Index].Strength;
                                        }
                                        break;
                                }
                            }
                            Cards[Index].Strength = 1f;
                        }
                        break;

                    case Groups.Special:
                        {
                            if (TriggerActive)
                            {
                                Casts[CurrentCast].Blocks[CurrentBlock].CardInfo.Add(Cards[Index]);
                            }
                            else
                            {
                                if (Cards[Index].Spell == (byte)Special.RandomCard)
                                {
                                    Cards[Index] = GetRandomCard(Cards[Index]);
                                    Index--;
                                }
                            }
                        }
                        break;
                }

                // Check for unended casts, need to wrap around

                if (Index == Cards.Count - 1 && !IsWrappingAround)
                {
                    if (TriggerActive || Triggers > 0 || MulticastAmount > 0 || CurrentCast < CatalystCastAmount)
                    {
                        // Add the wrap-around cards now!
                        foreach (CardInfo cardInfo in CardWrapReady)
                        {
                            Cards.Add(cardInfo);
                        }

                        CatalystEndIndex = Index - ExtraCardsInserted;
                        IsWrappingAround = true;
                    }
                }

                if (Index == Cards.Count - 1 && IsWrappingAround)
                {
                    break;
                }

                Index++;
            }

            // After creating the casts set the ending point minus the cards inserted
            if (!IsWrappingAround)
            {
                CatalystEndIndex = Index - ExtraCardsInserted;
            }
        }

        #region Random
        public static CardInfo GetRandomCard(CardInfo _cardInfo)
        {
            if (_cardInfo.IsRandomCard)
            {
                byte NewGroup = (_cardInfo.Group == (byte)Groups.Special && _cardInfo.Spell == (byte)Special.RandomCard) ? GetRandomGroup() : _cardInfo.Group;
                short NewSpell = GetRandomSpell(NewGroup);

                CardInfo info = GetCardInfo(_cardInfo.Group, _cardInfo.Spell);
                if (info != null)
                {
                    _cardInfo = info;
                }
            }

            return _cardInfo;
        }

        public static byte GetRandomGroup()
        {
            bool Valid = false;
            byte NewGroup = (byte)Groups.Projectile;

            while (!Valid)
            {
                NewGroup = (byte)Main.rand.Next(0, Enum.GetNames(typeof(Groups)).Length);
                if (NewGroup != (byte)Groups.Special)
                {
                    Valid = true;
                }
            }

            return NewGroup;
        }

        public static short GetRandomSpell(byte Group)
        {
            // Invalid new spell
            short NewSpell = -1;
            bool Valid = false;

            // Check if the given group exists in CardItems
            if (!CardItems.ContainsKey(Group))
            {
                return NewSpell;
            }

            while (!Valid)
            {
                switch ((Groups)Group)
                {
                    case Groups.Projectile:
                        {
                            NewSpell = (short)Main.rand.Next(0, Enum.GetNames(typeof(Formation)).Length);
                        }
                        break;
                    case Groups.Formation:
                        {
                            NewSpell = (short)Main.rand.Next(0, Enum.GetNames(typeof(Formation)).Length);
                        }
                        break;
                    case Groups.Trajectory:
                        {
                            NewSpell = (short)Main.rand.Next(0, Enum.GetNames(typeof(Trajectory)).Length);
                        }
                        break;
                    case Groups.ProjectileModifier:
                        {
                            NewSpell = (short)Main.rand.Next(0, Enum.GetNames(typeof(ProjectileModifier)).Length);
                        }
                        break;
                    case Groups.CatalystModifier:
                        {
                            NewSpell = (short)Main.rand.Next(0, Enum.GetNames(typeof(CatalystModifier)).Length);
                        }
                        break;
                    case Groups.Element:
                        {
                            NewSpell = (short)Main.rand.Next(0, Enum.GetNames(typeof(Element)).Length);
                        }
                        break;
                    case Groups.Multicast:
                        {
                            NewSpell = (short)Main.rand.Next(0, Enum.GetNames(typeof(Multicast)).Length);
                        }
                        break;
                    case Groups.Trigger:
                        {
                            NewSpell = (short)Main.rand.Next(0, Enum.GetNames(typeof(Trigger)).Length);
                        }
                        break;
                    case Groups.Repeat:
                        {
                            NewSpell = (short)Main.rand.Next(0, Enum.GetNames(typeof(Repeat)).Length);
                        }
                        break;
                    case Groups.CastDelay:
                        {
                            NewSpell = (short)Main.rand.Next(0, Enum.GetNames(typeof(CastDelay)).Length);
                        }
                        break;
                    case Groups.Multiplication:
                        {
                            NewSpell = (short)Main.rand.Next(0, Enum.GetNames(typeof(Multiplication)).Length);
                        }
                        break;
                    default: 
                        {
                            return NewSpell;
                        } 
                }

                if (!CardItems[Group][NewSpell].IsRandomCard)
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
            Projectile,
            Formation,
            Trajectory,
            ProjectileModifier,
            CatalystModifier,
            Element,
            Multicast,
            Trigger,
            Repeat,
            CastDelay,
            Multiplication,
            Special
        }

        public enum Projectile
        { 
            Shot,
            MyOwnProjectileInstance
        }

        public enum Formation
        {
            // Default straight line
            Straight,

            // Duplicates projectile and add spread, like a shotgun
            DoubleScatter,
            TripleScatter,
            QuadrupleScatter,
            QuintupleScatter,
            RandomScatter,

            // Duplicates projectile and place them side-by-side
            DoubleFork,
            TripleFork,
            QuadrupleFork,
            QuintupleFork,
            RandomFork,

            // Duplicates projectile and cast it in a circle around the player
            Quadragon,
            Pentagon,
            Hexagon,
            Heptagon,
            Octagon,
            Randagon
        }

        public enum Trajectory
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
            Daedalus
        }

        public enum ProjectileModifier
        {
            // Changes speed over time
            Acceleration,
            Deceleration,

            // Change base speed
            SpeedUp,
            SpeedDown,
            Stationary,

            // Change base spread
            SpreadUp,
            SpreadDown,
            AccurateAF,

            // Alter the position where the projectiles are spawned
            DistantCast,
            TeleportCast,
            ReverseCast,

            // Change the amount of tile bounces this projectile can do
            BounceUp,
            BounceDown,
            FallingFlat,

            // Change penetration stats
            PenetrationUp,
            PenetrationDown,
            PenetrationZero,
            PenetrationAll,

            // Change gravity 
            Gravity,
            NoGravity,
            AntiGravity,

            // Change homing
            Homing,
            RotateToEnemy,

            // Tile collision
            Ghosting,
            Collide,

            // Lifetime
            LifetimeUp, 
            LifetimeDown,
            Instant,

            // Rotation
            RotateLeft90,
            RotateRight90,
            RotateLeft45,
            RotateRight45,
        }

        public enum CatalystModifier
        { 
        
        }

        public enum Element
        {
            Sun,
            Moon,
            Fire,
            Water,
            Wood,
            Metal,
            Earth
        }

        public enum Multicast
        {

        }

        public enum Trigger
        {

        }

        public enum Special
        { 
            RandomCard
        }

        public enum Repeat
        { 
            Repeat2, 
            Repeat3, 
            Repeat4, 
            Repeat5
        }

        public enum CastDelay
        {
            DelayCastBy1,
            DelayCastBy2,
            DelayCastBy3,
            DelayCastBy4,
            DelayCastBy5,
        }

        public enum Multiplication
        {
            MultiplyBy2,
            MultiplyBy3,
            MultiplyBy4,
            MultiplyBy5,
            DivideBy2,
            DivideBy3,
            DivideBy4,
            DivideBy5
        }
        #endregion
    }
}