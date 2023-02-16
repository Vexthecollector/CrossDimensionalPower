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
    }
}
