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


        public CompsTesseract setTesseractToZero(PowerNet net)
        {


            CompsTesseract tesseract = null;

            for (int i = net.powerComps.Count - 1; i >= 0; i--)
            {
                if (net.powerComps[i] is CompsTesseract tesseract1)
                {
                    if (tesseract == null)
                    {
                        tesseract = tesseract1;

                    }
                    else
                    {
                        tesseract1.PowerOutput = 0;
                    }
                }
            }
            return tesseract;
        }

        /*
        public void DistributePower()
        {
            List<PowerNet> nets = new List<PowerNet>();
            List<CompsTesseract> tesseracts = TesseractNet.Instance.Tesseracts;
            float tesPower = 0;
            for (int i = tesseracts.Count - 1; i >= 0; i--)
            {
                nets.AddDistinct(tesseracts[i].PowerNet);
                tesPower += tesseracts[i].PowerOutput;
            }
            float totalAvailable = 0;


            //Log.Message("Initial Total: "+totalAvailable);
            //Sets all Networks to be net 0 if they have free power.

            foreach (PowerNet net in nets.Where(net => (net.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick) > 0))
            {
                CompsTesseract tesseract = setTesseractToZero(net);
                tesseract.PowerOutput -= ((net.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick));
                totalAvailable -= tesseract.PowerOutput;
            }

            //Log.Message("All Total: " + totalAvailable);

            //Fill up all networks to be at least at 0
            foreach (PowerNet net in nets.Where(net => (GetAllPowerInNetwork(net)) < 0))
            {
                float curEnergy = GetAllPowerInNetwork(net);
                CompsTesseract tesseract = setTesseractToZero(net);
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
            //Ensures that all Networks have at least 500 Watts to protect against random changes in power consumption.

            if ((totalAvailable / nets.Count) > 500)
            {
                foreach (PowerNet net in nets)
                {
                    CompsTesseract tesseract = setTesseractToZero(net);

                    float curProd = (net.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick);
                    tesseract.PowerOutput += 500 - curProd;
                    totalAvailable -= 500;
                    curProd = (net.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick);
                }
            }

            //Log.Message("Total after all 500: " + totalAvailable);
            //All Excess power will be send to powernets with batteries

            List<PowerNet> netsWithBattery = nets.Where(item => item.batteryComps != null).ToList();
            float toDistribute;
            if (netsWithBattery.Count > 0)
            {

                toDistribute = totalAvailable / netsWithBattery.Count;
                totalAvailable = 0;
                //Log.Message("To Distribute Batteries: "+toDistribute);
                foreach (PowerNet net in netsWithBattery)
                {
                    CompsTesseract tesseract = setTesseractToZero(net);

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
                    CompsTesseract tesseract = setTesseractToZero(net);

                    tesseract.PowerOutput += toDistribute;
                }
            }
        }
        */
        public void DistributePowerBetter()
        {
            Dictionary<PowerNet, bool> nets = new Dictionary<PowerNet, bool>();
            List<CompsTesseract> tesseracts = TesseractNet.Instance.Tesseracts;
            for (int i = tesseracts.Count - 1; i >= 0; i--)
            {
                nets.AddDistinct(tesseracts[i].PowerNet, true);
            }
            float totalAvailablePower = nets.Sum(t => t.Key.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick) - TesseractNet.Instance.tesseractPower;

            float networkPowerSurplus = totalAvailablePower / nets.Count;
            foreach (var tesseract in tesseracts)
            {
                if (nets[tesseract.PowerNet])
                {
                    tesseract.PowerOutput += networkPowerSurplus - (tesseract.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick);
                    nets[tesseract.PowerNet] = false;
                }
            }
        }
    }
}
