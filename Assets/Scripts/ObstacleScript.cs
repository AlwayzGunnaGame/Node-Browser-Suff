using Socket.Quobject.SocketIoClientDotNet.Client;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Linq;

public class ObstacleScript : MonoBehaviour
{
    public List<string> randomWordList;
    private StreamReader stream;
    private QSocket socket;
    public InputField guessWord;
    public Text randomWord;
    public Text timeBox;
    public Text obstacleCountBox;
    public Text scoreBox;
    private int obstacleRequiredNum;
    private float timeUntilObstacle = 1;
    private bool timeRunning = false;
    private float savedUpTime;
    private int obstaclesPassed = 0;
    

    void Start()
    {
        //using (stream = new StreamReader("EveryWord.txt"))
        //{
        //    string line;
        //    while((line = stream.ReadLine()) != null)
        //    {
        //        randomWordList.Add(line);
        //    }
        //}
        TextAsset mytxtData = (TextAsset)Resources.Load("EveryWord");
        string[] stringSeparators = new string[] { "\r\n" };
        randomWordList = mytxtData.text.Split(stringSeparators, StringSplitOptions.None).ToList();
        
        Debug.Log("start");
        socket = IO.Socket("https://mobile-party-time.herokuapp.com");
        //socket = IO.Socket("ws://localhost:3000");

        socket.On(QSocket.EVENT_CONNECT, () => {
            Debug.Log("Connected");
            //socket.Emit("chat message", "test");
        });

        socket.On("chat message", data => {
            Debug.Log(data);
            //chatBox.text = chatBox.text + "\n" + data;
            //displayMessage = true;
            //chatToPost = data.ToString();
        });

        socket.On("display-error", data => {
            Debug.Log("Error Occurred: " + data);
        });
        
        socket.On("player-1-connected", () =>
        {
            //playerName = "Player 1";
            Debug.Log("I am Player 1");
        });
        
        socket.On("player-2-connected", () =>
        {
            //if(playerName == "")
            //playerName = "Player 2";
            Debug.Log("I am Player 2");
        });

        socket.On("player-1-start", () =>
        {
            //if(playerName == "Player 1")
            //{
            //    startRandom = true;
            //}
            //else
            //{
            //    startBonus = true;
            //}
        });

        socket.On("player-2-start", () =>
        {
            //if(playerName == "Player 2")
            //{
            //    startRandom = true;
            //}
            //else
            //{
            //    startBonus = true;
            //}
        });

        socket.On("player-1-win", () =>
        {
            //displayMessage = true;
            //chatToPost = "Player 1 Won!";
            //player1Win = true;
        });

        socket.On("player-2-win", () =>
        {
            //displayMessage = true;
            //chatToPost = "Player 2 Won!";
            //player2Win = true;
        });

        socket.On("room-created", data =>
        {
            //Debug.Log("Start Creating");
            ////joinLobbyMenu.SetActive(false);
            //Debug.Log("1");
            ////roomMenu.SetActive(true);
            //Debug.Log("2");
            ////roomNumber.text = "Room " + data;
            //Debug.Log("Room Done being created");
            //roomNum = data;
            //changeUI = true;
        });

        socket.On("all-ready", () =>
        {
            //allReady = true;
            Debug.Log("Everyone is ready");
            //if(playerName == "Player 1")
            //{
            //    startRandom = true;
            //    timeRunning = true;
            //}
        });

        socket.On("result-tie", () =>
        {
            //resultBox.text = "It Was A Tie.";
        });



        socket.On("room-joined", data =>
        {
            //roomNum = data;
            //changeUI = true;
        });
    }

    private void Update()
    {
        if (timeRunning && timeUntilObstacle > 0)
        {
            timeUntilObstacle -= Time.deltaTime;
            timeBox.text = timeUntilObstacle.ToString();
        }
        if(timeUntilObstacle <= 0)
        {
            if(obstacleRequiredNum > 0)
            {
                //Lose
            }
            else
            {
                randomWord.color = Color.black;
                obstaclesPassed++;
                scoreBox.text = "Score:\n" + obstaclesPassed;
                obstacleRequiredNum = 1 + obstaclesPassed;
                obstacleCountBox.text = "Words Needed:\n" + obstacleRequiredNum;
                timeUntilObstacle = 10 + savedUpTime;
                savedUpTime = 0;
            }
        }
    }

    public void StartGame()
    {
        timeRunning = true;
        timeUntilObstacle = 5;
        obstacleRequiredNum = 1;
        obstacleCountBox.text = "Words Needed:\n" + obstacleRequiredNum;
        RandomizeWord();
    }

    public void GuessWord()
    {
        if(guessWord.text == randomWord.text)
        {
            guessWord.text = "";
            obstacleRequiredNum--;
            obstacleCountBox.text = "Words Needed:\n" + obstacleRequiredNum;
            RandomizeWord();
        }
        if(obstacleRequiredNum < 0)
        {
            savedUpTime += .25f;
        }
    }

    private void RandomizeWord()
    {
        if(obstacleRequiredNum <= 0)
        {
            randomWord.color = Color.green;
        }
        else
        {
            randomWord.color = Color.black;
        }
        randomWord.text = randomWordList[Random.Range(0, randomWordList.Count)];
    }
}