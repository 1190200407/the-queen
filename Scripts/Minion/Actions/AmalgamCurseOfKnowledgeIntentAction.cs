using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace ComicChess.TheQueen;

/// <summary>
/// 聚合体特殊意图：从指定 3 张牌中选择 1 张并立刻执行其 <see cref="KnowledgeDemon.IChoosable.OnChosen"/>。
/// 按原版 KnowledgeDemon 的 CurseOfKnowledge/ChooseCurse 流程实现。
/// </summary>
public sealed class AmalgamCurseOfKnowledgeIntentAction : AmalgamActionModel
{
    private static readonly LocString CurseOfKnowledgeDoneLine =
        new ("monsters", "FRIENDLY_AMALGAM.CURSE_OF_KNOWLEDGE.doneLine");

    public AmalgamCurseOfKnowledgeIntentAction()
        : base(1m)
    {
    }

    protected override MoveState CreateMoveState() =>
        new(
            "AMALGAM_CURSE_OF_KNOWLEDGE",
            _ => Task.CompletedTask,
            new AmalgamSpecialIntent("AMALGAM_CURSE_OF_KNOWLEDGE.description"));

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        _ = choiceContext;

        if (amalgam.CombatState is not { } combatState
            || amalgam.PetOwner is not { } queen
            || !queen.Creature.IsAlive)
        {
            return;
        }

        // 生成候选卡（给玩家实例），再打开 Choose-a-card 界面。
        List<CardModel> cards =
        [
            combatState.CreateCard<Rejuvenate>(queen),
            combatState.CreateCard<MindClarity>(queen),
            combatState.CreateCard<Disintegration>(queen),
        ];

        // 仅展示可选项（防御性过滤，避免未来调整导致界面报错）。
        cards = cards.Where(static c => c is KnowledgeDemon.IChoosable).ToList();
        if (cards.Count == 0)
        {
            return;
        }

        CardModel chosen = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), cards, queen);
        if (chosen is KnowledgeDemon.IChoosable choosable)
        {
            await choosable.OnChosen();
            // 对齐原版 <see cref="KnowledgeDemon.CurseOfKnowledge"/>：选择结算后播放 doneLine。
            TalkCmd.Play(CurseOfKnowledgeDoneLine, amalgam, VfxColor.Cyan, VfxDuration.Standard);
        }
    }
}

