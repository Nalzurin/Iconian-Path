using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IconianPsycasts
{
    public struct Backgrounds
    {
        public List<BackstoryDef> common;
        public List<BackstoryDef> rare;
        public List<BackstoryDef> epic;
        public List<BackstoryDef> legendary;

    }
    public struct TraitDegree
    {
        public string def;
        public int degree;
    }
    public struct Traits
    {
        public List<TraitDegree> common;
        public List<TraitDegree> rare;
        public List<TraitDegree> epic;
        public List<TraitDegree> legendary;
    }
    public class RerollExtension : DefModExtension
    {
        public Backgrounds backgrounds;
        public Traits traits;

    }
}
