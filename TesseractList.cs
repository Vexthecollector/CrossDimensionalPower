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



    public class WorldComponent_Tesseracts : WorldComponent
    {
        public static WorldComponent_Tesseracts Instance;
        int timer = 0;
        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            timer++;
            if (timer > 300)
            {
                timer = 0;
                TesseractNetManager.Instance.EqualizePower();
            }
        }


        public WorldComponent_Tesseracts(World world) : base(world) => Instance = this;


    }

    public class TesseractNet
    {



        private static readonly TesseractNet instance = new TesseractNet();
        public static TesseractNet Instance
        {
            get { return instance; }
        }
        private List<PowerNet> powerNets = new List<PowerNet>();
        private List<CompsTesseract> tesseracts = new List<CompsTesseract>();
        private List<TesseractList> tesseractLists = new List<TesseractList>();

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

    public class TesseractList
    {
        public TesseractList(PowerNet powerNet, float curEnergyGain, CompsTesseract tesseract)
        {
            this.powerNet = powerNet;
            this.curEnergyGain = curEnergyGain;
            this.tesseract = tesseract;

        }

        public TesseractList(PowerNet powerNet, float curEnergyGain)
        {
            this.powerNet = powerNet;
            this.curEnergyGain = curEnergyGain;

        }

        public PowerNet powerNet;
        public float curEnergyGain;
        public CompsTesseract tesseract;
        public float curStoredEnergy = 0;
        public float maxStoredEnergy = 0;
    }



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
            if (TesseractNet.Instance.TesseractLists.NullOrEmpty()) return;
            Log.Message("Equalizing");
            
            float totalAvailable = TesseractNet.Instance.TesseractLists.Sum(item =>item.curEnergyGain);
            //float totalAvailable = TesseractNet.Instance.TesseractLists.Sum(item => { if (item.curEnergyGain > 0) return item.curEnergyGain; return 0; });
            Log.Message("Total: "+totalAvailable);
            foreach (TesseractList tesseractClass in TesseractNet.Instance.TesseractLists.Where(item => item.curEnergyGain < 0))
            {
                if (tesseractClass.curEnergyGain < totalAvailable)
                {
                    totalAvailable += tesseractClass.curEnergyGain;
                    tesseractClass.curEnergyGain = 0;
                    tesseractClass.tesseract.PowerOutput = 0;
                    tesseractClass.tesseract.curPower = 0;
                }
                else
                {
                    tesseractClass.curEnergyGain = tesseractClass.curEnergyGain + totalAvailable;
                    totalAvailable = 0;
                    tesseractClass.tesseract.PowerOutput = tesseractClass.curEnergyGain;
                    tesseractClass.tesseract.curPower = tesseractClass.curEnergyGain;
                }
            }
            Log.Message("Total after set to 0: " + totalAvailable);
            if (totalAvailable / TesseractNet.Instance.TesseractLists.Count() > 2000)
            {
                foreach (TesseractList tesseractClass in TesseractNet.Instance.TesseractLists.Where(item => item.curEnergyGain < 0))
                {

                    totalAvailable -= 2000;
                    tesseractClass.curEnergyGain = 2000;
                    tesseractClass.tesseract.PowerOutput = 2000;
                    tesseractClass.tesseract.curPower = 2000;

                }
            }
            Log.Message("Total After 2000 each: " + totalAvailable);
            CheckBatteries();

            List<TesseractList> tesseractClassesWB = TesseractNet.Instance.TesseractLists.Where(item => item.maxStoredEnergy > 0).ToList();
            float toDistribute = totalAvailable / tesseractClassesWB.Count();
            totalAvailable = 0;
            Log.Message("toDistribute: " + toDistribute);
            foreach (TesseractList tesseractClass in tesseractClassesWB)
            {
                tesseractClass.curEnergyGain = toDistribute;
                tesseractClass.tesseract.PowerOutput = toDistribute;
                tesseractClass.tesseract.curPower = toDistribute;
            }
            TesseractNetConnectionMaker.Instance.RecheckClasses();
        }

        public void CheckBatteries()
        {
            foreach (TesseractList tesseractClass in TesseractNet.Instance.TesseractLists)
            {
                if (!tesseractClass.powerNet.batteryComps.NullOrEmpty())
                {

                    tesseractClass.curStoredEnergy = tesseractClass.powerNet.CurrentStoredEnergy();
                    tesseractClass.maxStoredEnergy = tesseractClass.powerNet.batteryComps.Sum(battery => battery.Props.storedEnergyMax);
                }

            }

        }
    }

    public class TesseractNetConnectionMaker
    {


        private static TesseractNetConnectionMaker instance = new TesseractNetConnectionMaker();
        public static TesseractNetConnectionMaker Instance
        {
            get { return instance; }
        }



        private void AddPowerNet(PowerNet powerNet)
        {
            TesseractNet.Instance.PowerNets.AddDistinct(powerNet);
            AddClass(powerNet);

        }

        private void RemovePowerNet(PowerNet powerNet)
        {
            TesseractNet.Instance.PowerNets.Remove(powerNet);
        }

        public void AddTesseract(CompsTesseract tesseract)
        {
            Log.Message("Added Tesseract");
            TesseractNet.Instance.Tesseracts.AddDistinct(tesseract);
        }

        public void RemoveTesseract(CompsTesseract tesseract)
        {
            TesseractNet.Instance.Tesseracts.Remove(tesseract);
            CheckPowerNetsForRemoval(tesseract.PowerNet);
        }

        private void CheckPowerNetsForRemoval(PowerNet powerNet)
        {
            if (TesseractNet.Instance.Tesseracts.Count(item => item.PowerNet == powerNet) == 1)
            {
                RemovePowerNet(powerNet);
                RemoveClass(powerNet);
            };
        }

        public void FillClasses()
        {

            foreach (PowerNet powerNet in TesseractNet.Instance.PowerNets)
            {
                AddClass(powerNet);
            }
        }

        public void RecheckClasses()
        {
            GenerateNetsFromTesseracts();
            foreach (TesseractList tesseractClass in TesseractNet.Instance.TesseractLists)
            {
                if (CheckForNet(tesseractClass.powerNet))
                {
                    tesseractClass.curEnergyGain = (tesseractClass.powerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick) - tesseractClass.tesseract.PowerOutput;
                    tesseractClass.tesseract = (CompsTesseract)tesseractClass.powerNet.powerComps.First(item => item.GetType() == typeof(CompsTesseract));
                }
                else
                {
                    RemoveClass(tesseractClass.powerNet);
                }
            }
        }

        public void GenerateNetsFromTesseracts()
        {
            foreach(CompsTesseract tesseract in TesseractNet.Instance.Tesseracts)
            {
                AddPowerNet(tesseract.PowerNet);
            }
        }

        public void AddClass(PowerNet powerNet)
        {

            if (!TesseractNet.Instance.TesseractLists.Any(item => item.powerNet == powerNet))
            {
                CompsTesseract tesseract = (CompsTesseract)powerNet.powerComps.First(item => item.GetType() == typeof(CompsTesseract));
                TesseractNet.Instance.TesseractLists.Add(new TesseractList(powerNet, (powerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick) - tesseract.PowerOutput, tesseract));
            }
        }

        public void RemoveClass(PowerNet powerNet)
        {
            TesseractNet.Instance.TesseractLists.Remove(TesseractNet.Instance.TesseractLists.Find(item => item.powerNet == powerNet));
        }

        public bool CheckForNet(PowerNet powerNet)
        {
            return TesseractNet.Instance.PowerNets.Contains(powerNet);
        }


    }


}
