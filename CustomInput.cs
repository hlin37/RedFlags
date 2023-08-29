using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomInput : MonoBehaviour
{

    [SerializeField]
    private Text description;

    [SerializeField]
    private GameObject inputField;

    private void Update() {
        if (Input.GetKeyUp(KeyCode.Return)) {
            changeText();
        }
    }

    private void changeText() {
        if (inputField.GetComponent<Text>().text != "") {
             description.text = inputField.GetComponent<Text>().text;
        }
        //description.text = inputField.GetComponent<Text>().text;
        this.gameObject.SetActive(false);
    }
}
