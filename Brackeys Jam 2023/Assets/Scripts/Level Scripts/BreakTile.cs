using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakTile : MonoBehaviour
{
    
    public int maxHealth;
    public float currentHealth;
    public GameObject carriedItem;
    public Transform spawnPoint;

    public Animator myAnim;
    public float breakThreshold;


    private void Start()
    {
        myAnim = GetComponent<Animator>();
        currentHealth = maxHealth;
        breakThreshold = maxHealth - 1;
    }
    private void Update()
    {
        if(Mathf.Round(currentHealth) < maxHealth)
        {
            myAnim.SetTrigger("DisplayDamage");
        }

        if(currentHealth <= breakThreshold)
        {
            breakThreshold--;
            myAnim.SetTrigger("Damage");
        }
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
        myAnim.SetTrigger("Kill");
        StartCoroutine(waitKill(0.7f));
    }

    private IEnumerator waitKill(float time)
    {
        yield return new WaitForSeconds(time);
        if (carriedItem != null)
        {
            GameObject droppedItem = Instantiate(carriedItem, new Vector3(spawnPoint.position.x, spawnPoint.position.y), transform.rotation);
            droppedItem.transform.SetParent(null);

        }
        Destroy(gameObject);
    }


}
