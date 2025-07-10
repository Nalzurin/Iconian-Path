using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace IconianPsycasts.TrashEater
{
    public class JobGiver_TrashEater : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            CompTrashEater comp = pawn.TryGetComp<CompTrashEater>(); 
            if(comp == null)
            {
                return null;
            }
            if(comp.AmountToAutofill == 0)
            {
                return null;
            }

        }
    }
}
