const io = require('socket.io')(3000);

io.on('connection', socket => {
  // either with send()
  console.log('a user connected');
  socket.on('chat', (msg) => {
    io.emit('chat', msg);
  });

});