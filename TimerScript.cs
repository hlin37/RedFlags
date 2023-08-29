using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class TimerScript : MonoBehaviour
{
    // public float timeLeft;

    // public bool timeOn = false;

    private int startTime;

    public float Countdown = 100f;

    public const string CountdownStartTime = "StartTime";

    [SerializeField]
    private int timeLeft;

    [SerializeField]
    private Text timerText;

    private GameManagerScript gameManagerScript;

    void Awake() {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
    }

    void Start() {
        this.startTime = tryGetStartTime();
    }

    void Update() {
        timeLeft = (int) TimeRemaining();
        //Debug.Log(timeLeft);
    }

    private int tryGetStartTime() {
        int startTimestamp = PhotonNetwork.ServerTimestamp;
        object startTimeFromProps;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(CountdownStartTime, out startTimeFromProps)) {
            startTimestamp = (int)startTimeFromProps;
        }
        return startTimestamp;
    }

    private float TimeRemaining() {
        float returnTime;
        if (gameManagerScript.returnState() == GameManagerScript.GameState.destroyCards) {
            this.Countdown += 60f;
        }
        int timer = PhotonNetwork.ServerTimestamp - this.startTime;
        returnTime = this.Countdown - timer / 1000f;
        return returnTime;
    }

    public int returnTimeLeft() {
        return timeLeft;
    }

    public void addTime(int time) {
        timeLeft =  timeLeft + time;
    }

    public Text returnTimerText() {
        return timerText;
    }

    // public void changeTimer(bool turnTimeOn) {
    //     timeOn = turnTimeOn;
    // }
}
