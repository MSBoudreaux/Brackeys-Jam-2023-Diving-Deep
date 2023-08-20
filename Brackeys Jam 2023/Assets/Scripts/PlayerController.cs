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
    public GroundCheck groundCheck;

    public Camera myCam;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        MovementInput();
        ActionInput();

    }

    private void FixedUpdate()
    {
        isGrounded = groundCheck.gCheck;
        MoveControl();
    }

    private void MovementInput()
    {
        moveX = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump"))
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
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 touchPos = myCam.ScreenToWorldPoint(Input.mousePosition);
            touchPos.z = 0f;
            Debug.Log(touchPos.ToString());
            RaycastHit2D hit2D = Physics2D.Raycast(touchPos, touchPos - rb.transform.position);
            Debug.Log(hit2D.collider.ToString() + ": " + hit2D.collider.transform.position.ToString());

            if (hit2D.collider.CompareTag("Breakable"))
            {
                if ((rb.transform.position - hit2D.collider.transform.position).magnitude <= myStats.range)
                {
                    Debug.Log("In range!");
                }
                else Debug.Log("Out of range!");
            }
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

}
