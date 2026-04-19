namespace ComicChess.TheQueen;

public static class AmalgamActionRegistry
{
    public const string Offense = "offense";
    public const string Block = "block";
    public const string Vulnerable = "vulnerable";

    public static AmalgamActionModel? Create(string actionId, decimal amount)
    {
        if (amount <= 0m)
        {
            return null;
        }

        return actionId switch
        {
            Offense => new AmalgamOffenseIntentAction(amount),
            Block => new AmalgamGainBlockIntentAction(amount),
            Vulnerable => new AmalgamApplyVulnerableIntentAction(amount),
            _ => null
        };
    }

    public static AmalgamActionModel? CreateOffense(decimal damage) => Create(Offense, damage);

    public static AmalgamActionModel? CreateOffenseMulti(decimal damagePerHit, int hitCount)
    {
        if (damagePerHit <= 0m || hitCount <= 0)
        {
            return null;
        }

        return new AmalgamMultiHitOffenseIntentAction(damagePerHit, hitCount);
    }

    public static AmalgamActionModel? CreateBlock(decimal block) => Create(Block, block);

    public static AmalgamActionModel? CreateVulnerable(decimal stacks) => Create(Vulnerable, stacks);

    public static AmalgamActionModel CreateEmptyCup() => new AmalgamEmptyCupIntentAction();
}

