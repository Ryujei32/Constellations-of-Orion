using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace ConstellationsOfOrion.Content.Items.Materials
{
	public class ConstelliteBar : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 24;
			Item.maxStack = 9999;
			Item.ResearchUnlockCount = 100;

			Item.value = Item.sellPrice(silver: 86);
			Item.rare = ItemRarityID.Pink;


			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 10;
			Item.useAnimation = 15;

			Item.useTurn = true;
			Item.autoReuse = true;
			Item.consumable = true;

			Item.createTile = ModContent.TileType<Content.Tiles.ConstelliteBarTile>();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "Flavor",
				"\"Refined from celestial remnants\""));
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient<ConstelliteOre>(4)
				.AddTile(TileID.Hellforge)
				.Register();
		}
	}
}
