using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakTile : MonoBehaviour
{
    
    public int maxHealth;
    public float currentHealth;
    public int breakLevel;
    public GameObject carriedItem;
    public Transform spawnPoint;

    public Animator myAnim;
    public float breakThreshold;

    public AudioSource myAudio;
    public AudioClip[] myClips;
    float bounceWait = 0.517f;
    bool isWaitingBounce;
    bool isBreaking = false;
    bool isFirstBreak = true;


    private void Start()
    {
        myAnim = GetComponent<Animator>();
        myAudio = FindObjectOfType<PlayerController>().myAudio;
        currentHealth = maxHealth;
        breakThreshold = maxHealth - 1;
    }
    private void Update()
    {
        if(Mathf.Round(currentHealth) < maxHealth)
        {
            if (isFirstBreak)
            {
                PlayClip(0);
                isFirstBreak = false;
            }
            myAnim.SetTrigger("DisplayDamage");
        }

        if(currentHealth <= breakThreshold)
        {
            breakThreshold--;
            myAnim.SetTrigger("Damage");
            PlayClip(0);
        }
    }

    public void MineDamage(float incDamage, int inBreakLevel)
    {
        if(inBreakLevel >= breakLevel)
        {
            currentHealth -= incDamage * Time.deltaTime;
            if (currentHealth <= 0)
            {
                DestroyThis();
            }
        }


        else if (!isWaitingBounce)
        {
            StartCoroutine(PlayBounceNoise(bounceWait));
            isWaitingBounce = true;
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
        if (!isBreaking)
        {
            PlayClip(1);
            isBreaking = true;
        }
        myAnim.SetTrigger("Kill");
        StartCoroutine(waitKill(0.7f));
    }

    private void PlayClip(int clipIndex)
    {
        AudioClip inClip = myClips[clipIndex];
        myAudio.PlayOneShot(inClip);
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

    IEnumerator PlayBounceNoise(float time)
    {
        yield return new WaitForSeconds(time);
        PlayClip(2);
        isWaitingBounce = false;
    }


}
