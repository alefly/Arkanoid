using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusScript : MonoBehaviour
{
    private GameController.BonusType type;
    System.Action<GameController.BonusType> OnCollisionBonus = delegate (GameController.BonusType type) { };
    
    void Start()
    {
        OnCollisionBonus = GameController.Instance.ActivationBonus;
    }

    public void SetType(GameController.BonusType type) {
        this.type = type;
        switch (type) {
            case GameController.BonusType.extensionBonus:
                GetComponent<SpriteRenderer>().color = GameController.Instance.WITHEXTENSIONBONUSCOLOR;
                break;
            case GameController.BonusType.ballBonus:
                GetComponent<SpriteRenderer>().color = GameController.Instance.WITHBALLBONUSCOLOR;
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == GameController.PLATFORMLAYER)
        {
            OnCollisionBonus(type);
        }
        Destroy(gameObject);
    }
}
