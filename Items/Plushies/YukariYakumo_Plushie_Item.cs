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
            // DisplayName.SetDefault("Yukari Yakumo Plushie");
            // Tooltip.SetDefault("The mastermind gap youkai. Perhaps this doll borrows part of her power");
        }

        public override string AddEffectTooltip()
        {
            return "Teleport to the mouse cursor using skill button\r\n" +
                    "+15% damage, +50 penetration";
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

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase damage by 5 percent
            player.GetDamage(DamageClass.Generic) += 0.15f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Increase Armor penetration by 50 points
            player.GetArmorPenetration(DamageClass.Generic) += 50;

            // Teleport effect
            // Only execute if this is our own player
            if (player.whoAmI == Main.myPlayer)
            {
                // Check if hotkey has been pressed
                if (player.GetModPlayer<KourindouPlayer>().SkillKeyPressed)
                {
                    // Calculate the destination
                    Vector2 destination = new(Main.MouseWorld.X - player.width / 2f, Main.MouseWorld.Y);

                    // Inverted gravity support
                    if ((int)player.gravDir == 1)
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

                            // Inform other clients of the teleport
                            if (Main.netMode == NetmodeID.MultiplayerClient)
                            {
                                NetMessage.SendData(
                                    MessageID.TeleportEntity,
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
    }
}