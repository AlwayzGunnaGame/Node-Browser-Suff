const express = require('express');
const app = express();
app.use(express.static(__dirname));
const http = require('http');
const server = http.createServer(app);
const io = require('socket.io')(server,{
    //path: "/socket.io",
    //pingInterval: 10 * 1000,
    //pingTimeout: 5000,
    //transports: ["websocket"],
  });
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
	socket.emit("d", {t:"display-error", d:error});
    }else{
	userConnected(socket.client.id);
	createRoom(roomId, socket.client.id);
	socket.roomId = roomId;
	socket.emit("d", {t:"room-created", d:roomId});
	socket.emit("d", {t:"player-1-connected"});
	socket.join(roomId);
	room = roomId;
	console.log('room created ', roomId);
    }
  })

  socket.on("set-name", nickname => {
    socket.username = nickname;
    console.log('Welcome ', socket.username);
  })

  socket.on("join-room", roomId => {
     console.log('Joining room ', roomId);
     if(!rooms[roomId]){
	const error = "This room doesn't exist";
	socket.emit("d", {t:"display-error", d:error});
     }else{
	userConnected(socket.client.id);
	joinRoom(roomId, socket.client.id);
	socket.emit("d", {t:"room-joined", d:roomId});
	socket.emit("d", {t:"player-2-connected"});
	socket.join(roomId);
	room = roomId;
     }
  })

  socket.on("correct-answer", correctName => {
    if(correctName == "Player 1"){
	io.to(room).emit("d", {t:"player-2-start"});
    }else{
	io.to(room).emit("d", {t:"player-1-start"});
    }
  })

  socket.on("lose", loserName => {
    if(loserName == "Player 1"){
	io.to(room).emit("d", {t:"player-2-win"});
    }else{
	io.to(room).emit("d", {t:"player-1-win"});
    }
  })

  socket.on("player-1-ready", readyNum => {
    console.log('Player 1 readied up with number ', readyNum);
    player1Rdy = readyNum;
    if(player2Rdy != 0){
	io.to(room).emit("d", {t:"all-ready"});
	player1Rdy = 0;
	player2Rdy = 0;
    }else{
	io.to(room).emit("d", {t:'chat message', d:"Waiting on Player 2"});
    }  
  })

  socket.on("player-2-ready", readyNum => {
    console.log('Player 2 readied up with number ', readyNum);
    player2Rdy = readyNum;
    if(player1Rdy != 0){
	io.to(room).emit("d", {t:"all-ready"});
	player1Rdy = 0;
	player2Rdy = 0;
    }else{
	io.to(room).emit("d", {t:'chat message', d:"Waiting on Player 1"});
    }  
  })

  socket.on("player-1-choice", choiceNum => {
    player1Rdy = choiceNum;
    if(player2Rdy != 0){
	if(player1Rdy == player2Rdy){
	  io.to(room).emit("d", {t:'chat message', d:"It was a tie."});
	  io.to(room).emit("d", {t:'result-tie'});  
	}else{
	  if(player1Rdy == 1 && player2Rdy == 3){
	    io.to(room).emit("d", {t:'player-1-win'});
	  }else if(player1Rdy == 2 && player2Rdy == 1){
	    io.to(room).emit("d", {t:'player-1-win'});
	   }else if(player1Rdy == 3 && player2Rdy == 2){
	    io.to(room).emit("d", {t:'player-1-win'});
	   }else{
	    io.to(room).emit("d", {t:'player-2-win'});
	   }
	}
	player1Rdy = 0;
	player2Rdy = 0;
    }else{
	io.to(room).emit("d", {t:'chat message', d:"Waiting on Player 2"});
    }  
  })

socket.on("player-2-choice", choiceNum => {
    console.log('Player 2 readied up with number ', choiceNum);
    player2Rdy = choiceNum;
    if(player1Rdy != 0){
	if(player1Rdy == player2Rdy){
	  io.to(room).emit("d", {t:'chat message', d:"It was a tie."});  
	}else{
	  if(player1Rdy == 1 && player2Rdy == 3){
	    io.to(room).emit("d", {t:'player-1-win'});
	  }else if(player1Rdy == 2 && player2Rdy == 1){
	    io.to(room).emit("d", {t:'player-1-win'});
	   }else if(player1Rdy == 3 && player2Rdy == 2){
	    io.to(room).emit("d", {t:'player-1-win'});
	   }else{
	    io.to(room).emit("d", {t:'player-2-win'});
	   }
	}
	player1Rdy = 0;
	player2Rdy = 0;
    }else{
	io.to(room).emit("d", {t:'chat message', d:"Waiting on Player 1"});
    }  
  })

  socket.on('disconnect', () => {
    console.log(socket.username, ' disconnected');
  });
  socket.on('chat message', (msg) => {
    if(room == 0){
      io.emit("d", {t:'chat message', d:msg});
    }else{
      io.to(room).emit("d", {t:'chat message', d:msg});
    }
  });
});

server.listen(process.env.PORT || 3000, () => {
  console.log('listening on *:80');
  console.log(process.env.PORT);
});