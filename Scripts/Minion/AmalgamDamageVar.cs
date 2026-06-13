using System.Globalization;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

public sealed class AmalgamDamageVar : DynamicVar
{
    public const string DefaultName = "AmalgamDamage";

    public ValueProp Props { get; set; }

    public AmalgamDamageVar(decimal damage, ValueProp props)
        : base(DefaultName, damage)
    {
        Props = props;
    }

    public AmalgamDamageVar(string name, decimal damage, ValueProp props)
        : base(name, damage)
    {
        Props = props;
    }

    public static decimal GetEffectiveFlat(CardModel card, AmalgamDamageVar damageVar)
    {
        decimal num = damageVar.BaseValue;
        EnchantmentModel? enchantment = card.Enchantment;
        if (enchantment == null)
        {
            return num;
        }

        num += enchantment.EnchantDamageAdditive(num, damageVar.Props);
        num *= enchantment.EnchantDamageMultiplicative(num, damageVar.Props);
        return num;
    }

    public static decimal GetEffectiveFlat(CardModel card, string dynamicVarKey)
    {
        DynamicVar dv = card.DynamicVars[dynamicVarKey];
        return dv is AmalgamDamageVar adv ? GetEffectiveFlat(card, adv) : dv.BaseValue;
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        if (card.Enchantment != null && !card.IsEnchantmentPreview)
        {
            EnchantedValue = GetEffectiveFlat(card, this);
        }

        Player? owner = card.Owner;
        ICombatState? combatState = card.CombatState ?? owner?.Creature.CombatState;
        bool inCombat = CombatManager.Instance is { IsInProgress: true };

        if (!(runGlobalHooks || inCombat) || owner is not Player player || combatState is not CombatState cs)
        {
            PreviewValue = GetEffectiveFlat(card, this);
            return;
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(cs, player);
        if (amalgam is not { IsAlive: true })
        {
            PreviewValue = GetEffectiveFlat(card, this);
            return;
        }

        decimal num = BaseValue;
        Creature? receiver = AmalgamIntentDamagePreview.ResolveDamageReceiverForPetAttackPreview(cs, target);
        if (receiver is { IsAlive: true })
        {
            num = AmalgamIntentDamagePreview.PreviewOutgoing(amalgam, receiver, num, Props, card, previewMode);
        }
        else
        {
            Creature[] pool = AmalgamOffenseTargeting.GetPreviewReceiverPool(cs, player);
            if (pool.Length > 0)
            {
                num = AmalgamIntentDamagePreview.PreviewOutgoingConsensusAmongReceivers(
                    amalgam,
                    pool,
                    num,
                    Props,
                    card,
                    previewMode);
            }
        }

        PreviewValue = num;
    }

    public override string ToString() => ((int)PreviewValue).ToString(CultureInfo.InvariantCulture);
}
