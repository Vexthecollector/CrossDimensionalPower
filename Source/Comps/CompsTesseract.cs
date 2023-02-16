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

        public override void PostDeSpawn(Map previousMap)
        {
            base.PostDeSpawn(previousMap);
            TesseractNetConnectionMaker.Instance.RemoveTesseract(this);
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

    }



}
