const express = require('express');
const app = express();
const http = require('http');
const server = http.createServer(app);
const io = require('socket.io')(server);
let room = 0;
let player1Rdy = 0;
let player2Rdy = 0;

app.get('/', (req, res) => {
  res.sendFile(__dirname + '/index.html');
});

const {userConnected, connectedUsers, initializeChoices, moves, makeMove, choices} = require("./util/users");
const {createRoom, joinRoom, exitRoom, rooms} = require("./util/rooms");


io.on('connection', (socket) => {
  console.log('a user connected');
  socket.on("create-room", (roomId) =>{
    if(rooms[roomId]){
	const error = "This room already exists";
	socket.emit("display-error", error);
    }else{
	userConnected(socket.client.id);
	createRoom(roomId, socket.client.id);
	socket.emit("room-created", roomId);
	socket.emit("player-1-connected");
	socket.join(roomId);
	room = roomId;
	console.log('room created ', roomId);
    }
  })

  socket.on("join-room", roomId => {
     console.log('Joining room ', roomId);
     if(!rooms[roomId]){
	const error = "This room doesn't exist";
	socket.emit("display-error", error);
     }else{
	userConnected(socket.client.id);
	joinRoom(roomId, socket.client.id);
	socket.emit("room-joined", roomId);
	socket.emit("player-2-connected");
	socket.join(roomId);
	room = roomId;
     }
  })

  socket.on("correct-answer", correctName => {
    if(correctName == "Player 1"){
	io.to(room).emit("player-2-start");
    }else{
	io.to(room).emit("player-1-start");
    }
  })

  socket.on("lose", loserName => {
    if(loserName == "Player 1"){
	io.to(room).emit("player-2-win");
    }else{
	io.to(room).emit("player-1-win");
    }
  })

  socket.on("player-1-ready", readyNum => {
    console.log('Player 1 readied up with number ', readyNum);
    player1Rdy = readyNum;
    if(player2Rdy != 0){
	io.to(room).emit("all-ready");
	player1Rdy = 0;
	player2Rdy = 0;
    }else{
	io.to(room).emit('chat message', "Waiting on Player 2");
    }  
  })

  socket.on("player-2-ready", readyNum => {
    console.log('Player 2 readied up with number ', readyNum);
    player2Rdy = readyNum;
    if(player1Rdy != 0){
	io.to(room).emit("all-ready");
	player1Rdy = 0;
	player2Rdy = 0;
    }else{
	io.to(room).emit('chat message', "Waiting on Player 1");
    }  
  })

  socket.on("player-1-choice", choiceNum => {
    player1Rdy = choiceNum;
    if(player2Rdy != 0){
	if(player1Rdy == player2Rdy){
	  io.to(room).emit('chat message', "It was a tie.");
	  io.to(room).emit('result-tie');  
	}else{
	  if(player1Rdy == 1 && player2Rdy == 3){
	    io.to(room).emit('player-1-win');
	  }else if(player1Rdy == 2 && player2Rdy == 1){
	    io.to(room).emit('player-1-win');
	   }else if(player1Rdy == 3 && player2Rdy == 2){
	    io.to(room).emit('player-1-win');
	   }else{
	    io.to(room).emit('player-2-win');
	   }
	}
	player1Rdy = 0;
	player2Rdy = 0;
    }else{
	io.to(room).emit('chat message', "Waiting on Player 2");
    }  
  })

socket.on("player-2-choice", choiceNum => {
    console.log('Player 2 readied up with number ', choiceNum);
    player2Rdy = choiceNum;
    if(player1Rdy != 0){
	if(player1Rdy == player2Rdy){
	  io.to(room).emit('chat message', "It was a tie.");  
	}else{
	  if(player1Rdy == 1 && player2Rdy == 3){
	    io.to(room).emit('player-1-win');
	  }else if(player1Rdy == 2 && player2Rdy == 1){
	    io.to(room).emit('player-1-win');
	   }else if(player1Rdy == 3 && player2Rdy == 2){
	    io.to(room).emit('player-1-win');
	   }else{
	    io.to(room).emit('player-2-win');
	   }
	}
	player1Rdy = 0;
	player2Rdy = 0;
    }else{
	io.to(room).emit('chat message', "Waiting on Player 1");
    }  
  })

  socket.on('disconnect', () => {
    console.log('user disconnected');
  });
  socket.on('chat message', (msg) => {
    io.emit('chat message', msg);
  });
});

server.listen(process.env.PORT || 3000, () => {
  console.log('listening on *:80');
});