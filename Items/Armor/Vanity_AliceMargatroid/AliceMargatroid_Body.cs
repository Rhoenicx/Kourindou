using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Kourindou.Items.Armor.Vanity_AliceMargatroid
{
    [AutoloadEquip(EquipType.Body)]
    public class AliceMargatroid_Body : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Test Shirt");
            ArmorIDs.Body.Sets.HidesArms[Item.bodySlot] = false;
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 50, 0, 0);
            Item.rare = ItemRarityID.Blue;

            // Hitbox
            Item.width = 24;
            Item.height = 20;
            
            // Armor-Specific
            Item.vanity = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            //player.GetModPlayer<GensokyoPlayer>().WearingShortGensokyoVanityBody = true;
        }
    }
}