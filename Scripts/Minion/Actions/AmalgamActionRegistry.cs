namespace ComicChess.TheQueen;

public static class AmalgamActionRegistry
{
    public const string Offense = "offense";

    public static AmalgamActionModel? Create(string actionId, decimal amount)
    {
        if (amount <= 0m)
        {
            return null;
        }

        return actionId switch
        {
            Offense => new AmalgamOffenseIntentAction(amount),
            _ => null
        };
    }

    public static AmalgamActionModel? CreateOffense(decimal damage) => Create(Offense, damage);
}

