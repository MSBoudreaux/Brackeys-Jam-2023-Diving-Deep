using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;

    public bool isIFrames;
    public float iFrameTime;

    public float score;

    //action stats: attack , damage, mining speed, light
    public int atkDamage;
    public int breakSpeed;
    public int range;
    public int lightRange;
    public int breakLevel;
    public bool isRage;


    public Animator stateAnim;

    void Start()
    {
        
    }

    void Update()
    {
        stateAnim.SetBool("IsIFrames", isIFrames);
        stateAnim.SetBool("IsRageMode", isRage);
    }

    public void addHealth(int inHP)
    {
        if(!isIFrames || !isRage || inHP > 0)
        {
            if(inHP < 0)
            {
                isIFrames = true;
                StartCoroutine(iFrames(iFrameTime));
            }

            currentHealth += inHP;

            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
        }

        if(currentHealth <= 0)
        {
            //ded
        }

    }

    public void GetPowerup(PickupItem.PickupBoost inBoost, int inValue)
    {
        switch (inBoost)
        {
            case PickupItem.PickupBoost.Health:
                addHealth(inValue);
                return;
            case PickupItem.PickupBoost.MaxHealth:
                maxHealth += inValue;
                return;
            case PickupItem.PickupBoost.Damage:
                atkDamage += inValue;
                return;
            case PickupItem.PickupBoost.Range:
                range += inValue;
                return;
            case PickupItem.PickupBoost.MiningSpeed:
                breakSpeed += inValue;
                return;
            case PickupItem.PickupBoost.Light:
                lightRange += inValue;
                return;
            case PickupItem.PickupBoost.Score:
                score += inValue;
                return;
            case PickupItem.PickupBoost.PickaxeUp:
                breakLevel += inValue;
                return;
            case PickupItem.PickupBoost.AngyMode:
                isRage = true;
                StartCoroutine(StartRage(inValue));
                return;

        }

    }

    IEnumerator iFrames(float time)
    {
        yield return new WaitForSeconds(time);
        isIFrames = false;
    }

    IEnumerator StartRage(float time)
    {
        yield return new WaitForSeconds(time);
        isRage = false;
    }
}
