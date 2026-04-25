using System.Threading.Tasks;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ComicChess.TheQueen;

/// <summary>控制台：直接给聚合体召唤指定血量。</summary>
public sealed class SummonConsoleCmd : AbstractConsoleCmd
{
    public override string CmdName => "summon";

    public override string Args => "<amount:int>";

    public override string Description => "Summon Amalgam with the given amount (HP).";

    public override bool IsNetworked => true;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (args.Length == 0)
        {
            return new CmdResult(success: false, "The first argument must be an int.");
        }

        if (!int.TryParse(args[0], out int amount))
        {
            return new CmdResult(success: false, "The first argument must be an int.");
        }

        if (issuingPlayer == null)
        {
            return new CmdResult(success: false, "This command only works during a run.");
        }

        if (issuingPlayer.PlayerCombatState == null)
        {
            return new CmdResult(success: false, "This command only works in combat.");
        }

        if (amount < 0)
        {
            return new CmdResult(success: false, "The summon amount cannot be negative.");
        }

        Task task = FriendlyAmalgamCmd.Summon(new ThrowingPlayerChoiceContext(), issuingPlayer, amount, source: null);
        return new CmdResult(task, success: true, $"Summoned '{amount}'.");
    }
}

