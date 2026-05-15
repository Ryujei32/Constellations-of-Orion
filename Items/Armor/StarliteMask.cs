// Content/Items/Armor/StarliteMask.cs

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class StarliteMask : ModItem
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
            player.maxMinions += 3;
            player.GetDamage(DamageClass.Summon) += 0.15f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<StarliteBreastplate>()
                && legs.type == ModContent.ItemType<StarliteGreaves>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "+1 max minion\nIncreased summon knockback";

            player.maxMinions += 1;
            player.GetKnockback(DamageClass.Summon) += 0.75f;
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