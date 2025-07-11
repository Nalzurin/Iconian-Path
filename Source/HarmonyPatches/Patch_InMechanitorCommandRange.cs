using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace IconianPsycasts.HarmonyPatches
{
    [HarmonyPatch]
    public static class Patch_InMechanitorCommandRange
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(MechanitorUtility), nameof(MechanitorUtility.InMechanitorCommandRange));
        }

        public static bool Postfix(bool __result, Pawn mech)
        {
            if(mech is Pawn_Summoned)
            {
                return true;
            }
            return false;
        }
    }
}
