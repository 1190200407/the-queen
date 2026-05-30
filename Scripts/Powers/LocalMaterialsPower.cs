using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

public sealed class LocalMaterialsPower : QueenPowerModel
{
    private sealed class Data
    {
        public int cardGeneratedCount;
        public int triggerCount;
    }

    protected override object? InitInternalData() => new Data();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
	public override bool IsInstanced => true;

    public override int DisplayAmount => GetInternalData<Data>().triggerCount - GetInternalData<Data>().cardGeneratedCount;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("TriggerCount", 0m)
    ];

    internal void ConfigureThresholdAsync(int threshold, Creature applier, CardModel? cardSource)
    {
        if (threshold <= 0)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        data.triggerCount = threshold;
        data.cardGeneratedCount = 0;
        base.DynamicVars["TriggerCount"].BaseValue = threshold;
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
    {
        if (!addedByPlayer || card.Owner?.Creature != base.Owner || Amount <= 0m)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        data.cardGeneratedCount++;
        if (data.cardGeneratedCount >= data.triggerCount)
        {
            Flash();
            await QueenCardCmd.AddSoulLamp(new ThrowingPlayerChoiceContext(), creator, 1);
            data.cardGeneratedCount = 0;
        }
        InvokeDisplayAmountChanged();
    }
}
