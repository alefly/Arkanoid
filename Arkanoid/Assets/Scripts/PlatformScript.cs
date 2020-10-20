using UnityEngine;

public class PlatformScript : MonoBehaviour
{
    public const float Y = -5.5f;
    public float xExtremePoint = 5.7f;
    public float extensionValue = 0.5f;
    public float speed = 0.2f;

    public void Move(float x)
    {
        if (x > -xExtremePoint && x < xExtremePoint)
        {
            transform.position = new Vector3(x, Y, 0);
        }
    }

    public void Extension()
    {
        xExtremePoint -= extensionValue / 2f;
        transform.localScale = new Vector3(transform.localScale.x + extensionValue, transform.localScale.y, transform.localScale.z);
        if (transform.localScale.x + transform.position.x > xExtremePoint)
        {
            transform.position = new Vector3(xExtremePoint, transform.position.y, 0);
        }
        if (-transform.localScale.x + transform.position.x < -xExtremePoint)
        {
            transform.position = new Vector3(-xExtremePoint, transform.position.y, 0);
        }
    }
}
