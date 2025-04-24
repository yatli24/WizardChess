using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;  // Required for UI components like Button

public class ExtraTurnButton : MonoBehaviour
{
    // Reference to the game manager or the script controlling the turn logic
    public Chessboard gameManager;

    [NonSerialized] public int remainingWhiteExtraTurns = 1;
    [NonSerialized] public int remainingBlackExtraTurns = 1;

    [NonSerialized] public int remainingWhiteTeleports = 1;
    [NonSerialized] public int remainingBlackTeleports = 1;

    public bool unlimitedWhiteTeleport = false;
    public bool unlimitedBlackTeleport = false;

    public TextMeshProUGUI whiteExtraTurnButtonText;
    public TextMeshProUGUI blackExtraTurnButtonText;

    public TextMeshProUGUI whiteTeleportButtonText;
    public TextMeshProUGUI blackTeleportButtonText;


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
            gameManager.whiteTeleportActive = true;

            if (!unlimitedWhiteTeleport)
            {
                remainingWhiteTeleports--;
                whiteTeleportButtonText.text = $"Activate White Teleport ({remainingWhiteTeleports})";
            }

            Debug.Log($"White teleport activated! Remaining: {(unlimitedWhiteTeleport ? "Infinite" : remainingWhiteTeleports.ToString())}");
        }
    }

    // black teleportation
    public void SetBlackTeleport()
    {
        if (!gameManager.isWhiteTurn && (unlimitedBlackTeleport || remainingBlackTeleports > 0))
        {
            gameManager.blackTeleportActive = true;

            if (!unlimitedBlackTeleport)
            {
                remainingBlackTeleports--;
                blackTeleportButtonText.text = $"Activate Black Teleport ({remainingBlackTeleports})";
            }

            Debug.Log($"Black teleport activated! Remaining: {(unlimitedBlackTeleport ? "Infinite" : remainingBlackTeleports.ToString())}");
        }
    }


}