using System;
using System.Linq;
using System.Collections;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Items.Consumables
{
    public class FumoCola : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fumo Cola");
            Tooltip.SetDefault("Coca Cola es Fumo");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 16;
            Item.height = 28;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.noUseGraphic = false;

            // Item consumable and stack size
            Item.maxStack = 99;
            Item.consumable = true;

            // Usesound
            Item.UseSound = new SoundStyle("Kourindou/Sounds/Custom/Soda") with { Volume = 0.66f, PitchVariance = 1f };
        }

        public override bool? UseItem(Player player)
        {
            player.GetModPlayer<KourindouPlayer>().FumoColaAnimationTimer = Item.useAnimation;

            if (player.whoAmI == Main.myPlayer)
            {
                if (player.GetModPlayer<KourindouPlayer>().EquippedPlushies.Count < 1)
                {
                    // Needs to be Red's potion...
                    player.AddBuff(BuffID.Slow, 60);
                }
                else
                {
                    int[] plushieIDs = player.GetModPlayer<KourindouPlayer>().EquippedPlushies.ToArray();
                    int FumoColaTurnIntoPlushieID = plushieIDs[Main.rand.Next(plushieIDs.Length)];
                    player.GetModPlayer<KourindouPlayer>().FumoColaTurnIntoPlushieID = FumoColaTurnIntoPlushieID;

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        ModPacket packet = Mod.GetPacket();
                        packet.Write((byte) KourindouMessageType.FumoCola);
                        packet.Write((int) FumoColaTurnIntoPlushieID);
                        packet.Send();
                    }
                }
            }

            return true;
        }
    }
}
