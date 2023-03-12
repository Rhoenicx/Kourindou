using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Kourindou.KourindouSpellcardSystem;
using System.Collections.Generic;
using Kourindou.Items.Spellcards;

namespace Kourindou.Items.Catalysts
{
    public class Catalyst : ModItem
    {

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
        }

        public override bool CanUseItem(Player player)
        {
            List<CardItem> CatalystCards = CardToCardItemList(new List<Card>
            {
/* 00 */        new Card() { Group = (byte)Groups.ProjectileModifier, Spell = (byte)ProjectileModifier.Acceleration },
/* 01 */        new Card() { Group = (byte)Groups.Trigger, Spell = (byte)Trigger.Trigger },
/* 02 */        new Card() { Group = (byte)Groups.Empty, Spell = 0 },
/* 03 */        new Card() { Group = (byte)Groups.Multicast, Spell = (byte)Multicast.DoubleCast },
/* 04 */        new Card() { Group = (byte)Groups.Multicast, Spell = (byte)Multicast.DoubleCast },
/* 05 */        new Card() { Group = (byte)Groups.Empty, Spell = 0 },
/* 06 */        new Card() { Group = (byte)Groups.Formation, Spell = (byte)Formation.Octagon },
/* 07 */        new Card() { Group = (byte)Groups.Projectile, Spell = (byte)KourindouSpellcardSystem.Projectile.ShotBlue },
/* 08 */        new Card() { Group = (byte)Groups.Projectile, Spell = (byte)KourindouSpellcardSystem.Projectile.ShotBlue },
/* 09 */        new Card() { Group = (byte)Groups.ProjectileModifier, Spell = (byte)ProjectileModifier.Explosion },
/* 10 */        new Card() { Group = (byte)Groups.Projectile, Spell = (byte)KourindouSpellcardSystem.Projectile.ShotBlue },
/* 11 */        new Card() { Group = (byte)Groups.Projectile, Spell = (byte)KourindouSpellcardSystem.Projectile.ShotBlue },
/* 12 */        new Card() { Group = (byte)Groups.Empty, Spell = 0 },
/* 13 */        new Card() { Group = (byte)Groups.Empty, Spell = 0 },
/* 14 */        new Card() { Group = (byte)Groups.ProjectileModifier, Spell = (byte)ProjectileModifier.BounceDown },
/* 15 */        new Card() { Group = (byte)Groups.Projectile, Spell = (byte)KourindouSpellcardSystem.Projectile.ShotBlue },
/* 16 */        new Card() { Group = (byte)Groups.ProjectileModifier, Spell = (byte)ProjectileModifier.LifetimeUp },
/* 17 */        new Card() { Group = (byte)Groups.Projectile, Spell = (byte)KourindouSpellcardSystem.Projectile.ShotBlue },
/* 18 */        new Card() { Group = (byte)Groups.Empty, Spell = 0 },
/* 19 */        new Card() { Group = (byte)Groups.Projectile, Spell = (byte)KourindouSpellcardSystem.Projectile.ShotBlue },
/* 20 */        new Card() { Group = (byte)Groups.Empty, Spell = 0 },
            });

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
