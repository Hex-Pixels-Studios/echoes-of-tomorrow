using UnityEngine;

public class PlayerUpgrade : MonoBehaviour
{
    UpgradeType? currentUpgrade;

    public bool HasUpgrade => currentUpgrade.HasValue;
    public UpgradeType? CurrentUpgrade => currentUpgrade;

    public void GrantUpgrade(UpgradeType upgrade)
    {
        currentUpgrade = upgrade;
        // notify combat system so it can push the icon change to the HUD
        GetComponent<CombatSystem>()?.NotifyUpgradeGranted(upgrade);
        Debug.Log($"{gameObject.name} received upgrade: {upgrade}");
    }

    public void ConsumeUpgrade()
    {
        Debug.Log($"{gameObject.name} used upgrade: {currentUpgrade}");
        currentUpgrade = null;
    }
}
