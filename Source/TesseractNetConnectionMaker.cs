using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace CrossDimensionalPower
{
    public class TesseractNetConnectionMaker
    {


        private static TesseractNetConnectionMaker instance = new TesseractNetConnectionMaker();
        public static TesseractNetConnectionMaker Instance
        {
            get { return instance; }
        }



        private void AddPowerNet(CompsTesseract tesseract)
        {
            TesseractNet.Instance.PowerNets.AddDistinct(tesseract.PowerNet);
            //AddClass(tesseract);

        }

        private void CheckPowerNetForRemoval()
        {
            try
            {


                foreach (PowerNet powerNet in TesseractNet.Instance.PowerNets)
                {
                    bool hasTesseract = false;
                    {
                        foreach (CompsTesseract tesseract in TesseractNet.Instance.Tesseracts)
                            if (tesseract.PowerNet == powerNet) hasTesseract = true;
                    }
                    if (!hasTesseract)
                    {
                        ForceRemovePowerNet(powerNet);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.Message(ex.Message);
            }
        }


        private void ForceRemovePowerNet(PowerNet powerNet)
        {
            TesseractNet.Instance.PowerNets.Remove(powerNet);
        }

        private void RemovePowerNet(PowerNet powerNet)
        {
            if (!TesseractNet.Instance.TesseractLists.Any(item => item.powerNet == powerNet))
                TesseractNet.Instance.PowerNets.Remove(powerNet);

        }

        public void AddTesseract(CompsTesseract tesseract)
        {
            TesseractNet.Instance.Tesseracts.AddDistinct(tesseract);
        }

        public void RemoveTesseract(CompsTesseract tesseract)
        {
            TesseractNet.Instance.Tesseracts.Remove(tesseract);
            RemovePowerNet(tesseract.PowerNet);
            //RemoveClass(tesseract);
        }
        /*
        public void FillClasses()
        {

            foreach (CompsTesseract tesseract in TesseractNet.Instance.Tesseracts)
            {
                AddClass(tesseract);
            }
        }*/
        /*
        public void RecheckClasses()
        {
            GenerateNetsFromTesseracts();
            CheckPowerNetForRemoval();
            foreach (TesseractList tesseractClass in TesseractNet.Instance.TesseractLists)
            {
                if (TesseractNet.Instance.TesseractLists.Any(item => item.tesseract.Count() > 0))
                {

                    tesseractClass.curEnergyGain = (tesseractClass.tesseract[0].PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick) - tesseractClass.tesseract[0].PowerOutput;
                    tesseractClass.tesseract[0] = (CompsTesseract)tesseractClass.powerNet.powerComps.First(item => item.GetType() == typeof(CompsTesseract));
                }


                else
                {
                    RemoveClass(tesseractClass.tesseract[0]);
                }
            }
        }

        public void CheckDuplicateTesseracts()
        {
            List<CompsTesseract> current;
            foreach (TesseractList tesseractClass in TesseractNet.Instance.TesseractLists)
            {
                current = tesseractClass.tesseract;
                foreach (CompsTesseract tesseract in tesseractClass.tesseract)
                {

                }
            }
        }

        public void GenerateIfNotExist()
        {
            foreach (CompsTesseract tesseract in TesseractNet.Instance.Tesseracts)
            {
                if (tesseract.TesseractList == null)
                {
                    AddPowerNet(tesseract);
                }
            }
        }
        */

        public void GenerateNetsFromTesseracts()
        {
            foreach (CompsTesseract tesseract in TesseractNet.Instance.Tesseracts)
            {
                AddPowerNet(tesseract);
            }
        }


        /*
        public void AddClass(CompsTesseract tesseract)
        {
            if (tesseract.TesseractList == null && ((TesseractNet.Instance.TesseractLists.Count() > 0) ?
                !TesseractNet.Instance.TesseractLists.Any(item => item.tesseract.Any(tes => tes == tesseract)) : true))
            {
                float curenergy = tesseract.PowerNet.CurrentEnergyGainRate();
                curenergy = curenergy / CompPower.WattsToWattDaysPerTick;
                curenergy = curenergy - tesseract.PowerOutput;
                TesseractList tesseractList = new TesseractList(tesseract.PowerNet, curenergy, tesseract);
                TesseractNet.Instance.TesseractLists.Add(tesseractList);
                tesseract.TesseractList = tesseractList;
            }

        }

        public void RemoveClass(CompsTesseract tesseract)
        {
            if (tesseract.TesseractList.tesseract.Count() == 1)
                TesseractNet.Instance.TesseractLists.Remove(tesseract.TesseractList);
            else
            {
                tesseract.TesseractList.tesseract.Remove(tesseract);
            }
        }

        public bool CheckForNet(PowerNet powerNet)
        {
            return TesseractNet.Instance.PowerNets.Contains(powerNet);
        }
        */

    }
}
