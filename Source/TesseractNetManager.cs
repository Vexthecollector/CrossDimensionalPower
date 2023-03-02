using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CrossDimensionalPower
{
    public class TesseractNetManager
    {
        private TesseractNetManager()
        {
            TesseractNetConnectionMaker.Instance.GenerateNetsFromTesseracts();
        }

        private static TesseractNetManager instance = new TesseractNetManager();
        public static TesseractNetManager Instance
        {
            get { return instance; }
        }


        public float GetAllPowerInNetwork(PowerNet net)
        {
            float num = 0;
            for (int i = 0; i < net.powerComps.Count; i++)
            {

                num += net.powerComps[i].EnergyOutputPerTick;

            }
            return num / CompPower.WattsToWattDaysPerTick;
        }


        public void DistributePower()
        {
            List<PowerNet> nets = new List<PowerNet>();
            List<CompsTesseract> tesseracts = TesseractNet.Instance.Tesseracts;
            //TesseractNet.Instance.Tesseracts.ForEach(tes => nets.AddDistinct(tes.PowerNet));
            float tesPower = 0;
            for (int i = tesseracts.Count-1; i >= 0; i--)
            {
                nets.AddDistinct(tesseracts[i].PowerNet);
                tesPower += tesseracts[i].PowerOutput;
            }
            //float tesPower = tesseracts.Sum(tes => tes.PowerOutput);
            float totalAvailable = 0;


            //Log.Message("Initial Total: "+totalAvailable);
            //Sets all Networks to be net 0 if they have free power.
            foreach (PowerNet net in nets.Where(net => (net.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick) > 0))
            {
                CompsTesseract tesseract = (CompsTesseract)net.powerComps.First(item => item is CompsTesseract);
                net.powerComps.ForEach(item => { if (item != tesseract && item is CompsTesseract) item.PowerOutput = 0; });
                tesseract.PowerOutput -= ((net.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick));
                totalAvailable -= tesseract.PowerOutput;
            }





            //Log.Message("All Total: " + totalAvailable);

            //Fill up all networks to be at least at 0
            foreach (PowerNet net in nets.Where(net => (GetAllPowerInNetwork(net)) < 0))
            {
                float curEnergy = GetAllPowerInNetwork(net);
                CompsTesseract tesseract = (CompsTesseract)net.powerComps.First(item => item is CompsTesseract);
                net.powerComps.ForEach(item => { if (item != tesseract && item is CompsTesseract) item.PowerOutput = 0; });
                if (-curEnergy < totalAvailable)
                {
                    tesseract.PowerOutput = -curEnergy;
                    totalAvailable += curEnergy;
                }
                else
                {
                    tesseract.PowerOutput = curEnergy + totalAvailable;
                    totalAvailable = 0;
                }
            }

            //Log.Message("Total after all 0: " + totalAvailable);

            if ((totalAvailable / nets.Count) > 500)
            {
                foreach (PowerNet net in nets)
                {
                    CompsTesseract tesseract = (CompsTesseract)net.powerComps.First(item => item is CompsTesseract);
                    net.powerComps.ForEach(item => { if (item != tesseract && item is CompsTesseract) item.PowerOutput = 0; });

                    float curProd = (net.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick);
                    tesseract.PowerOutput += 500 - curProd;
                    totalAvailable -= 500;
                    curProd = (net.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick);
                }
            }

            //Log.Message("Total after all 500: " + totalAvailable);

            List<PowerNet> netsWithBattery = nets.Where(item => item.batteryComps != null).ToList();
            float toDistribute;
            if (netsWithBattery.Count > 0)
            {

                toDistribute = totalAvailable / netsWithBattery.Count;
                totalAvailable = 0;
                //Log.Message("To Distribute Batteries: "+toDistribute);
                foreach (PowerNet net in netsWithBattery)
                {
                    CompsTesseract tesseract = (CompsTesseract)net.powerComps.First(item => item is CompsTesseract);
                    net.powerComps.ForEach(item => { if (item != tesseract && item is CompsTesseract) item.PowerOutput = 0; });

                    tesseract.PowerOutput += toDistribute;
                }
            }
            else
            {
                toDistribute = totalAvailable / nets.Count;
                totalAvailable = 0;
                //Log.Message("To Distribute No Batteries: " + toDistribute);
                foreach (PowerNet net in nets)
                {
                    CompsTesseract tesseract = (CompsTesseract)net.powerComps.First(item => item.GetType() == typeof(CompsTesseract));
                    net.powerComps.ForEach(item => { if (item != tesseract && item is CompsTesseract) item.PowerOutput = 0; });

                    tesseract.PowerOutput += toDistribute;
                }
            }


        }

        /*
        public void EqualizePower()
        {
            TesseractNetConnectionMaker.Instance.RecheckClasses();
            if (TesseractNet.Instance.TesseractLists.NullOrEmpty()) return;

            float totalAvailable = TesseractNet.Instance.TesseractLists.Sum(item => item.curEnergyGain);

            //float totalAvailable = TesseractNet.Instance.TesseractLists.Sum(item => { if (item.curEnergyGain > 0) return item.curEnergyGain; return 0; });
            foreach (TesseractList tesseractClass in TesseractNet.Instance.TesseractLists.Where(item => item.curEnergyGain < 0))
            {
                if (tesseractClass.curEnergyGain < totalAvailable)
                {
                    totalAvailable += tesseractClass.curEnergyGain;
                    tesseractClass.curEnergyGain = 0;
                    tesseractClass.tesseract[0].PowerOutput = 0;
                    tesseractClass.tesseract[0].curPower = 0;
                }
                else
                {
                    tesseractClass.curEnergyGain = tesseractClass.curEnergyGain + totalAvailable;
                    totalAvailable = 0;
                    tesseractClass.tesseract[0].PowerOutput = tesseractClass.curEnergyGain;
                    tesseractClass.tesseract[0].curPower = tesseractClass.curEnergyGain;
                }
            }
            if (totalAvailable / TesseractNet.Instance.TesseractLists.Count() > 500)
            {
                foreach (TesseractList tesseractClass in TesseractNet.Instance.TesseractLists.Where(item => item.curEnergyGain < 0))
                {

                    totalAvailable -= 500;
                    tesseractClass.curEnergyGain = 500;
                    tesseractClass.tesseract[0].PowerOutput = 500;
                    tesseractClass.tesseract[0].curPower = 500;

                }
            }
            CheckBatteries();

            List<TesseractList> tesseractClassesWB = TesseractNet.Instance.TesseractLists.Where(item => item.maxStoredEnergy > 0).ToList();
            float toDistribute = totalAvailable / tesseractClassesWB.Count();
            totalAvailable = 0;
            foreach (TesseractList tesseractClass in tesseractClassesWB)
            {
                tesseractClass.curEnergyGain = toDistribute;
                tesseractClass.tesseract[0].PowerOutput = toDistribute;
                tesseractClass.tesseract[0].curPower = toDistribute;
            }

        }

        public void CheckBatteries()
        {
            foreach (TesseractList tesseractClass in TesseractNet.Instance.TesseractLists)
            {
                if (!tesseractClass.tesseract[0].PowerNet.batteryComps.NullOrEmpty())
                {
                    tesseractClass.curStoredEnergy = tesseractClass.tesseract[0].PowerNet.CurrentStoredEnergy();
                    tesseractClass.maxStoredEnergy = tesseractClass.tesseract[0].PowerNet.batteryComps.Sum(battery => battery.Props.storedEnergyMax);
                }

            }

        }
        */
    }
}
