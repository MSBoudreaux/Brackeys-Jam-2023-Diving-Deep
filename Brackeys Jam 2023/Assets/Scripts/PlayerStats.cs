using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;

public class PlayerStats : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;

    public bool isIFrames;
    public float iFrameTime;

    public float score;
    public float myQuota;

    //action stats: attack , damage, mining speed, light
    public int atkDamage;
    public int breakSpeed;
    public int range;
    public int lightRange;
    public int breakLevel;
    public bool isRage;

    public Animator stateAnim;

    public Light2D myLight;

    //update UI
    public Slider hpBar;
    public TextMeshProUGUI HPnumber;
    public TextMeshProUGUI atkNumber;
    public TextMeshProUGUI speedNumber;
    public TextMeshProUGUI rangeNumber;
    public TextMeshProUGUI lightNumber;
    public TextMeshProUGUI digLevel;
    public TextMeshProUGUI scoreNumber;
    public TextMeshProUGUI quotaNumber;
    public TextMeshProUGUI rageTimer;

    public TextMeshProUGUI textPopup;


    void Start()
    {

    }

    void Update()
    {
        stateAnim.SetBool("IsIFrames", isIFrames);
        stateAnim.SetBool("IsRageMode", isRage);

        myLight.pointLightInnerRadius = lightRange;
        myLight.pointLightOuterRadius = lightRange + 1f;
        myLight.intensity = 1.7f + (lightRange / 10f);

        //update UI
        hpBar.value = currentHealth;
        HPnumber.text = currentHealth.ToString();
        atkNumber.text = atkDamage.ToString();
        speedNumber.text = breakSpeed.ToString();
        rangeNumber.text = range.ToString();
        lightNumber.text = (lightRange - 1).ToString();
        digLevel.text = breakLevel.ToString();
        scoreNumber.text = score.ToString();
        if (score >= myQuota)
        {
            quotaNumber.text = "Quota Met!";
        }
        else quotaNumber.text = myQuota.ToString();
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
                if(inValue > lightRange)
                {
                    lightRange = inValue;
                }

                /*if(lightRange == 3)
                {
                    myLight.color = new Color(255, 199,  146, 255);
                }
                if(lightRange == 4)
                {
                    myLight.color = new Color(84, 144, 173, 255);
                }*/

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

    public IEnumerator popupTextWait (float time)
    {
        yield return new WaitForSeconds(time);
        textPopup.text = "";
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
