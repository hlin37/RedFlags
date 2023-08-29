using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WhiteCard : MonoBehaviour
{

    [SerializeField]
    public int uniqueCardNumber {get; private set;}

    [SerializeField]
    private Text whiteText;

    private string _whiteHand = "WhiteHand";
    private string _player = "Player";

    public void setCardText(Card redCard) {
        whiteText.text = redCard.returnDescription();
        uniqueCardNumber = redCard.returnIdentifier();
    }

    public void setCardText(string description, int identifier) {
        whiteText.text = description;
        uniqueCardNumber = identifier;
    }

    public void setCardText(string description) {
        whiteText.text= description;
    }

    public void setParent(int playerID) {
        GameObject whiteHand = GameObject.Find(_player + playerID + _whiteHand);
        this.transform.SetParent(whiteHand.transform, false);
        this.transform.localScale = Vector3.one;
        this.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    public void setParent(GameObject canvas) {
        this.transform.SetParent(canvas.transform, false);
        this.transform.localScale = Vector3.one;
        this.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    public Text returnText() {
        return whiteText;
    }
}
