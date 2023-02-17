using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace CrossDimensionalPower
{
    public class CompsCDPBattery : ThingComp
    {
        private float curCharge=0;
        private float maxCharge;
        private float curChargePercent { get { return curCharge / maxCharge; } }
        private CompFlickable compFlickable;
        private CompBreakdownable compBreakdownable;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compBreakdownable = parent.GetComp<CompBreakdownable>();
            compFlickable = parent.GetComp<CompFlickable>();
        }

        public override void PostDeSpawn(Map map)
        {
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
                       && (compFlickable == null || compFlickable.SwitchIsOn) && (curCharge<maxCharge);
            }
        }

        public override string CompInspectStringExtra()
        {
            string text = "";
            text="Current Charge: ("+curCharge+"/"+maxCharge+")\n"+"Percent: ("+curChargePercent+")\n";

            return text + base.CompInspectStringExtra();
        }

    }
}
