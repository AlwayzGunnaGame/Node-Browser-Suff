using Socket.Quobject.SocketIoClientDotNet.Client;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Feedbacks;
using System;
using System.IO;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Linq;

public class TestObject : MonoBehaviour
{
    public List<string> randomWordList;
    private StreamReader stream;
    private QSocket socket;
    public InputField field;
    public InputField chatTextField;
    public InputField wordGuess;
    public Text randomWord;
    public Text chatBox;
    public Text roomNumber;
    public Text resultBox;
    public Text countdown;
    public Text scoreText;
    public float countdownNum = 10;
    private string playerName = "";
    public GameObject joinLobbyMenu;
    public GameObject roomMenu;
    private int myScore;
    private bool changeUI;
    private bool displayMessage;
    private bool allReady;
    private bool allChoices;
    private bool player1Win;
    private bool player2Win;
    private object roomNum;
    private string chatToPost;
    private bool startRandom;
    private bool lostAlready;
    private bool timeRunning;
    private bool startBonus;
    //public string[] randomWords;
    public GameObject readyButton;
    public GameObject gameChoices;
    public MMFeedback feedback;
    public GameObject winScreen;
    public GameObject loseScreen;
    

    void Start()
    {
        //using (stream = new StreamReader("EveryWord.txt"))
        //{
        //    string line;
        //    while((line = stream.ReadLine()) != null)
        //    {
        //        randomWordList.Add(line);
        //    }
        //} //string txt = mytxtData.text;
        TextAsset mytxtData = (TextAsset)Resources.Load("EveryWord");
        string[] stringSeparators = new string[] { "\r\n" };
        randomWordList = mytxtData.text.Split(stringSeparators, StringSplitOptions.None).ToList();

        Debug.Log("start");
        //socket = IO.Socket("https://mobile-party-time.herokuapp.com");
        socket = IO.Socket("ws://localhost:3000");

        socket.On(QSocket.EVENT_CONNECT, () => {
            Debug.Log("Connected");
            //socket.Emit("chat message", "test");
        });

        socket.On("chat message", data => {
            Debug.Log(data);
            //chatBox.text = chatBox.text + "\n" + data;
            displayMessage = true;
            chatToPost = data.ToString();
        });

        socket.On("display-error", data => {
            Debug.Log("Error Occurred: " + data);
        });
        
        socket.On("player-1-connected", () =>
        {
            playerName = "Player 1";
            Debug.Log("I am Player 1");
        });
        
        socket.On("player-connected", data =>
        {
            if(playerName == "")
            playerName = "Player " + data;
            Debug.Log("I am Player " + data);
        });
        
        socket.On("player-1-start", () =>
        {
            if(playerName == "Player 1")
            {
                startRandom = true;
            }
            else
            {
                startBonus = true;
            }
        });

        socket.On("player-2-start", () =>
        {
            if(playerName == "Player 2")
            {
                startRandom = true;
            }
            else
            {
                startBonus = true;
            }
        });

        socket.On("player-1-win", () =>
        {
            displayMessage = true;
            chatToPost = "Player 1 Won!";
            player1Win = true;
        });

        socket.On("player-2-win", () =>
        {
            displayMessage = true;
            chatToPost = "Player 2 Won!";
            player2Win = true;
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
            roomNum = data;
            changeUI = true;
        });

        socket.On("all-ready", () =>
        {
            allReady = true;
            Debug.Log("Everyone is ready");
            if(playerName == "Player 1")
            {
                startRandom = true;
                timeRunning = true;
            }
        });

        socket.On("result-tie", () =>
        {
            resultBox.text = "It Was A Tie.";
        });



        socket.On("room-joined", data =>
        {
            roomNum = data;
            changeUI = true;
        });
    }

    private void Update()
    {
        if (timeRunning)
        {
            if (countdownNum > 0)
            {
                countdownNum -= Time.deltaTime;
            }
            else
            {
                countdownNum = 0;
                socket.Emit("lose", playerName);
                timeRunning = false;
            }
            countdown.text = countdownNum.ToString();
        }
        if (changeUI)
        {
            ChangeUI();
        }
        if (displayMessage)
        {
            DisplayMessage();
        }
        if (player1Win)
        {
            Player1Win();
        }
        if (player2Win)
        {
            Player2Win();
        }
        if (allReady)
        {
            AllReady();
        }
        if (startRandom)
        {
            RandomizeWord();
        }
        if (startBonus)
        {
            RandomizeBonus();
        }
    }

    private void OnDestroy()
    {
        socket.Disconnect();
    }

    public void DeliverMessage()
    {
        if (chatTextField.text.Length > 0)
        {
            socket.Emit("chat message", (playerName + ": " + chatTextField.text));
        }
    }

    public void CreateRoom()
    {
        socket.Emit("create-room", field.text);
    }

    public void JoinRoom()
    {
        socket.Emit("join-room", field.text);
    }

    private void ChangeUI()
    {
        changeUI = false;
        joinLobbyMenu.SetActive(false);
        roomMenu.SetActive(true);
        roomNumber.text = "Room: " + roomNum.ToString();
    }

    private void DisplayMessage()
    {
        displayMessage = false;
        chatBox.text = chatBox.text + "\n" + chatToPost;
    }

    private void AllReady()
    {
        allReady = false;
        readyButton.SetActive(false);
        gameChoices.SetActive(true);
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
        countdownNum = 10;
        countdown.text = countdownNum.ToString();
        myScore = 0;
        scoreText.text = ("Score:\n" + myScore);
    }

    public void ReadyUp()
    {
        if(playerName == "Player 1")
        {
            socket.Emit("player-1-ready", 1);
        }
        else
        {
            socket.Emit("player-2-ready", 1);
        }
    }

    public void MakeChoice(int choice)
    {
        if(playerName == "Player 1")
        {
            socket.Emit("player-1-choice", choice);
        }
        else
        {
            socket.Emit("player-2-choice", choice);
        }
    }

    public void ClearText()
    {
        chatTextField.text = "";
    }

    public void GuessWord()
    {
        if(wordGuess.text.Length > 0)
        {
            if(wordGuess.text == randomWord.text)
            {
                if (timeRunning)
                {
                    wordGuess.text = "";
                    randomWord.text = "";
                    timeRunning = false;
                    myScore++;
                    scoreText.text = ("Score:\n" + myScore);
                    socket.Emit("correct-answer", playerName);
                }
                else
                {
                    countdownNum++;
                    countdown.text = countdownNum.ToString();
                    RandomizeBonus();
                    wordGuess.text = "";
                }
            }
        }
    }

    private void WrongAnswer()
    {
        resultBox.color = Color.red;
        resultBox.text = "Try Again";
        feedback.Play(feedback.transform.position);
    }

    private void Player1Win()
    {
        player1Win = false;
        if (playerName == "Player 1")
        {
            //resultBox.color = Color.green;
            //resultBox.text = "You Win!";
            winScreen.SetActive(true);
        }
        else
        {
            //resultBox.color = Color.red;
            //resultBox.text = "You Lose.";
            loseScreen.SetActive(true);
        }
        randomWord.text = "";
        gameChoices.SetActive(false);
        resultBox.text = "";
        readyButton.SetActive(true);
    }
    private void Player2Win()
    {
        player2Win = false;
        if (playerName == "Player 2")
        {
            //resultBox.color = Color.green;
            //resultBox.text = "You Win!";
            winScreen.SetActive(true);
        }
        else
        {
            //resultBox.color = Color.red;
            //resultBox.text = "You Lose.";
            loseScreen.SetActive(true);
        }
        randomWord.text = "";
        gameChoices.SetActive(false);
        resultBox.text = "";
        readyButton.SetActive(true);
    }

    private void RandomizeWord()
    {
        startRandom = false;
        randomWord.color = Color.black;
        randomWord.text = randomWordList[Random.Range(0, randomWordList.Count)];
        timeRunning = true;
        wordGuess.text = "";
    }

    private void RandomizeBonus()
    {
        startBonus = false;
        randomWord.color = Color.green;
        randomWord.text = randomWordList[Random.Range(0, randomWordList.Count)];
    }
}