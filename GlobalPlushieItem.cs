using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using TerraUI.Utilities;
using Kourindou.Items.Plushies;

namespace Kourindou {
    internal class GlobalPlushieItem : GlobalItem 
    {
        public override bool CanRightClick(Item item) 
        { 
            if (item.modItem == null)
            {
                return base.CanRightClick(item);
            }

            return (item.modItem.GetType().IsSubclassOf(typeof(Plushie)) && !Kourindou.OverrideRightClick() && Main.LocalPlayer.GetModPlayer<KourindouPlayer>().plushiePower == 2);  
        }

        public override void RightClick(Item item, Player player) 
        {
            if (!CanRightClick(item)) 
            {
                return;
            }
            
            player.GetModPlayer<KourindouPlayer>().EquipPlushie(false, item);
        }
    }
}