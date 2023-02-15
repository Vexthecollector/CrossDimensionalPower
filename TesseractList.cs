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

        public static List<CompsTesseract> ThisPowerNetTesseracts(CompsTesseract tesseract)
        {
            List<CompsTesseract> compsTesseracts = WorldComponent_Tesseracts.Instance.Tesseracts.FindAll(item => item.PowerNet == tesseract.PowerNet);
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
        int timer = 0;
        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            timer++;
            if (timer > 300)
            {
                timer = 0;
            }
        }

        private List<CompsTesseract> tesseracts = new List<CompsTesseract>();

        public WorldComponent_Tesseracts(World world) : base(world) => Instance = this;
        public List<CompsTesseract> Tesseracts
        {
            get { return this.tesseracts; }
        }


        /*
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<CompsTesseract>(ref this.tesseracts, "tesseract", LookMode.Reference);
        }*/

    }

    public class TesseractNet
    {
        public static TesseractNet Instance;
        private List<PowerNet> powerNets = new List<PowerNet>();
        private List<CompsTesseract> tesseracts = new List<CompsTesseract>();

        public List<PowerNet> PowerNets
        {
            get { return this.powerNets; }
        }
        public List<CompsTesseract> Tesseracts
        {
            get { return this.tesseracts; }
        }
    }

    public class TesseractClassIDK
    {
        public TesseractClassIDK(PowerNet powerNet, float curEnergyGain, CompsTesseract tesseract)
        {
            this.powerNet = powerNet;
            this.curEnergyGain = curEnergyGain;
            this.tesseract = tesseract;
            
        }

        public PowerNet powerNet;
        public float curEnergyGain;
        public CompsTesseract tesseract;
        public float curStoredEnergy=0;
        public float maxStoredEnergy=0;
    }



    public class TesseractNetManager
    {
        List<PowerNet> powerNets = TesseractNet.Instance.PowerNets;
        List<TesseractClassIDK> tesseractClasses = new List<TesseractClassIDK>();


        public void EqualizePower()
        {
            CheckBatteries();
            tesseractClasses.OrderByDescending(item => item.curEnergyGain);
            float totalAvailable = tesseractClasses.Sum(item => { if (item.curEnergyGain > 0) return item.curEnergyGain; return 0; });
            foreach (TesseractClassIDK tesseractClass in tesseractClasses.Where(item => item.curEnergyGain < 0))
            {
                if (tesseractClass.curEnergyGain < totalAvailable)
                {
                    totalAvailable += tesseractClass.curEnergyGain;
                    tesseractClass.curEnergyGain = 0;
                    tesseractClass.tesseract.PowerOutput = 0;
                }
                else
                {
                    tesseractClass.curEnergyGain = tesseractClass.curEnergyGain + totalAvailable;
                    totalAvailable = 0;
                    tesseractClass.tesseract.PowerOutput = tesseractClass.curEnergyGain;
                }
            }

            if (totalAvailable / tesseractClasses.Count() > 2000)
            {
                foreach (TesseractClassIDK tesseractClass in tesseractClasses.Where(item => item.curEnergyGain < 0))
                {

                    totalAvailable -= 2000;
                    tesseractClass.curEnergyGain = 2000;
                    tesseractClass.tesseract.PowerOutput = 2000;

                }
            }

            List<TesseractClassIDK> tesseractClassesWB = tesseractClasses.Where(item => item.maxStoredEnergy > 0).ToList();
            float toDistribute=totalAvailable / tesseractClassesWB.Count();
            totalAvailable = 0;

            foreach (TesseractClassIDK tesseractClass in tesseractClassesWB)
            {
                tesseractClass.curEnergyGain = toDistribute;
                tesseractClass.tesseract.PowerOutput= toDistribute;
            }

        }

        public void CheckBatteries()
        {
            foreach(TesseractClassIDK tesseractClass in tesseractClasses)
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
        public static TesseractNetConnectionMaker Instance;
        List<PowerNet> powerNets = TesseractNet.Instance.PowerNets;
        List<CompsTesseract> tesseracts = TesseractNet.Instance.Tesseracts;
        List<TesseractClassIDK> tesseractClasses = new List<TesseractClassIDK>();


        private void AddPowerNet(PowerNet powerNet)
        {
            powerNets.AddDistinct(powerNet);

        }

        private void RemovePowerNet(PowerNet powerNet)
        {
            powerNets.Remove(powerNet);
        }

        public void AddTesseract(CompsTesseract tesseract)
        {
            tesseracts.AddDistinct(tesseract);
            AddPowerNet(tesseract.PowerNet);
            AddClass(tesseract.PowerNet);
        }

        public void RemoveTesseract(CompsTesseract tesseract)
        {
            tesseracts.Remove(tesseract);
            CheckPowerNetsForRemoval(tesseract.PowerNet);
        }

        private void CheckPowerNetsForRemoval(PowerNet powerNet)
        {
            if (tesseracts.Count(item => item.PowerNet == powerNet) == 1)
            {
                RemovePowerNet(powerNet);
                RemoveClass(powerNet);
            };
        }

        public void FillClasses()
        {

            foreach (PowerNet powerNet in powerNets)
            {
                AddClass(powerNet);
            }
        }

        public void RecheckClasses()
        {
            foreach (TesseractClassIDK tesseractClass in tesseractClasses)
            {
                if (CheckForNet(tesseractClass.powerNet))
                {
                    tesseractClass.curEnergyGain = tesseractClass.powerNet.CurrentEnergyGainRate();
                    tesseractClass.tesseract = (CompsTesseract)tesseractClass.powerNet.powerComps.First(item => item.GetType() == typeof(CompsTesseract));
                }
                else
                {
                    RemoveClass(tesseractClass.powerNet);
                }
            }
        }

        public void AddClass(PowerNet powerNet)
        {
            if(!tesseractClasses.Any(item => item.powerNet == powerNet))
                tesseractClasses.Add(new TesseractClassIDK(powerNet, powerNet.CurrentEnergyGainRate(), (CompsTesseract)powerNet.powerComps.First(item => item.GetType() == typeof(CompsTesseract))));
        }

        public void RemoveClass(PowerNet powerNet)
        {
            tesseractClasses.Remove(tesseractClasses.Find(item => item.powerNet == powerNet));
        }

        public bool CheckForNet(PowerNet powerNet)
        {
            return powerNets.Contains(powerNet);
        }


    }


}
