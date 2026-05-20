/// <summary>
/// Internal data class for BattleEntities representing targets and actions during combat.
/// </summary>
[System.Serializable]
public class BattleEntities
{
    /// <summary>
    /// Enum defining possible actions a battler can perform during their turn.
    /// </summary>
    public enum Action { Attack, Run }
    
    public Action BattleAction;

    public string Name;
    public int Level;
    public int CurrentHealth;
    public int MaxHealth;
    public int Strength;
    public int Initiative;
    public bool IsPlayer;
    public UnityEngine.Sprite Portrait;
    public BattleVisuals BattleVisuals;
    public int Target;

    /// <summary>
    /// Initializes all stats for a battle entity from party member or enemy data.
    /// </summary>
    /// <param name="name">The name of the entity.</param>
    /// <param name="currentHealth">The current health of the entity.</param>
    /// <param name="maxHealth">The maximum health of the entity.</param>
    /// <param name="strength">The attack power/damage of the entity.</param>
    /// <param name="initiative">The turn order priority of the entity.</param>
    /// <param name="level">The level of the entity.</param>
    /// <param name="isPlayer">Whether this entity belongs to the player's party.</param>
    public void SetEntityValues(string name, int currentHealth, int maxHealth, int strength, int initiative, int level, bool isPlayer, UnityEngine.Sprite portrait = null)
    {
        Name = name;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        Strength = strength;
        Initiative = initiative;
        IsPlayer = isPlayer;
        Level = level;
        Portrait = portrait;
    }

    /// <summary>
    /// Sets the target index for this entity's next action in the allBattlers list.
    /// </summary>
    /// <param name="target">The index of the target entity in the allBattlers list.</param>
    public void SetTarget(int target)
    {
        Target = target;
    }

    /// <summary>
    /// Updates the visual health bar for this entity after damage is taken or health changes.
    /// </summary>
    public void UpdateUI()
    {
        BattleVisuals.ChangeHealth(CurrentHealth);
    }
}
