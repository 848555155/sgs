﻿namespace Sanguosha.Core.Cards;

public enum CardCategory
{
    Basic = 1 << 1,
    Equipment = 1 << 2,
    Tool = 1 << 3,
    ImmediateTool = Tool | (1 << 4),
    DelayedTool = Tool | (1 << 5),
    DefensiveHorse = Equipment | (1 << 6),
    OffensiveHorse = Equipment | (1 << 7),
    Armor = Equipment | (1 << 8),
    Weapon = Equipment | (1 << 9),
    Hero = 1 << 10,
    Unknown = 1 << 31
}

public static class CardCategoryManager
{
    public static bool IsCardCategory(CardCategory category, CardCategory belongsTo)
    {
        return (category & belongsTo) == belongsTo;
    }

    public static CardCategory BaseCategoryOf(CardCategory category)
    {
        return category & (CardCategory.Basic | CardCategory.Equipment | CardCategory.Tool);
    }

    public static bool IsCardCategory(this CardHandler cardType, CardCategory belongsTo)
    {
        return IsCardCategory(cardType.Category, belongsTo);
    }

    public static CardCategory BaseCategory(this CardHandler cardType)
    {
        return BaseCategoryOf(cardType.Category);
    }
}
