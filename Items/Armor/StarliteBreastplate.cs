using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Body)]
    public class StarliteBreastplate : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 22;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Pink;
            Item.defense = 24;
        }

        public override void UpdateEquip(Player player)
        {
            player.statLifeMax2 += 40;
            player.endurance += 0.08f;
            player.moveSpeed += 0.08f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 24)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}