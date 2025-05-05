
using UnityEngine;

public class Enemy : MonoBehaviour, IBattler
{
    [Header("Enemy Stats")]
    public int MaxHP = 15;
    public int CurrentHP { get; private set; }

    void Awake()
    {
        CurrentHP = MaxHP;
    }

    public void TakeDamage(int amount)
    {
        CurrentHP = Mathf.Clamp(CurrentHP - amount, 0, MaxHP);
        //CurrentHP = CurrentHP - 1;
        Debug.Log($"Enemy HP: {CurrentHP}/{MaxHP}");

        // TODO: play hit VFX or death when CurrentHP == 0
    }

   
    public AttackData ChooseAttack()
    {
       
        return null;
    }
}
