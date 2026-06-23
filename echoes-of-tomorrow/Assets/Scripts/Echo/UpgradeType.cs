public enum UpgradeType
{
    // combat - held until player presses Use
    Grenade,
    FireMissile,
    IceFreeze,
    Teleport,

    // vitals - applied instantly on echo collection, no  press needed
    HealSmall, // restore 30 health
    HealFull, // full health restore
    StaminaBoost, // restore 40 stamina
    StaminaFull, // full stamina restore
}
