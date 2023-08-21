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
                MovementInput();
                FlipPlayer();
                ActionInput();
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
            Debug.Log("move = " + moveX.ToString() + ", toJump = " + toJump.ToString());
        }
    }

    private void ActionInput()
    {
        if (Input.GetMouseButton(0))
        {
            state = PlayerState.Attacking;

            //add force to slow player down
            rb.AddForce(new Vector2(speed * 0 * 100 * Time.deltaTime, 0), ForceMode2D.Force);

            //find object under cursor

            Vector3 touchPos = myCam.ScreenToWorldPoint(Input.mousePosition);
            touchPos.z = 0f;
            Debug.Log(touchPos.ToString());
            RaycastHit2D hit2D = Physics2D.Raycast(touchPos, touchPos - rb.transform.position, myStats.range, ~ignoreHitboxLayer);
            Debug.Log(hit2D.collider.ToString() + ": " + hit2D.collider.transform.position.ToString());

            //flip player to face for attack
            if (touchPos.x > 0)
            {
                Vector3 myScale = spriteTransform.localScale;
                myScale.x = Mathf.Abs(myScale.x);
                spriteTransform.localScale = myScale;
            }
            else if (touchPos.x < 0)
            {
                Vector3 myScale = spriteTransform.localScale;
                myScale.x = Mathf.Abs(myScale.x) * -1;
                spriteTransform.localScale = myScale;

            }

            //if breakable and in range, start breaking

            if (hit2D.collider.CompareTag("Breakable"))
            {
                if ((rb.transform.position - hit2D.collider.transform.position).magnitude <= myStats.range)
                {
                    Debug.Log("In range!");
                    hit2D.collider.GetComponent<BreakTile>().MineDamage(myStats.breakSpeed);
                }
                else Debug.Log("Out of range!");
            }


        }
        else
        {
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

    private void FlipPlayer()
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

    private void OnTriggerEnter(Collider collision)
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

    }

    private bool checkGround()
    {
        bool ground = false;
        return ground;
    }

}
