using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Items.CraftingMaterials
{
    public class SkyBlueFabric : ModItem
    {
        public override void SetStaticDefaults() 
        {
			DisplayName.SetDefault("Sky Blue Fabric");
		}

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.Silk);
            item.width = 32;
            item.height = 26;
        }

        public override void AddRecipes()
        {
            
        }
    }
}