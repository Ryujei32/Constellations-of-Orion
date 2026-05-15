// ============================================
// CONTENT/ITEMS/MATERIALS/SUNSTONEBAR.CS
// ============================================

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Items.Materials
{
	public class SunstoneBar : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 25;
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 24;

			Item.maxStack = 9999;

			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Orange;

			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;

			Item.consumable = true;
			Item.createTile = ModContent.TileType<Content.Tiles.SunstoneBarTile>();
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient<SunstoneOre>(4)
				.AddTile(TileID.Hellforge)
				.Register();
		}
	}
}