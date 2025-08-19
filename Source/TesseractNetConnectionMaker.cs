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

        }

        private void RemovePowerNet(PowerNet powerNet)
        {
            if (!TesseractNet.Instance.TesseractLists.Any(item => item.powerNet == powerNet))
                TesseractNet.Instance.PowerNets.Remove(powerNet);
        }

        public void AddTesseract(CompsTesseract tesseract)
        {

            TesseractNet.Instance.Tesseracts.AddDistinct(tesseract);
            Log.Message("Added Tesseract"+tesseract.Name);
        }

        public void RemoveTesseract(CompsTesseract tesseract)
        {
            TesseractNet.Instance.Tesseracts.Remove(tesseract);
            RemovePowerNet(tesseract.PowerNet);
        }

        public void GenerateNetsFromTesseracts()
        {
            foreach (CompsTesseract tesseract in TesseractNet.Instance.Tesseracts)
            {
                AddPowerNet(tesseract);
            }
        }

    }
}
