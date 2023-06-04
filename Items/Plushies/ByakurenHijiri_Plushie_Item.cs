using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.Plushies
{
    public class ByakurenHijiri_Plushie_Item : PlushieItem
    {
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
            Item.createTile = TileType<ByakurenHijiri_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<ByakurenHijiri_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 2)
                .AddIngredient(ItemType<BrownFabric>(), 2)
                .AddIngredient(ItemType<PurpleFabric>(), 1)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemType<BrownThread>(), 2)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase Life regen by +1 
            player.lifeRegen += 1;

            // Reduce damage from all sources by 50%
            player.GetDamage(DamageClass.Generic) -= 0.50f;

            // but increase all melee damage with 100% (+50% offset from generic)
            player.GetDamage(DamageClass.Melee) += 1.50f;

            // Increase max HP by 25%
            player.statLifeMax2 = (int)Math.Floor((double)player.statLifeMax2 * 1.25);
        }

        public override void PlushiePostUpdateEquips(Player player, int amountEquipped)
        {
            // Set critrate to 0, generic has 4% as default
            player.GetCritChance(DamageClass.Default) = 0f;
            player.GetCritChance(DamageClass.Melee) = 0f;
            player.GetCritChance(DamageClass.Magic) = 0f;
            player.GetCritChance(DamageClass.Ranged) = 0f;
            player.GetCritChance(DamageClass.Summon) = 0f;
            player.GetCritChance(DamageClass.MagicSummonHybrid) = 0f;
            player.GetCritChance(DamageClass.Throwing) = 0f;
            player.GetCritChance(DamageClass.MeleeNoSpeed) = 0f;
            player.GetCritChance(DamageClass.SummonMeleeSpeed) = 0f;
        }

        public override void PlushieModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers, int amountEquipped)
        {
            if (item.CountsAsClass(DamageClass.Melee))
            {
                modifiers.CritDamage *= 2f;
                modifiers.Knockback *= 3f;
            }
        }

        public override void PlushieModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers, int amountEquipped)
        {
            if (proj.CountsAsClass(DamageClass.Melee))
            {
                modifiers.CritDamage *= 2f;
                modifiers.Knockback *= 3f;
            }
        }

        public override void PlushieModifyWeaponCrit(Player myPlayer, Item item, ref float crit, int amountEquipped)
        {
            crit = 4f;
        }
    }
}