mergeInto(LibraryManager.library, {

  SendSocketIO: function(type, message) {
    var t = Pointer_stringify(type);
    var m = Pointer_stringify(message);
    console.log("SendSocketIO -- type: " + t + ", message: " + m);
    socket.emit(t, m);
  },

  DisconnectSocketIO: function() {
    console.log("DisconnectSocketIO");
    socket.disconnect();
  }

});