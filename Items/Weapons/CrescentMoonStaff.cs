using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Projectiles.Weapons;

namespace Kourindou.Items.Weapons
{
    public class CrescentMoonStaff : MultiUseItem
    {
        // Config
        public const int FlameAmount = 4;
        public const int NormalDamage = 45;
        public const int FlameDamage = 30;
        public const int LaserDamage = 60;
        public float FlameMultiplier = (float)FlameDamage / (float)NormalDamage;
        public float LaserMultiplier = (float)LaserDamage / (float)NormalDamage;

        // Spawn Counter
        public int NormalCounter = 0;

        public override bool HasNormal(Player player)
        {
            return true;
        }
        public override int GetNormalCounter(Player player)
        {     
            return player.GetModPlayer<KourindouPlayer>().CrescentMoonStaffFlames.Count;
        }
        public override bool HasSkill(Player player)
        {
            return player.GetModPlayer<KourindouPlayer>().CrescentMoonStaffFlames.Count < FlameAmount;
        }
        public override int GetSkillID(Player player)
        {
            return 1;
        }


        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crescent Moon Staff");
            Tooltip.SetDefault("");
            Item.staff[Item.type] = true;
        }

        public override void SetDefaults()
        {
            // info
            Item.value = Item.buyPrice(0, 34, 0, 0);
            Item.rare = ItemRarityID.Yellow;

            // hitbox
            Item.width = 64;
            Item.height = 64;

            // usage
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 50;
            Item.useAnimation = 50;
            Item.autoReuse = true;
            Item.noMelee = true;

            // stats
            Item.damage = NormalDamage;
            Item.crit = 4;
            Item.knockBack = 1f;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 3;

            // sound
            Item.UseSound = SoundID.Item43;

            // shoot
            Item.shoot = ProjectileType<CrescentMoonStaffStar>();
            Item.shootSpeed = 7f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            switch (this._AttackID)
            {
                case 0:
                    if (NormalCounter > player.GetModPlayer<KourindouPlayer>().CrescentMoonStaffFlames.Count)
                    {
                        NormalCounter = 0;
                    }

                    if (NormalCounter == 0 || player.GetModPlayer<KourindouPlayer>().CrescentMoonStaffFlames.ContainsKey(NormalCounter - 1))
                    {
                        Projectile.NewProjectile(
                            source,
                            NormalCounter == 0 ? player.Center + (Vector2.Normalize(velocity) * 56f) : Main.projectile[player.GetModPlayer<KourindouPlayer>().CrescentMoonStaffFlames[NormalCounter - 1]].Center,
                            NormalCounter == 0 ? velocity : Vector2.Normalize(Main.projectile[player.GetModPlayer<KourindouPlayer>().CrescentMoonStaffFlames[NormalCounter - 1]].Center.DirectionTo(Main.MouseWorld)) * velocity.Length(),
                            type,
                            damage,
                            knockback,
                            player.whoAmI,
                            NormalCounter,
                            Main.rand.Next(0, 2)
                        );
                    }

                    NormalCounter++;
                    break;

                case 1:
                    // Get the stats of the current flames
                    Vector2 rotation = new Vector2(1f, 0f);
                    short direction = (short)Main.rand.Next(0, 2);
                    foreach (KeyValuePair<int, int> pair in player.GetModPlayer<KourindouPlayer>().CrescentMoonStaffFlames)
                    {
                        rotation = Main.projectile[pair.Value].velocity;
                        direction = (short)((int)Main.projectile[pair.Value].ai[0] & 0xffff);
                        break;
                    }

                    // Detect the first free slot
                    int free = 0;
                    while (player.GetModPlayer<KourindouPlayer>().CrescentMoonStaffFlames.ContainsKey(free))
                    {
                        free++;
                    }

                    int ID = Projectile.NewProjectile(
                        source,
                        player.Center,
                        Vector2.Normalize(rotation),
                        ProjectileType<CrescentMoonStaffFlame>(),
                        (int)(damage * FlameMultiplier),
                        knockback,
                        player.whoAmI,
                        direction + ((player.GetModPlayer<KourindouPlayer>().CrescentMoonStaffFlames.Count + 1) << 16),
                        free
                    );
                    player.GetModPlayer<KourindouPlayer>().CrescentMoonStaffFlames.Add(free, ID);

                    break;

                default:
                    base.Shoot(player, source, position, velocity, type, damage, knockback);
                    break;
            }

            return false;
        }

        public override void SetItemStats(Player player, int AttackID, int AttackCounter)
        {
            this._AttackID = AttackID;
            this._AttackCounter = AttackCounter;

            switch (AttackID)
            {
                case 0:
                    Item.useStyle = ItemUseStyleID.Shoot;
                    Item.useTime = (int)(50 / Math.Pow(1.5, AttackCounter));
                    Item.useAnimation = (int)(50 / Math.Pow(1.5, AttackCounter));
                    break;

                case 1:
                    Item.useStyle = ItemUseStyleID.Swing;
                    Item.useTime = 45;
                    Item.useAnimation = 45;
                    break;

                default:
                    base.SetItemStats(player, AttackID, AttackCounter);
                    break;
            }
        }
    }
}
