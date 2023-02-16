using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using Verse;
using RimWorld;

namespace CrossDimensionalPower
{
    public class TesseractList
    {
        public TesseractList(PowerNet powerNet, float curEnergyGain, CompsTesseract tesseract)
        {
            this.powerNet = powerNet;
            this.curEnergyGain = curEnergyGain;
            this.tesseract.Add(tesseract);
        }

        public PowerNet powerNet;
        public float curEnergyGain;
        public List<CompsTesseract> tesseract = new List<CompsTesseract>();
        public float curStoredEnergy = 0;
        public float maxStoredEnergy = 0;
    }
}
