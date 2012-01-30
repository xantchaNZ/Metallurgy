using System.Collections.Generic;
using System.Windows.Forms;
using Data;
using Data.Types;
using Data.Types.Enums;

namespace Metallurgy
{
	public partial class MainForm : Form
	{
		/*
			****************
			*  METULLARGY  *
			****************

		Tool that will
		Be a unit Database
		 * Searching, sorting, groups etc
		Show Unit Rankings
		 * Show total unit average damages
		 * Show unit potential damages
		 * Calculate true rally value for rally units
		 * Calculate true boost value for boost units
		 * Calculate bonuses towards heal prevention, damage prevention for epics, AV, typhon etc)
		Allow you to drag and drop build forces
		Run test attacks vs epics (test ratios etc etc) Test Hits
		Calculate, DMG per stam, ratios etc for forces
		Calculate stam costs to hit caps in epics

		 * TODO:
		 * 
		 * Think about how to handle boost effect that are reinforced in.
		 * Put "Unit Tests" In
		 * 
		 * Fix Riggs - Reinforcements for Reports
		 * Fix Mammoth Tank Boost reporting
		 * Add contraction for multiple units of same type
		 * 
		*/

		private ObjectDatabase database;

		public MainForm()
		{
			InitializeComponent();

			database = ObjectDatabase.Database;
			var pw = database.GetUnitByName("Photon Walker");

			var force = new Force
			{
				Name = "Robots Test",
				Formation = database.GetFormationByName("Rage Vindicator")
			};

			force.AddUnit(database.GetUnitByName("Field Marshall Riggs"));
			for (var i = 0; i < force.Formation.AssaultSlots - 1; i++)
				force.AddUnit(pw);
			force.AddUnit(database.GetUnitByName("Mammoth Tank"));
			force.AddUnit(database.GetUnitByName("Beowulf Cluster"));
			force.AddUnit(database.GetUnitByName("Omega, Machine of War"));

			force.AddReinforcement(pw, 2);

			var enemy = new Force
			{
				Name = "Dummy",
				IsEpicBoss = true
			};

			ForceReportTextBox.Text = force.ForceReport(enemy);
			ForceReportTextBox.SelectionStart = ForceReportTextBox.Text.Length;
		}

		private void RunTests()
		{
			var database = ObjectDatabase.Database;
			var riggs = database.GetUnitByName("Field Marshall Riggs");
			var pw = database.GetUnitByName("Photon Walker");
			var mt = database.GetUnitByName("Mammoth Tank");
			var beowulf = database.GetUnitByName("Beowulf Cluster");
			var omega = database.GetUnitByName("Omega, Machine of War");
			var rageVindi = database.GetFormationByName("Rage Vindicator");

			var force = new Force
			{
				Name = "Robots Test",
				Formation = rageVindi
			};

			var riggsDmg = riggs.CalculateAverageDamage(force, null, null, true);
			var pwNDmg = pw.CalculateAverageDamage(force, null, null, false);
			var pwEDmg = pw.CalculateAverageDamage(force, null, null, true);
			var mtDmg = mt.CalculateAverageDamage(force, null, null, true);
			var omegaDmg = omega.CalculateAverageDamage(force, null, null, true);

			var omegaBoost = pw.CalculateBoostBonus(force, null, omega.Abilities[0].Effects[0], true);
			var mtBoost = mt.CalculateBoostBonus(force, null, rageVindi.Abilities[0].Effects[0], true);

			force.AddUnit(riggs);
			force.AddUnit(riggs);
			for (int i = 0; i < force.Formation.AssaultSlots - 1; i++)
				force.AddUnit(pw);
			force.AddUnit(mt);
			force.AddUnit(beowulf);
			force.AddUnit(omega);

			force.AddReinforcement(pw, 2);

			var boosts = force.GetBoostAbilities();
			var poweredPW = pw.CalculateAverageDamage(force, null, boosts, true); // This is counting the vs epic & vs non-epic twice for rallys

			// These don't count reinforcements from Riggs
			var beoBonus = beowulf.CalculateBoostToForce(force, null, true);
			var rvBonus = rageVindi.CalculateBoostToForce(force, null, true);
			var omegaBonus = omega.CalculateBoostToForce(force, null, true);

			var riggsReinf = riggs.CalculateReinforcedDamage(force, null, boosts, true);

			force.ResetCombat();
			var riggsTotal = riggs.CalculateTotalDamageContribution(force, null, boosts, true);
			var pwTotal = pw.CalculateTotalDamageContribution(force, null, boosts, true);
			var mtTotal = mt.CalculateTotalDamageContribution(force, null, boosts, true);
			var beowulfTotal = beowulf.CalculateTotalDamageContribution(force, null, boosts, true);
			var omegaTotal = omega.CalculateTotalDamageContribution(force, null, boosts, true);

			force.ResetCombat();
			var enemy = new Force
			{
				Name = "Dummy",
				IsEpicBoss = true
			};
			var forceDmg = force.CalculateAverageForceDamage(enemy);
		}

		private void SeedDatabase()
		{
			/*var rageVindi = new Formation
			{
				Name = "Rage Vindicator",
				CommanderSlots = 10,
				AssaultSlots = 30,
				StructureSlots = 14,
				VindicatorSlots = 1,
				Abilities = new List<Ability>
				{
					new Ability("RAAAAGEE!", 0.50, Effect.CreateRallyEffect(Classification.Assault, 1))
				},
			};*/

			var riggs = new Unit
			{
				Name = "Field Marshall Riggs",
				Classifications = new List<Classification> {Classification.Commander},
				IsUnique = true,
				Attack = 100,
				Defence = 80,
				IsEpicBossForceUnit = false,
				Abilities = new List<Ability>
				{
					new Ability
					{
						Name = "Technophage",
						ProcChance = 0.50,
						Effects = new List<Effect>
						{
							Effect.CreateDamageEffect(3, 7, vsEpic: false),
							new Effect
							{
								Type = EffectType.Reinforce,
								TargetType = Classification.Robotic,
								EffectValue = 2
							}
						}
					}
				},
				Source = "Cataclysm Drop: 33% at 160k Tier, 33% at 280k Tier"
			};

		    var effect = Effect.CreateDamageEffect(2, 6, vsEpic: false);
		    effect.MinBonusVsEpic = 6;
		    effect.MaxBonusVsEpic = 16;
			var pw = new Unit
			{
				Name = "Photon Walker",
				Classifications = new List<Classification> {Classification.Assault, Classification.Robotic},
				IsUnique = false,
				Attack = 24,
				Defence = 36,
				IsEpicBossForceUnit = false,
				Abilities = new List<Ability>
				{
					new Ability("Quantize", 0.60, effect),
				},
				Source = "Cataclysm Drop: 33% at 160k Tier, 33% at 280k Tier"
			};

			var mt = new Unit
			{
				Name = "Mammoth Tank",
				Classifications = new List<Classification> {Classification.Assault, Classification.Armoured},
				IsUnique = true,
				Attack = 100,
				Defence = 100,
				IsEpicBossForceUnit = false,
				Abilities = new List<Ability>
				{
					new Ability("Siege Shatter", 0.50, Effect.CreateDamageEffect(9, 27, vsEpic: true)),
					new Ability("Gun Siege", 0.50, Effect.CreateFlurryDamageEffect(3, 9, 3, vsEpic: true)),
				},
				Source = "Cataclysm Drop: 33% at 160k Tier, 33% at 280k Tier"
			};

			var beowulf = new Unit
			{
				Name = "Beowulf Cluster",
				Classifications = new List<Classification> {Classification.Structure},
				IsUnique = true,
				Attack = 50,
				Defence = 50,
				IsEpicBossForceUnit = false,
				Abilities = new List<Ability>
				{
					new Ability("Machine Vision", 0.50, Effect.CreateRallyEffect(Classification.Robotic, 1))
				},
				Source = "Wasteland - Mission 5"
			};

			var omega = new Unit
			{
				Name = "Omega, Machine of War",
				Classifications = new List<Classification> {Classification.Vindicator},
				IsUnique = true,
				Attack = 400,
				Defence = 240,
				IsEpicBossForceUnit = false,
				Abilities = new List<Ability>
				{
					new Ability("Uplink", 0.60, Effect.CreateBoostEffect(Classification.Robotic, 50)),
					new Ability("Final Lockdown", 0.60, Effect.CreateJamEffect(Classification.Assault, 1)),
					new Ability("Magneton Cannon", 0.65, Effect.CreateDamageEffect(11, 33, vsEpic: false), Effect.CreateJamEffect(Classification.Commander, 1)),
				},
				Source = "Cataclysm Collections"
			};

			var database = ObjectDatabase.Database;
			database.Units.Add(riggs);
			database.Units.Add(pw);
			database.Units.Add(mt);
			database.Units.Add(beowulf);
			database.Units.Add(omega);
			//database.Formations.Add(rageVindi);

			database.Save();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			database.Save();
		}
	}
}
