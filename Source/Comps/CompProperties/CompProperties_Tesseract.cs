using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace CrossDimensionalPower
{
    public class CompProperties_Tesseract : CompProperties_Power
    {
        public CompProperties_Tesseract()
        {
            compClass = typeof(CompsTesseract);
        }
        public ThingDef thing;
    }
}
