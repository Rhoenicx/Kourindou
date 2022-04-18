using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Kourindou.Items.Armor.Vanity_AliceMargatroid
{
    [AutoloadEquip(EquipType.Legs)]
    public class AliceMargatroid_Legs : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Alice's Skirt");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 50, 0, 0);
            Item.rare = ItemRarityID.Blue;

            // Hitbox
            Item.width = 20;
            Item.height = 14;

            // Armor-Specific
            Item.vanity = true;
        }

        public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
        {
            if (!male)
            {
                //equipSlot = mod.GetEquipSlot("AliceMargatroid_Legs_FemaleLegs", EquipType.Legs);
            }
            else
            {
                //equipSlot = mod.GetEquipSlot("AliceMargatroid_Legs_Legs", EquipType.Legs);
            }
        }

        //public override void UpdateVanity(Player player, EquipType type)
        //{
            //player.GetModPlayer<GensokyoPlayer>().WearingGensokyoVanityLegs = true;
        //}
    }   
}