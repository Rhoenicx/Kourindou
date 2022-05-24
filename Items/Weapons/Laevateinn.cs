using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Projectiles.Weapons;

namespace Kourindou.Items.Weapons
{
    public class Laevateinn : MultiUseItem
    {
        public int NormalDamage = 100;

        public override bool HasNormal(Player player)
        {
            return true;
        }

        public override int GetNormalCounter(Player player)
        {
            return this._AttackCounter;
        }

        public override bool HasSkill(Player player)
        {
            return true;
        }
        public override int GetSkillID(Player player)
        {
            return 3;
        }


        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Laevateinn");
            Tooltip.SetDefault("");
            Item.staff[Item.type] = true;
        }

        public override void SetDefaults()
        {
            // info
            Item.value = Item.buyPrice(2, 38, 0, 0);
            Item.rare = ItemRarityID.Red;

            // hitbox
            Item.width = 64;
            Item.height = 64;

            // usage
            Item.useStyle = ItemUseStyleID.Thrust;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.autoReuse = true;

            // stats
            Item.damage = NormalDamage;
            Item.crit = 7;
            Item.knockBack = 1f;
            Item.DamageType = DamageClass.Melee;

            // sound
            Item.UseSound = SoundID.Item1;

            // shoot
            //Item.shoot = ProjectileType<CrescentMoonStaffStar>();
            //Item.shootSpeed = 7f;
        }
        public override bool CanUseItem(Player player)
        {
            if (_AttackID == 0)
            {
                _AttackCounter++;
                if (_AttackCounter > 5)
                {
                    _AttackCounter = 0;
                }
            }

            return base.CanUseItem(player);
        }

        public override void SetItemStats(Player player, int AttackID, int AttackCounter)
        {
            this._AttackID = AttackID;
            this._AttackCounter = AttackCounter;

            switch (AttackID)
            {
                case 0:
                    switch (AttackCounter)
                    {
                        case 0:
                            Item.useStyle = ItemUseStyleID.Thrust;
                            Item.useTime = 30;
                            Item.useAnimation = 30;
                            Item.damage = NormalDamage;
                            break;

                        case 1:
                            Item.useStyle = ItemUseStyleID.Thrust;
                            Item.useTime = 10;
                            Item.useAnimation = 10;
                            Item.damage = (int)(NormalDamage * 0.8);
                            break;

                        case 2:
                            Item.useStyle = ItemUseStyleID.Thrust;
                            Item.useTime = 10;
                            Item.useAnimation = 10;
                            Item.damage = (int)(NormalDamage * 0.8);
                            break;

                        case 3:
                            Item.useStyle = ItemUseStyleID.Thrust;
                            Item.useTime = 10;
                            Item.useAnimation = 10;
                            Item.damage = (int)(NormalDamage * 0.8);
                            break;

                        case 4:
                            Item.useStyle = ItemUseStyleID.Thrust;
                            Item.useTime = 30;
                            Item.useAnimation = 30;
                            Item.damage = (int)(NormalDamage * 1.4);
                            break;

                        case 5:
                            Item.useStyle = ItemUseStyleID.Swing;
                            Item.useTime = 40;
                            Item.useAnimation = 40;
                            Item.damage = (int)(NormalDamage * 2.0);
                            break;
                    }
                    break;

                case 3:
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
