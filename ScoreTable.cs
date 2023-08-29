using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ScoreTable : MonoBehaviourPun
{
    private GameManagerScript gameManagerScript;

    [SerializeField]
    private GameObject textPrefab;

    [SerializeField]
    private GameObject nameGrid;

    void Awake() {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
    }

    void Start() {
        setUpScoreTable();
        this.gameObject.SetActive(false);
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Tab)) {
            this.gameObject.SetActive(false);
        }
    }

    public void setUpScoreTable() {
        if (PhotonNetwork.LocalPlayer.IsLocal) {
            foreach (Player player in PhotonNetwork.PlayerList) {
                GameObject tempTextBox = PhotonNetwork.Instantiate(textPrefab.name, transform.position, transform.rotation);
                tempTextBox.name = player.NickName;
                tempTextBox.transform.SetParent(nameGrid.transform);
                tempTextBox.transform.position = Vector3.zero;
                tempTextBox.GetComponent<Text>().text = player.NickName;

            }
        }
    }

    public GameObject returnGrid() {
        return nameGrid;
    }
}
