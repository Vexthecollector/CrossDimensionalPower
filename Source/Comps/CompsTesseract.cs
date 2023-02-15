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
    public class CompsTesseract : CompPower
    {
        private static readonly Texture2D teleporterIcon = ContentFinder<Texture2D>.Get("teleporter");
        public string Name { get; set; }
        private CompFlickable compFlickable;
        private CompBreakdownable compBreakdownable;
        public CompsTesseract connectedTesseract;
        


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compBreakdownable = parent.GetComp<CompBreakdownable>();
            compFlickable = parent.GetComp<CompFlickable>();
            if (respawningAfterLoad) return;
            Find.WindowStack.Add(new Dialog_RenameTesseract(this));
            WorldComponent_Tesseracts.Instance.Tesseracts.Add(this);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
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
            yield return new Command_Action
            {
                action = () => Find.WindowStack.Add(new Dialog_RenameTesseract(this)),
                defaultLabel = "Rename Tesseract",
                defaultDesc = "Rename the Tesseract"
            };
        }

        public void process_Outputs()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            foreach (CompsTesseract output in WorldComponent_Tesseracts.Instance.Tesseracts.Except(this))
            {


                list.Add(new FloatMenuOption("Select " + output.Name, () =>
                {
                    connectedTesseract = output;
                }));

            }
            if (list.Any<FloatMenuOption>())
            {
                Find.WindowStack.Add(new FloatMenu(list));
            }
        }


        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(300))
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
            if (connectedTesseract != null)
            {

                if (CanInputNow)
                {
                    Log.Message("CrossDimensionalPower");
                    Log.Message("Test Input "+this.PowerNet.CurrentEnergyGainRate());
                    Log.Message("Test Output "+connectedTesseract.PowerNet.CurrentEnergyGainRate());
                    

                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
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
