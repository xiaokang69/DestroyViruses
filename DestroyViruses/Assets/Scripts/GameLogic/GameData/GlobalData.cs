﻿using System;
namespace DestroyViruses
{
    public class GlobalData
    {
        public static bool isBattleTouchOn { get; set; } = false;
        public static float slowDownFactor { get { return !isBattleTouchOn ? ConstTable.table.noTouchSlowDown : 1f; } }
    }
}
