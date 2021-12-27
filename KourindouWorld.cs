using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;

namespace Kourindou
{
    public class KourindouWorld : ModWorld
    {
        // Cotton Plants
        public int CottonPlants = 0;

        private const int Small_CottonMax = 50;
        private const int Medium_CottonMax = 75;
        private const int Large_CottonMax = 100;

        // Flax Plants
        public int FlaxPlants = 0;

        private const int Small_FlaxMax = 50;
        private const int Medium_FlaxMax = 75;
        private const int Large_FlaxMax = 100;

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            int index = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
            tasks.Insert(index + 1, new PassLegacy("[Kourindou] Placing Cotton", PlacingCottonPlants));
            tasks.Insert(index + 1, new PassLegacy("[Kourindou] Placing Flax", PlacingFlaxPlants));
        }

        public void PlacingCottonPlants(GenerationProgress progress = null)
        {

        }

        public void PlacingFlaxPlants(GenerationProgress progress = null)
        {

        }
    }
}