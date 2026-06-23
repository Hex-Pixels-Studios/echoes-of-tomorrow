using UnityEngine;

public class PlayerUpgrade : MonoBehaviour
{
    UpgradeType? currentUpgrade;
    bool hasUpgrade;

    public bool HasUpgrade => hasUpgrade;
    public UpgradeType? CurrentUpgrade => currentUpgrade;

    public void GrantUpgrade(UpgradeType upgrade)
    {
        currentUpgrade = upgrade;
        hasUpgrade = true;
    }

    public void ConsumeUpgrade()
    {
        Debug.Log($"{gameObject.name} used upgrade: {currentUpgrade}");
        currentUpgrade = null;
        hasUpgrade = false;
    }
}
