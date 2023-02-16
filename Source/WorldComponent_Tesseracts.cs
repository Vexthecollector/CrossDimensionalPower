using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using RimWorld.Planet;

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
                TesseractNetManager.Instance.DistributePower();
            }
        }


        public WorldComponent_Tesseracts(World world) : base(world) => Instance = this;


    }
}
