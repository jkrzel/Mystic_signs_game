
using UnityEngine;

public class Player : MonoBehaviour, IBattler
{
    [Header("Player Stats")]
    public int MaxHP = 25;
    public int CurrentHP { get; private set; }

    void Awake()
    {
        CurrentHP = MaxHP;
    }

    // TODO: trigger damage/heal update UI
    public void TakeDamage(int amount)
    {
        //Negative heals
        CurrentHP = Mathf.Clamp(CurrentHP - amount, 0, MaxHP);
        Debug.Log($"Player HP: {CurrentHP}/{MaxHP}");

        
    }
}
