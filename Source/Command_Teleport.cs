using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.PsychicRitualRoleDef;
using System.Security.Cryptography;
using VanillaPsycastsExpanded;
using static UnityEngine.GraphicsBuffer;
using Verse.Noise;

namespace IconianPsycasts
{
    [StaticConstructorOnStartup]
    public class Command_Teleport : Command
    {
        public Building_TurretSentry turret;
        public float range;
        public new static readonly Texture2D BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityButBG");
        public new static readonly Texture2D BGTexShrunk = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityButBGShrunk");
        private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));
        public override Texture2D BGTexture => BGTex;
        public override Texture2D BGTextureShrunk => BGTexShrunk;
        public override bool Disabled
        {
            get
            {
                DisabledCheck();
                return disabled;
            }
            set
            {
                disabled = value;
            }
        }
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {

            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
            if (KeyBindingDefOf.QueueOrder.IsDownEvent)
            {
                Widgets.FillableBar(rect, 0f, cooldownBarTex, null, doBorder: false);
            }
            else if (turret.teleportCooldownTicksRemaining > 0)
            {
                float value = Mathf.InverseLerp(turret.teleportCooldownTicksTotal, 0f, turret.teleportCooldownTicksRemaining);
                Widgets.FillableBar(rect, Mathf.Clamp01(value), cooldownBarTex, null, doBorder: false);
                if (turret.teleportCooldownTicksRemaining > 0)
                {
                    Text.Font = GameFont.Tiny;
                    string text = turret.teleportCooldownTicksRemaining.ToStringTicksToPeriod();
                    Vector2 vector = Text.CalcSize(text);
                    vector.x += 2f;
                    Rect rect2 = rect;
                    rect2.x = rect.x + rect.width / 2f - vector.x / 2f;
                    rect2.width = vector.x;
                    rect2.height = vector.y;
                    Rect position = rect2.ExpandedBy(8f, 0f);
                    Text.Anchor = TextAnchor.UpperCenter;
                    GUI.DrawTexture(position, TexUI.GrayTextBG);
                    Widgets.Label(rect2, text);
                    Text.Anchor = TextAnchor.UpperLeft;
                }
            }
            if (result.State == GizmoState.Interacted && turret.teleportCooldownTicksRemaining <= 0)
            {
                return result;
            }
            return new GizmoResult(result.State);
        }

        protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
        {
            if (Mouse.IsOver(butRect))
            {
                if (parms.multipleSelected)
                {
                    if (turret.Map != null)
                    {
                        GenUI.DrawArrowPointingAtWorldspace(turret.DrawPos, Find.Camera);
                    }
                }
            }
            DisabledCheck();
            return base.GizmoOnGUIInt(butRect, parms);
        }

        protected virtual void DisabledCheck()
        {
            disabled = turret.teleportCooldownTicksRemaining > 0;
            if (disabled)
            {
                DisableWithReason("AbilityOnCooldown".Translate(turret.teleportCooldownTicksRemaining.ToStringTicksToPeriod()).Resolve().CapitalizeFirst());
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            Find.DesignatorManager.Deselect();
            Find.Targeter.BeginTargeting(TargetingParameters.ForDropPodsDestination(), delegate (LocalTargetInfo target)
            {
                Effecter portalEffecter = DefOfs.Iconian_TeleportEffect.Spawn(turret.PositionHeld, turret.MapHeld, new Vector3(0, 3, 0));
                Building_TurretSentry thing = (Building_TurretSentry)ThingMaker.MakeThing(turret.def);
                thing.HitPoints = turret.HitPoints - 1250 / Helper.TurretHealthTimeRatio;
                thing.Duration = turret.Duration;
                thing.TryGetComp<CompBreakLinkBuilding>().Pawn = turret.compBreakLink.Pawn;
                thing.teleportCooldownTicksTotal = turret.teleportCooldownTicksTotal;
                thing.teleportCooldownTicksRemaining = thing.teleportCooldownTicksTotal;
                GenSpawn.Spawn(thing, target.Cell, turret.MapHeld);
                Effecter portalEffecterTarget = DefOfs.Iconian_TeleportEffect.Spawn(target.Cell, turret.MapHeld, new Vector3(0, 3, 0));

                turret.Destroy(DestroyMode.Vanish);
            },
            null,
            delegate (LocalTargetInfo target)
            {
                if (target.Cell.DistanceTo(turret.Position) < range)
                {
                    return true;
                }
                Messages.Message("Nuhuh".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            },null,null,null,true,null, delegate
            {
                GenDraw.DrawRadiusRing(turret.Position, range);
            });
        }

        public override void GizmoUpdateOnMouseover()
        {
            if (Find.CurrentMap == null)
            {
                return;
            }
            if (range > 0f && range < GenRadial.MaxRadialPatternRadius)
            {
                GenDraw.DrawRadiusRing(turret.Position, range);
            }
        }

        protected void DisableWithReason(string reason)
        {
            disabledReason = reason;
            disabled = true;
        }
    }
}
