using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using static Terraria.ModLoader.ModContent;
using Kourindou;

namespace Kourindou
{
    public static class CrossModContent
    {
        public static void SetupGensokyo(Mod Gensokyo, Mod _Kourindou)
        {
            // Mod call for accessing shop
            Gensokyo.Call(
                "RegisterShopAccess",
                _Kourindou.Name
            );

            // Checking NPC's and saving their type IDs
            Gensokyo.TryFind<ModNPC>("AliceMargatroid", out ModNPC Alice);
            if (Alice != null)
            {
                Kourindou.Gensokyo_AliceMargatroid_Type = Alice.Type;
            }

            Gensokyo.TryFind<ModNPC>("Cirno", out ModNPC Cirno);
            if (Cirno != null)
            {
                Kourindou.Gensokyo_Cirno_Type = Cirno.Type;
            }

            Gensokyo.TryFind<ModNPC>("EternityLarva", out ModNPC Larva);
            if (Larva != null)
            {
                Kourindou.Gensokyo_EternityLarva_Type = Larva.Type;
            }

            Gensokyo.TryFind<ModNPC>("HinaKagiyama", out ModNPC Hina);
            if (Hina != null)
            {
                Kourindou.Gensokyo_HinaKagiyama_Type = Hina.Type;
            }

            Gensokyo.TryFind<ModNPC>("KaguyaHouraisan", out ModNPC Kaguya);
            if (Kaguya != null)
            {
                Kourindou.Gensokyo_KaguyaHouraisan_Type = Kaguya.Type;
            }

            Gensokyo.TryFind<ModNPC>("Kisume", out ModNPC Kisume);
            if (Kisume != null)
            {
                Kourindou.Gensokyo_Kisume_Type = Kisume.Type;
            }

            Gensokyo.TryFind<ModNPC>("LilyWhite", out ModNPC Lily);
            if (Lily != null)
            {
                Kourindou.Gensokyo_LilyWhite_Type = Lily.Type;
            }

            Gensokyo.TryFind<ModNPC>("MayumiJoutouguu", out ModNPC Mayumi);
            if (Mayumi != null)
            {
                Kourindou.Gensokyo_MayumiJoutouguu_Type = Mayumi.Type;
            }

            Gensokyo.TryFind<ModNPC>("MedicineMelancholy", out ModNPC Medicine);
            if (Medicine != null)
            {
                Kourindou.Gensokyo_MedicineMelancholy_Type = Medicine.Type;
            }

            Gensokyo.TryFind<ModNPC>("MinamitsuMurasa", out ModNPC Murasa);
            if (Murasa != null)
            {
                Kourindou.Gensokyo_MinamitsuMurasa_Type = Murasa.Type;
            }

            Gensokyo.TryFind<ModNPC>("NitoriKawashiro", out ModNPC Nitori);
            if (Nitori != null)
            {
                Kourindou.Gensokyo_NitoriKawashiro_Type = Nitori.Type;
            }

            Gensokyo.TryFind<ModNPC>("Rumia", out ModNPC Rumia);
            if (Rumia != null)
            {
                Kourindou.Gensokyo_Rumia_Type = Rumia.Type;
            }

            Gensokyo.TryFind<ModNPC>("SakuyaIzayoi", out ModNPC Sakuya);
            if (Sakuya != null)
            {
                Kourindou.Gensokyo_SakuyaIzayoi_Type = Sakuya.Type;
            }

            Gensokyo.TryFind<ModNPC>("SeijaKijin", out ModNPC Seija);
            if (Seija != null)
            {
                Kourindou.Gensokyo_SeijaKijin_Type = Seija.Type;
            }

            Gensokyo.TryFind<ModNPC>("Sekibanki", out ModNPC Sekibanki);
            if (Sekibanki != null)
            {
                Kourindou.Gensokyo_Sekibanki_Type = Sekibanki.Type;
            }

            Gensokyo.TryFind<ModNPC>("TenshiHinanawi", out ModNPC Tenshi);
            if (Tenshi != null)
            {
                Kourindou.Gensokyo_TenshiHinanawi_Type = Tenshi.Type;
            }

            Gensokyo.TryFind<ModNPC>("ToyosatomimiNoMiko", out ModNPC Miko);
            if (Miko != null)
            {
                Kourindou.Gensokyo_ToyosatomimiNoMiko_Type = Miko.Type;
            }

            Gensokyo.TryFind<ModNPC>("UtsuhoReiuji", out ModNPC Utsuho);
            if (Utsuho != null)
            {
                Kourindou.Gensokyo_UtsuhoReiuji_Type = Utsuho.Type;
            }

            Gensokyo.TryFind<ModNPC>("Seiran", out ModNPC Seiran);
            if (Seiran != null)
            {
                Kourindou.Gensokyo_Seiran_Type = Seiran.Type;
            }

            Gensokyo.TryFind<ModNPC>("KoishiKomeiji", out ModNPC Koishi);
            if (Koishi != null)
            {
                Kourindou.Gensokyo_KoishiKomeiji_Type = Koishi.Type;
            }

            Gensokyo.TryFind<ModNPC>("CasterDoll", out ModNPC Caster);
            if (Caster != null)
            {
                Kourindou.Gensokyo_CasterDoll_Type = Caster.Type;
            }

            Gensokyo.TryFind<ModNPC>("LancerDoll", out ModNPC Lancer);
            if (Lancer != null)
            {
                Kourindou.Gensokyo_LancerDoll_Type = Lancer.Type;
            }

            Gensokyo.TryFind<ModNPC>("Fairy_Bone", out ModNPC Fairy_Bone);
            if (Fairy_Bone != null)
            {
                Kourindou.Gensokyo_Fairy_Bone_Type = Fairy_Bone.Type;
            }

            Gensokyo.TryFind<ModNPC>("Fairy_Flower", out ModNPC Fairy_Flower);
            if (Fairy_Flower != null)
            {
                Kourindou.Gensokyo_Fairy_Flower_Type = Fairy_Flower.Type;
            }

            Gensokyo.TryFind<ModNPC>("Fairy_Lava", out ModNPC Fairy_Lava);
            if (Fairy_Lava != null)
            {
                Kourindou.Gensokyo_Fairy_Lava_Type = Fairy_Lava.Type;
            }

            Gensokyo.TryFind<ModNPC>("Fairy_Snow", out ModNPC Fairy_Snow);
            if (Fairy_Snow != null)
            {
                Kourindou.Gensokyo_Fairy_Snow_Type = Fairy_Snow.Type;
            }

            Gensokyo.TryFind<ModNPC>("Fairy_Stone", out ModNPC Fairy_Stone);
            if (Fairy_Stone != null)
            {
                Kourindou.Gensokyo_Fairy_Stone_Type = Fairy_Stone.Type;
            }

            Gensokyo.TryFind<ModNPC>("Fairy_Sunflower", out ModNPC Fairy_Sunflower);
            if (Fairy_Sunflower != null)
            {
                Kourindou.Gensokyo_Fairy_Sunflower_Type = Fairy_Sunflower.Type;
            }

            Gensokyo.TryFind<ModNPC>("Fairy_Thorn", out ModNPC Fairy_Thorn);
            if (Fairy_Thorn != null)
            {
                Kourindou.Gensokyo_Fairy_Thorn_Type = Fairy_Thorn.Type;
            }

            Gensokyo.TryFind<ModNPC>("Fairy_Crystal", out ModNPC Fairy_Crystal);
            if (Fairy_Crystal != null)
            {
                Kourindou.Gensokyo_Fairy_Crystal_Type = Fairy_Crystal.Type;
            }

            Gensokyo.TryFind<ModNPC>("Fairy_Spore", out ModNPC Fairy_Spore);
            if (Fairy_Spore != null)
            {
                Kourindou.Gensokyo_Fairy_Spore_Type = Fairy_Spore.Type;
            }

            Gensokyo.TryFind<ModNPC>("Fairy_Sand", out ModNPC Fairy_Sand);
            if (Fairy_Sand != null)
            {
                Kourindou.Gensokyo_Fairy_Sand_Type = Fairy_Sand.Type;
            }

            Gensokyo.TryFind<ModNPC>("Fairy_Water", out ModNPC Fairy_Water);
            if (Fairy_Water != null)
            {
                Kourindou.Gensokyo_Fairy_Water_Type = Fairy_Water.Type;
            }

            Gensokyo.TryFind<ModNPC>("Fairy_Blood", out ModNPC Fairy_Blood);
            if (Fairy_Blood != null)
            {
                Kourindou.Gensokyo_Fairy_Blood_Type = Fairy_Blood.Type;
            }

            Gensokyo.TryFind<ModNPC>("Fairy_Metal", out ModNPC Fairy_Metal);
            if (Fairy_Metal != null)
            {
                Kourindou.Gensokyo_Fairy_Metal_Type = Fairy_Metal.Type;
            }
        }
    }
}