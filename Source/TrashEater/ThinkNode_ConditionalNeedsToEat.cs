using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace IconianPsycasts
{
    public class ThinkNode_ConditionalNeedsToEat : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            CompTrashEater comp = pawn.TryGetComp<CompTrashEater>();
            if(comp == null)
            {
                return false;
            }
            if(comp.AmountToAutofill == 0)
            {
                return false;
            }
            return true;
        }
    }
}
