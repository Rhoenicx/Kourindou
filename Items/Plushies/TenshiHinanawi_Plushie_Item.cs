using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;
using Terraria.Map;
using Terraria.WorldBuilding;

namespace Kourindou.Items.Plushies
{
    public class TenshiHinanawi_Plushie_Item : PlushieItem
    {
        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Blue;

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
            Item.createTile = TileType<TenshiHinanawi_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true; 
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<TenshiHinanawi_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<RainbowFabric>(), 1)
                .AddIngredient(ItemType<BlackFabric>(), 1)
                .AddIngredient(ItemType<BlueFabric>(), 2)
                .AddIngredient(ItemType<SkyBlueFabric>(), 2)
                .AddIngredient(ItemID.Silk, 1)
                .AddIngredient(ItemType<RainbowThread>(), 1)
                .AddIngredient(ItemID.BlackThread, 1)
                .AddIngredient(ItemType<BlueThread>(), 2)
                .AddIngredient(ItemType<SkyBlueThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 1)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase damage by 20%
            player.GetDamage(DamageClass.Generic) += 0.20f;
        }

        public override void PlushiePostUpdateEquips(Player player, int amountEquipped)
        {
            // Increase life regen by 50%
            player.lifeRegen += (int)Math.Floor(player.lifeRegen * 0.5);

            // Increase mana by 100%
            player.statManaMax2 += player.statManaMax2;
        }

        public override void PlushieOnHurt(Player player, Player.HurtInfo info, int amountEquipped)
        {
            player.GetModPlayer<KourindouPlayer>().TenshiPlushie_Damage = info.Damage;
            player.GetModPlayer<KourindouPlayer>().TenshiPlushie_Revenge = true;
        }

        public override void PlushieModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers, int amountEquipped)
        {
            if (player.GetModPlayer<KourindouPlayer>().TenshiPlushie_Revenge)
            {
                modifiers.SourceDamage += player.GetModPlayer<KourindouPlayer>().TenshiPlushie_Damage * 5;
                player.GetModPlayer<KourindouPlayer>().TenshiPlushie_Revenge = false;
            }
        }

        public override void PlushieModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers, int amountEquipped)
        {
            if (player.GetModPlayer<KourindouPlayer>().TenshiPlushie_Revenge)
            {
                modifiers.SourceDamage += player.GetModPlayer<KourindouPlayer>().TenshiPlushie_Damage * 5;
                player.GetModPlayer<KourindouPlayer>().TenshiPlushie_Revenge = false;
            }
        }
    }
}