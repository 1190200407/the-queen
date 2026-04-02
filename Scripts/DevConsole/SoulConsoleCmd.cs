using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;

namespace ComicChess.TheQueen;

public class SoulConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "soul";

	public override string Args => "<amount:int>";

	public override string Description => "Adds Soul Lamp stacks to player";

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
			return new CmdResult(success: false, "The soul amount cannot be negative.");
		}

		Task task = QueenCardCmd.AddSoulLamp(issuingPlayer, amount);
		return new CmdResult(task, success: true, $"Added '{amount}' Soul Lamp.");
	}
}
