using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Items.Spellcards
{
    public abstract class CardItem : ModItem
    {
        // Group of this card
        public abstract byte Group { get; set; }

        // Spell of this card
        public abstract short Spell { get; set; }

        // Strength of this card, used for things like the amount of projectile in a formation
        public abstract float Strength { get; set; }

        // Angle of this card, used for formation scatter angles
        public abstract float Angle { get; set; }

        // UseTime that this card adds to the catalyst
        public abstract int AddUseTime { get; set; }

        // Cooldown that this card adds to the catalyst
        public abstract int AddCooldown { get; set; }

        // Spread that this card adds to the catalyst
        public abstract float AddSpread { get; set; }

        // If this card needs to be replaced by another card
        public abstract bool IsRandomCard { get; set; }     

        public override void Load()
        {
            // When loading the mod add this Item
            KourindouSpellcardSystem.AddCardItem(Group, Spell, this);
            base.Load();
        }

        public CardInfo GetCardInfo()
        {
            return new CardInfo()
            {
                Group = Group,
                Spell = Spell,
                Strength = Strength,
                Angle = Angle,
                AddUseTime = AddUseTime,
                AddCooldown = AddCooldown,
                AddSpread = AddSpread,
                IsRandomCard = IsRandomCard
            };
        }
    }
}
