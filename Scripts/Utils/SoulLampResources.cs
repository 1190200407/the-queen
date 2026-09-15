using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;
using ComicChess.TheQueen;

public static class SoulLampResources
{
    public static SecondaryResourceDefinition SoulLampDefinition { get; private set; } = null!;
    public static string SoulLampId { get; private set; } = string.Empty;

    public static void Register()
    {
        var registry = RitsuLibFramework.GetSecondaryResourceRegistry(Entry.ModId);

        SoulLampDefinition = registry.Register("SoulLamp", new SecondaryResourceDefinition(
            defaultAmount: 0,
            baseMaxAmount: int.MaxValue,
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
            persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
            smallIconPath: "res://TheQueen/images/charui/soul_lamp_text.png",
            largeIconPath: "res://TheQueen/images/charui/soul_lamp.png"
        ));
        SoulLampId = SoulLampDefinition.Id;
    }
}