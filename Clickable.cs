using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GameManagerScript.GameState;

public class Clickable : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerClickHandler
{
    [SerializeField]
    private Color selectBlue;

    [SerializeField]
    private Color unSelectRed;

    [SerializeField]
    private Color unSelectWhite;

    private Image img;

    private GameManagerScript gameManagerScript;

    private void Awake() {
        img = this.GetComponent<Image>();
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
    }

    public void OnPointerClick(PointerEventData eventData) {
        OnSelect(eventData);
    }

    public void OnSelect(BaseEventData eventData) {
        if (gameManagerScript.returnState() == GameManagerScript.GameState.selectingWhiteCards) {
            
            // This ensures that if a player chooses a red card, it won't be selected.
            if (this.gameObject.name.Contains("Red")) {
                img.color = unSelectRed;
            }

            // Player selecting a white card
            else if (this.gameObject.name.Contains("White")) {

                if (this.gameObject.GetComponent<WhiteCard>().returnText().text == "Custom Card") {
                    this.gameObject.transform.GetChild(1).gameObject.SetActive(true);
                }


                // If white card is not one of the two cards clicked, turn it blue
                if (!gameManagerScript.returnCardsClicked().Contains(this.gameObject)) {

                    if (this.gameObject.GetComponent<WhiteCard>().returnText().text == "Custom Card") {
                        return;
                    }

                    // If the current number of selected cards is less than 2, turn it blue
                    if (gameManagerScript.returnCardsClicked().Count < 2) {
                        gameManagerScript.returnCardsClicked().Add(this.gameObject);
                        img.color = selectBlue;
                    }

                    // If the current count of selected cards is 2, turn the first card selected white and the new selected card blue
                    else {
                        gameManagerScript.returnCardsClicked()[0].GetComponent<Clickable>().OnDeselect(eventData);
                        gameManagerScript.returnCardsClicked().RemoveAt(0);
                        gameManagerScript.returnCardsClicked().Add(this.gameObject);
                        img.color = selectBlue;
                    }
                }
                // If same card is clicked again, turn it back to white
                else {
                    gameManagerScript.returnCardsClicked().Remove(this.gameObject);
                    img.color = unSelectWhite;
                }
            }
        }
        else if (gameManagerScript.returnState() == GameManagerScript.GameState.selectingRedCard) {
            if (this.gameObject.name.Contains("White")) {
                img.color = unSelectWhite;
            }

            else if (this.gameObject.name.Contains("Red")) {
                if (!gameManagerScript.returnCardsClicked().Contains(this.gameObject)) {

                    if (gameManagerScript.returnCardsClicked().Count < 1) {
                        gameManagerScript.returnCardsClicked().Add(this.gameObject);
                        img.color = selectBlue;
                    }

                    else {
                        gameManagerScript.returnCardsClicked()[0].GetComponent<Clickable>().OnDeselect(eventData);
                        gameManagerScript.returnCardsClicked().RemoveAt(0);
                        gameManagerScript.returnCardsClicked().Add(this.gameObject);
                        img.color = selectBlue;
                    }
                }

                else {
                    gameManagerScript.returnCardsClicked().Remove(this.gameObject);
                    img.color = unSelectRed;
                }
            }
        }
    }

    public void OnDeselect(BaseEventData eventData) {
        if (this.gameObject.name.Contains("White")) {
            img.color = unSelectWhite;
        }
        else {
            img.color = unSelectRed;
        }
    }
}
