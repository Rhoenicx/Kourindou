using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;
using Terraria.DataStructures;
using Terraria.Map;

namespace Kourindou.Items.Plushies
{
    public class RanYakumo_Plushie_Item : PlushieItem
    {
        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Yellow;

            // Hitbox
            Item.width = 32;
            Item.height = 32;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.autoReuse = true;
            Item.useTurn = true;

            // Tile placement fields
            Item.consumable = true;
            Item.createTile = TileType<RanYakumo_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<RanYakumo_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
				.AddIngredient(ItemType<BlueFabric>(), 1)
                .AddIngredient(ItemType<BrownFabric>(), 1)
                .AddIngredient(ItemType<YellowFabric>(), 1)
                .AddIngredient(ItemID.Silk, 3)
				.AddIngredient(ItemType<BlueFabric>(), 1)
                .AddIngredient(ItemType<BrownThread>(), 1)
                .AddIngredient(ItemType<YellowThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increased minion damage by 10 percent + Stacks
            player.GetDamage(DamageClass.Generic) += 0.10f + (0.05f * player.GetModPlayer<KourindouPlayer>().RanPlushie_Stacks);

            // Increase movement speed by 10 percent + Stacks
            if (player.accRunSpeed > player.maxRunSpeed)
            {
                player.accRunSpeed *= 1.1f + (0.05f * player.GetModPlayer<KourindouPlayer>().RanPlushie_Stacks);
                player.maxRunSpeed *= 1.1f + (0.05f * player.GetModPlayer<KourindouPlayer>().RanPlushie_Stacks);
            }
            else
            {
                player.maxRunSpeed *= 1.1f + (0.05f * player.GetModPlayer<KourindouPlayer>().RanPlushie_Stacks);
                player.accRunSpeed = player.maxRunSpeed;
            }
        }

        public override void PlushiePostUpdateEquips(Player player, int amountEquipped)
        {
            // Increase player's max hp by 10% + Stacks
            player.statLifeMax2 = (int)Math.Floor((double)player.statLifeMax2 * (1.1 + (0.05 * player.GetModPlayer<KourindouPlayer>().RanPlushie_Stacks)));
        }

        public override void PlushieOnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit, int damageDone, int amountEquipped)
        {
            if (target.life <= 0 && !target.friendly && target.lifeMax > 5)
            { 
                ManageStacks(player);
            }
        }

        public override void PlushieOnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone, int amountEquipped)
        {
            if (target.life <= 0 && !target.friendly && target.lifeMax > 5)
            {
                ManageStacks(player);
            }
        }

        public void ManageStacks(Player player)
        {
            // Increase kill counter
            if (player.GetModPlayer<KourindouPlayer>().RanPlushie_Stacks < 8)
            {
                player.GetModPlayer<KourindouPlayer>().RanPlushie_EnemieKillCounter++;
            }

            // Increase stacks
            if (player.GetModPlayer<KourindouPlayer>().RanPlushie_EnemieKillCounter >= 10 && player.GetModPlayer<KourindouPlayer>().RanPlushie_Stacks < 8)
            {
                player.GetModPlayer<KourindouPlayer>().RanPlushie_Stacks++;
                player.GetModPlayer<KourindouPlayer>().RanPlushie_EnemieKillCounter = 0;

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)KourindouMessageType.RanPlushieStacks);
                    packet.Write((byte)Main.myPlayer);
                    packet.Write((byte)player.GetModPlayer<KourindouPlayer>().RanPlushie_Stacks);
                    packet.Send(-1, Main.myPlayer);
                }
            }
        }

        public override void PlushieKill(Player myPlayer, double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource, int amountEquipped)
        {
            myPlayer.GetModPlayer<KourindouPlayer>().RanPlushie_EnemieKillCounter = 0;
            myPlayer.GetModPlayer<KourindouPlayer>().RanPlushie_Stacks = 0;

            if (Main.myPlayer == myPlayer.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)KourindouMessageType.RanPlushieStacks);
                packet.Write((byte)Main.myPlayer);
                packet.Write((byte)myPlayer.GetModPlayer<KourindouPlayer>().RanPlushie_Stacks);
                packet.Send(-1, Main.myPlayer);
            }
        }
    }
}