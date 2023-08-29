using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class DeckOfCards : MonoBehaviourPun
{

    // List of Red Cards
    private Queue<Card> redList = new Queue<Card>();

    // List of White Cards
    private Queue<Card> whiteList = new Queue<Card>();

    private string path = "Assets/Text/";

    void Awake() {
    }

    void Start() {
        if (PhotonNetwork.IsMasterClient) {
            string[] linesOfText = readAllLines("red");
            for (int index = 0; index < 50; index++) {
                Card redCard = new Card();
                redCard.setText(linesOfText[index]);
                redCard.setNumber(index);
                redList.Enqueue(redCard);
            }
            linesOfText = readAllLines("white");
            for (int index = 15; index < 65; index++) {
                Card whiteCard = new Card();
                whiteCard.setText(linesOfText[index - 15]);
                whiteCard.setNumber(index);
                whiteList.Enqueue(whiteCard);
            }
        }
    }

    public string[] readAllLines(string color) {
        string[] lines;
        if (color.Equals("red")) {
            lines = File.ReadAllLines(path + "red.txt");
        }
        else {
            lines = File.ReadAllLines(path + "white.txt");
        }
        return lines;
    }

    public Queue<Card> returnRedDeck() {
        return redList;
    }

    public Queue<Card> returnWhiteDeck() {
        return whiteList;
    }


}
