using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AldenCombat2D : MonoBehaviour
{
    // Player Stats - Load them on awake
    public int level = 1;
    public int vitality = 5;
    public int defense = 5;
    public int strength = 5;
    private int critRate = 70; // x100 to get % value

    // Common stats - need to be calculated on awake
    public int maxHealth = 100;
    public int health = 100;
    public int mana = 100;

    // Attacks Scaling
    [Header("Attacks & Skills Scaling")]
    public int attackScaling = 1;
    public int skill1Scaling = 1;
    public int skill2Scaling = 1;
    public int skill3Scaling = 1;
    public int defendScaling = 1;

    // Actions info
    public int attackTimeCost = 30;
    public int defendTimeCost = 30;
    public int skill1TimeCost = 30;
    public int skill2TimeCost = 30;
    public int skill3TimeCost = 30;

    // Alden Components
    public Animator aldenAnimator = null;
    public Animator normalDamageAnimator = null;
    public TMP_Text normalDamageText = null;
    public GameObject hitFXObject = null;
    public Animator shieldAuraAnimator = null;

    // Private Variables
    private int damageToDo = 1;
    private bool isCritical = false;
    private double randomNumber = 0;
    private Animator hitFXAnimator = null;
    private bool isDefending = false;

    private void Start()
    {
        hitFXAnimator = hitFXObject.gameObject.GetComponent<Animator>();
    }

    public void AldenAttack(int targetPosition, int selfPosition)
    {
        isCritical = false;

        // CALCULATING DAMAGE
        // Scaling damage based on strength and the skill's scaling mulitplier
        damageToDo = strength * attackScaling;
        // Scaling damage based on active blessings
        // Scaling damage based on active afflictions
        // Checking to see if this attack will be a critical hit
        Random.InitState(System.DateTime.Now.Millisecond);
        randomNumber = Random.Range(1, 101);
        if (randomNumber <= critRate)
        {
            isCritical = true;
            damageToDo *= 2;
        }

        // Play the attack animation
        if(selfPosition == 1)
        {
            if (targetPosition == 0)
            {
                aldenAnimator.SetTrigger("Attack_1-1");  // Deal the damage by calling DealDamage() using animation event
            }
            else if (targetPosition == 1)
            {
                aldenAnimator.SetTrigger("Attack_1-2");
            }
            else if (targetPosition == 2)
            {
                aldenAnimator.SetTrigger("Attack_1-3");
            }
            else if (targetPosition == 3)
            {
                aldenAnimator.SetTrigger("Attack_1-4");
            }
        }
        else if(selfPosition == 2)
        {
            // same as above
        }
        else if (selfPosition == 3)
        {
            // same as above
        }
        else if (selfPosition == 4)
        {
            // same as above
        }

        // time cost will be added in playercombat
    }

    public void AldenDefend()
    {
        aldenAnimator.SetTrigger("AldenDefend");
        isDefending = true;
        shieldAuraAnimator.SetTrigger("ShieldAuraActive");
        // time cost will be added in playercombat
    }

    public void AldenTakeDamage(int incomingDamage)
    {
        // play damage animation
        aldenAnimator.SetTrigger("AldenHurt");

        // play hit FX
        int randomRotation = Random.Range(0, 45);
        hitFXObject.transform.Rotate(0f, 0f, randomRotation);
        hitFXAnimator.SetTrigger("HitEffect");

        // calculate def reductions
        incomingDamage -= defense;

        // calculate blessings reductions, if any

        // Reduce damage if defending
        if (isDefending == true)
        {
            incomingDamage = (int)(incomingDamage * 0.5f);
            isDefending = false;
            shieldAuraAnimator.SetTrigger("ShieldAuraIdle");
        }

        // play damage number animation
        normalDamageText.text = incomingDamage.ToString();
        normalDamageAnimator.SetTrigger("DamagePopup");

        // reduce player health
        health -= incomingDamage;

        CheckDeathCondition();
    }

    private void CheckDeathCondition()
    {
        if (health < 0)
        {
            health = 0;
        }

        // Death Condition
        if (health <= 0)
        {
            //isDefeated = true;
            aldenAnimator.SetTrigger("AldenDeath");
            CombatManager.instance.ReportDeath("Alden");
        }
    }

    public void DealDamage()
    {
        // Tell combat manager to deal damage to active enemy
        CombatManager.instance.HandleDealtDamage(damageToDo, isCritical);

        if (isCritical)
        {
            CameraShake.instance.StartShake(1);
            HitStop.instance.StartHitStop(0);
        }
        else
        {
            CameraShake.instance.StartShake(0);
        }
    }

    public void RequestResumeTime()
    {
        CombatManager.instance.ResumeTime();
    }

    private void PlaySoundEffect(string sfxName)
    {
        AudioManager.instance.PlaySound(sfxName);
    }
}
