using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Kourindou.Items.Plushies
{
    public abstract class Plushie : ModItem
    {
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                
            }

            return true;
        }


        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            switch(player.GetModPlayer<KourindouPlayer>().plushiePower)
            {
                case 0:
                    break;


                case 1:
                    break;


                case 2:
                    PlushieEquipEffects(player);
                    break;


            }
        }

        public override bool CanEquipAccessory(Player player, int slot)
        {
            if (slot > 0)
            {
                return false;
            }

            if (player.GetModPlayer<KourindouPlayer>().plushiePower != 2)
            {
                return false;
            }

            return true;
        }

        public override bool NewPreReforge ()
        {
            return false;
        }

        public virtual void PlushieEquipEffects(Player player)
        {

        }
    }
}