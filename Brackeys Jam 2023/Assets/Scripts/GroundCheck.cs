using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    BoxCollider2D myCol;
    public bool gCheck;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            gCheck = true;
        }
        else gCheck = false;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            gCheck = false;
        }
    }

}
