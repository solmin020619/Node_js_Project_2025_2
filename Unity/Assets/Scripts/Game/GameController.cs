using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameView gameView;
    public PlayerModel playerModel;
    private GameAPI gameAPI;

    private void Start()
    {
        gameAPI = gameObject.AddComponent<GameAPI>();
        gameView.SetRegisterButtonListener(OnRegisterButtonClicked);
        gameView.SetLoginButtonListener(OnLoginButtonClicked);
    }

    public void OnRegisterButtonClicked()
    {
        string playerName = gameView.playerNameInput.text;
        StartCoroutine(gameAPI.RegisterPlayer(playerName, "1234")); 
    }

    public void OnLoginButtonClicked()
    {
        string playerName = gameView.playerNameInput.text;
        StartCoroutine(LoginPlayerCoroutine(playerName, "1234"));
    }

    private IEnumerator LoginPlayerCoroutine(string playername,string password)
    {
        yield return gameAPI.LoginPlayer(playername, password, player =>
        {
            playerModel = player;
            UpdateResourcesDisplay();
        });
    }

    private void UpdateResourcesDisplay()
    {
        if(playerModel != null)
        {
            gameView.SetPlayerName(playerModel.playerName);
            gameView.UpdateResources(playerModel.metal, playerModel.crystal, playerModel.deuterium);
            
        }
    }
}
