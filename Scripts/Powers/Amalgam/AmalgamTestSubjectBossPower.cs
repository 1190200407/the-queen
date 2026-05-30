using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ComicChess.TheQueen;

/// <summary>
/// 实验体之力：聚合体每回合在「激怒 / 疼痛戳刺 / 无实体」之间循环。
/// 当进入无实体时，将聚合体透明化；离开无实体时恢复正常颜色。
/// </summary>
public sealed class AmalgamTestSubjectBossPower : QueenPowerModel
{
    private enum Mode
    {
        Enrage = 0,
        PainfulStabs = 1,
        Intangible = 2,
    }

    private sealed class Data
    {
        public int mode;
        public bool initialized;
    }

    protected override object? InitInternalData() => new Data();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (base.Owner is not { IsAlive: true })
        {
            return;
        }

        // 获得该能力的当下就开始循环（从激怒开始）。
        Data data = GetInternalData<Data>();
        if (data.initialized)
        {
            return;
        }

        data.initialized = true;
        data.mode = (int)Mode.Enrage;
        await ApplyModeAsync(Mode.Enrage);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;
        if (base.Owner is not { IsAlive: true } || player != base.Owner.PetOwner)
        {
            return;
        }

        Data data = GetInternalData<Data>();

        // 获得当下已初始化过：之后每回合 +1。
        data.mode = (data.mode + 1) % 3;

        await ApplyModeAsync((Mode)data.mode);
    }

    private async Task ApplyModeAsync(Mode mode)
    {
        // 清理旧状态（无实体依赖持续回合；这里主动移除以保证“只在该回合生效”的循环语义）。
        await PowerCmd.Remove<AmalgamEnragePower>(base.Owner);
        await PowerCmd.Remove<AmalgamPainfulStabsPower>(base.Owner);
        await PowerCmd.Remove<AmalgamIntangiblePower>(base.Owner);

        SetColor(Colors.White);

        switch (mode)
        {
            case Mode.Enrage:
                await PowerCmd.Apply<AmalgamEnragePower>(base.Owner, 1m, base.Owner.PetOwner?.Creature, null);
                break;
            case Mode.PainfulStabs:
                await PowerCmd.Apply<AmalgamPainfulStabsPower>(base.Owner, 1m, base.Owner.PetOwner?.Creature, null);
                break;
            case Mode.Intangible:
                await PowerCmd.Apply<AmalgamIntangiblePower>(base.Owner, 1m, base.Owner.PetOwner?.Creature, null);
                SetColor(StsColors.halfTransparentWhite);
                break;
        }
    }

    private void SetColor(Color color)
    {
        (NCombatRoom.Instance?.GetCreatureNode(base.Owner))?.GetSpecialNode<CanvasGroup>("%CanvasGroup")?.SetSelfModulate(color);
    }

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        _ = choiceContext;
        _ = deathAnimLength;
        if (wasRemovalPrevented || creature != base.Owner)
        {
            return Task.CompletedTask;
        }

        // 防止残留透明。
        SetColor(Colors.White);
        return Task.CompletedTask;
    }
}

