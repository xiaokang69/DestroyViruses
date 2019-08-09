﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DestroyViruses
{
    public class EventVirus
    {
        public enum ActionType
        {
            HIT,
        }

        public ActionType action { get; private set; }
        public VirusBase virus { get; private set; }
        public float value { get; private set; }

        private static EventVirus sIns;
        public static EventVirus Get(ActionType action, VirusBase virus, float value)
        {
            if (sIns == null)
                sIns = new EventVirus();
            sIns.action = action;
            sIns.virus = virus;
            sIns.value = value;
            return sIns;
        }
    }
}