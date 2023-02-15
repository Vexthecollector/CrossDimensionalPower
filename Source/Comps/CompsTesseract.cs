using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace CrossDimensionalPower
{
	[StaticConstructorOnStartup]
	public class CompsTesseract : CompPowerTrader
	{
		private static readonly Texture2D teleporterIcon = ContentFinder<Texture2D>.Get("teleporter");
		private static readonly Texture2D renameIcon = ContentFinder<Texture2D>.Get("relay_rename");
		public string Name;
		private CompFlickable compFlickable;
		private CompBreakdownable compBreakdownable;
		private CompsTesseract connectedTesseract;
		private List<CompsTesseract> connectedList = new List<CompsTesseract>();
		public new CompProperties_Tesseract Props => (CompProperties_Tesseract)props;




		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			compBreakdownable = parent.GetComp<CompBreakdownable>();
			compFlickable = parent.GetComp<CompFlickable>();
			WorldComponent_Tesseracts.Instance.Tesseracts.AddDistinct(this);
			if (respawningAfterLoad) return;
			Find.WindowStack.Add(new Dialog_RenameTesseract(this));
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			if (connectedTesseract != null) connectedTesseract.connectedList.Remove(this);
			WorldComponent_Tesseracts.Instance.Tesseracts.Remove(this);
		}

		public bool CanOutputNow
		{
			get
			{
				return (compBreakdownable == null || !compBreakdownable.BrokenDown)
					   && (compFlickable == null || compFlickable.SwitchIsOn);
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (connectedList.NullOrEmpty())
			{
				yield return new Command_Action
				{
					action = delegate
					{
						process_Outputs();
					},
					defaultLabel = "Select Tesseract",
					defaultDesc = "Select the Tesseract",
					icon = teleporterIcon
				};
			}
			yield return new Command_Action
			{
				action = () => Find.WindowStack.Add(new Dialog_RenameTesseract(this)),
				defaultLabel = "Rename",
				defaultDesc = "Rename the Tesseract",
				icon = renameIcon
			};
		}

		public void process_Outputs()
		{

			List<FloatMenuOption> list = new List<FloatMenuOption>();

			foreach (CompsTesseract output in WorldComponent_Tesseracts.Instance.Tesseracts.Except(this))
			{

				if (output.connectedTesseract != this)
				{

					list.Add(new FloatMenuOption("Select " + output.Name, () =>
					{
						if (connectedTesseract != null) connectedTesseract.connectedList.Remove(this);
						connectedTesseract = output;
						connectedTesseract.connectedList.Add(this);

					}));
				}

			}
			if (list.Any<FloatMenuOption>())
			{
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}


		public override void CompTick()
		{
			base.CompTick();
			if (parent.IsHashIntervalTick(500))
				CompTickRare();
		}

		public bool CanInputNow
		{
			get
			{
				return (compBreakdownable == null || !compBreakdownable.BrokenDown)
					   && (compFlickable == null || compFlickable.SwitchIsOn);
			}
		}

		public override void CompTickRare()
		{
			base.CompTickRare();
			if (connectedTesseract != null && connectedList.NullOrEmpty())
			{

				if (CanInputNow && !this.PowerNet.powerComps.Contains(connectedTesseract))
				{
					float tPower = TesseractList.TesseractPowerOutput(TesseractList.ThisPowerNetTesseracts(this));
					float transfer = GetPowerNetStrength() - tPower;
					connectedTesseract.setPowerOutput(transfer * ((100 - Props.lossPercent) / 100) * ((100 - connectedTesseract.Props.lossPercent) / 100));
					setPowerOutput(-transfer);
				}

			}
		}

		public float GetPowerNetStrength()
		{
			return this.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick;
		}

		public float GetPowerNetStored()
		{
			return this.PowerNet.CurrentStoredEnergy();
		}

		public float CompareStrength(PowerNet net1, PowerNet net2)
		{
			return (net1.CurrentEnergyGainRate() - net2.CurrentEnergyGainRate()) / CompPower.WattsToWattDaysPerTick;
		}

		public void setPowerOutput(float output)
		{
			base.PowerOutput = output;
		}

		public override string CompInspectStringExtra()
		{
			string text = "";
			string outputFor = "";
			if (!connectedList.NullOrEmpty())
			{
				foreach (CompsTesseract comp in connectedList.Except(connectedList.Last()))
				{
					outputFor += comp.Name + ",";
				}
				outputFor += connectedList.Last().Name;
				text = "Output for " + outputFor + "\n" + "Receiver Power Percentage Loss: " + Props.lossPercent + "%\n";
			}
			if (connectedList.NullOrEmpty()) text = "Transmitter Power Percentage Loss: " + Props.lossPercent + "%\n";
			return text + base.CompInspectStringExtra();
		}



	}
	public class Dialog_RenameTesseract : Dialog_Rename
	{
		public CompsTesseract Tesseract;

		public Dialog_RenameTesseract(CompsTesseract tesseract)
		{
			this.Tesseract = tesseract;
			this.curName = tesseract.Name ?? "Tesseract " + " #" + Rand.Range(1, 99).ToString("D2");
		}


		protected override void SetName(string name)
		{
			this.Tesseract.Name = name;
		}

	}
}
