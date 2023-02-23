using Terraria;
using Terraria.ID;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou.Items.Spellcards.Elements
{
    public class Electricity : CardItem
    {
        public override void Load()
        {
            // When loading this card, register it!
            RegisterCardItem((byte)Groups.Element, (byte)Element.Electricity, Type);
            base.Load();
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Electricity");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Defaults of this card
            Group = (byte)Groups.Element;
            Spell = (byte)Element.Electricity;
            Variant = 0;
            Amount = 1f;
            AddUseTime = 0;
            AddCooldown = 0;
            AddRecharge = 0;
            AddSpread = 0f;
            FixedAngle = 0f;
            IsRandomCard = false;
            IsConsumable = false;
            NeedsProjectileCard = false;

            // Card Color
            CardColor = CardColors.Green;

            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 20;
            Item.height = 28;
        }
    }
}
