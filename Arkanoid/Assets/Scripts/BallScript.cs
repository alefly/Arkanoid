using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    System.Action<GameObject, GameObject> OnCollisionBall = delegate (GameObject gameObject, GameObject gameObjectBall) { };

    Rigidbody2D _rb2d;
    public Rigidbody2D RB2D
    {
        get
        {
            if (_rb2d == null)
                _rb2d = GetComponent<Rigidbody2D>();
            return _rb2d;
        }
    }
    
    void Start()
    {
        OnCollisionBall += GameController.Instance.OnCollisionBall;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer != GameController.WALLLAYER)
        {
            OnCollisionBall(collision.gameObject, gameObject);
        }
    }

    public void StartMove()
    {
        RB2D.gravityScale = 0;
        RB2D.AddForce(new Vector2(3f, 7f), ForceMode2D.Impulse);
    }
}
