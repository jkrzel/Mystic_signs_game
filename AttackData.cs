using UnityEngine;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Combat/AttackData")]
public class AttackData : ScriptableObject
{
    [Header("Basic Info")]
    public string attackName;     
    public bool isHealing;      // true if heals

    [Header("Combat Values")]
    public int power = 1;      // damage (positive) or heal (negative) value

    [Header("VFX/SFX (optional)")]
    public GameObject vfxPrefab;
    public AudioClip soundEffect;
}
