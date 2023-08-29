using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RedCard : MonoBehaviour
{

    [SerializeField]
    public int uniqueCardNumber {get; private set;}

    [SerializeField]
    private Text redText;

    private string _redHand = "RedHand";
    private string _player = "Player";

    public void setCardText(Card redCard) {
        redText.text = redCard.returnDescription();
        uniqueCardNumber = redCard.returnIdentifier();
    }

    public void setCardText(string description, int identifier) {
        redText.text = description;
        uniqueCardNumber = identifier;
    }

    public void setCardText(string description) {
        redText.text= description;
    }

    public void setParent(int playerID) {
        GameObject redHand = GameObject.Find(_player + playerID + _redHand);
        this.transform.SetParent(redHand.transform, false);
        this.transform.localScale = Vector3.one;
        this.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    public void setParent(GameObject canvas) {
        this.transform.SetParent(canvas.transform, false);
        this.transform.localScale = Vector3.one;
        this.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    public Text returnText() {
        return redText;
    }
}
