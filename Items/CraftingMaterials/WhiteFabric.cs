using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Items.CraftingMaterials
{
    public class WhiteFabric : ModItem
    {
        public override void SetStaticDefaults() 
        {
			DisplayName.SetDefault("White Fabric");
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