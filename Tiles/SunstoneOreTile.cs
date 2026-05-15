// Ore Name Chosen: Sunstone Ore
// Tier:
// Better than Crimtane/Demonite
// Slightly stronger than Hellstone
// Requires Nightmare/Deathbringer Pickaxe or better

//////////////////////////////////////////////////////////////
// CONTENT/TILES/SUNSTONEORETILE.CS
//////////////////////////////////////////////////////////////

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Tiles
{
	public class SunstoneOreTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = false;
			Main.tileBlockLight[Type] = true;
			Main.tileSpelunker[Type] = true;
			Main.tileOreFinderPriority[Type] = 420;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 975;

			TileID.Sets.Ore[Type] = true;

			MineResist = 3f;
			MinPick = 65; // Nightmare/Deathbringer tier

			HitSound = SoundID.Tink;
			DustType = DustID.GoldCoin;

			AddMapEntry(new Color(221, 193, 122), CreateMapEntryName());
		}

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		public override void RandomUpdate(int i, int j)
		{
			if (Main.rand.NextBool(12))
			{
				Dust.NewDust(
					new Vector2(i * 16, j * 16),
					16,
					16,
					DustID.GoldFlame,
					0f,
					-0.3f
				);
			}
		}
	}
}