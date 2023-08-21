using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakTile : MonoBehaviour
{
    
    public int maxHealth;
    public float currentHealth;
    public GameObject carriedItem;

    public Animator myAnim;

    public void Start()
    {
        currentHealth = maxHealth;
    }

    public void MineDamage(float incDamage)
    {
        currentHealth -= incDamage * Time.deltaTime;
        if (currentHealth <= 0)
        {
            DestroyThis();
        }
    }

    public void InstantDamage(float incDamage)
    {
        currentHealth -= incDamage;
        if (currentHealth <= 0)
        {
            DestroyThis();
        }
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }


}
