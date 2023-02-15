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
    }

    public class WorldComponent_Tesseracts : WorldComponent
    {
        public static WorldComponent_Tesseracts Instance;

        public WorldComponent_Tesseracts(World world) : base(world) => Instance = this;
        private List<CompsTesseract> tesseracts = new List<CompsTesseract>();
        public List<CompsTesseract> Tesseracts
        {
            get { return this.tesseracts; }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref this.tesseracts, "Select ", LookMode.Reference);
        }

    }
}
