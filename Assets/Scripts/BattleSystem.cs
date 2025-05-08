using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleSystem : MonoBehaviour
{
    public Player player;
    public Enemy enemy;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI Player_HP_txt;

    private Queue<AttackData> spellQueue = new Queue<AttackData>();

    void OnEnable()
    {
        SpellReceiver.OnSpellReceived += EnqueueSpell;
    }
    void OnDisable()
    {
        SpellReceiver.OnSpellReceived -= EnqueueSpell;
    }

   
    private void EnqueueSpell(string spellName)
    {
        spellQueue.Clear();

        var data = Resources.Load<AttackData>($"Attacks/{spellName}");
        if (data != null)
        {
            spellQueue.Enqueue(data);
            Debug.Log($"[BattleSystem] Queued spell: {data.name} (power={data.power})");
        }
        else
        {
            Debug.LogWarning($"[BattleSystem] No AttackData at Resources/Attacks/{spellName}");
        }
    }


    private IEnumerator ProcessSpellsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(4f);

            if (spellQueue.Count > 0)
            {
                var atk = spellQueue.Dequeue();
                HandleSpell(atk);
            }
        }
    }

    private void HandleSpell(AttackData atk)
    {
        Debug.Log($"Processing {atk.attackName} (power {atk.power})");

        var target = atk.isHealing ? (IBattler)player : enemy;
        target.TakeDamage(atk.isHealing ? -atk.power : atk.power);

        UpdateUI();
        Debug.Log($"After attack: Enemy HP = {enemy.CurrentHP}");
        Debug.Log($"After attack: Player HP = {player.CurrentHP}");
    }


    private void UpdateUI()
    {
        if (hpText != null)
            hpText.text = $"Enemy HP: {enemy.CurrentHP}";

        if (Player_HP_txt != null)
            Player_HP_txt.text = $"Player HP: {player.CurrentHP}";

    }

    void Start()
    {
        StartCoroutine(ProcessSpellsRoutine());
        UpdateUI();
    }
}
