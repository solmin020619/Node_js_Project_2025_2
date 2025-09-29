using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameView gameView;
    public GameController gameController;

    private void Start()
    {
        gameController = gameView.gameObject.AddComponent<GameController>();
        gameController.gameView = gameView;

    }
}
