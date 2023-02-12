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
    public class CardInfo
    { 
        public byte Group { get; set; }
        public short Spell { get; set; }
        public float Strength { get; set; }
        public float Angle { get; set; }
        public float AddUseTime { get; set; }
        public float AddCooldown { get; set; }
        public float AddSpread { get; set; }
        public bool IsRandomCard { get; set; }
    }

    // Cast block info
    public class CastBlockInfo
    {
        public int Repeat { get; set; }
        public int TotalCastDelay { get; set; }
        public List<CardInfo> CardInfo { get; set;}
    }

    // Cast Info
    public class CastInfo
    {
        public int MaxUseTime { get; set; }
        public List<CastBlockInfo> Blocks { get; set; }
    }

    // Casts
    public class Cast
    {
        public int Cooldown { get; set; }
        public List<CastInfo> Casts { get; set; }
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

        public static CardItem GetCardItem(byte group, short spell)
        {
            if (CardItems.ContainsKey(group) && CardItems[group].ContainsKey(spell))
            {
                return CardItems[group][spell];
            }

            return null;
        }

        // Create the casting blocks and return it as a list
        public static Cast GenerateCasts(List<CardInfo> cardInfo, bool shuffle = false, int castAmount = 1)
        {
            // 1 - Apply wrap-around: Cards placed on the end without a projectile gets moved to the front
            ApplyWrapAround(ref cardInfo);

            // 2 - First iteration to create the possible casts from the placed cards
            Cast cast = ApplyFirstCastIteration(cardInfo, castAmount, shuffle);

            foreach (CastInfo _castInfo in cast.Casts)
            {
                // 3 apply the multiplication card group
                _castInfo.Blocks[0].CardInfo = ApplyMultiplicationGroup(_castInfo.Blocks[0].CardInfo);
                
                // 4 apply potential random spells
                _castInfo.Blocks[0].CardInfo = ApplySpecialRandomSpell(_castInfo.Blocks[0].CardInfo);

                // 5 apply potential multiplication cards added in step 4
                _castInfo.Blocks[0].CardInfo = ApplyMultiplicationGroup(_castInfo.Blocks[0].CardInfo);

                _castInfo.Blocks = ApplySecondCastIteration(_castInfo.Blocks[0].CardInfo);
            }

            return cast;
        }

        // 1 - Wrap around fix
        public static void ApplyWrapAround(ref List<CardInfo> cardInfo)
        {
            int Amount = 0;

            // Search for cards that need to be wrapped-around
            for (int i = cardInfo.Count - 1; i > 0; i--)
            {
                // If we find a projectile we should stop counting
                if (cardInfo[i].Group == (byte)Groups.Projectile)
                {
                    break;
                }

                Amount++;
            }

            if (Amount > 0) 
            {
                List<CardInfo> InsertFront = new();

                for (int i = cardInfo.Count - 1; i > cardInfo.Count - 1 - Amount; i--)
                {
                    InsertFront.Add(cardInfo[i]);
                    cardInfo.RemoveAt(i);
                }

                for (int i = InsertFront.Count - 1; i >= 0; i--)
                {
                    cardInfo.Insert(0, InsertFront[i]);
                }
            }
        }
        
        // 2 - First iteration: create the cast [0]
        public static Cast ApplyFirstCastIteration(List<CardInfo> cardInfo, int castAmount, bool shuffle)
        {
            // New variables
            Cast cast = new();
            int CurrentCast = 0;
            int MultiCastNeedsProjectile = 0;
            int TriggerNeedsProjectile = 0;

            if (!shuffle)
            {
                // Determine the cast breakpoints
                for (int i = 0; i < cardInfo.Count; i++)
                {
                    // Create a new Cast with the current cast number
                    if (cast.Casts.ElementAtOrDefault(CurrentCast) == null)
                    {
                        cast.Casts.Add(new());
                    }

                    // Create a new Casting Block if there is none 
                    if (cast.Casts[CurrentCast].Blocks.ElementAtOrDefault(0) == null)
                    {
                        cast.Casts[CurrentCast].Blocks.Add(new());
                    }

                    // Add the card to the current cast
                    cast.Casts[CurrentCast].Blocks[0].CardInfo.Add(cardInfo[i]);

                    // Determine if we need to advance to the next cast
                    switch (cardInfo[i].Group)
                    {
                        case (byte)Groups.Multicast:
                            {
                                MultiCastNeedsProjectile += (int)cardInfo[i].Strength;
                            }
                            break;

                        case (byte)Groups.Trigger:
                            {
                                TriggerNeedsProjectile += (int)cardInfo[i].Strength;
                            }
                            break;

                        case (byte)Groups.Projectile:
                            {
                                if (TriggerNeedsProjectile > 0)
                                {
                                    // Reduce needed projectile for triggers
                                    TriggerNeedsProjectile--;
                                }
                                else if (MultiCastNeedsProjectile > 0)
                                {
                                    // Reduce needed projectile for multicasts
                                    MultiCastNeedsProjectile--;
                                }
                                else
                                {
                                    // Increase to the next cast
                                    CurrentCast++;
                                }
                            }
                            break;
                    }
                }

            }
            else
            {
                for (int y = 0; y < castAmount; y++)
                {
                    bool BreakLoopI = false;

                    for (int i = Main.rand.Next(0, cardInfo.Count); i < cardInfo.Count; i++)
                    {
                        // Create a new Cast with the current cast number
                        if (cast.Casts.ElementAtOrDefault(CurrentCast) == null)
                        {
                            cast.Casts.Add(new());
                        }

                        // Create a new Casting Block if there is none 
                        if (cast.Casts[CurrentCast].Blocks.ElementAtOrDefault(0) == null)
                        {
                            cast.Casts[CurrentCast].Blocks.Add(new());
                        }

                        // Add the card to the current cast
                        cast.Casts[CurrentCast].Blocks[0].CardInfo.Add(cardInfo[i]);

                        // Determine if we need to advance to the next cast
                        switch (cardInfo[i].Group)
                        {
                            case (byte)Groups.Multicast:
                                {
                                    MultiCastNeedsProjectile += (int)cardInfo[i].Strength;
                                }
                                break;

                            case (byte)Groups.Trigger:
                                {
                                    TriggerNeedsProjectile += (int)cardInfo[i].Strength;
                                }
                                break;

                            case (byte)Groups.Projectile:
                                {
                                    if (TriggerNeedsProjectile > 0)
                                    {
                                        // Reduce needed projectile for triggers
                                        TriggerNeedsProjectile--;
                                    }
                                    else if (MultiCastNeedsProjectile > 0)
                                    {
                                        // Reduce needed projectile for multicasts
                                        MultiCastNeedsProjectile--;
                                    }
                                    else
                                    {
                                        // Increase to the next cast
                                        BreakLoopI = true;
                                    }
                                }
                                break;
                        }

                        if (BreakLoopI)
                        {
                            CurrentCast++;
                            break;
                        }
                    }
                }
            }
            return cast;
        }

        // 3 & 5 - Multiply cards
        public static List<CardInfo> ApplyMultiplicationGroup(List<CardInfo> cardInfo)
        {
            bool MultiplicationsFound = true;
            while (MultiplicationsFound)
            {
                bool Found = false;
                int Index = 0;
                float MultiplicationValue = 0;

                for (int i = 0; i < cardInfo.Count; i++)
                {
                    if (cardInfo[i].Group == (byte)Groups.Multiplication)
                    {
                        Found = true;
                        Index = i;
                        MultiplicationValue = cardInfo[i].Strength;
                        break;
                    }

                    // Prevent infinite loop
                    if (i == cardInfo.Count - 1)
                    {
                        MultiplicationsFound = false;
                    }
                }

                if (cardInfo.ElementAtOrDefault(Index + 1) == null)
                {
                    continue;
                }

                if (Found)
                {
                    for (int i = 1; i < Math.Ceiling(MultiplicationValue); i++)
                    {
                        cardInfo.Insert(Index + 1, cardInfo[Index + 1]);
                    }

                    cardInfo.RemoveAt(Index);
                }
            }

            return cardInfo;
        }

        // 4 - Replace Random Spells
        public static List<CardInfo> ApplySpecialRandomSpell(List<CardInfo> cardInfo)
        {
            for (int i = 0; i < cardInfo.Count; i++)
            {
                if (cardInfo[i].IsRandomCard)
                {
                    byte NewGroup = (cardInfo[i].Group == (byte)Groups.Special && cardInfo[i].Spell == (byte)Special.RandomCard) ? GetRandomGroup() : cardInfo[i].Group;
                    short NewSpell = GetRandomSpell(NewGroup);

                    CardItem item = GetCardItem(NewGroup, NewSpell);
                    if (item != null)
                    {
                        cardInfo[i] = item.GetCardInfo();
                    }
                    else
                    {
                        cardInfo.RemoveAt(i);
                        i--;
                    }
                }
            }

            return cardInfo;
        }

        // 5 - Second iteration: create the cast blocks
        public static List<CastBlockInfo> ApplySecondCastIteration(List<CardInfo> cardInfo)
        {
            List<CastBlockInfo> castBlockInfo = new();

            int CurrentBlock = 0;
            int MultiCastNeedsProjectile = 0;
            bool TriggerActive = false;
            int TriggerNeedsProjectile = 0;

            for (int i = 0; i < cardInfo.Count; i++)
            {

            }

            return castBlockInfo;
        }

        #region Random
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
            MultiplyBy5
        }
        #endregion
    }
}