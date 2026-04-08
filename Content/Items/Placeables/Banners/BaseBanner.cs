using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Items.Placeables.Banners
{
	public abstract class BaseBanner : ModItem
	{
		public virtual int BannerTileID => ModContent.TileType<Content.Tiles.Banners.StarliteSlimeBannerTile>();
		public virtual int BannerTileStyle => 0;

		// ⭐ NPC linked to banner
		public virtual int BonusNPCID => 0;

		public override void SetStaticDefaults()
		{
			ItemID.Sets.KillsToBanner[Type] = ItemID.Sets.DefaultKillsForBannerNeeded;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(BannerTileID, BannerTileStyle);

			Item.width = 12;
			Item.height = 28;
			Item.maxStack = 999;
			Item.ResearchUnlockCount = 3;

			Item.value = Item.sellPrice(silver: 2);
			Item.rare = ItemRarityID.Blue;
		}
	}
}
