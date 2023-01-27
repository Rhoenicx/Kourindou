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
    public class PatchouliKnowledge_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Patchouli Knowledge Plushie");
            Tooltip.SetDefault("The magician of the scarlet mansion");
        }

        public override string AddEffectTooltip()
        {
            return "Doubled magic damage but non-magic attacks deal no damage\r\n" +
                    "Movement speed is halved, +30% magic crit, -50% mana cost";
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.LightPurple;

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
            Item.createTile = TileType<PatchouliKnowledge_Plushie_Tile>();
            
            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<PatchouliKnowledge_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.Book, 1)
                .AddIngredient(ItemType<PinkFabric>(), 2)
                .AddIngredient(ItemType<PurpleFabric>(), 1)
                .AddIngredient(ItemType<VioletFabric>(), 2)
                .AddIngredient(ItemID.Silk, 1)
                .AddIngredient(ItemID.PinkThread, 1)
                .AddIngredient(ItemType<PurpleThread>(), 1)
                .AddIngredient(ItemType<VioletThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase magic crit rate by 30 percent
            player.GetCritChance(DamageClass.Magic) += 30;

            // Reduce mana consumption by 50 percent
            player.manaCost -= 0.50f;
        }

        public override void PlushiePostUpdateEquips(Player player, int amountEquipped)
        {
            // Increase Magic damage by 2 times
            player.GetDamage(DamageClass.Magic) *= 2.00f;

            // All other damage types deal zero, really into negatives because other mods might increase this
            player.GetDamage(DamageClass.Ranged).Flat = 0f;
            player.GetDamage(DamageClass.Summon).Flat = 0f;
            player.GetDamage(DamageClass.Melee).Flat = 0f;
            player.GetDamage(DamageClass.Throwing).Flat = 0f;

            // reduce movespeed
            if (player.accRunSpeed > player.maxRunSpeed)
            {
                player.accRunSpeed *= 0.5f;
                player.maxRunSpeed *= 0.5f;
            }
            else
            {
                player.maxRunSpeed *= 0.5f;
                player.accRunSpeed = player.maxRunSpeed;
            }
        }

        public override bool? PlushieCanHit(Player myPlayer, Item item, Projectile proj, NPC npc, Player player, int amountEquipped)
        {
            if ((item != null && (item.CountsAsClass(DamageClass.Melee) || item.CountsAsClass(DamageClass.Ranged) || item.CountsAsClass(DamageClass.Throwing)))
                || (proj != null && (proj.CountsAsClass(DamageClass.Melee) || proj.CountsAsClass(DamageClass.Ranged) || proj.CountsAsClass(DamageClass.Throwing) || proj.minion)))
            {
                return false;
            }

            return base.PlushieCanHit(myPlayer, item, proj, npc, player, amountEquipped);
        }
    }
}