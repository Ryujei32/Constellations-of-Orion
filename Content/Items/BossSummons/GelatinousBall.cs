using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace ConstellationsOfOrion.Content.Items.BossSummons
{
	public class GelatinousBall : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 3;
		}

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 26;

			Item.maxStack = 9999;

			Item.useAnimation = 45;
			Item.useTime = 45;
			Item.useStyle = ItemUseStyleID.HoldUp;

			Item.consumable = true;

			Item.rare = ItemRarityID.Pink;
			Item.value = Item.buyPrice(silver: 50);

			Item.UseSound = SoundID.Roar;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "Summon",
				"Summons Cosmic Astraeus"));

			tooltips.Add(new TooltipLine(Mod, "Flavor",
				"\"It pulses with celestial energy\""));
		}

		public override bool CanUseItem(Player player)
		{
			return !NPC.AnyNPCs(
				ModContent.NPCType<Content.NPCs.Bosses.Astraeus.CosmicAstraeus>()
			);
		}

		public override bool? UseItem(Player player)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				NPC.SpawnOnPlayer(
					player.whoAmI,
					ModContent.NPCType<Content.NPCs.Bosses.Astraeus.CosmicAstraeus>()
				);
			}
			else
			{
				NetMessage.SendData(
					MessageID.SpawnBossUseLicenseStartEvent,
					number: player.whoAmI,
					number2: ModContent.NPCType<Content.NPCs.Bosses.Astraeus.CosmicAstraeus>()
				);
			}

			return true;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Gel, 25)
				.AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 3)
				.AddTile(TileID.DemonAltar)
				.Register();
		}
	}
}
