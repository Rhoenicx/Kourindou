using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Kourindou.Tiles.Plushies;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou.Items.Spellcards.Empty
{
    public class EmptyCard : CardItem
    {
        public override void SetStaticDefaults()
        {
            // When loading this card, register it!
            RegisterCardItem((byte)Groups.Empty, 0, Type);
            
            DisplayName.SetDefault("Empty Spellcard");
            Tooltip.SetDefault("You shouldn't have this you CHEATER!");
        }

        public override void SetCardDefaults()
        {
            // Defaults of this card
            Group = (byte)Groups.Empty;
            Spell = 0;
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

            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 20;
            Item.height = 28;
        }
    }
}
