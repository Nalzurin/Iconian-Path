using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IconianPsycasts
{
    public interface ISummonSource
    {
        public List<Thing> SummonListForReading { get;}
        public void AddSummon(Thing thing);
        public void RemoveSummon(Thing thing);
    }
}
