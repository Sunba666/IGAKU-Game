using UnityEngine;

[CreateAssetMenu(
    fileName = "CharacterData_New",
    menuName = "异格/角色选择/角色数据")]
public sealed class CharacterSelectCharacterData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string characterId = "character_id";
    [SerializeField] private string displayName = "角色名";

    [Header("Visuals")]
    [SerializeField] private Sprite portrait;
    [SerializeField] private Sprite largePreview;

    [Header("Availability")]
    [Tooltip("只有角色的战斗逻辑、动画和场景数据都已接入后才勾选。未勾选时会显示黑色遮罩并禁止选择。")]
    [SerializeField] private bool implemented;

    [Header("Optional Description")]
    [SerializeField] private string combatType;
    [TextArea(2, 4)]
    [SerializeField] private string description;

    public string CharacterId => characterId;
    public string DisplayName => displayName;
    public Sprite Portrait => portrait;
    public Sprite LargePreview => largePreview != null ? largePreview : portrait;
    public bool Implemented => implemented;
    public string CombatType => combatType;
    public string Description => description;
}
