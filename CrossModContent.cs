using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using static Terraria.ModLoader.ModContent;

namespace Kourindou
{
    public static class CrossModContent
    {
        public static void SetupGensokyo(Mod Gensokyo, Mod Kourindou)
        {
            Gensokyo.Call(
                "RegisterShopAccess",
                Kourindou.Name
            );
        }

        public static void SetupHairLoader(Mod HairLoader, Mod Kourindou)
        {

        }
    }
}