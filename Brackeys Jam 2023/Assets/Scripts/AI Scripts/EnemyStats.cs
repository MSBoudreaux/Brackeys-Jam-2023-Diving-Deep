using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{

    public int maxHealth;
    int currentHealth;

    public int damage;
    public float speed;
    public float jumpHeight;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }


    public void takeDamage(PlayerStats incStats)
    {
        currentHealth -= incStats.atkDamage;
    }
    public void setHealth(int inHealth)
    {
        currentHealth = inHealth;
    }

    public int getHealth()
    {
        return currentHealth;
    }

}
