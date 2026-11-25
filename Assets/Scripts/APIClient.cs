using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Cryptography;

public class APIClient : MonoBehaviour
{
    // keep only one instance of API Client
    public static string SessionCode { get; set; }
    public static APIClient Instance { get; private set; }
    private HubConnection conn;

    // list possible events that Unity should listen for
    public static event Action<UpdateClient> Test;

    public static event Action<UpdateClient> Paraglider;

    public static event Action<UpdateClient> Snowflake;

    public static event Action<UpdateClient> Candle;

    public static event Action<UpdateClient> Tree;

    public static event Action<UpdateClient> Cage;

    public static event Action<UpdateClient> Pedestal;


    public static string API_PATH;
    public static string SIGNAL_PATH;

    // Unity work on main thread
    private readonly ConcurrentQueue<Action> actions =
        new ConcurrentQueue<Action>();

    private static string GenerateSessionCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        const int length = 8;

        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
            .ToArray());
    }

    // used for creating API instance
    // called immediately due to Monobehavior and association with empty game obj
    private void Awake()
    {
        Console.WriteLine("Creating API instance.");

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SessionCode = GenerateSessionCode();
        API_PATH = "https://split-signal-server-6235.onrender.com/game/";
        SIGNAL_PATH = "https://split-signal-server-6235.onrender.com/gamehub";

        Console.WriteLine($"Starting game with session code: {SessionCode}.");

        DontDestroyOnLoad(gameObject);
    }
    

    // connect to SignalR
    private async void Start()
    {
        // initial connection
        conn = new HubConnectionBuilder()
            .WithUrl(SIGNAL_PATH)
            .WithAutomaticReconnect()
            .Build();
 
        // available actions
        conn.On<UpdateClient>("TestMessage", msg =>
        {
            Debug.Log($"SignalR message received: {msg.Object}, {msg.Status}");
            actions.Enqueue(() =>
            {
                Debug.Log("Executing queued action");
                Test?.Invoke(msg);
            });
        });

        conn.On<UpdateClient>("paraglider", msg =>
        {

            Debug.Log($"SignalR message received: {msg.Object}, {msg.Status}");
            actions.Enqueue(() =>
            {
                Debug.Log("Executing queued action");
                Paraglider?.Invoke(msg);
            });
        });

        conn.On<UpdateClient>("snowflake", msg =>
        {

            Debug.Log($"SignalR message received: {msg.Object}, {msg.Status}");
            actions.Enqueue(() =>
            {
                Debug.Log("Executing queued action");
                Snowflake?.Invoke(msg);
            });
        });

        conn.On<UpdateClient>("candle", msg =>
        {

            Debug.Log($"SignalR message received: {msg.Object}, {msg.Status}");
            actions.Enqueue(() =>
            {
                Debug.Log("Executing queued action");
                Candle?.Invoke(msg);
            });
        });

        conn.On<UpdateClient>("tree", msg =>
        {

            Debug.Log($"SignalR message received: {msg.Object}, {msg.Status}");
            actions.Enqueue(() =>
            {
                Debug.Log("Executing queued action");
                Tree?.Invoke(msg);
            });
        });

        conn.On<UpdateClient>("cage", msg =>
        {

            Debug.Log($"SignalR message received: {msg.Object}, {msg.Status}");
            actions.Enqueue(() =>
            {
                Debug.Log("Executing queued action");
                Cage?.Invoke(msg);
            });
        });

        conn.On<UpdateClient>("pedestal", msg =>
        {

            Debug.Log($"SignalR message received: {msg.Object}, {msg.Status}");
            actions.Enqueue(() =>
            {
                Debug.Log("Executing queued action");
                Pedestal?.Invoke(msg);
            });
        });

        // open persistent conn
        await conn.StartAsync();

        await conn.InvokeAsync(
            "CreateSession",
            SessionCode,
            "Unity"
        );

        Debug.Log($"Connected to SignalR with {SessionCode}");
    }

    // update the status of an object in the DB
    public static void UpdateReq(string Object, string Status, string Notify)
    {
        UpdateRequest req = new UpdateRequest
        {
            SessionCode = SessionCode, 
            Object = Object, 
            Status = Status, 
            Notify = Notify
        };

        Instance.StartCoroutine(Instance.UpdateRequest(req));
    }

    // check the status of an object in the DB
    public static void CheckReq(string Object)
    {
        CheckRequest req = new CheckRequest
        {
            SessionCode = SessionCode,
            Object = Object
        };

        Instance.StartCoroutine(Instance.CheckRequest(req));
    }

    public static string GetSessionCode()
    {
        return SessionCode;
    }

    private void Update()
    {
        while (actions.TryDequeue(out Action action))
        {
            action.Invoke();
        }
    }

    IEnumerator UpdateRequest(UpdateRequest req)
    {

        string json = JsonUtility.ToJson(req);

        Debug.Log(req.SessionCode);
        Debug.Log(req.Object);
        Debug.Log(req.Status);
        Debug.Log(req.Notify);

        Debug.Log(json);

        using (UnityWebRequest www = UnityWebRequest.Post(API_PATH + "Update", json, "application/json"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
    }

    IEnumerator CheckRequest(CheckRequest req)
    {
        string json = JsonUtility.ToJson(req);

        Debug.Log(req.SessionCode);
        Debug.Log(req.Object);

        Debug.Log(json);

        using (UnityWebRequest www = UnityWebRequest.Post(API_PATH + "Check", json, "application/json"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
    }
}
