// Content/Items/Armor/StarliteHeadgear.cs

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class StarliteHeadgear : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 20;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Pink;
            Item.defense = 10;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Ranged) += 0.18f;
            player.GetCritChance(DamageClass.Ranged) += 8f;
            player.ammoCost80 = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<StarliteBreastplate>()
                && legs.type == ModContent.ItemType<StarliteGreaves>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "25% chance to not consume ammo\n10% increased ranged damage\n5% increased ranged critical strike chance";

            player.ammoCost75 = true;
            player.GetDamage(DamageClass.Ranged) += 0.10f;
            player.GetCritChance(DamageClass.Ranged) += 5f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}