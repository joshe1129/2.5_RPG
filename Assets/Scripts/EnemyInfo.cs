using UnityEngine;

[CreateAssetMenu(menuName = "NewEnemy")]
public class EnemyInfo : ScriptableObject
{
    public string enemyName;
    public int baseHealth;
    public int baseStr;
    public int baseInitiative;
    public GameObject enemyBattleVisualPrefab;
    public GameObject enemyDungeonVisualPrefab;
}
