namespace ConstellationsOfOrion.Content.Items.Placeables.Banners
{
	public class StarliteSlimeBanner : BaseBanner
	{
		// ⭐ Style 0 since this is your first banner
		public override int BannerTileStyle => 0;

		// ⭐ Link to your slime NPC
		public override int BonusNPCID =>
			Terraria.ModLoader.ModContent.NPCType<Content.NPCs.Enemies.StarliteSlime>();
	}
}
