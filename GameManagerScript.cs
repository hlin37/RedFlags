using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Threading;

public class GameManagerScript : MonoBehaviourPun
{
    public enum GameState {
        
        chooseSinglePlayer,
        cardDistribution,
        selectingWhiteCards,
        placeWhiteCards,
        readingWhiteCards,
        waitingForSetup,
        selectingRedCard,
        readingRedCard,
        selectingWinner,
        end,
        destroyCards,

    }

    // the current state of the game
    private GameState gameState;

    // RaiseEvent ID for Distrbuting RedCards;
    private const byte redCardHandling = 1;

    // RaiseEvent ID for Distributing WhiteCards;
    private const byte whiteCardHandling = 2;

    // RaiseEvent ID for sendingWhiteCards to Single Player to Read;
    private const byte sendingWhiteCards = 3;

    // RaiseEvent ID to choose single player;
    private const byte singleNumber = 4;

    private const byte sendingRedCard = 5;

    private const byte sendWinner = 6;

    [SerializeField]
    private GameObject redCardPrefab;

    [SerializeField]
    private GameObject whiteCardPrefab;

    [SerializeField]
    private GameObject canvasOnSingle;

    private List<GameObject> cardsClicked = new List<GameObject>();

    private GameObject timer;

    private bool timerUp;

    private int winner;

    // descriptions of the cards sent in by each player. playerID -> two cards they submitted;
    [SerializeField]
    private Dictionary<int, string[]> playerCardDescriptions = new Dictionary<int, string[]>();

    [SerializeField]
    private int[] orderOfSinglePlayers;

    // PlayerID to Sabotage;
    private int playerToAttack;

    private bool gameOver = true;

    [SerializeField]
    private GameObject scoreTable;

    private void Awake() {
        timer = GameObject.Find("TimerCanvas");
        gameState = GameState.chooseSinglePlayer;
    }

    void Start() {
        if (PhotonNetwork.LocalPlayer.IsLocal) {
            foreach (Player player in PhotonNetwork.PlayerList) {
                playerCardDescriptions.Add(player.ActorNumber, new string[3]);
            }
        }
    }

    private void Update() {
        updateTimerText(timer.GetComponent<TimerScript>().returnTimeLeft());
        seeScoreBoard();
        if (gameState == GameState.chooseSinglePlayer) {
            Debug.Log("runs here");
            changeCameraLocation();
            chooseSingle();
            updateSingle();
            gameState = GameState.waitingForSetup;
        }
        if (gameState == GameState.waitingForSetup) {
            if (timer.GetComponent<TimerScript>().returnTimeLeft() < 90) {
                //changeCameraLocation();
                //Debug.Log("Running here");
                gameState = GameState.cardDistribution;
            }
        }
        if (gameState == GameState.cardDistribution) {
            distributeCards();
            gameOver = false;
            changeCameraLocation();
            findPlayerToAttack();
            gameState = GameState.selectingWhiteCards;
        }
        if (gameState == GameState.selectingWhiteCards) {
            if (timer.GetComponent<TimerScript>().returnTimeLeft() < 80) {
                sendCardsToSingle();
                gameState = GameState.readingWhiteCards;
                changeCameraLocation();
            }
        }
        if (gameState == GameState.readingWhiteCards) {
            returnCardsClicked().Clear();
            if (timer.GetComponent<TimerScript>().returnTimeLeft() < 70) {
                moveWhiteCard();
                gameState = GameState.selectingRedCard;
                changeCameraLocation();
            }
        }
        if (gameState == GameState.selectingRedCard) {
            if (timer.GetComponent<TimerScript>().returnTimeLeft() < 55) {
                sendRedCardToSingle();
                gameState = GameState.readingRedCard;
                changeCameraLocation();
            }
        }
        if (gameState == GameState.readingRedCard) {
            if (timer.GetComponent<TimerScript>().returnTimeLeft() < 50) {
                destroyRedCard();
                //changeCameraLocation();
                gameState = GameState.selectingWinner;
            }
            //destroyRedCard();
        }
        if (gameState == GameState.selectingWinner) {
            if (timer.GetComponent<TimerScript>().returnTimeLeft() < 45) {
                changeCameraLocation();
                gameState = GameState.end;
            }
        }
        if (gameState == GameState.destroyCards) {
            destroyAllCards();
            updateScoreBoard();
            returnCardsClicked().Clear();
            //timer.GetComponent<TimerScript>().addTime(timer.GetComponent<TimerScript>().returnTimeLeft());
            gameState = GameState.chooseSinglePlayer;
        }
    }

    private void updateTimerText(int timeLeft) {
        timer.GetComponent<TimerScript>().returnTimerText().text = timeLeft.ToString();
    }

    private void seeScoreBoard() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            scoreTable.SetActive(true);
        }
    }

    private void updateScoreBoard() {
        scoreTable.SetActive(true);
        GameObject nameGrid = GameObject.Find("NameGrid");
        foreach (Transform playerScore in nameGrid.transform) {
            if (playerScore.gameObject.name == PhotonNetwork.LocalPlayer.Get(winner).NickName) {
                int number = System.Int32.Parse(playerScore.GetChild(0).GetComponent<Text>().text);
                number++;
                playerScore.GetChild(0).GetComponent<Text>().text = number.ToString();
            }
        }
        scoreTable.SetActive(false);
    }

    private void chooseSingle() {
        if (gameOver) {
            if (PhotonNetwork.IsMasterClient) {
                orderOfSinglePlayers = new int[PhotonNetwork.PlayerList.Length];
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++) {
                    orderOfSinglePlayers[i] = PhotonNetwork.PlayerList[i].ActorNumber;
                }
                shuffle(orderOfSinglePlayers);
                object[] datas = new object[] {orderOfSinglePlayers};
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others};
                PhotonNetwork.RaiseEvent(singleNumber, datas, raiseEventOptions, SendOptions.SendUnreliable);
            }
        }
    }

    private void findPlayerToAttack() {
        if (PhotonNetwork.LocalPlayer.IsLocal) {
            if (PhotonNetwork.LocalPlayer.ActorNumber != orderOfSinglePlayers[0]) {
                int index = System.Array.IndexOf(orderOfSinglePlayers, PhotonNetwork.LocalPlayer.ActorNumber);
                //Debug.Log("This is my " + index + "and my playerID is " + PhotonNetwork.LocalPlayer.ActorNumber);
                if (index == PhotonNetwork.PlayerList.Length - 1) {
                    playerToAttack = orderOfSinglePlayers[1];
                }
                else {
                    playerToAttack = orderOfSinglePlayers[index + 1];
                }
            }
        }
    }

    private void distributeCards() {
        if (PhotonNetwork.IsMasterClient) {
            Queue<Card> redList = this.gameObject.GetComponent<DeckOfCards>().returnRedDeck();
            Queue<Card> whiteList = this.gameObject.GetComponent<DeckOfCards>().returnWhiteDeck();
            if (gameOver) {
                for (int i = 0; i < 3; i++) {
                    foreach (Player player in PhotonNetwork.PlayerList) {
                        if (player.IsLocal) {
                            GameObject redCard = PhotonNetwork.Instantiate(redCardPrefab.name, transform.position, transform.rotation);
                            redCard.GetComponent<RedCard>().setCardText(redList.Dequeue());
                            redCard.GetComponent<RedCard>().setParent(player.ActorNumber);
                        }
                        else {
                            Card redCard = redList.Dequeue();
                            object[] datas = new object[] {redCard.returnDescription(), redCard.returnIdentifier(), player.ActorNumber};
                            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others};
                            PhotonNetwork.RaiseEvent(redCardHandling, datas, raiseEventOptions, SendOptions.SendUnreliable);
                        }
                    }
                }
                for (int i = 0 ; i < 4; i++) {
                    foreach (Player player in PhotonNetwork.PlayerList) {
                        if (player.IsLocal) {
                            GameObject whiteCard = PhotonNetwork.Instantiate(whiteCardPrefab.name, transform.position, transform.rotation);
                            whiteCard.GetComponent<WhiteCard>().setCardText(whiteList.Dequeue());
                            whiteCard.GetComponent<WhiteCard>().setParent(player.ActorNumber);
                        }
                        else {
                            Card whiteCard = whiteList.Dequeue();
                            object[] datas = new object[] {whiteCard.returnDescription(), whiteCard.returnIdentifier(), player.ActorNumber};
                            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others};
                            PhotonNetwork.RaiseEvent(whiteCardHandling, datas, raiseEventOptions, SendOptions.SendUnreliable);
                        }
                    }
                }
            }
            else {
                foreach (Player player in PhotonNetwork.PlayerList) {
                    if (player.ActorNumber != orderOfSinglePlayers[0]) {
                        if (player.IsLocal) {
                            GameObject redCard = PhotonNetwork.Instantiate(redCardPrefab.name, transform.position, transform.rotation);
                            redCard.GetComponent<RedCard>().setCardText(redList.Dequeue());
                            redCard.GetComponent<RedCard>().setParent(player.ActorNumber);
                        }
                        else {
                            Card redCard = redList.Dequeue();
                            object[] datas = new object[] {redCard.returnDescription(), redCard.returnIdentifier(), player.ActorNumber};
                            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others};
                            PhotonNetwork.RaiseEvent(redCardHandling, datas, raiseEventOptions, SendOptions.SendUnreliable);
                        }
                    }
                }
                for (int i = 0 ; i < 2; i++) {
                    foreach (Player player in PhotonNetwork.PlayerList) {
                        if (player.ActorNumber != orderOfSinglePlayers[0]) {
                            if (player.IsLocal) {
                                GameObject whiteCard = PhotonNetwork.Instantiate(whiteCardPrefab.name, transform.position, transform.rotation);
                                whiteCard.GetComponent<WhiteCard>().setCardText(whiteList.Dequeue());
                                whiteCard.GetComponent<WhiteCard>().setParent(player.ActorNumber);
                            }
                            else {
                                Card whiteCard = whiteList.Dequeue();
                                object[] datas = new object[] {whiteCard.returnDescription(), whiteCard.returnIdentifier(), player.ActorNumber};
                                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others};
                                PhotonNetwork.RaiseEvent(whiteCardHandling, datas, raiseEventOptions, SendOptions.SendUnreliable);
                            }
                        }
                    }
                }
            }
        }
    }

    public void changeCameraLocation() {
        if (gameState == GameState.cardDistribution || gameState == GameState.selectingRedCard) {
            Vector3[] cameraLocations = { new Vector3(-560f, 750f, -10f) , new Vector3(560f, 750f, -10f), new Vector3(-560f, 240f, -10f)};
            if (PhotonNetwork.LocalPlayer.IsLocal) {
                Camera cam = GameObject.Find("Main Camera").GetComponent<Camera>();
                cam.orthographicSize = 245f;
                if (PhotonNetwork.LocalPlayer.ActorNumber == orderOfSinglePlayers[0]) {
                    cam.transform.position = new Vector3(0f, -750f, -10f);
                }
                else {
                    cam.transform.position = cameraLocations[PhotonNetwork.LocalPlayer.ActorNumber - 1];
                }
            }
        }
        else if (gameState == GameState.readingWhiteCards || gameState == GameState.selectingWinner) {
            if (PhotonNetwork.LocalPlayer.IsLocal) {
                Camera cam = GameObject.Find("Main Camera").GetComponent<Camera>();
                cam.transform.position = new Vector3(0f, -750f, -10f);
            }
        }
        else if (gameState == GameState.readingRedCard || gameState ==  GameState.chooseSinglePlayer) {
            if (PhotonNetwork.LocalPlayer.IsLocal) {
                Camera cam = GameObject.Find("Main Camera").GetComponent<Camera>();
                cam.transform.position = new Vector3(2000f, 0f, -10f);
            }
        }
    }

    public void sendCardsToSingle() {
        if (PhotonNetwork.LocalPlayer.IsLocal) {
            if (PhotonNetwork.LocalPlayer.ActorNumber != orderOfSinglePlayers[0]) {
                GameObject dropZone = GameObject.Find("Player" + PhotonNetwork.LocalPlayer.ActorNumber + "WhiteSubmitZone");
                List<string> submittedCards = new List<string>();
                string[] submittedDescriptions;
                foreach (Transform card in dropZone.transform) {
                    string description = card.gameObject.GetComponent<WhiteCard>().returnText().text;
                    submittedCards.Add(description);
                }
                if (submittedCards.Count != 2) {
                    submittedCards = fillRemainingCards(submittedCards.Count, submittedCards);
                }
                submittedDescriptions = submittedCards.ToArray();
                object[] datas = new object[] {submittedDescriptions, PhotonNetwork.LocalPlayer.ActorNumber};
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All};
                //RaiseEventOptions raiseEventOptions = new RaiseEventOptions { TargetActors = new int[] {1}};
                PhotonNetwork.RaiseEvent(sendingWhiteCards, datas, raiseEventOptions, SendOptions.SendUnreliable);
            }
        }
    }

    public List<string> fillRemainingCards(int numberOfCards, List<string> submittedCards) {
        if (PhotonNetwork.LocalPlayer.IsLocal) {
            if (PhotonNetwork.LocalPlayer.ActorNumber != orderOfSinglePlayers[0]) {
                GameObject whiteHand = GameObject.Find("Player" + PhotonNetwork.LocalPlayer.ActorNumber + "WhiteHand");
                GameObject dropZone = GameObject.Find("Player" + PhotonNetwork.LocalPlayer.ActorNumber + "WhiteSubmitZone");
                for (int i = submittedCards.Count; i < 2; i++) {
                    int number = UnityEngine.Random.Range(0, whiteHand.transform.childCount);
                    Debug.Log(whiteHand.transform.childCount);
                    string description = whiteHand.transform.GetChild(number).GetComponent<WhiteCard>().returnText().text;
                    whiteHand.transform.GetChild(number).SetParent(dropZone.transform, false);
                    submittedCards.Add(description);
                }
            }
        }
        return submittedCards;
    }

    public void placeCardsOnSingle(int playerID) {
        if (PhotonNetwork.LocalPlayer.IsLocal) {
        //if (PhotonNetwork.IsMasterClient) {
            //if (PhotonNetwork.LocalPlayer.IsLocal && PhotonNetwork.LocalPlayer.ActorNumber == 1) {
                GameObject canvas = PhotonNetwork.Instantiate(canvasOnSingle.name, transform.position, transform.rotation);
                canvas.name = "Player" + playerID + "WhiteCards";
                canvas.transform.SetParent(GameObject.Find("SinglePlayer").transform);
                foreach (string description in playerCardDescriptions[playerID]) {
                    if (description != null) {
                        GameObject whiteCard = PhotonNetwork.Instantiate(whiteCardPrefab.name, transform.position, transform.rotation);
                        whiteCard.GetComponent<WhiteCard>().setCardText(description);
                        whiteCard.GetComponent<WhiteCard>().setParent(canvas);
                    }
                }
            //}
        }
    }

    public void moveWhiteCard() {
        if (PhotonNetwork.LocalPlayer.IsLocal) {
            if (PhotonNetwork.LocalPlayer.ActorNumber != orderOfSinglePlayers[0]) {
                GameObject canvas = GameObject.Find("Player" + playerToAttack + "WhiteCards");
                GameObject submitZone = GameObject.Find("Player" + PhotonNetwork.LocalPlayer.ActorNumber + "WhiteSubmitZone");
                foreach (Transform card in submitZone.transform) {
                    if (card.GetComponent<PhotonView>().IsMine) {
                        PhotonNetwork.Destroy(card.gameObject);
                    }
                }
                for (int i = 0; i < 1; i++) {
                    canvas.transform.GetChild(0).gameObject.GetComponent<WhiteCard>().setParent(submitZone);
                    canvas.transform.GetChild(0).gameObject.GetComponent<WhiteCard>().setParent(submitZone);
                }
            }
        }
    }

    public void sendRedCardToSingle() {
        if (PhotonNetwork.LocalPlayer.IsLocal) {
            if (PhotonNetwork.LocalPlayer.ActorNumber != orderOfSinglePlayers[0]) {
                GameObject dropZone = GameObject.Find("Player" + PhotonNetwork.LocalPlayer.ActorNumber + "RedSubmitZone");
                string[] submittedRedDescriptions = new string[1];
                foreach (Transform card in dropZone.transform) {
                    string description = card.gameObject.GetComponent<RedCard>().returnText().text;
                    submittedRedDescriptions[0] = description;
                }
                if (submittedRedDescriptions[0] == null || submittedRedDescriptions[0] == "") {
                    Debug.Log("red card");
                    submittedRedDescriptions = fillRedCard(submittedRedDescriptions);
                }
                object[] datas = new object[] {submittedRedDescriptions, playerToAttack, PhotonNetwork.LocalPlayer.ActorNumber};
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All};
                //RaiseEventOptions raiseEventOptions = new RaiseEventOptions { TargetActors = new int[] {1}};
                PhotonNetwork.RaiseEvent(sendingRedCard, datas, raiseEventOptions, SendOptions.SendUnreliable);
            
            }
        }
    }

    public string[] fillRedCard(string[] submittedRedDescriptions) {
        if (PhotonNetwork.LocalPlayer.IsLocal) {
            if (PhotonNetwork.LocalPlayer.ActorNumber != orderOfSinglePlayers[0]) {
                GameObject redHand = GameObject.Find("Player" + PhotonNetwork.LocalPlayer.ActorNumber + "RedHand");
                GameObject dropZone = GameObject.Find("Player" + PhotonNetwork.LocalPlayer.ActorNumber + "RedSubmitZone");
                
                int number = UnityEngine.Random.Range(0, redHand.transform.childCount);
                string description = redHand.transform.GetChild(number).GetComponent<RedCard>().returnText().text;
                redHand.transform.GetChild(number).SetParent(dropZone.transform, false);
                submittedRedDescriptions[0] = description;
                
            }
        }
        return submittedRedDescriptions;
    }

    public void placeRedCardOnSingle(int playerID) {
        if (PhotonNetwork.LocalPlayer.IsLocal) {
            if (PhotonNetwork.LocalPlayer.ActorNumber == playerID) {
                GameObject canvas  = GameObject.Find("Player" + playerToAttack + "WhiteCards");
                GameObject submitZone = GameObject.Find("Player" + PhotonNetwork.LocalPlayer.ActorNumber + "WhiteSubmitZone");
                foreach (Transform card in submitZone.transform) {
                    submitZone.transform.GetChild(0).gameObject.GetComponent<WhiteCard>().setParent(canvas);
                    submitZone.transform.GetChild(0).gameObject.GetComponent<WhiteCard>().setParent(canvas);
                }
            }
        }
        createRedCard();
    }

    // NOT SURE WHY createRedCard() only works in that spot of the method. It creates two cards. So
    // IM JUST GOING TO DELETE THE EXTRA CARDS :)
    public void destroyRedCard() {
        if (PhotonNetwork.LocalPlayer.IsLocal) {
            GameObject singleCanvas = GameObject.Find("SinglePlayer");
            List<GameObject> redCardList = new List<GameObject>();
            foreach (Transform canvas in singleCanvas.transform) {
                if (canvas.childCount > 3) {
                    PhotonNetwork.Destroy(canvas.GetChild(3).gameObject);
                }
            }
        }
    }

    public void createRedCard() {
        for (int i = 1; i < orderOfSinglePlayers.Length; i++) {
            if (playerCardDescriptions[orderOfSinglePlayers[i]][2] != null) {
                GameObject canvas = GameObject.Find("Player" + orderOfSinglePlayers[i] + "WhiteCards");
                GameObject redCard = PhotonNetwork.Instantiate(redCardPrefab.name, transform.position, transform.rotation);
                redCard.GetComponent<RedCard>().setCardText(playerCardDescriptions[orderOfSinglePlayers[i]][2]);
                redCard.GetComponent<RedCard>().setParent(canvas);
                //Debug.Log("here");
            }
        }
    }

    public void destroyAllCards() {
        if (PhotonNetwork.LocalPlayer.IsLocal) {
            GameObject canvas = GameObject.Find("SinglePlayer");
            foreach (Transform playerCanvas in canvas.transform) {
                if (playerCanvas.GetComponent<PhotonView>().IsMine) {
                    PhotonNetwork.Destroy(playerCanvas.gameObject);
                }
            }
            GameObject submitZone = GameObject.Find("Player" + PhotonNetwork.LocalPlayer.ActorNumber + "RedSubmitZone");
            foreach (Transform card in submitZone.transform) {
                if (card.GetComponent<PhotonView>().IsMine) {
                    PhotonNetwork.Destroy(card.gameObject);
                }
            }
        }
    }

    private void updateSingle() {
        if (!gameOver) {
            int size = orderOfSinglePlayers.Length;
            int[] shiftNums = new int[size];

            for (int i = 0; i < size; i++)
            {
                shiftNums[i] = orderOfSinglePlayers[(i + 1) % size];
            }

            orderOfSinglePlayers = shiftNums;
        }
    }


    private void OnEnable() {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable() {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    // Simulate handing out card to each individual person rather than give 3 cards to one person and then 3 cards to the next etc.
    public void OnEvent(EventData obj) {
        if (obj.Code == singleNumber) {
            object[] datas  = (object[]) obj.CustomData;
            int[] order = (int[]) datas[0];
            //Debug.Log(order[0]);
            //Debug.Log(order[1]);
            //orderOfSinglePlayers = order;
            if (PhotonNetwork.LocalPlayer.IsLocal) {
                addToOrder(order);
            }
        }
        else if (obj.Code == redCardHandling) {
            object[] datas = (object[]) obj.CustomData;
            string description = (string) datas[0];
            int uniqueIdenifier = (int) datas[1];
            int playerID  = (int) datas[2];

            // Check if the client is the localPlayer and if localPlayer.ActorNumber matches
            // This is to prevent Player 3 from getting Player 2's card
            if (PhotonNetwork.LocalPlayer.IsLocal && PhotonNetwork.LocalPlayer.ActorNumber == playerID) {
                GameObject redCard = PhotonNetwork.Instantiate(redCardPrefab.name, transform.position, transform.rotation);
                redCard.GetComponent<RedCard>().setCardText(description, uniqueIdenifier);
                redCard.GetComponent<RedCard>().setParent(playerID);
            }
        }

        else if (obj.Code == whiteCardHandling) {
            object[] datas = (object[]) obj.CustomData;
            string description = (string) datas[0];
            int uniqueIdenifier = (int) datas[1];
            int playerID  = (int) datas[2];

            // Check if the client is the localPlayer and if localPlayer.ActorNumber matches
            // This is to prevent Player 3 from getting Player 2's card
            if (PhotonNetwork.LocalPlayer.IsLocal && PhotonNetwork.LocalPlayer.ActorNumber == playerID) {
                GameObject whiteCard = PhotonNetwork.Instantiate(whiteCardPrefab.name, transform.position, transform.rotation);
                whiteCard.GetComponent<WhiteCard>().setCardText(description, uniqueIdenifier);
                whiteCard.GetComponent<WhiteCard>().setParent(playerID);
            }
        }

        else if (obj.Code == sendingWhiteCards) {
            object[] datas = (object[]) obj.CustomData;
            string[] description = (string[]) datas[0];
            int playerID = (int) datas[1];
            // if (!playerCardDescriptions.ContainsKey(playerID)) {
            //     playerCardDescriptions.Add(playerID, new string[3]);
            // }
            playerCardDescriptions[playerID][0] = description[0];
            playerCardDescriptions[playerID][1] = description[1];
            placeCardsOnSingle(playerID);
        }

        else if (obj.Code == sendingRedCard) {
            object[] datas = (object[]) obj.CustomData;
            string[] description = (string[]) datas[0];
            int redCardOnWhichPlayer = (int) datas[1];
            int playerID = (int) datas[2];
            addToDescription(redCardOnWhichPlayer, description[0]);
            placeRedCardOnSingle(playerID);
        }

        else if (obj.Code == sendWinner) {
            object[] datas = (object[]) obj.CustomData;
            int winnerID = (int) datas[0];
            winner = winnerID;
            setColorAndText(winnerID);
        }
    }

    public void setColorAndText(int winnerID) {
        GameObject canvas = GameObject.Find("Player" + winnerID + "WhiteCards");
        canvas.GetComponent<ClickCanvas>().setCanvasGreen();
        gameState = GameState.destroyCards;
    }

    private void addToDescription(int playerIDToAttack, string description) {
        // Debug.Log(playerIDToAttack + " " + description);
        playerCardDescriptions[playerIDToAttack][2] = description;
    }

    private void addToOrder(int[] order) {
        this.orderOfSinglePlayers = order;
    }

    public GameState returnState() {
        return gameState;
    }

    public List<GameObject> returnCardsClicked() {
        return cardsClicked;
    }

    private void shuffle(int[] texts)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < texts.Length; t++ )
        {
            int tmp = texts[t];
            int r = Random.Range(t, texts.Length);
            texts[t] = texts[r];
            texts[r] = tmp;
        }
    }

    public int returnSingle() {
        return orderOfSinglePlayers[0];
    }

    public void setWinner(int playerID) {
        winner = playerID;
        
        object[] datas = new object[] {playerID};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others};
        //RaiseEventOptions raiseEventOptions = new RaiseEventOptions { TargetActors = new int[] {1}};
        PhotonNetwork.RaiseEvent(sendWinner, datas, raiseEventOptions, SendOptions.SendUnreliable);
    }

    public int returnWinner() {
        return winner;
    }

    public void setState(GameState state) {
        this.gameState = state;
    }
}
