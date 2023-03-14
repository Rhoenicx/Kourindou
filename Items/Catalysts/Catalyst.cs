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
using Kourindou.Items.Spellcards.Empty;

namespace Kourindou.Items.Catalysts
{
    public class Catalyst : ModItem
    {
        // Cards that are placed on this Catalyst
        public int CardSlotAmount = 1;
        public List<CardItem> CardItemsOnCatalyst = new List<CardItem>();

        public override ModItem Clone(Item newEntity)
        {
            if (newEntity.ModItem is Catalyst newCatalyst)
            {
                newCatalyst.CardItemsOnCatalyst = new List<CardItem>(CardItemsOnCatalyst);
            }

            return base.Clone(newEntity);
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("CardSlotAmount", CardSlotAmount);

            for (int i = 0; i < CardSlotAmount; i++)
            {
                if (CardItemsOnCatalyst.ElementAtOrDefault(i) == null)
                {
                    CardItemsOnCatalyst.Insert(i, GetCardItem((byte)Groups.Empty, 0));
                }

                tag.Add("Card[" + i + "].Group", CardItemsOnCatalyst[i].Group);
                tag.Add("Card[" + i + "].Spell", CardItemsOnCatalyst[i].Spell);
                tag.Add("Card[" + i + "].Stack", CardItemsOnCatalyst[i].Item.stack);
            
                Kourindou.Instance.Logger.Debug("SaveData - " + i + " - " + CardItemsOnCatalyst[i].Item.Name);
            }
        }

        public override void LoadData(TagCompound tag)
        {
            // Load the amount of cards this catalyst may hold
            CardSlotAmount = tag.GetInt("CardSlotAmount");

            for (int i = 0; i < CardSlotAmount; i++)
            {
                if (CardItemsOnCatalyst.ElementAtOrDefault(i) == null)
                {
                    CardItemsOnCatalyst.Insert(i, GetCardItem((byte)Groups.Empty, 0));
                }

                byte Group = tag.GetByte("Card[" + i + "].Group");
                byte Spell = tag.GetByte("Card[" + i + "].Spell");
                int Stack = tag.GetInt("Card[" + i + "].Stack");

                CardItemsOnCatalyst[i] = GetCardItem(Group, Spell, Stack);
                Kourindou.Instance.Logger.Debug("LoadData - " + i + " - " + CardItemsOnCatalyst[i].Item.Name);              
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Testing Catalyst");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 20;
            Item.height = 28;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.autoReuse = false;
            Item.useTurn = true;

            // Cards
            CardSlotAmount = 1;

            // Base Cards
            for (int i = 0; i < CardSlotAmount; i++)
            {
                if (CardItemsOnCatalyst.ElementAtOrDefault(i) == null)
                {
                    CardItemsOnCatalyst.Insert(i, GetCardItem((byte)Groups.Empty, 0));
                }
            }
        }

        public override bool CanUseItem(Player player)
        {
            CardItemsOnCatalyst[1] = GetCardItem((byte)Groups.ProjectileModifier, (byte)ProjectileModifier.Acceleration);
            return false;

            List<CardItem> CatalystCards = new List<CardItem>()
            {
/* 00 */        GetCardItem((byte)Groups.ProjectileModifier,    (byte)ProjectileModifier.Acceleration               ),
/* 01 */        GetCardItem((byte)Groups.Trigger,               (byte)Trigger.Trigger                               ),
/* 02 */        GetCardItem((byte)Groups.Empty,                 0                                                   ),
/* 03 */        GetCardItem((byte)Groups.Multicast,             (byte)Multicast.DoubleCast                          ),
/* 04 */        GetCardItem((byte)Groups.Multicast,             (byte)Multicast.DoubleCast                          ),
/* 05 */        GetCardItem((byte)Groups.Empty,                 0                                                   ),
/* 06 */        GetCardItem((byte)Groups.Formation,             (byte)Formation.Octagon                             ),
/* 07 */        GetCardItem((byte)Groups.Projectile,            (byte)KourindouSpellcardSystem.Projectile.ShotBlue  ),
/* 08 */        GetCardItem((byte)Groups.Projectile,            (byte)KourindouSpellcardSystem.Projectile.ShotBlue  ),
/* 09 */        GetCardItem((byte)Groups.ProjectileModifier,    (byte)ProjectileModifier.Explosion                  ),
/* 10 */        GetCardItem((byte)Groups.Projectile,            (byte)KourindouSpellcardSystem.Projectile.ShotBlue  ),
/* 11 */        GetCardItem((byte)Groups.Projectile,            (byte)KourindouSpellcardSystem.Projectile.ShotBlue  ),
/* 12 */        GetCardItem((byte)Groups.Empty,                 0                                                   ),
/* 13 */        GetCardItem((byte)Groups.Empty,                 0                                                   ),
/* 14 */        GetCardItem((byte)Groups.ProjectileModifier,    (byte)ProjectileModifier.BounceDown                 ),
/* 15 */        GetCardItem((byte)Groups.Projectile,            (byte)KourindouSpellcardSystem.Projectile.ShotBlue  ),
/* 16 */        GetCardItem((byte)Groups.ProjectileModifier,    (byte)ProjectileModifier.LifetimeUp                 ),
/* 17 */        GetCardItem((byte)Groups.Projectile,            (byte)KourindouSpellcardSystem.Projectile.ShotBlue  ),
/* 18 */        GetCardItem((byte)Groups.Empty,                 0                                                   ),
/* 19 */        GetCardItem((byte)Groups.Projectile,            (byte)KourindouSpellcardSystem.Projectile.ShotBlue  ),
/* 20 */        GetCardItem((byte)Groups.Empty,                 0                                                   ),
            };

            SetSlotPositions(ref CatalystCards);

            Cast cast = GenerateCast(
                CatalystCards, 
                GetCardItem((byte)Groups.ProjectileModifier, (byte)ProjectileModifier.Snowball), 
                true, 
                0, 
                2,
                true
            );

            //----- DEBUG -----//

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

            return !cast.FailedToCast;
        }

    }
}
