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
        public new CompProperties_Tesseract Props => (CompProperties_Tesseract)props;




        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compBreakdownable = parent.GetComp<CompBreakdownable>();
            compFlickable = parent.GetComp<CompFlickable>();
            TesseractNetConnectionMaker.Instance.AddTesseract(this);
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

    }



}
