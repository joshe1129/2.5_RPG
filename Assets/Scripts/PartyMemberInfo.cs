using UnityEngine;

[CreateAssetMenu(menuName = "NewPartyMember")]
public class PartyMemberInfo : ScriptableObject
{
    public string memberName;
    public int startingLevel;
    public int baseHealth;
    public int baseStr;
    public int baseInitiative;
    public GameObject memberBattleVisualPrefab;
    public GameObject memberOverworldVisualPrefab;
    public Sprite memberHUDSprite;
}
