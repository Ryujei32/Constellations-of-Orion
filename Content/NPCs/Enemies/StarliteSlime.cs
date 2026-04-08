using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;

namespace ConstellationsOfOrion.Content.NPCs.Enemies
{
	public class StarliteSlime : ModNPC
	{
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 3;
		}

		public override void SetDefaults()
		{
			NPC.width = 32;
			NPC.height = 32;

			NPC.damage = 28;
			NPC.defense = 10;
			NPC.lifeMax = 140;

			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;

			NPC.value = 180f;
			NPC.knockBackResist = 0.8f;

			// ⭐ Blue Slime AI
			NPC.aiStyle = 1;
			AIType = NPCID.BlueSlime;
			AnimationType = NPCID.BlueSlime;

			// ⭐ ✅ CORRECT BANNER PLACEMENT (THIS IS WHERE IT BELONGS)
			Banner = Type;
			BannerItem = ModContent.ItemType<Content.Items.Placeables.Banners.StarliteSlimeBanner>();
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (!Main.hardMode)
				return 0f;

			if (spawnInfo.Player.ZoneOverworldHeight)
			{
				return Main.dayTime ? 0.04f : 0.07f;
			}

			return 0f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(
				ItemDropRule.Common(
					ModContent.ItemType<Content.Items.Materials.ConstelliteOre>(),
					1,
					4,
					6
				)
			);

			npcLoot.Add(ItemDropRule.Common(ItemID.Gel, 1, 1, 3));

			npcLoot.Add(ItemDropRule.Common(ItemID.SlimeStaff, 10000));
		}
	}
}
