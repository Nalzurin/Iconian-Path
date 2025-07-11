using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace IconianPsycasts
{
    [StaticConstructorOnStartup]
    public class TrashEaterGizmo : Gizmo
    {
        private CompTrashEater eater;

        private const float Width = 160f;

        private static readonly Texture2D BarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.34f, 0.42f, 0.43f));

        private static readonly Texture2D BarHighlightTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.43f, 0.54f, 0.55f));

        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

        private static readonly Texture2D DragBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.74f, 0.97f, 0.8f));

        private const int Increments = 24;

        private static bool draggingBar;

        private float lastTargetValue;

        private float targetValue;

        private static List<float> bandPercentages;

        public TrashEaterGizmo(CompTrashEater eater)
        {
            this.eater = eater;
            targetValue = (float)eater.maxToFill / (float)eater.maxStored;
            if (bandPercentages == null)
            {
                bandPercentages = new List<float>();
                int num = 12;
                for (int i = 0; i <= num; i++)
                {
                    float item = 1f / (float)num * (float)i;
                    bandPercentages.Add(item);
                }
            }
        }

        public override float GetWidth(float maxWidth)
        {
            return 160f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(10f);
            Widgets.DrawWindowBackground(rect);
            Text.Font = GameFont.Small;
            TaggedString labelCap = eater.Props.thingToEat.LabelCap;
            float height = Text.CalcHeight(labelCap, rect2.width);
            Rect rect3 = new Rect(rect2.x, rect2.y, rect2.width, height);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect3, labelCap);
            Text.Anchor = TextAnchor.UpperLeft;
            lastTargetValue = targetValue;
            float num = rect2.height - rect3.height;
            float num2 = num - 4f;
            float num3 = (num - num2) / 2f;
            Rect rect4 = new Rect(rect2.x, rect3.yMax + num3, rect2.width, num2);
            Widgets.DraggableBar(rect4, BarTex, BarHighlightTex, EmptyBarTex, DragBarTex, ref draggingBar, eater.PercentageFull, ref targetValue, bandPercentages, 24);
            Text.Anchor = TextAnchor.MiddleCenter;
            rect4.y -= 2f;
            Widgets.Label(rect4, eater.currentStored + " / " + eater.maxStored);
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect4, () => GetResourceBarTip(), Gen.HashCombineInt(eater.GetHashCode(), 34242369));
            if (lastTargetValue != targetValue)
            {
                eater.maxToFill = Mathf.RoundToInt(targetValue * (float)eater.maxStored);
            }
            return new GizmoResult(GizmoState.Clear);
        }

        private string GetResourceBarTip()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Concat("MechCarrierAutofillResources".Translate() + " " + eater.Props.thingToEat.label + ": ", eater.maxToFill.ToString()));
            stringBuilder.AppendInNewLine("MechCarrierClickToSetAutofillAmount".Translate());
            stringBuilder.AppendLine();
            stringBuilder.AppendInNewLine("MechCarrierAutofillDesc".Translate(eater.parent.def.label, eater.Props.pawnToSpawn.labelPlural));
            return stringBuilder.ToString();
        }
    }
}
