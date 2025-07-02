using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VEF.Abilities;

namespace IconianPsycasts
{
    public class Dialog_SelectTrait : Window
    {
        private Vector2 scrollPosition;
        private float scrollRectAnimalSelectionHeight = 2000f;
        private float selectedEntryHeight = 75f;
        private Ability_RerollBackgroundTrait source;
        private Pawn target;
        private bool selected = false;
        private Trait oldTrait = null;
        public override Vector2 InitialSize => new Vector2(Mathf.Min(Screen.width - 50, 720f), 720f);
        public Dialog_SelectTrait(Ability_RerollBackgroundTrait _source, Pawn _target)
        {
            doCloseX = false;
            doCloseButton = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            draggable = false;
            drawShadow = true;
            onlyOneOfTypeAllowed = true;
            resizeable = false;
            source = _source;
            target = _target;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect traitRect = inRect;
            traitRect.y += 25f;
            traitRect.height -= 25f;
            traitRect = traitRect.ContractedBy(10f);

            Widgets.DrawMenuSection(traitRect);
            traitRect = traitRect.ContractedBy(4f);
            Rect scrollRect = new Rect(0f, 0f, traitRect.width - 16f, scrollRectAnimalSelectionHeight);
            Widgets.BeginScrollView(traitRect, ref scrollPosition, scrollRect);
            GUI.BeginGroup(scrollRect);
            DoEntries(target.story.traits, traitRect);
            GUI.EndGroup();
            Widgets.EndScrollView();
        }
        public void DoEntries(TraitSet traits, Rect inRect)
        {
            List<Trait> allTraits = new List<Trait>(traits.allTraits);
            int k = 0;
            foreach (Trait trait in allTraits)
            {
                Rect traitRect = new Rect(0f, k * selectedEntryHeight, inRect.width, selectedEntryHeight);
                traitRect.ContractedBy(20f);
                Rect labelRect = traitRect;
                labelRect.width *= 0.75f;
                labelRect.ContractedBy(5f, 0);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(labelRect, trait.LabelCap);
                Rect buttonRect = traitRect;
                buttonRect.width *= 0.20f;
                buttonRect.height *= 0.75f;
                buttonRect.x = labelRect.xMax;
                if (Widgets.ButtonText(buttonRect, "Select".Translate(), true))
                {
                    selected = true;
                    oldTrait = trait;
                    Close();
                }
                Text.Anchor = TextAnchor.UpperLeft;

                k++;
            }
        }
        public override void PostClose()
        {
            if (!selected)
            {
                source.Cancel();
            }
            else
            {
                source.DoReroll(oldTrait);
            }
            base.PostClose();
        }

    }
}
