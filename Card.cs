using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Random=System.Random;

[System.Serializable]

public class Card
{

    // The text description of the card
    private string description;

    // The unique number associated to this card
    private int numberIdentifier;

    public Card() {

    }

    public void setText(string description) {
        this.description = description;
    }

    public void setNumber(int number) {
        numberIdentifier = number;
    }

    public string returnDescription() {
        return description;
    }

    public int returnIdentifier() {
        return numberIdentifier;
    }
}
