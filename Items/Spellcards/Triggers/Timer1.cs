using Terraria;
using Terraria.ID;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou.Items.Spellcards.Triggers
{
    public class Timer1 : CardItem
    {
        public override void SetStaticDefaults()
        {
            // When loading this card, register it!
            RegisterCardItem((byte)Groups.Trigger, (byte)Trigger.Timer1, Type);
            
            // DisplayName.SetDefault("Timer 1");
            // Tooltip.SetDefault("");
        }

        public override void SetCardDefaults()
        {
            // Defaults of this card
            Group = (byte)Groups.Trigger;
            Spell = (byte)Trigger.Timer1;
            Variant = 0;
            Amount = 1f;
            Value = 60f;
            AddUseTime = 0;
            AddCooldown = 0;
            AddRecharge = 0;
            AddSpread = 0f;
            FixedAngle = 0f;
            IsRandomCard = false;
            IsConsumable = false;
            NeedsProjectileCard = true;

            // Card Color
            CardColor = CardColors.Brown;

            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 20;
            Item.height = 28;
        }
    }
}
