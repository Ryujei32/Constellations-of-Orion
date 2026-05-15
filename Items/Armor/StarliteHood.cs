// Content/Items/Armor/StarliteHood.cs

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class StarliteHood : ModItem
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
            player.GetDamage(DamageClass.Magic) += 0.20f;
            player.statManaMax2 += 200;
            player.manaCost -= 0.15f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<StarliteBreastplate>()
                && legs.type == ModContent.ItemType<StarliteGreaves>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Greatly increased mana regeneration";

            player.manaRegenBonus += 35;
            player.statManaMax2 += 40;
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