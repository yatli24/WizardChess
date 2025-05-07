using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;  // Required for UI components like Button

public class AbilityButtonsManager : MonoBehaviour
{
    // Reference to the game manager or the script controlling the turn logic
    public Chessboard gameManager;

    [NonSerialized] public int remainingWhiteExtraTurns = 2;
    [NonSerialized] public int remainingBlackExtraTurns = 2;

    [NonSerialized] public int remainingWhiteTeleports = 2;
    [NonSerialized] public int remainingBlackTeleports = 2;

    public bool unlimitedWhiteTeleport = false;
    public bool unlimitedBlackTeleport = false;

    public TextMeshProUGUI whiteExtraTurnButtonText;
    public TextMeshProUGUI blackExtraTurnButtonText;

    public TextMeshProUGUI whiteTeleportButtonText;
    public TextMeshProUGUI blackTeleportButtonText;

    public GameObject whiteTeleportButtonAura;
    public GameObject blackTeleportButtonAura;

    public GameObject whiteTeleportButtonColor;
    public GameObject blackTeleportButtonColor;


    private void Awake()
    {

        // set text to available extra turns at awake
        whiteExtraTurnButtonText.text = $"Activate White Extra Turn ({remainingWhiteExtraTurns})";
        blackExtraTurnButtonText.text = $"Activate Black Extra Turn ({remainingBlackExtraTurns})";

        // set text to availabel teleports at awake
        whiteTeleportButtonText.text = $"Activate White Teleport ({remainingWhiteTeleports})";
        blackTeleportButtonText.text = $"Activate Black Teleport ({remainingBlackTeleports})";
    }


    // Method to set whiteExtraTurn to true
    public void SetWhiteExtraTurn()
    {
        if (gameManager.isWhiteTurn && remainingWhiteExtraTurns > 0)
        {
            if (gameManager.whiteExtraTurn == false)
            {
                gameManager.whiteExtraTurn = true;
                remainingWhiteExtraTurns--;
                whiteExtraTurnButtonText.text = $"Activate White Extra Turn ({remainingWhiteExtraTurns})";
                Debug.Log($"White activated extra turn! {remainingWhiteExtraTurns} Remaining!");
            }
            
            else
            {
                return;
            }
        }
    }

    // Method to set blackExtraTurn to true
    public void SetBlackExtraTurn()
    {
        if (!gameManager.isWhiteTurn && remainingBlackExtraTurns > 0)
        {
            if(gameManager.blackExtraTurn == false)
            {
                gameManager.blackExtraTurn = true;
                remainingBlackExtraTurns--;
                blackExtraTurnButtonText.text = $"Activate Black Extra Turn ({remainingBlackExtraTurns})";
                Debug.Log($"Black activated extra turn! {remainingBlackExtraTurns} Remaining!");
            }

            else
            {
                return;
            }
            
        }
    }

    // white teleportation
    public void SetWhiteTeleport()
    {
        if (gameManager.isWhiteTurn && (unlimitedWhiteTeleport || remainingWhiteTeleports > 0))
        {
            if (!gameManager.whiteTeleportActive)
            {
                gameManager.whiteTeleportActive = true;

                if (!unlimitedWhiteTeleport)
                {
                    remainingWhiteTeleports--;
                    Debug.Log($"[WHITE TELEPORT] Remaining teleports after use: {remainingWhiteTeleports}");
                    whiteTeleportButtonText.text = $"Activate White Teleport ({remainingWhiteTeleports})";
                    gameManager.abilityJustUsed = true;
                }

                Debug.Log($"White teleport activated! Remaining: {(unlimitedWhiteTeleport ? "Infinite" : remainingWhiteTeleports.ToString())}");
            }
        }
        else
        {
            Debug.Log("White teleport NOT activated: no charges or not your turn.");
        }
    }



    // black teleportation
    public void SetBlackTeleport()
    {
        if (!gameManager.isWhiteTurn && (unlimitedBlackTeleport || remainingBlackTeleports > 0))
        {
            if (!gameManager.blackTeleportActive)
            {
                gameManager.blackTeleportActive = true;

                if (!unlimitedBlackTeleport)
                {
                    remainingBlackTeleports--;
                    Debug.Log($"[BLACK TELEPORT] Remaining teleports after use: {remainingBlackTeleports}");
                    blackTeleportButtonText.text = $"Activate Black Teleport ({remainingBlackTeleports})";
                    gameManager.abilityJustUsed = true;
                }

                Debug.Log($"Black teleport activated! Remaining: {(unlimitedBlackTeleport ? "Infinite" : remainingBlackTeleports.ToString())}");
            }
        }
        else
        {
            Debug.Log("Black teleport NOT activated: no charges or not your turn.");
        }
    }



}