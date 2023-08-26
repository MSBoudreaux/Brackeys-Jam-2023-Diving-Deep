using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
    public PlayerStats myStats;

    public float speed;
    public float jumpHeight;
    public float fallThreshold;
    public float fallSpeed;

    float moveX;
    bool toJump;

    public bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundLayer;

    public Camera myCam;

    public LayerMask ignoreHitboxLayer;
    public float hitstunMulti;

    //animation data
    public Animator myAnim;
    public Transform spriteTransform;

    //audio data
    public AudioSource myAudio;
    public AudioClip[] myClips;

    public bool waitAmbience;
    public bool whichAmbience;
    public float ambienceDelay;

    public enum PlayerState
    {
        FreeMovement,
        Jumping, 
        Attacking,
        Hitstun,
        Dead,
        Win
    }

    public PlayerState state;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (!waitAmbience)
        {
            ambienceDelay = Random.Range(15f, 20f);
            waitAmbience = true;
            StartCoroutine(ambienceTimer(ambienceDelay));
        }

        if (myStats.currentHealth <= 0 && state != PlayerState.Dead)
        {
            state = PlayerState.Dead;
            myAnim.SetTrigger("Kill");
            PlayClip(11);
            StartCoroutine(waitLevelDed(3.6f));
        }



        switch (state)
        {
            case PlayerState.FreeMovement:
                checkGround();
                MovementInput();
                FlipPlayer(0);
                ActionInput();

                myAnim.SetFloat("Speed", rb.velocity.x);

                return;

            case PlayerState.Jumping:
                return;

            case PlayerState.Attacking:
                ActionInput();
                return;
            case PlayerState.Hitstun:
                return;
            case PlayerState.Dead:
                return;
            case PlayerState.Win:
                return;
        }

    }

    private void FixedUpdate()
    {
        isGrounded = checkGround();
        MoveControl();
    }

    private void MovementInput()
    {
        moveX = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            toJump = true;
        }


        if (moveX != 0 || toJump == true)
        {
            //Debug.Log("move = " + moveX.ToString() + ", toJump = " + toJump.ToString());
        }
    }

    private void ActionInput()
    {
        if (Input.GetMouseButton(0))
        {
            state = PlayerState.Attacking;
            myAnim.SetBool("Attacking", true);

            //add force to slow player down
            if (isGrounded)
            {
                rb.AddForce(new Vector2(0f, -10), ForceMode2D.Force);
            }

            //find object under cursor

            Vector3 touchPos = myCam.ScreenToWorldPoint(Input.mousePosition);
            touchPos.z = 0f;
            //Debug.Log(touchPos.ToString());
            RaycastHit2D hit2D = Physics2D.Raycast(touchPos, touchPos - rb.transform.position, myStats.range, ~ignoreHitboxLayer);
            //Debug.Log(hit2D.collider.ToString() + ": " + hit2D.collider.transform.position.ToString());

            FlipPlayer(touchPos.x);

            //if breakable and in range, start breaking

            if (hit2D.collider.CompareTag("Breakable"))
            {
                if ((rb.transform.position - hit2D.collider.transform.position).magnitude <= myStats.range)
                {
                    //Debug.Log("In range!");
                    hit2D.collider.GetComponent<BreakTile>().MineDamage(myStats.breakSpeed, myStats.breakLevel);
                }
                //else Debug.Log("Out of range!");
            }


        }
        else
        {
            myAnim.SetBool("Attacking", false);
            state = PlayerState.FreeMovement;
        }
    }

    private void MoveControl()
    {
        float finalSpeed;
        if (!isGrounded)
        {
            finalSpeed = speed / 2;
        }
        else finalSpeed = speed;


        rb.AddForce(new Vector2(finalSpeed * moveX * 100 * Time.deltaTime, 0), ForceMode2D.Force);

        if (toJump && isGrounded)
        {
            rb.AddForce(new Vector2(0, jumpHeight * 100 * Time.deltaTime), ForceMode2D.Impulse);
            toJump = false;
            PlayClip(0);
        }
    }

    private void FlipPlayer(float touchPos)
    {
        if (state == PlayerState.FreeMovement)
        {
            if (rb.velocity.x > 0.1)
            {
                Vector3 myScale = spriteTransform.localScale;
                myScale.x = Mathf.Abs(myScale.x);
                spriteTransform.localScale = myScale;
            }
            else if (rb.velocity.x < -0.1)
            {
                Vector3 myScale = spriteTransform.localScale;
                myScale.x = Mathf.Abs(myScale.x) * -1;
                spriteTransform.localScale = myScale;
            }
        }
        else if (state == PlayerState.Attacking)
        {
            //flip player to face for attack
            if (touchPos > rb.transform.position.x)
            {
                Vector3 myScale = spriteTransform.localScale;
                myScale.x = Mathf.Abs(myScale.x);
                spriteTransform.localScale = myScale;
            }
            else if (touchPos < rb.transform.position.x)
            {
                Vector3 myScale = spriteTransform.localScale;
                myScale.x = Mathf.Abs(myScale.x) * -1;
                spriteTransform.localScale = myScale;

            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Pickup"))
        {

            PickupItem inPickup;
            bool isProjectile = false;
            Debug.Log("picking up!");
            if (collision.transform.GetComponent<PickupItem>())
            {
                inPickup = collision.transform.GetComponent<PickupItem>();

            }
            else 
            {
            inPickup = collision.transform.GetComponentInParent<PickupItem>();
                isProjectile = true;

            }


            if (inPickup.myStat == PickupItem.PickupBoost.AngyMode)
            {
                PlayClip(9);
            }
            else if(inPickup.myStat == PickupItem.PickupBoost.Health && inPickup.value < 0)
            {
                PlayClip(1);
            }
            else if(inPickup.myStat == PickupItem.PickupBoost.Score)
            {
                PlayClip(7);
            }
            else
            {
                PlayClip(8);
            }
            myStats.GetPowerup(inPickup.myStat, inPickup.value);
            if (isProjectile)
            {
                Destroy(collision.transform.parent.gameObject);
            }
            else
            {
                Destroy(collision.transform.gameObject);
            }
        }

        else if (collision.transform.CompareTag("EnemyAttack") && myStats.isIFrames == false && state != PlayerState.Dead)
        {
            Debug.Log("player hit");
            Transform inHitboxPos = collision.transform.parent.transform;
            EnemyStats incStats = collision.transform.GetComponentInParent<EnemyStats>();

            PlayClip(1);

            state = PlayerState.Hitstun;
            myAnim.SetTrigger("Damage");
            StopAllCoroutines();
            StartCoroutine(waitForHitstun(0.75f));
            rb.AddForce(new Vector2(-(inHitboxPos.position.x - transform.position.x), -(inHitboxPos.position.y - transform.position.y) * 15).normalized * hitstunMulti, ForceMode2D.Impulse);
            Debug.Log("incoming damage" + incStats.damage.ToString());
            myStats.addHealth(-incStats.damage);

        }

        else if (collision.transform.CompareTag("LevelEnd"))
        {
            myStats.textPopup.gameObject.SetActive(true);

            if(myStats.score >= myStats.myQuota)
            {
                state = PlayerState.Win;
                PlayClip(10);
                myAnim.SetTrigger("Win");
                StartCoroutine(waitLevelEnd(3.85f));
            }
            else
            {
                myStats.textPopup.text = "Quota not met!";
                StartCoroutine(myStats.popupTextWait(3f));
            }

        }
    }

    IEnumerator waitLevelEnd(float time)
    {
        yield return new WaitForSeconds(time);
        LevelManager myMan = FindObjectOfType<LevelManager>();
        myMan.LoadNewScene(myMan.mySceneIndex + 1);
    }

    IEnumerator waitLevelDed(float time)
    {
        yield return new WaitForSeconds(time);
        LevelManager myMan = FindObjectOfType<LevelManager>();
        myMan.ReloadScene();


    }
    IEnumerator waitForHitstun(float time)
    {
        yield return new WaitForSeconds(time);
        state = PlayerState.FreeMovement;
    }




    private bool checkGround()
    {
        bool ground = Physics2D.BoxCast(groundCheck.position, new Vector2(0.16f, 0.11f),0, new Vector2(0f, 0f), 0, groundLayer.value);
        return ground;
    }

    private void PlayClip(int clipIndex)
    {
        AudioClip inClip = myClips[clipIndex];
        myAudio.PlayOneShot(inClip);
    }

    IEnumerator ambienceTimer(float time)
    {
        yield return new WaitForSeconds(time);
        whichAmbience = !whichAmbience;
        waitAmbience = false;

        if (whichAmbience)
        {
            PlayClip(5);
        }
        else
        {
            PlayClip(6);
        }


    }

}
