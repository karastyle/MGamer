using CarlosLab.Common.Attributes;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Demo")]
    public class HasSlotNotUsed : InputFromSource<bool>
    {
        protected override bool OnGetRawInput(in InputContext context)
        {
            var hasNotUsed = AIGameManager.Player.HasSlotNotUsed(); 
            return hasNotUsed;
        }
    }
}