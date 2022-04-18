using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Kourindou.Items.Armor.Vanity_AliceMargatroid
{
    [AutoloadEquip(EquipType.Head)]
    public class AliceMargatroid_Head : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Alice's Headband");
            ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = true; // Don't draw the head at all. Used by Space Creature Mask
            ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = false; // Draw hair as if a hat was covering the top. Used by Wizards Hat
            ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true; // Draw all hair as normal. Used by Mime Mask, Sunglasses
            ArmorIDs.Head.Sets.DrawBackHair[Item.headSlot] = false;
            ArmorIDs.Head.Sets.DrawsBackHairWithoutHeadgear[Item.headSlot] = false; 
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 50, 0, 0);
            Item.rare = ItemRarityID.Blue;

            // Hitbox
            Item.width = 26;
            Item.height = 14;
            
            // Armor-Specific
            Item.vanity = true;
        }
    }
}