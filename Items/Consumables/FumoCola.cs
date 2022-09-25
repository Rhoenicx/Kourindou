using System;
using System.Linq;
using System.Collections;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Buffs;

namespace Kourindou.Items.Consumables
{
    public class FumoCola : ModItem
    {

        public static float OpeningProgress = 0.8f;
        public static float DrinkProgress = 0.3f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fumo Cola");
            Tooltip.SetDefault("Coca Cola is Fumo");
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
            Item.useTime = 90;
            Item.useAnimation = 90;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.noUseGraphic = true;

            // Item consumable and stack size
            Item.maxStack = 99;
            Item.consumable = true;

            // Buff
            Item.buffType = BuffType<Buff_FumoCola>();
            Item.buffTime = 36000;

            // Usesound
            Item.UseSound = new SoundStyle("Kourindou/Sounds/Custom/Soda") with { Volume = 0.66f, PitchVariance = 1f };
        }

        public override bool? UseItem(Player player)
        {
            player.GetModPlayer<KourindouPlayer>().FumoColaAnimationTimer = Item.useAnimation;

            if (player.whoAmI == Main.myPlayer)
            {
                if (player.GetModPlayer<KourindouPlayer>().EquippedPlushies.Count > 0)
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
