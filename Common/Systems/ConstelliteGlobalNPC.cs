using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Chat;
using Terraria.Localization;
using System.Threading;

namespace ConstellationsOfOrion.Content.Systems
{
	public class ConstelliteGlobalNPC : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			// ⭐ Detect Wall of Flesh death
			if (npc.type == NPCID.WallofFlesh)
			{
				SpawnConstelliteOre();
			}
		}

		private void SpawnConstelliteOre()
		{
			// Multiplayer safety
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			ThreadPool.QueueUserWorkItem(_ =>
			{
				// ⭐ HARDMODE-STYLE MESSAGE (Titanium color)
				Color messageColor = new Color(200, 100, 255);

				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					Main.NewText("The world shimmers with constellations...", messageColor);
				}
				else
				{
					ChatHelper.BroadcastChatMessage(
						NetworkText.FromLiteral("The world shimmers with constellations..."),
						messageColor
					);
				}

				// ⭐ Spawn amount (close to vanilla tier)
				int splotches = (int)(200 * (Main.maxTilesX / 4200f));

				// ⭐ STRICT UNDERGROUND (NO SURFACE)
				int minY = (int)Main.rockLayer;
				int maxY = Main.maxTilesY - 300;

				for (int i = 0; i < splotches; i++)
				{
					int x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
					int y = WorldGen.genRand.Next(minY, maxY);

					WorldGen.OreRunner(
						x,
						y,
						WorldGen.genRand.Next(5, 9),
						WorldGen.genRand.Next(5, 9),
						(ushort)ModContent.TileType<Content.Tiles.ConstelliteOreTile>()
					);
				}
			});
		}
	}
}
