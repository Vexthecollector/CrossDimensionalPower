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
        public string Name;
        private CompFlickable compFlickable;
        private CompBreakdownable compBreakdownable;
        public float curPower;

        public TesseractList TesseractList;
        public new CompProperties_Tesseract Props => (CompProperties_Tesseract)props;




        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compBreakdownable = parent.GetComp<CompBreakdownable>();
            compFlickable = parent.GetComp<CompFlickable>();
            TesseractNetConnectionMaker.Instance.AddTesseract(this);
            this.powerOutputInt = 0;
            if (respawningAfterLoad) return;
        }

        public override void PostDeSpawn(Map map)
        {
            TesseractNetConnectionMaker.Instance.RemoveTesseract(this);
            base.PostDeSpawn(map);
        }

        public bool CanOutputNow
        {
            get
            {
                return (compBreakdownable == null || !compBreakdownable.BrokenDown)
                       && (compFlickable == null || compFlickable.SwitchIsOn);
            }
        }

        public bool CanInputNow
        {
            get
            {
                return (compBreakdownable == null || !compBreakdownable.BrokenDown)
                       && (compFlickable == null || compFlickable.SwitchIsOn);
            }
        }

        public override string CompInspectStringExtra()
        {
            string text = "";
            if(this.PowerOutput<0)
                text = "Current Power Draw: " + (-this.PowerOutput)+"\n";

            return text+base.CompInspectStringExtra();
        }

    }



}
