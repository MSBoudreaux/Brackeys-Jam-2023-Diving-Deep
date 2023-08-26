using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootEnemy : MonoBehaviour
{

    public EnemyStats myStats;

    public Transform playerLocation;
    public bool hasLOS;
    public LayerMask ignoreEnemyLayer;
    public LayerMask groundLayer;

    public float hitstunMulti;

    public float attackDelay;
    public bool canAttack;

    public float shootDelay;
    public float bulletSpeed;
    public Transform emitter;
    public GameObject projectile;

    public Rigidbody2D myRB;

    public enum enemyState
    {
        Asleep,
        Movement,
        Attack,
        Hitstun,
        Death
    }

    public enemyState myState;
    public Animator myAnim;

    public AudioSource myAudio;
    public AudioClip[] myClips;
    public bool waitIdleSound;
    float idleDelay;
    public bool waitExplosion;
    float explosionDelay = 0.2f;

    public Coroutine actionCoroutine;



    void Start()
    {
        myStats = GetComponent<EnemyStats>();
        myAnim = GetComponent<Animator>();
        myRB = GetComponent<Rigidbody2D>();
        myAudio = GetComponent<AudioSource>();
        playerLocation = FindObjectOfType<PlayerStats>().transform;
        canAttack = true;
    }

    void Update()
    {
        if (!waitIdleSound)
        {
            idleDelay = Random.Range(5f, 7f);
            waitIdleSound = true;
            StartCoroutine(waitIdle(idleDelay));
        }
    }

    private void FixedUpdate()
    {
        if (myStats.getHealth() <= 0)
        {
            myState = enemyState.Death;
            StartCoroutine(waitDie(1.5f));
            myAnim.SetTrigger("Die");
        }

        checkLOS();

        switch (myState)
        {
            case enemyState.Asleep:
                if (hasLOS == true)
                {
                    myState = enemyState.Movement;
                    PlayClip(0);
                    myAnim.SetTrigger("WakeUp");
                }
                return;
            case enemyState.Movement:
                Flip();

                myAnim.SetFloat("Speed", Mathf.Abs(myRB.velocity.x));

                if (canAttack && hasLOS)
                {
                    myState = enemyState.Attack;
                    myAnim.SetTrigger("Attack");
                    PlayClip(0);
                    canAttack = false;
                    actionCoroutine = StartCoroutine(waitForAttack(shootDelay));
                    StartCoroutine(waitAfterAttack(attackDelay));
                }
                return;
            case enemyState.Attack:
                return;
            case enemyState.Hitstun:
                return;
            case enemyState.Death:
                if (!waitExplosion)
                {
                    waitExplosion = true;
                    PlayClip(3);
                    StartCoroutine(waitExplode(explosionDelay));
                }
                return;

        }

    }

    public void Flip()
    {
        Transform spriteTransform = transform;

        if (playerLocation.transform.position.x > myRB.transform.position.x)
        {
            Vector3 myScale = spriteTransform.localScale;
            myScale.x = Mathf.Abs(myScale.x);
            spriteTransform.localScale = myScale;
        }
        else
        {
            Vector3 myScale = spriteTransform.localScale;
            myScale.x = Mathf.Abs(myScale.x) * -1;
            spriteTransform.localScale = myScale;

        }
    }

    public bool checkLOS()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(playerLocation.position.x - transform.position.x, playerLocation.position.y - transform.position.y), 10f, ~ignoreEnemyLayer);
        //Debug.Log(hit.transform.name);
        if (hit.transform.name == "Player")
        {
            hasLOS = true;
            myAudio.volume = 1f;
        }
        else
        {
            hasLOS = false;
            myAudio.volume = 0.5f;
        }


        return hasLOS;
    }

    public void Shoot()
    {
        Vector3 shootDir = (playerLocation.transform.position - transform.position);
        //Debug.Log(shootDir.ToString());

        GameObject myProj = Instantiate(projectile, new Vector3(emitter.transform.position.x, emitter.transform.position.y, emitter.transform.position.z), Quaternion.Euler(shootDir), emitter);
        myProj.GetComponent<PickupItem>().value = -myStats.damage;
        myProj.transform.SetParent(null);
        myProj.transform.GetComponent<Rigidbody2D>().AddForce(-myProj.transform.right * bulletSpeed, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("PlayerAttack") && myState != enemyState.Death)
        {
            Debug.Log("enemy hit");
            PlayerStats incStats = collision.transform.GetComponentInParent<PlayerStats>();
            myState = enemyState.Hitstun;
            myAnim.SetTrigger("TakeDamage");
            if (actionCoroutine != null)
            {
                StopCoroutine(actionCoroutine);
            }
            actionCoroutine = StartCoroutine(waitForHitstun(1.1f));



            if (incStats.isRage)
            {
                myRB.AddForce(new Vector2(playerLocation.position.x - transform.position.x, -(playerLocation.position.y - transform.position.y) * 15).normalized * 5 * -hitstunMulti, ForceMode2D.Impulse);
                myStats.setHealth(0);
            }
            else
            {
                PlayClip(2);
                myRB.AddForce(new Vector2(playerLocation.position.x - transform.position.x, -(playerLocation.position.y - transform.position.y) * 15).normalized * incStats.atkDamage * -hitstunMulti, ForceMode2D.Impulse);
                myStats.takeDamage(incStats);
            }
        }
    }

    IEnumerator waitForHitstun(float time)
    {
        yield return new WaitForSeconds(time);
        myState = enemyState.Movement;
    }
    IEnumerator waitForAttack(float time)
    {
        yield return new WaitForSeconds(time);
        myState = enemyState.Movement;
        //Shoot here
        Shoot();
    }
    IEnumerator waitAfterAttack(float time)
    {
        yield return new WaitForSeconds(time);
        canAttack = true;
    }
    IEnumerator waitDie(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(transform.gameObject);
    }
    IEnumerator waitIdle(float time)
    {
        yield return new WaitForSeconds(time);
        waitIdleSound = false;
        PlayClip(1);
    }
    IEnumerator waitExplode(float time)
    {
        yield return new WaitForSeconds(time);
        waitExplosion = false;

    }
    private void PlayClip(int clipIndex)
    {
        AudioClip inClip = myClips[clipIndex];
        myAudio.PlayOneShot(inClip);
    }

}

