using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundEnemy : MonoBehaviour
{

    public EnemyStats myStats;

    public Transform playerLocation;
    public bool hasLOS;
    public LayerMask ignoreEnemyLayer;
    public LayerMask groundLayer;
    public bool isGrounded;

    public float hitstunMulti;

    public Rigidbody2D myRB;
    public Transform groundCheck;
    public Transform attackCheck;
    public Transform jumpCheck;

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
    }

    private void Update()
    {
        if (!waitIdleSound)
        {
            idleDelay = Random.Range(5f, 7f);
            waitIdleSound = true;
            StartCoroutine(waitIdle(idleDelay));
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {


        if(myStats.getHealth() == 0)
        {
            myState = enemyState.Death;
            StartCoroutine(waitDie(1.5f));
            myAnim.SetTrigger("Die");
        }

        checkLOS();
        isGrounded = checkGround();

        switch (myState)
        {
            case enemyState.Asleep:
                if(hasLOS == true)
                {
                    myState = enemyState.Movement;
                    PlayClip(0);
                    myAnim.SetTrigger("WakeUp");
                }
                return;
            case enemyState.Movement:
                Flip();
                GroundMove();
                checkJump();
                myAnim.SetFloat("Speed", Mathf.Abs(myRB.velocity.x));

                if (checkAttack())
                {
                    myState = enemyState.Attack;
                    myAnim.SetTrigger("Attack");
                    PlayClip(0);
                    actionCoroutine = StartCoroutine(waitForAttack(1.35f));
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

    private void GroundMove()
    {
        if (!isGrounded)
        {
            float airSpeed = myStats.speed / 3f;
            myRB.AddForce(new Vector2(airSpeed * Mathf.Clamp((playerLocation.transform.position.x - transform.position.x), -1f, 1f) * 100 * Time.deltaTime, 0), ForceMode2D.Force);
        }
        else myRB.AddForce(new Vector2(myStats.speed * Mathf.Clamp((playerLocation.transform.position.x - transform.position.x), -1f, 1f) * 100 * Time.deltaTime, 0), ForceMode2D.Force);
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

    private bool checkGround()
    {
        bool ground = Physics2D.BoxCast(groundCheck.position, new Vector2(.66f, 0.12f), 0, new Vector2(0f, 0f), 0, groundLayer);
        return ground;
    }

    private bool checkJump()
    {
        bool Jump = false;
        bool jumpUp = Physics2D.BoxCast(jumpCheck.position, new Vector2(.30f * transform.localScale.x, .30f * transform.localScale.y), 90f, new Vector2(jumpCheck.localPosition.x, jumpCheck.localPosition.y), 0f, groundLayer);

        //Debug.Log(jumpUp.ToString());
        if (jumpUp && isGrounded)
        {
            myRB.AddForce(new Vector2(0, 1 * myStats.jumpHeight), ForceMode2D.Impulse);
        }



        return Jump;
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

    private void OnDrawGizmos()
    {
        //Gizmos.DrawCube(groundCheck.position, new Vector2(.68f, 0.12f));
        //Gizmos.DrawCube(new Vector3(attackCheck.position.x, attackCheck.position.y, 0), new Vector3(.50f, .75f, 1f));
        //Gizmos.DrawCube(jumpCheck.position, new Vector2(.30f, .30f));
    }
}
