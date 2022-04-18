using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.Plushies
{
    public class YukariYakumo_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Yukari Yakumo Plushie");
            Tooltip.SetDefault("The mastermind gap youkai. Perhaps this doll borrows part of her power");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Purple;

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
            Item.createTile = TileType<YukariYakumo_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;

        }
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<YukariYakumo_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase damage by 5 percent
            player.GetDamage(DamageClass.Generic) += 0.05f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Increase Armor penetration by 50 points
            player.armorPenetration += 50;

            // Teleport effect
            // Only execute if this is our own player
            if (player.whoAmI == Main.myPlayer)
            {
                // Check if hotkey has been pressed
                if (player.GetModPlayer<KourindouPlayer>().YukariYakumoTPKeyPressed)
                {
                    // Calculate the destination
                    Vector2 destination = new Vector2(Main.MouseWorld.X - player.width / 2f, Main.MouseWorld.Y);

                    // Inverted gravity support
                    if ((int) player.gravDir == 1)
                    {
                        destination.Y -= player.height;
                    }

                    // Check if we are currently out of bounds of the map
                    if (destination.X > 50f && destination.X < (Main.maxTilesX * 16) - 50 && destination.Y > 50f && destination.Y < (Main.maxTilesY * 16) - 50)
                    {
                        // Check if the destination is inside solid blocks
                        if (!Collision.SolidCollision(destination, player.width, player.height))
                        {   
                            // Teleport player
                            player.Teleport(destination, 1, 0);

                            // Deal damage if the player already has this debuff, aka used tp before
                            if (player.HasBuff(BuffID.ChaosState))
                            {
                                player.Hurt(
                                    (int)Main.rand.Next(0,2) == 0 ? PlayerDeathReason.ByCustomReason(player.name + " played with the boundaries too much") :
                                        PlayerDeathReason.ByCustomReason(player.name + " breached the boundary between life and death"), 
                                    (int)Math.Ceiling(((player.statLifeMax2 * 0.1) / (1 - player.endurance) + (Main.expertMode ? player.statDefense * 0.75 : player.statDefense * 0.5))),
                                    0,
                                    false,
                                    false,
                                    false,
                                    -1);
                            }

                            // Add ChaosState buff
                            player.AddBuff(BuffID.ChaosState, 300);

                            // Inform other clients of the teleport
                            if (Main.netMode == NetmodeID.MultiplayerClient)
                            {
                                NetMessage.SendData(
                                    MessageID.Teleport,
                                    -1, -1, null,
                                    0,
                                    player.whoAmI,
                                    destination.X,
                                    destination.Y,
                                    1);
                            }
                        }
                    }
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<SilverFabric>(), 2)
                .AddIngredient(ItemType<YellowFabric>(), 2)
                .AddIngredient(ItemType<PurpleFabric>(), 1)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<PurpleThread>(), 1)
                .AddIngredient(ItemType<SilverThread>(), 2)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }
    }
}