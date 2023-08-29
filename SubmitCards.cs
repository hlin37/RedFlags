using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SubmitCards : MonoBehaviour
{
    private GameManagerScript gameManagerScript;

    private string _dropZone = "SubmitZone";

    private string _player = "Player";

    private string _whiteHand = "WhiteHand";

    private string _redHand = "RedHand";

    private bool submitted = false;

    private int playerID;

    [SerializeField]
    private Color unSelectWhite;

    [SerializeField]
    private Color unSelectRed;

    
    private void Awake() {
        if (PhotonNetwork.LocalPlayer.IsLocal) {
            playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        }
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
    }

    public void OnClick() {

        if (gameManagerScript.returnState() == GameManagerScript.GameState.selectingWhiteCards) {
            // If the number of cards is not equal to 2
            if (gameManagerScript.returnCardsClicked().Count != 2) {
                Debug.Log("Not Enough Cards");
            }
            else {
                GameObject dropZone = GameObject.Find(_player + playerID + "White" + _dropZone);
                GameObject whiteHand = GameObject.Find(_player + playerID + _whiteHand);
                while (dropZone.transform.childCount > 0) {
                    Transform previousSelectedCard = dropZone.transform.GetChild(0);
                    previousSelectedCard.SetParent(whiteHand.transform, false);
                    previousSelectedCard.gameObject.GetComponent<Image>().color = unSelectWhite;
                }
                foreach (GameObject card in gameManagerScript.returnCardsClicked()) {
                    card.transform.SetParent(dropZone.transform, false);
                    card.GetComponent<Image>().color = unSelectWhite;
                }
                gameManagerScript.returnCardsClicked().Clear();
            }
        }

        else if (gameManagerScript.returnState() == GameManagerScript.GameState.selectingRedCard) {
            if (gameManagerScript.returnCardsClicked().Count != 1) {
                Debug.Log("Not Enough Cards");
            }
            else {
                GameObject dropZone = GameObject.Find(_player + playerID + "Red" + _dropZone);
                GameObject redHand = GameObject.Find(_player + playerID + _redHand);
                while (dropZone.transform.childCount > 0) {
                    Transform previousSelectedCard = dropZone.transform.GetChild(0);
                    previousSelectedCard.SetParent(redHand.transform, false);
                    previousSelectedCard.gameObject.GetComponent<Image>().color = unSelectRed;
                }
                foreach (GameObject card in gameManagerScript.returnCardsClicked()) {
                    card.transform.SetParent(dropZone.transform, false);
                    card.GetComponent<Image>().color = unSelectRed;
                }
                gameManagerScript.returnCardsClicked().Clear();
            }
        }

            // Move all the cards from dropzone back into the hand
            // Add Selected Cards to DropZone
            // Clear Clicked
    }

}
