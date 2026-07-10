using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private string groundTag = "Ground";
    public bool isGround = false;

    public bool IsGround()
    {
        return isGround;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(groundTag))
        {
            isGround = true;
            Debug.Log("地面に接地");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag(groundTag))
        {
            isGround = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(groundTag))
        {
            isGround = false;
            Debug.Log("地面から離れた");
        }
    }
}
