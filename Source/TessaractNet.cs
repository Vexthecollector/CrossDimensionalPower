using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CrossDimensionalPower
{
    public class TesseractNet
    {
        private readonly List<PowerNet> powerNets;
        private readonly List<CompsTesseract> tesseracts;
        private readonly List<TesseractList> tesseractLists;
        private TesseractNet()
        {
            powerNets = new List<PowerNet>();
            tesseracts = new List<CompsTesseract>();
            tesseractLists = new List<TesseractList>();
        }




        private static readonly TesseractNet instance = new TesseractNet();
        public static TesseractNet Instance
        {
            get { return instance; }
        }

        public List<PowerNet> PowerNets
        {
            get { return this.powerNets; }
        }
        public List<CompsTesseract> Tesseracts
        {
            get { return this.tesseracts; }
        }

        public List<TesseractList> TesseractLists
        {
            get { return this.tesseractLists; }
        }
    }
}
