// Content/Items/Armor/StarliteHelmet.cs

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class StarliteHelmet : ModItem
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
            player.GetDamage(DamageClass.Melee) += 0.18f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.10f;
            player.GetCritChance(DamageClass.Melee) += 8f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<StarliteBreastplate>()
                && legs.type == ModContent.ItemType<StarliteGreaves>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Greatly increased melee knockback\n10% increased movement speed";

            player.GetKnockback(DamageClass.Melee) += 1.25f;
            player.moveSpeed += 0.10f;
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