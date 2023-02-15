using System;
using Terraria.ModLoader;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou.Items.Spellcards
{
    public abstract class CardItem : ModItem
    {
        // Group of this card
        public abstract byte Group { get; } //= (byte)Groups.Empty;

        // Spell of this card
        public abstract byte Spell { get; } //= 0;

        // Variant of this card
        public abstract byte Variant { get; }

        // Strength of this card, used for things like the amount of projectile in a formation
        public abstract float Strength { get; set; } //= 1f;

        // UseTime that this card adds to the catalyst
        public abstract int AddUseTime { get; set; } //= 0;

        // Cooldown that this card adds to the catalyst
        public abstract int AddCooldown { get; set; } //= 0;

        // Recharge that this card adds to the catalyst
        public abstract int AddRecharge { get; set; } //= 0;

        // Spread that this card adds to the catalyst
        public abstract float AddSpread { get; set; } //= 0f;

        // Angle of this card, used for formation scatter angles
        public abstract float FixedAngle { get; set; } //= 0f

        // If this card needs to be replaced by another card
        public abstract bool IsRandomCard { get; set; } //= false;

        // If this card is a cunsumable card
        public abstract bool IsConsumable { get; set; } //= false;

        // If this card needs a projectile to work
        public abstract bool NeedsProjectileCard { get; set; } //= false;

        // If this card has been inserted
        public bool IsInsertedCard { get; set; } = false;

        // Position of this card on the catalyst slots
        public int SlotPosition { get; set; } = -1;

        public override void Load()
        {
            // When loading this card, register it!
            RegisterCardItem(Group, Spell, this.Type);
            base.Load();
        }

        public virtual void ApplyMultiplication(float input)
        {
            // The input is the multiplication amount, so should be 2f to 5f
            Strength *= input;
            AddUseTime = (int)Math.Ceiling(this.AddUseTime * input);
            AddCooldown = (int)Math.Ceiling(this.AddCooldown * input);
            AddRecharge = (int)Math.Ceiling(this.AddRecharge * input);
            AddSpread *= input;
        }

        public virtual byte ApplyRandomCard()
        {
            return 255;
        }
    }
}