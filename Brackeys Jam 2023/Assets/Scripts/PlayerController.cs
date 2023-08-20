using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;

    public float speed;
    public float jumpHeight;
    public float fallThreshold;
    public float fallSpeed;

    float moveX;
    bool toJump;

    public bool isGrounded;
    public GroundCheck groundCheck;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        moveX = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            toJump = true;
        }

        if(moveX != 0 || toJump == true)
        {
            Debug.Log("move = " + moveX.ToString() + ", toJump = " + toJump.ToString());
        }

    }

    private void FixedUpdate()
    {
        isGrounded = groundCheck.gCheck;
        MoveControl();
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
