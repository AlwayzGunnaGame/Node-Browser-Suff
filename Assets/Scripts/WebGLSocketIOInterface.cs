using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Socket.Quobject.SocketIoClientDotNet.Client;

public class WebGLSocketIOInterface : MonoBehaviour
{
    public static WebGLSocketIOInterface instance;
    public QSocket sock;
    public Dictionary<string, Action> callbacks = new Dictionary<string, Action>();
    public Dictionary<string, Action<string>> callbacksWithString = new Dictionary<string, Action<string>>();

    [DllImport("__Internal")]
    private static extern void SendSocketIO(string type, string message);

    [DllImport("__Internal")]
    private static extern void DisconnectSocketIO();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

#if !UNITY_WEBGL || UNITY_EDITOR
        sock = IO.Socket("https://mobile-party-time.herokuapp.com");
        //sock = IO.Socket("ws://localhost:3000");

        sock.On("d", (data) => {
            Debug.Log(data);
            instance.Receive(data.ToString());
        });
#endif
    }

    public class SocketIOMessage
    {
        public string t;
        public string d;
    }

    // from socket io on page
    public void Receive(string value)
    {
        var val = JsonConvert.DeserializeObject<SocketIOMessage>(value);
        if (val == null)
            throw new Exception("Bad Socket IO Message: " + value);
        Debug.Log(val.t);
        Debug.Log(val.d);
        if (val.d != null)
        {
            // invoke callback if income message value exists
            if (callbacksWithString.ContainsKey(val.t))
                callbacksWithString[val.t](val.d);
            else
                Debug.Log(val.t + " doesn't exist in callbacksWithString");
        }
        else
        {
            // invoke callback without string if it exists
            if (callbacks.ContainsKey(val.t))
                callbacks[val.t]();
            else
                Debug.Log(val.t + " doesn't exist in callbacks");
        }
    }

    public void Emit(string eventString, object args)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        sock.Emit(eventString, args);
#else
        // send a string along to socket io javascript on page
        SendSocketIO(eventString, JsonConvert.SerializeObject(args));
#endif
    }

    public void Disconnect()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        sock.Disconnect();
#else
        // disconnect
        DisconnectSocketIO();
#endif
    }

    public void On(string eventString, Action fn)
    {
        //Debug.Log("Register Event: " + eventString);
        callbacks[eventString] = fn;
    }

    public void On(string eventString, Action<string> fn)
    {
        //Debug.Log("Register Event with string: " + eventString);
        callbacksWithString[eventString] = fn;
    }
}
