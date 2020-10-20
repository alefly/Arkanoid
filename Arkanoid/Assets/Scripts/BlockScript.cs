using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScript : MonoBehaviour
{
    public GameController.BlockType type;
    public bool isBonus;

    public void SetType(GameController.BlockType blockType, bool bonus)
    {
        type = blockType;
        switch (type)
        {
            case GameController.BlockType.simple:
                GetComponent<SpriteRenderer>().color = GameController.Instance.SIMPLECOLOR;
                break;
            case GameController.BlockType.doubled:
                GetComponent<SpriteRenderer>().color = GameController.Instance.DOUBLEDCOLOR;
                break;
            case GameController.BlockType.wall:
                GetComponent<SpriteRenderer>().color = GameController.Instance.WALLCOLOR;
                break;
            case GameController.BlockType.withExtensionBonus:
                GetComponent<SpriteRenderer>().color = GameController.Instance.WITHEXTENSIONBONUSCOLOR;
                SetIsBonus(bonus);
                break;
            case GameController.BlockType.withBallBonus:
                GetComponent<SpriteRenderer>().color = GameController.Instance.WITHBALLBONUSCOLOR;
                SetIsBonus(bonus);
                break;
        }
    }

    public void SetIsBonus(bool bonus)
    {
        isBonus = bonus;
    }
}
