using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;
using static HarmonyLib.Code;

namespace IconianPsycasts
{
    [StaticConstructorOnStartup]
    public class Iconian_Minefield : ThingWithComps
    {
        private static readonly MaterialPropertyBlock matPropertyBlock = new MaterialPropertyBlock();

        private static readonly Material TornadoMaterial = MaterialPool.MatFrom("Effects/Conflagrator/FireTornado/FireTornadoFat", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.Tornado);

        private static readonly FleckDef FireTornadoPuff = DefDatabase<FleckDef>.GetNamed("VPE_FireTornadoDustPuff");

        public int ticksLeftToDisappear = -1;

        private int spawnTick;

        public float radius = 10.9f;
        public Pawn caster;

        private bool isEnemyInRadius = false;
        private int mineSpawnInterval= 600;


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref spawnTick, "spawnTick", 0);
            Scribe_Values.Look(ref ticksLeftToDisappear, "ticksLeftToDisappear", 0);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                Vector3 vector = base.Position.ToVector3Shifted();
                spawnTick = Find.TickManager.TicksGame;
            }
        }

        public static void ThrowPuff(Vector3 loc, Map map, float scale, Color color)
        {
            if (loc.ShouldSpawnMotesAt(map))
            {
                FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, FireTornadoPuff, 1.9f * scale);
                dataStatic.rotationRate = Rand.Range(-60, 60);
                dataStatic.velocityAngle = Rand.Range(0, 360);
                dataStatic.velocitySpeed = Rand.Range(0.6f, 0.75f);
                dataStatic.instanceColor = color;
                map.flecks.CreateFleck(dataStatic);
            }
        }

        protected override void Tick()
        {
            if (!base.Spawned)
            {
                return;
            }
            if (ticksLeftToDisappear == 0 || caster == null || caster.DeadOrDowned)
            {
                Messages.Message("Minefield over", new TargetInfo(base.Position, base.Map), MessageTypeDefOf.NeutralEvent);
                Destroy();
            }
            if (ticksLeftToDisappear > 0 )
            {
                ticksLeftToDisappear--;
            }
/*            if (this.IsHashIntervalTick(mineSpawnInterval))
            {
                if (mines.Count < 10)
                {
                    IntVec3 spawnPos = GenRadial.RadialCellsAround(Position, radius, true).RandomElement();
                    Iconian_Mine mine = (Iconian_Mine)ThingMaker.MakeThing(DefOfs.Iconian_Mine);
                    mine.ticksLeftToDisappear = 1200;
                    mine.movementSpeed = 1.5f;
                    mine.targetPosition = spawnPos;
                    GenSpawn.Spawn(mine, spawnPos, Map, WipeMode.VanishOrMoveAside);
                }
            }*/

        }
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Rand.PushState();
            Rand.Seed = thingIDNumber;
            for (int i = 0; i < 90; i++)
            {
                //DrawTornadoPart(PartsDistanceFromCenter.RandomInRange, Rand.Range(0f, 360f), Rand.Range(0.9f, 1.1f), Rand.Range(0.52f, 0.88f));
            }
            Rand.PopState();
        }

        /*        private void DrawTornadoPart(float distanceFromCenter, float initialAngle, float speedMultiplier, float colorMultiplier)
                {
                    int ticksGame = Find.TickManager.TicksGame;
                    float num = 1f / distanceFromCenter;
                    float num2 = 25f * speedMultiplier * num;
                    float num3 = (initialAngle + (float)ticksGame * num2) % 360f;
                    Vector2 vector = realPosition.Moved(num3, AdjustedDistanceFromCenter(distanceFromCenter));
                    vector.y += distanceFromCenter * 4f;
                    vector.y += ZOffsetBias;
                    Vector3 vector2 = new Vector3(vector.x, AltitudeLayer.Weather.AltitudeFor() + 3f / 74f * Rand.Range(0f, 1f), vector.y);
                    float num4 = distanceFromCenter * 3f;
                    float num5 = 1f;
                    if (num3 > 270f)
                    {
                        num5 = GenMath.LerpDouble(270f, 360f, 0f, 1f, num3);
                    }
                    else if (num3 > 180f)
                    {
                        num5 = GenMath.LerpDouble(180f, 270f, 1f, 0f, num3);
                    }
                    float num6 = Mathf.Min(distanceFromCenter / (PartsDistanceFromCenter.max + 2f), 1f);
                    float num7 = Mathf.InverseLerp(0.18f, 0.4f, num6);
                    Vector3 vector3 = new Vector3(Mathf.Sin((float)ticksGame / 1000f + (float)(thingIDNumber * 10)) * 2f, 0f, 0f);
                    Vector3 pos = vector2 + vector3 * num7;
                    float a = Mathf.Max(1f - num6, 0f) * num5;
                    Color value = new Color(colorMultiplier, colorMultiplier, colorMultiplier, a);
                    matPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
                    Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0f, num3, 0f), new Vector3(num4, 1f, num4));
                    Graphics.DrawMesh(MeshPool.plane10, matrix, TornadoMaterial, 0, null, 0, matPropertyBlock);
                }*/

/*        public bool IsPawnTargetted(Pawn target)
        {
            return mines.Where(c => c.target == target).Any();
        }*/
    }
}
