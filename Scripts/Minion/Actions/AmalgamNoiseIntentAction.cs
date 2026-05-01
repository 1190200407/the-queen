using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：抽 1、下回合再抽 1，并为这两张牌附魔 <see cref="Dazed"/>。</summary>
public sealed class AmalgamNoiseIntentAction : AmalgamActionModel
{
    private const string NextTurnDrawParam = "nextTurnDraw";

    private readonly decimal _nextTurnDraw;

    public AmalgamNoiseIntentAction(decimal drawNow, decimal drawNextTurn)
        : base(new Dictionary<string, decimal>
        {
            [AmountParam] = drawNow,
            [NextTurnDrawParam] = drawNextTurn,
        })
    {
        _nextTurnDraw = GetParameterOrDefault(NextTurnDrawParam, 0m);
    }

    public override LocString IntentTitle => new("monsters", "FRIENDLY_AMALGAM.intent_draw.title");

    public override LocString GetIntentDescription()
    {
        LocString desc = new("intents", "AMALGAM_NOISE.description");
        desc.Add("Enchantment", new LocString("enchantments", ModelDb.Enchantment<Dazed>().Id.Entry + ".title"));
        return desc;
    }

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_NOISE",
            _ => Task.CompletedTask,
            new AmalgamNoiseIntent(ModelDb.Enchantment<Dazed>().Id.Entry));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (amalgam.PetOwner is not { Creature: { } ownerCreature } || !ownerCreature.IsAlive)
        {
            return;
        }

        Player? owner = ownerCreature.Player;
        if (owner == null)
        {
            return;
        }

        int drawNow = System.Math.Max(0, (int)Amount);
        if (drawNow > 0)
        {
            IEnumerable<CardModel> drawn = await CardPileCmd.Draw(choiceContext, drawNow, owner);
            foreach (CardModel card in drawn)
            {
                CardCmd.Enchant(ModelDb.Enchantment<Dazed>().ToMutable(), card, amount: 1m);
            }
        }

        int drawNextTurn = System.Math.Max(0, (int)_nextTurnDraw);
        if (drawNextTurn > 0)
        {
            await PowerCmd.Apply<NoisePendingPower>(ownerCreature, drawNextTurn, ownerCreature, cardSource: null);
        }
    }
}

