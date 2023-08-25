using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyEnemy : MonoBehaviour
{
    public EnemyStats myStats;

    public Transform playerLocation;
    public bool hasLOS;
    public LayerMask ignoreEnemyLayer;
    public LayerMask groundLayer;

    public float hitstunMulti;

    public Rigidbody2D myRB;
    public Transform attackCheck;

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



    // Start is called before the first frame update
    void Start()
    {
        myStats = GetComponent<EnemyStats>();
        myAnim = GetComponent<Animator>();
        myRB = GetComponent<Rigidbody2D>();
        myAudio = GetComponent<AudioSource>();
        playerLocation = FindObjectOfType<PlayerStats>().transform;
    }

    // Update is called once per frame
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
        if (myStats.getHealth() == 0)
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
                AirMove();
                myAnim.SetFloat("Speed", Mathf.Abs(myRB.velocity.magnitude));

                if (checkAttack())
                {
                    myState = enemyState.Attack;
                    myAnim.SetTrigger("Attack");
                    PlayClip(0);
                    actionCoroutine = StartCoroutine(waitForAttack(1.850f));
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

    private void AirMove()
    {
        //if we are on the left

        Vector2 destination = new Vector2();
        if(transform.position.x < playerLocation.transform.position.x)
        {
            destination = new Vector2(playerLocation.transform.position.x - 1, playerLocation.transform.position.y);
        }
        else
        {
            destination = new Vector2(playerLocation.transform.position.x + 1, playerLocation.transform.position.y);
        }

        myRB.AddForce((destination - new Vector2(transform.position.x, transform.position.y)).normalized * myStats.speed, ForceMode2D.Force);
    }

    public void Flip()
    {
        Transform spriteTransform = transform;
        if (myRB.velocity.x > 0.1)
        {
            Vector3 myScale = spriteTransform.localScale;
            myScale.x = Mathf.Abs(myScale.x);
            spriteTransform.localScale = myScale;
        }
        else if (myRB.velocity.x < -0.1)
        {
            Vector3 myScale = spriteTransform.localScale;
            myScale.x = Mathf.Abs(myScale.x) * -1;
            spriteTransform.localScale = myScale;
        }
    }
    public bool checkLOS()
    {
        LayerMask layermaskToCheck;

        if (myState != enemyState.Asleep)
        {
            layermaskToCheck = LayerMask.GetMask("Enemy", "EnemyHitbox", "Terrain", "PlayerHitbox", "Pickup");
        }

        else layermaskToCheck = ignoreEnemyLayer;


        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(playerLocation.position.x - transform.position.x, playerLocation.position.y - transform.position.y), 10f, ~layermaskToCheck);
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
    private bool checkAttack()
    {
        bool isAttack = Physics2D.BoxCast(attackCheck.position, new Vector2(.50f, .75f), 90f, new Vector2(attackCheck.localPosition.x, attackCheck.localPosition.y), 0f, ~LayerMask.GetMask("Enemy", "EnemyHitbox", "Terrain", "PlayerHitbox", "Pickup", "LevelEnd"));

        return isAttack;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("PlayerAttack") && myState != enemyState.Death)
        {
            Debug.Log("enemy hit");
            PlayerStats incStats = collision.transform.GetComponentInParent<PlayerStats>();
            myState = enemyState.Hitstun;
            myAnim.SetTrigger("TakeDamage");
            if(actionCoroutine != null)
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

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (myState != enemyState.Asleep && !collision.transform.CompareTag("Player"))
        {
            Physics2D.IgnoreCollision(collision.collider, collision.otherCollider);
        }
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        if (myState != enemyState.Asleep && !collision.transform.CompareTag("Player"))
        {
            Physics2D.IgnoreCollision(collision.collider, collision.otherCollider);
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
