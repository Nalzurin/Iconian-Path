using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
namespace IconianPsycasts.HarmonyPatches
{
    [StaticConstructorOnStartup]
    [HarmonyPatch]
    public static class Patch_TurretSentryInspectPane
    {
        private static readonly Texture2D BarBGTex = SolidColorMaterials.NewSolidColorTexture(new ColorInt(10, 10, 10).ToColor);

        private static readonly Texture2D HealthTex = SolidColorMaterials.NewSolidColorTexture(new ColorInt(35, 35, 35).ToColor);
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(RimWorld.InspectPaneFiller), nameof(RimWorld.InspectPaneFiller.DrawHealth));
        }
        private static void DrawTurretHealth(WidgetRow row, Building_TurretSentry t)
        {
            float fillPct;
            string label;
            if (t.HitPoints >= t.Duration)
            {
                GUI.color = Color.white;
            }
            else if ((float)t.HitPoints > (float)t.Duration * 0.5f)
            {
                GUI.color = Color.yellow;
            }
            else if (t.HitPoints > 0)
            {
                GUI.color = Color.red;
            }
            else
            {
                GUI.color = Color.grey;
            }
            fillPct = (float)t.HitPoints / (float)t.Duration;
            label = t.HitPoints.ToStringCached() + " / " + t.Duration.ToStringCached();
            row.FillableBar(93f, 16f, fillPct, label, HealthTex, BarBGTex);
            GUI.color = Color.white;
        }
        public static bool Prefix(WidgetRow row, Thing t)
        {
            if(t is Building_TurretSentry s)
            {
                DrawTurretHealth(row, s);
                return false;
            }
            return true;
        }
    }
}
