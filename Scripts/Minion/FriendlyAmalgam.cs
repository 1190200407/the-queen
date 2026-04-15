using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MinionLib.Models;

namespace ComicChess.TheQueen;

public class FriendlyAmalgam : MinionModel
{
    public override int MaxInitialHp => 1;
    public override int MinInitialHp => 1;

    protected override string VisualsPath => base.VisualsPath;

    public override Task OnSummon(Player owner, Creature self, MinionSummonOptions options)
    {
        //TODO 加上为你而死，初始化意图效果
        return Task.CompletedTask;
    }
}