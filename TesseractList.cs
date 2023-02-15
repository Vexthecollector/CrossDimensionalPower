using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using Verse;

namespace CrossDimensionalPower
{
    public class TesseractList
    {
        
        public static List<CompsTesseract> ThisPowerNetTesseracts(CompsTesseract tesseract)
        {
            List<CompsTesseract> compsTesseracts =WorldComponent_Tesseracts.Instance.Tesseracts.FindAll(item => item.PowerNet == tesseract.PowerNet);
            return compsTesseracts;
        }
        public static float TesseractPowerOutput(List<CompsTesseract> list)
        {
            if (list.NullOrEmpty()) return 0;
            float positive = list.Sum(item => { if (item.PowerOutput > 0) return item.PowerOutput; else return 0; });
            return positive;
        }

    }

    public class WorldComponent_Tesseracts : WorldComponent
    {
        public static WorldComponent_Tesseracts Instance;

        private List<CompsTesseract> tesseracts = new List<CompsTesseract>();
        public WorldComponent_Tesseracts(World world) : base(world) => Instance = this;
        public List<CompsTesseract> Tesseracts
        {
            get { return this.tesseracts; }
        }



        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref this.tesseracts, "tesseract", LookMode.Reference);
        }

    }
}
