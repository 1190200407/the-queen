using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MinionLib.Models;

namespace ComicChess.TheQueen;

public class FriendlyAmalgam : MinionModel
{
    public override int MaxInitialHp => 1;
    public override int MinInitialHp => 1;

    protected override string VisualsPath => "res://TheQueen/scenes/creature_visuals/torch_head_amalgam_minion.tscn";

    public const string IdleAnimName = "idle_loop";
    public const string DeathAnimName = "die";
    public const string BuffAnimName = "buff";
    public const string DebuffAnimName = "_ignore/hug2";
    public const string AttackAnimName = "attack";
    public const string PowerAttackAnimName = "debuff";
    public const string SleepAnimName = "_ignore/string_rigging";

    public override Task OnSummon(Player owner, Creature self, MinionSummonOptions options)
    {
        //TODO 加上为你而死，初始化意图效果
        return Task.CompletedTask;
    }
}