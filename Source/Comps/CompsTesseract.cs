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
        private CompsTesseract outputTesseract;
        public ThingWithComps linkedThing;
        private List<CompsTesseract> inputList = new List<CompsTesseract>();
        public new CompProperties_Tesseract Props => (CompProperties_Tesseract)props;




        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compBreakdownable = parent.GetComp<CompBreakdownable>();
            compFlickable = parent.GetComp<CompFlickable>();
            WorldComponent_Tesseracts.Instance.Tesseracts.AddDistinct(this);
            if (linkedThing != null)
            {
                outputTesseract = linkedThing.GetComp<CompsTesseract>();
            }
            inputList = WorldComponent_Tesseracts.Instance.Tesseracts.Where(t => t.outputTesseract == this).ToList();
            if (respawningAfterLoad) return;
            Find.WindowStack.Add(new Dialog_RenameTesseract(this));
        }

        public override void PostDeSpawn(Map previousMap)
        {
            base.PostDeSpawn(previousMap);
            if (outputTesseract != null) outputTesseract.inputList.Remove(this);
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
            if (inputList.NullOrEmpty())
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

                if (output.outputTesseract != this)
                {

                    list.Add(new FloatMenuOption("Select " + output.Name, () =>
                    {
                        if (outputTesseract != null) outputTesseract.inputList.Remove(this);
                        outputTesseract = output;
                        linkedThing = outputTesseract.parent;
                        outputTesseract.inputList.Add(this);

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
            if (outputTesseract != null && inputList.NullOrEmpty())
            {

                if (CanInputNow && !this.PowerNet.powerComps.Contains(outputTesseract))
                {
                    float tPower = TesseractList.TesseractPowerOutput(TesseractList.ThisPowerNetTesseracts(this).Except(this).ToList());
                    float transfer = GetPowerNetStrength() - tPower-this.PowerOutput;
                    outputTesseract.setPowerOutput(transfer * ((100 - Props.lossPercent) / 100) * ((100 - outputTesseract.Props.lossPercent) / 100));
                    setPowerOutput(-transfer);
                }
                else if(CanInputNow && this.PowerNet.powerComps.Contains(outputTesseract))
                {
                    outputTesseract.setPowerOutput(0);
                    setPowerOutput(0);
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


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref this.linkedThing, "linkedTesseract");
            Scribe_Values.Look(ref this.Name, "Name");

        }

        public override string CompInspectStringExtra()
        {
            string text = "";
            string outputFor = "";
            if (!inputList.NullOrEmpty())
            {
                foreach (CompsTesseract comp in inputList.Except(inputList.Last()))
                {
                    outputFor += comp.Name + ",";
                }
                outputFor += inputList.Last().Name;
                text = "Output for " + outputFor + "\n" + "Receiver Power Percentage Loss: " + Props.lossPercent + "%\n";
            }
            if (inputList.NullOrEmpty()) text = "Transmitter Power Percentage Loss: " + Props.lossPercent + "%\n";
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
