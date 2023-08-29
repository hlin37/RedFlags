using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GameManagerScript.GameState;
using Photon.Pun;

public class ClickCanvas : MonoBehaviour, ISelectHandler, IPointerClickHandler
{

    private GameManagerScript gameManagerScript;

    [SerializeField]
    private Color selectGreen;

    private Image img;

    void Awake() {
        img = this.GetComponent<Image>();
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
    }
    public void OnPointerClick(PointerEventData eventData) {
        OnSelect(eventData);
    }

    public void OnSelect(BaseEventData eventData) {
        if (gameManagerScript.returnState() == GameManagerScript.GameState.end) {
            if (PhotonNetwork.LocalPlayer.IsLocal) {
                if (PhotonNetwork.LocalPlayer.ActorNumber == gameManagerScript.returnSingle()) {
                    int playerID = int.Parse((this.gameObject.name[6]).ToString());
                    gameManagerScript.setWinner(playerID);
                    gameManagerScript.setState(GameManagerScript.GameState.destroyCards);
                    img.color = selectGreen;
                }
            }
        }
    }

    public void setCanvasGreen() {
        img.color = selectGreen;
    }
}
