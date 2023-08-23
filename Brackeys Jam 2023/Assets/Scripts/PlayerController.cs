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

    //animation data
    public Animator myAnim;
    public Transform spriteTransform;

    public enum PlayerState
    {
        FreeMovement,
        Jumping, 
        Attacking
    }

    public PlayerState state;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
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
                    hit2D.collider.GetComponent<BreakTile>().MineDamage(myStats.breakSpeed);
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

        rb.AddForce(new Vector2(speed * moveX * 100 * Time.deltaTime, 0), ForceMode2D.Force);

        if (toJump && isGrounded)
        {
            rb.AddForce(new Vector2(0, jumpHeight * 100 * Time.deltaTime), ForceMode2D.Impulse);
            toJump = false;
        }
    }

    private void FlipPlayer(float touchPos)
    {
        if (state == PlayerState.FreeMovement)
        {
            if (rb.velocity.x > 0)
            {
                Vector3 myScale = spriteTransform.localScale;
                myScale.x = Mathf.Abs(myScale.x);
                spriteTransform.localScale = myScale;
            }
            else if (rb.velocity.x < 0)
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
            Debug.Log("picking up!");
            PickupItem inPickup = collision.transform.GetComponent<PickupItem>();
            myStats.GetPowerup(inPickup.myStat, inPickup.value);
            Destroy(collision.transform.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

    }

    private bool checkGround()
    {
        bool ground = Physics2D.BoxCast(groundCheck.position, new Vector2(0.16f, 0.11f),0, new Vector2(0f, 0f), 0, groundLayer.value);
        return ground;
    }

}
