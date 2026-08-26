using UnityEngine;
using TMPro;
using System.Collections;
using Unity.Netcode;

public class UIHealthBar : NetworkBehaviour
{
    [Header("Configuration UI")]
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI damageText;
    
    private Health playerHealth;
    private BotHealth botHealth; 

    void Start()
    {
        playerHealth = GetComponent<Health>();
        botHealth = GetComponent<BotHealth>();

        if (damageText != null) damageText.gameObject.SetActive(false);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (statsText == null) return;

        int niveau = 0;
        int pointsDeVie = 0;
        int degatsTotal = 0;

        if (playerHealth != null)
        {
            niveau = playerHealth.niveau.Value;
            pointsDeVie = playerHealth.currentHealth.Value;
            
            PlayerAttack pAtk = GetComponent<PlayerAttack>();
            if (pAtk != null) degatsTotal = pAtk.GetTotalDamage();
        }
        else if (botHealth != null)
        {
            niveau = botHealth.niveau.Value;
            pointsDeVie = botHealth.currentHealth.Value;
            
            EnemyAI eAtk = GetComponent<EnemyAI>();
            if (eAtk != null) degatsTotal = eAtk.GetCalculatedDamage();
        }

        statsText.text = $"Lv. {niveau} | ATK: {degatsTotal} | PV: {pointsDeVie}";
    }

    public void ShowDamage(int amount)
    {
        if (damageText != null)
        {
            StopAllCoroutines();
            StartCoroutine(DamageAnim(amount));
        }
    }

    IEnumerator DamageAnim(int amount)
    {
        damageText.text = $"-{amount}";
        damageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        damageText.gameObject.SetActive(false);
    }
}
