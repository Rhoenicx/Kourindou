﻿using System;
using Terraria;
using Terraria.ID;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou.Items.Spellcards.Trajectories
{
    public class RandomTrajectory : CardItem
    {
        public override void SetStaticDefaults()
        {
            // When loading this card, register it!
            RegisterCardItem((byte)Groups.Trajectory, (byte)Trajectory.RandomTrajectory, Type);
        }

        public override void SetCardDefaults()
        {
            // Defaults of this card
            Group = (byte)Groups.Trajectory;
            Spell = (byte)Trajectory.RandomTrajectory;
            Variant = 0;
            Amount = 1f;
            AddUseTime = 0;
            AddCooldown = 0;
            AddRecharge = 0;
            AddSpread = 0f;
            FixedAngle = 0f;
            IsRandomCard = true;
            IsConsumable = false;
            NeedsProjectileCard = true;

            // Card Color
            CardColor = CardColors.Blue;

            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 20;
            Item.height = 28;
        }

        public override float GetValue(bool max = false)
        {
            return Main.rand.Next(0, Enum.GetNames(typeof(Trajectory)).Length);
        }
    }
}
