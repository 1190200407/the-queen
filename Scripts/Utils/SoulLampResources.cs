using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;
using ComicChess.TheQueen;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

public static class SoulLampResources
{
    public const string SoulLampLocalId = "SoulLamp";

    public static SecondaryResourceDefinition SoulLampDefinition { get; private set; } = null!;
    public static string SoulLampId { get; private set; } = string.Empty;

    public static void Register()
    {
        var registry = RitsuLibFramework.GetSecondaryResourceRegistry(Entry.ModId);

        SoulLampDefinition = registry.Register(SoulLampLocalId, new SecondaryResourceDefinition(
            defaultAmount: 0,
            baseMaxAmount: int.MaxValue,
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
            persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
            smallIconPath: "res://TheQueen/images/charui/soul_lamp_text.png",
            largeIconPath: "res://TheQueen/images/charui/soul_lamp.png"
        ));
        SoulLampId = SoulLampDefinition.Id;
        registry.RegisterMultiplayerPlayerStateUi(
            SoulLampDefinition.LocalId,
            static _ => 
            {
                var style = new SecondaryResourceCounterStyle
                {
                    CounterSize = new Vector2(28f, 28f),
                    IconSize = new Vector2(30f, 30f),
                    FontSize = 16,
                    OutlineSize = 5,
                    FormatAmount = (amount, _) => amount.ToString(),
                    AmountLabelOffset = new Vector2(0, 2f),
                    //rgb(19, 100, 62)
                    OutlineColor = new Color(19f / 255f, 100f/ 255f, 62f / 225f)

                };
                return NSecondaryResourceCounter.Create(SoulLampDefinition, style);
            },
            static ctx => ctx.Node.Refresh(ctx.Player),
            new NodeAttachmentOptions
            {
                Name = "SoulLampSecondaryResourceCounter",
                InsertAfterName = "EnergyContainer",
            });
    }

    public static int GetAmount(Player? player)
    {
        if (player == null || string.IsNullOrWhiteSpace(SoulLampId))
        {
            return 0;
        }

        return SecondaryResourceCmd.Get(player, SoulLampId);
    }

    public static bool HasAny(Player? player) => GetAmount(player) > 0;
}
