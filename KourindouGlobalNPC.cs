using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Kourindou
{
    public class RemoveItemFromNPCShop : GlobalNPC
    {
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            // Here we remove Pink and Black Thread items from the Clothier's shop
            if (type == NPCID.Clothier)
            {
                // Remove Item form shopinventory
                for (int i = 0; i < nextSlot; i++)
                {
                    if (shop.item[i].type == ItemID.BlackThread || shop.item[i].type == ItemID.PinkThread)
                    {
                        shop.item[i] = new Item();
                    }
                }

                // Remove empty slots from shopinventory
                for (int i = nextSlot; i >= 0 ; i--)
                {
                    if (shop.item[i].type == 0)
                    {
                        for (int a = i; a < nextSlot; a++)
                        {
                            shop.item[a] = shop.item[a + 1];
                        }
                    }
                }
            }
        }
    }
}