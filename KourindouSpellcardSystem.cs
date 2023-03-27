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
using Kourindou.Items.Spellcards.Empty;
using Terraria.ModLoader;
using Kourindou.Projectiles;
using Kourindou.Items.Catalysts;
using Kourindou.Items.Spellcards.CatalystModifiers;
using Kourindou.Items.Spellcards.Formations;

namespace Kourindou
{
    #region CastProperties
    // Individual card, this is how the cards will look like when they get passed around on the network
    // And how they are saved on the player and catalyst
    // Cast block info
    public class CastBlock
    {
        public CastBlock(CastBlock castBlock = null)
        {
            if (castBlock != null)
            { 
                this.Repeat = castBlock.Repeat;
                this.Delay = castBlock.Delay;
                this.Timer = castBlock.Timer;
                this.TriggerActive = castBlock.TriggerActive;
                this.TriggerAmount = castBlock.TriggerAmount;
                for (int i = 0; i < castBlock.TriggerInOrder.Count; i++)
                {
                    this.TriggerInOrder.Add((CardItem)castBlock.CardItems[i].Item.Clone().ModItem);
                }
                for (int i = 0; i < castBlock.CardItems.Count; i++)
                {
                    this.CardItems.Add((CardItem)castBlock.CardItems[i].Item.Clone().ModItem);
                }
                this.IsDisabled = castBlock.IsDisabled;
            }
        }

        public int Repeat { get; set; } = 0;
        public int Delay { get; set; } = 0;
        public int Timer { get; set; } = 0;
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
        public float ChanceNoConsumeCard = 1f;

        // Amount of projectiles inside this Cast
        public int ProjectileAmount = 0;

        // The actual cast info
        public List<CastInfo> Casts { get; set; } = new List<CastInfo>();
        public List<int> ConsumedCards { get; set; } = new List<int>();
    }
    #endregion

    #region ProjectileProperties
    public class ProjectileInfo
    {
        public ProjectileInfo()
        { 
        
        }

        public ProjectileInfo(ProjectileInfo clone)
        {
            Type = clone.Type;
            Position = clone.Position;
            Velocity = clone.Velocity;
            Offset = clone.Offset;
            Spread = clone.Spread;
            Damage = clone.Damage;
            Knockback = clone.Knockback;
            Crit = clone.Crit;
            Stats = clone.Stats;
        }

        // Projectile type that needs to be spawned
        public int Type { get; set; } = ProjectileID.Bullet;
        public Vector2 Position { get; set; } = Vector2.Zero;
        public Vector2 Velocity { get; set; } = Vector2.Zero;
        public Vector2 Offset { get; set; } = Vector2.Zero;
        public float Spread { get; set; } = 0f;
        public float Damage { get; set; } = 0f;
        public float Knockback { get; set; } = 0f;
        public int Crit { get; set; } = 0;
        public Dictionary<byte, float> Stats { get; set; } = new();
    }
    #endregion

    #region Other
    public class OrderInfo
    { 
        public SpawnOrderStats Stats { get; set; }
        public float Value { get; set; }
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

        public static void ExecuteCards(Terraria.Projectile owner, CastBlock Block, Vector2 Position, Vector2 Offset, Vector2 Velocity, float Spread, float Damage, float Knockback, int Crit, bool IsCatalyst = false)
        {
            // Setup
            List<CardItem> AppliedOnEnding = new();
            List<CardItem> Payload = new();

            ProjectileInfo BaseProjectile = new()
            {
                Position = Position,
                Offset = Offset,
                Velocity = Velocity,
                Spread = Spread,
                Damage = Damage,
                Knockback = Knockback,
                Crit = Crit,
            };

            int Index = 0;
            bool EncounteredProjectile = false;
            bool NoSpread = false;

            // Loop throught the cards and setup spawn order dependent cards
            List<OrderInfo> SpawnOrder = new();
            while (Index < Block.CardItems.Count && !EncounteredProjectile)
            {
                switch (Block.CardItems[Index].Group)
                {
                    case (byte)Groups.CatalystModifier:
                        {
                            switch (Block.CardItems[Index].Spell)
                            {
                                case (byte)CatalystModifier.DistantCast:
                                    {
                                        SpawnOrder.Add(new OrderInfo() { Stats = SpawnOrderStats.DistantCast, Value = Block.CardItems[Index].GetValue() });
                                    }
                                    break;
                                case (byte)CatalystModifier.TeleportCast:
                                    {
                                        SpawnOrder.Add(new OrderInfo() { Stats = SpawnOrderStats.TeleportCast, Value = 1f });
                                    }
                                    break;

                                case (byte)CatalystModifier.ReverseCast:
                                    {
                                        SpawnOrder.Add(new OrderInfo() { Stats = SpawnOrderStats.ReverseCast, Value = Block.CardItems[Index].GetValue() });
                                    }
                                    break;

                                case (byte)CatalystModifier.Sniper:
                                    {
                                        NoSpread = true;
                                    }
                                    break;

                                default:
                                    {
                                        Block.CardItems[Index].ExecuteCard(ref BaseProjectile);
                                    }
                                    break;
                            }
                        }
                        break;

                    case (byte)Groups.ProjectileModifier:
                        {
                            switch (Block.CardItems[Index].Spell)
                            {
                                case (byte)ProjectileModifier.RotateLeft22_5:
                                case (byte)ProjectileModifier.RotateLeft45:
                                case (byte)ProjectileModifier.RotateLeft90:
                                case (byte)ProjectileModifier.RotateRight22_5:
                                case (byte)ProjectileModifier.RotateRight45:
                                case (byte)ProjectileModifier.RotateRight90:
                                case (byte)ProjectileModifier.RandomRotation:
                                    {
                                        SpawnOrder.Add(new OrderInfo() { Stats = SpawnOrderStats.Rotation, Value = Block.CardItems[Index].GetValue() });
                                    }
                                    break;

                                default:
                                    {
                                        Block.CardItems[Index].ExecuteCard(ref BaseProjectile);
                                    }
                                    break;
                            }
                        }
                        break;

                    case (byte)Groups.Formation:
                        {
                            switch (Block.CardItems[Index].Variant)
                            { 
                                case (byte)FormationVariant.None: 
                                    {
                                        switch (Block.CardItems[Index].Spell)
                                        { 
                                            case (byte)Formation.ForwardAndBack: 
                                                { 
                                                    SpawnOrder.Add(new OrderInfo() { Stats = SpawnOrderStats.ForwardAndBack, Value = 1f }); 
                                                } 
                                                break;

                                            case (byte)Formation.ForwardAndSide: 
                                                {
                                                    SpawnOrder.Add(new OrderInfo() { Stats = SpawnOrderStats.ForwardAndSide, Value = 1f });
                                                }
                                                break;

                                            case (byte)Formation.OnlySides: 
                                                {
                                                    SpawnOrder.Add(new OrderInfo() { Stats = SpawnOrderStats.OnlySides, Value = 1f });
                                                } 
                                                break;

                                            case (byte)Formation.Daedalus:
                                                {
                                                    if (SpawnOrder.Any(info => info.Stats == SpawnOrderStats.Daedalus))
                                                    {
                                                        SpawnOrder.First(info => info.Stats == SpawnOrderStats.Daedalus).Value += Block.CardItems[Index].GetValue();
                                                    }
                                                    else
                                                    {
                                                        SpawnOrder.Add(new OrderInfo() { Stats = SpawnOrderStats.Daedalus, Value = Block.CardItems[Index].GetValue() });
                                                    }
                                                }
                                                break;
                                        }
                                    } 
                                    break;

                                case (byte)FormationVariant.Fork: 
                                    {
                                        if (SpawnOrder.Any(info => info.Stats == SpawnOrderStats.Fork))
                                        {
                                            SpawnOrder.First(info => info.Stats == SpawnOrderStats.Fork).Value += Block.CardItems[Index].GetValue();
                                        }
                                        else
                                        {
                                            SpawnOrder.Add(new OrderInfo() { Stats = SpawnOrderStats.Fork, Value = Block.CardItems[Index].GetValue() });
                                        }
                                    } 
                                    break;

                                case (byte)FormationVariant.Scatter: 
                                    {
                                        if (SpawnOrder.Any(info => info.Stats == SpawnOrderStats.Scatter))
                                        {
                                            SpawnOrder.First(info => info.Stats == SpawnOrderStats.Scatter).Value += Block.CardItems[Index].GetValue();
                                        }
                                        else
                                        {
                                            SpawnOrder.Add(new OrderInfo() { Stats = SpawnOrderStats.Scatter, Value = Block.CardItems[Index].GetValue() });
                                        }
                                    } 
                                    break;

                                case (byte)FormationVariant.SomethingGon: 
                                    {
                                        if (SpawnOrder.Any(info => info.Stats == SpawnOrderStats.SomethingGon))
                                        {
                                            SpawnOrder.First(info => info.Stats == SpawnOrderStats.SomethingGon).Value += Block.CardItems[Index].GetValue();
                                        }
                                        else
                                        {
                                            SpawnOrder.Add(new OrderInfo() { Stats = SpawnOrderStats.SomethingGon, Value = Block.CardItems[Index].GetValue() });
                                        }
                                    } 
                                    break;
                                   
                            }

                            AppliedOnEnding.Add(Block.CardItems[Index]);
                        }
                        break;

                    case (byte)Groups.Projectile:
                        {
                            Block.CardItems[Index].ExecuteCard(ref BaseProjectile);
                            EncounteredProjectile = true;
                        }
                        break;

                    default:
                        {
                            Block.CardItems[Index].ExecuteCard(ref BaseProjectile);
                        }
                        break;
                }

                if (!EncounteredProjectile)
                {
                    Index++;
                }
            }

            if (NoSpread)
            {
                BaseProjectile.Spread = 0f;
            }

            // Everything after the projectile will be considered payload!
            for (int i = Index + 1; i < Block.CardItems.Count; i++)
            {
                Payload.Add(Block.CardItems[i]);
            }

            // if the encountered projectile is own instance
            if (Block.CardItems[Index].Group == (byte)Groups.Projectile
                && Block.CardItems[Index].Spell == (byte)Projectile.MyOwnProjectileInstance)
            {
                //if (owner.ModProjectile is SpellcardProjectile proj)
                //{ 
                //    
                //}
                return;
            }


            // Apply formations and order
            List<ProjectileInfo> Projectiles = new List<ProjectileInfo>() { BaseProjectile };
            bool AppliedFormation = false;
            foreach (OrderInfo info in SpawnOrder)
            {
                switch (info.Stats)
                {
                    case SpawnOrderStats.ForwardAndSide:
                        {
                            if (!AppliedFormation)
                            {
                                int i = Projectiles.Count - 1;
                                while (i >= 0)
                                {
                                    Projectiles.Insert(i + 1, new ProjectileInfo(Projectiles[i]) { Velocity = Projectiles[i].Velocity.RotatedBy(MathHelper.ToRadians(+90)) });
                                    Projectiles.Insert(i + 1, new ProjectileInfo(Projectiles[i]) { Velocity = Projectiles[i].Velocity.RotatedBy(MathHelper.ToRadians(-90)) });
                                    i--;
                                }

                                //AppliedFormation = true;
                            }
                        } 
                        break;

                    case SpawnOrderStats.ForwardAndBack:
                        {
                            if (!AppliedFormation)
                            {
                                int i = Projectiles.Count - 1;
                                while (i >= 0)
                                {
                                    Projectiles.Insert(i + 1, new ProjectileInfo(Projectiles[i]) { Velocity = Projectiles[i].Velocity.RotatedBy(MathHelper.ToRadians(+180)) });
                                    i--;
                                }

                                //AppliedFormation = true;
                            }
                        }
                        break;

                    case SpawnOrderStats.OnlySides:
                        {
                            if (!AppliedFormation)
                            {
                                int i = Projectiles.Count - 1;
                                while (i >= 0)
                                {
                                    Projectiles[i].Velocity.RotatedBy(MathHelper.ToRadians(-90));
                                    Projectiles.Insert(i + 1, new ProjectileInfo(Projectiles[i]) { Velocity = Projectiles[i].Velocity.RotatedBy(MathHelper.ToRadians(+90)) });
                                    i--;
                                }

                                //AppliedFormation = true;
                            }
                        } 
                        break;

                    case SpawnOrderStats.Scatter:
                        {
                            if (!AppliedFormation)
                            {
                                int amount = GetFlooredValue(info.Value, 0);
                                if (amount > 1)
                                {
                                    int i = Projectiles.Count - 1;
                                    while (i >= 0)
                                    {
                                        float TotalAngle = 90f;
                                        float AddedAngle = TotalAngle / (amount - 1);
                                        float Angle = -TotalAngle / 2f;
                                        Projectiles[i].Velocity = Projectiles[i].Velocity.RotatedBy(MathHelper.ToRadians(Angle));

                                        for (int f = 1; f < amount; f++)
                                        {
                                            Projectiles.Insert(i + 1, new ProjectileInfo(Projectiles[i]) { Velocity = Projectiles[i].Velocity.RotatedBy(MathHelper.ToRadians((AddedAngle * f))) });
                                        }

                                        i--;
                                    }
                                }

                                //AppliedFormation = true;
                            }
                        } 
                        break;

                    case SpawnOrderStats.Fork:
                        {
                            if (!AppliedFormation)
                            {
                                int amount = GetFlooredValue(info.Value, 0);
                                if (amount > 1)
                                {
                                    float SizeSQ = 10f;
                                    if (Projectiles.First().Type != ProjectileID.None)
                                    {
                                        Terraria.Projectile CalcProj = new Terraria.Projectile();
                                        CalcProj.SetDefaults(Projectiles.First().Type);
                                        SizeSQ = CalcProj.Size.Length();
                                    }

                                    int i = Projectiles.Count - 1;
                                    while (i >= 0)
                                    {
                                        float rotation = Projectiles[i].Velocity.ToRotation() + MathHelper.PiOver2;
                                        float StartOffset = (SizeSQ * (amount - 1)) - (amount % 2 == 0 ? SizeSQ/2f : 0f);
                                        Projectiles[i].Position -= new Vector2(StartOffset/2f, 0f).RotatedBy(rotation);

                                        for (int f = 1; f < amount; f++)
                                        {
                                            Projectiles.Insert(i + 1, new ProjectileInfo(Projectiles[i]) { Position = Projectiles[i].Position + new Vector2(SizeSQ * f, 0f).RotatedBy(rotation)});
                                        }

                                        i--;
                                    }

                                    //AppliedFormation = true;
                                }
                            }
                        }
                        break;

                    case SpawnOrderStats.SomethingGon:
                        {
                            if (!AppliedFormation)
                            {
                                int amount = GetFlooredValue(info.Value, 0);
                                if (amount > 1)
                                {
                                    int i = Projectiles.Count - 1;
                                    while (i >= 0)
                                    {
                                        float TotalAngle = 360f;
                                        float AddedAngle = TotalAngle / amount;

                                        for (int f = 1; f < amount; f++)
                                        {
                                            Projectiles.Insert(i + 1, new ProjectileInfo(Projectiles[i]) { Velocity = Projectiles[i].Velocity.RotatedBy(MathHelper.ToRadians((AddedAngle * f))) });
                                        }

                                        i--;
                                    }
                                }

                                //AppliedFormation = true;
                            }
                        } 
                        break;

                    case SpawnOrderStats.Daedalus:
                        {
                            if (!AppliedFormation)
                            {
                                // TODO
                                //AppliedFormation = true;
                            }
                        }
                        break;

                    case SpawnOrderStats.DistantCast:
                        {
                            foreach (ProjectileInfo proj in Projectiles)
                            {
                                proj.Position += new Vector2(240f * info.Value, 0f).RotatedBy(proj.Velocity.ToRotation());
                            }
                        } 
                        break;

                    case SpawnOrderStats.TeleportCast:
                        {
                            foreach (ProjectileInfo proj in Projectiles)
                            {
                                proj.Position = Main.MouseWorld;
                            }
                        } 
                        break;

                    case SpawnOrderStats.ReverseCast:
                        {
                            foreach (ProjectileInfo proj in Projectiles)
                            {
                                proj.Position += new Vector2(1024f, 0f).RotatedBy(proj.Velocity.ToRotation());
                                proj.Velocity = proj.Velocity.RotatedBy(MathHelper.ToRadians(180));
                            }
                        } 
                        break;

                    case SpawnOrderStats.Rotation:             
                        {
                            foreach (ProjectileInfo proj in Projectiles)
                            {
                                proj.Velocity = proj.Velocity.RotatedBy(MathHelper.ToRadians(info.Value));
                            }
                        } 
                        break;
                }
            }

            foreach (ProjectileInfo info in Projectiles)
            {
                int ID = Terraria.Projectile.NewProjectile(
                    owner.GetSource_FromAI(),
                    info.Position,
                    info.Velocity,
                    info.Type,
                    (int)info.Damage,
                    info.Knockback,
                    owner.owner,
                    info.Crit
                    );

                //Terraria.Projectile proj = Main.projectile[ID];
                //if (proj.ModProjectile is SpellcardProjectile SpProj)
                //{
                //    
                //}
            }
        }

        public static Cast GenerateCast(List<CardItem> Cards, CardItem AlwaysCastCard, bool IsCatalyst, int CatalystStartIndex = 0, int CatalystCastAmount = 1, bool IsVisualized = false)
        {
            // Here we generate the cast info which is passed to the execution part
            // => If this catalyst is a shuffle, the cards should first be shuffled.
            // => Be sure to use SetSlotPositions BEFORE casting and/or shuffling!!!

            // Cast List for storing the Cast blocks
            Cast CastProperties = new();

            // Create a new List with Cards and reset them to defaults
            for (int i = 0; i < Cards.Count; i++)
            {
                Cards[i].SetDefaults();
            }

            // Save wrap-around cards
            List<CardItem> CardsWrapReady = new();
            for (int i = 0; i < CatalystStartIndex; i++)
            {
                CardsWrapReady.Add(Cards[i]);
                if (IsVisualized)
                {
                    CardsWrapReady[^1].IsWrapped = true;
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
                        Cards.Insert(Index, AlwaysCastCard);

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
                        if (CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerActive && MultiplicationCard.Group != (byte)Groups.Empty)
                        {
                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(MultiplicationCard);
                        }
                        else
                        {
                            if (consume)
                            {
                                if (GetFlooredValue(Multiplications) > stack)
                                {
                                    Multiplications = (float)stack;
                                }

                                for (int y = 1; y < GetFlooredValue(Multiplications); y++)
                                {
                                    CastProperties.ConsumedCards.Add(Cards[Index].SlotPosition);
                                }
                            }

                            Cards[Index].ApplyMultiplication(Multiplications);
                        }

                        MultiplicationCard = GetCardItem((byte)Groups.Empty, 0);
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
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerAmount += GetFlooredValue(Cards[Index].GetValue(), 1) - 1;
                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                            }
                            else
                            {
                                for (int i = 1; i < GetFlooredValue(Cards[Index].GetValue(), 1); i++)
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

                                MulticastAmount += GetFlooredValue(Cards[Index].GetValue(), 1) - 1;
                            }
                        }
                        break;

                    case Groups.Trigger:
                        {
                            // If a trigger is active, this card should get send as payload
                            if (CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerActive)
                            {
                                if (Cards[Index].Spell == (byte)Trigger.Trigger)
                                {
                                    CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerAmount += GetFlooredValue(Cards[Index].GetValue(), 1);
                                }
                                else
                                {
                                    CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerAmount += 1;
                                }

                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].CardItems.Add(Cards[Index]);
                            }
                            // No trigger active: During wrap-around we do not want to add triggers.
                            else if (!IsWrappingAround)
                            {
                                if (Cards[Index].Spell == (byte)Trigger.Trigger)
                                {
                                    CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerAmount += GetFlooredValue(Cards[Index].GetValue(), 1);
                                    for (int i = 0; i < GetFlooredValue(Cards[Index].GetValue(), 1); i++)
                                    {
                                        CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerInOrder.Add(Cards[Index]);
                                    }
                                }
                                else
                                {
                                    CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].TriggerAmount += 1;
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
                                            CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].Repeat += GetRoundedValue(Cards[Index].GetValue(), 1);
                                        }
                                        break;

                                    case CatalystModifierVariant.Delay:
                                        {
                                            if (CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].Repeat > 0)
                                            {
                                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].Delay += GetRoundedValue(Cards[Index].GetValue(), 0);
                                            }
                                            else
                                            {
                                                CastProperties.Casts[CurrentCast].Blocks[CurrentBlock].Timer += GetRoundedValue(Cards[Index].GetValue(), 0);
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

            // Calculate total projectile amount of entire cast
            for (int c = 0; c < CastProperties.Casts.Count; c++)
            {
                for (int b = 0; b < CastProperties.Casts[c].Blocks.Count; b++)
                {
                    List<int> Amounts = new List<int>
                    {
                        1
                    };

                    int Slot = 0;
                    int Trig = CastProperties.Casts[c].Blocks[b].TriggerInOrder.Count;
                    int Repe = 0;
                    bool First = true;

                    for (int i = 0; i < CastProperties.Casts[c].Blocks[b].CardItems.Count; i++)
                    {
                        switch (CastProperties.Casts[c].Blocks[b].CardItems[i].Group)
                        {
                            case (byte)Groups.Multicast:
                                {
                                    for (int a = 1; a < GetFlooredValue(CastProperties.Casts[c].Blocks[b].CardItems[i].GetValue()); a++)
                                    {
                                        Amounts.Insert(Slot, Amounts[Slot]);
                                    }
                                }
                                break;

                            case (byte)Groups.Projectile:
                                {
                                    if (Repe > 0)
                                    {
                                        Amounts[Slot] *= Repe;
                                        Repe = 0;
                                    }

                                    if (Trig > 0)
                                    {
                                        Trig--;
                                    }
                                    else
                                    {
                                        if (!First)
                                        {
                                            Slot++;
                                        }
                                        else
                                        {
                                            First = false;
                                        }
                                    }
                                }
                                break;

                            case (byte)Groups.Trigger:
                                {
                                    if (CastProperties.Casts[c].Blocks[b].CardItems[i].Spell == (byte)Trigger.Trigger)
                                    {
                                        Trig += GetFlooredValue(CastProperties.Casts[c].Blocks[b].CardItems[i].GetValue());
                                    }
                                    else
                                    {
                                        Trig += 1;
                                    }
                                }
                                break;

                            case (byte)Groups.Formation:
                                {
                                    Amounts[Slot] *= GetFlooredValue(CastProperties.Casts[c].Blocks[b].CardItems[i].GetValue());
                                }
                                break;

                            case (byte)Groups.CatalystModifier:
                                {
                                    if (CastProperties.Casts[c].Blocks[b].CardItems[i].Variant == (byte)CatalystModifierVariant.Repeat)
                                    {
                                        Repe += GetFlooredValue(CastProperties.Casts[c].Blocks[b].CardItems[i].GetValue()) + 1;
                                    }
                                }
                                break;
                        }
                    }

                    CastProperties.ProjectileAmount += Amounts.Sum() * (CastProperties.Casts[c].Blocks[b].Repeat + 1);
                }
            }

            // Remove CastInfo and CastBlocks which don't have projectiles.
            for (int _CastInfo = CastProperties.Casts.Count - 1; _CastInfo >= 0; _CastInfo--)
            {
                for (int _CastBlock = CastProperties.Casts[_CastInfo].Blocks.Count - 1; _CastBlock >= 0; _CastBlock--)
                {
                    // Remove CastBlocks if it has no projectile that can be fired by the catalyst
                    if (IsCatalyst)
                    {
                        if (CastProperties.Casts[_CastInfo].Blocks[_CastBlock].CardItems.First(_CardItem => _CardItem.Group == (byte)Groups.Projectile).Spell == (byte)Projectile.MyOwnProjectileInstance)
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
                        if ((_CastBlock.Repeat * _CastBlock.Delay) + _CastBlock.Timer > CastProperties.MinimumUseTime)
                        {
                            CastProperties.MinimumUseTime = (_CastBlock.Repeat * _CastBlock.Delay) + _CastBlock.Timer;
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

            for (int i = 0; i < cast.Casts.Count; i++)
            {
                Kourindou.Instance.Logger.Debug("//----- Cast " + i + "-----//");

                for (int a = 0; a < cast.Casts[i].Blocks.Count; a++)
                {
                    Kourindou.Instance.Logger.Debug("   //----- Block " + a + " -----//");
                    Kourindou.Instance.Logger.Debug("   Repeat - " + cast.Casts[i].Blocks[a].Repeat);
                    Kourindou.Instance.Logger.Debug("   Delay - " + cast.Casts[i].Blocks[a].Delay);
                    Kourindou.Instance.Logger.Debug("   Timer - " + cast.Casts[i].Blocks[a].Timer);
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