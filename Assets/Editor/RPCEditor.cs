//Copyright FutureStyleGames 2020

using UnityEngine;
using UnityEditor;
using Discord;
using System;
using UnityEngine.SceneManagement;

public class RPCEditor : EditorWindow
{
    private static RPCEditor instance = null;

    private bool running = false;
    private Discord.Discord discord;

    private const string PLAYER_PREFS_KEY = "$DISCORD_RICH_PRESENCE_SHOULD_LOG_MESSAGES_INT$";
    private bool log = true;

    private bool showMore = false;

    private void Awake()
    {
        if (PlayerPrefs.HasKey(PLAYER_PREFS_KEY))
        {
            if (PlayerPrefs.GetInt(PLAYER_PREFS_KEY) == 0)
                log = false;
            else
                log = true;
        }
        else
        {
            PlayerPrefs.SetInt(PLAYER_PREFS_KEY, 1);
            log = true;
        }

        GetWindow<RPCEditor>("Discord RPC");

        InitializeDiscordRPC();
    }

    [MenuItem("Window/Discord RPC")]
    public static void ShowWindow()
    {
        GetWindow<RPCEditor>("Discord RPC");
    }

    void OnGUI()
    {
        GUI.skin.label.wordWrap = true;
        GUILayout.Space(4);
        GUILayout.Label("Discord Rich Presence Plugin for Unity");

        GUILayout.Space(20);

        GUILayout.Label("It will automatically initialize when you start the editor.");
        GUILayout.Label("Do not close this window otherwise it won't work correctly!", EditorStyles.boldLabel);

        GUILayout.Space(20);

        GUILayout.Label("If Discord RPC is disabled, click the button below to initialize it manually.");
        if(GUILayout.Button("START DISCORD RPC", GUILayout.Height(50)))
        {
            InitializeDiscordRPC();
        }

        GUILayout.Space(20);
        GUILayout.Label("When you close the editor, Discord RPC will automatically close. Click the button below to stop it manually.");
        if(GUILayout.Button("STOP DISCORD RPC", GUILayout.Height(50)))
        {
            StopRPC();
        }

        GUILayout.Space(20);
        if (running)
        {
            GUILayout.Label("Discord RPC is currently running.", EditorStyles.boldLabel);
        }
        else
        {
            GUILayout.Label("Discord RPC is disabled.", EditorStyles.boldLabel);
        }

        GUILayout.Space(50);

        if (!showMore)
        {
            if (GUILayout.Button("SHOW MORE", GUILayout.Height(30)))
            {
                showMore = true;
            }
        }

        if (showMore)
        {
            if (log)
            {
                GUILayout.Label("Message logging is on.");
                if (GUILayout.Button("TURN OFF", GUILayout.Height(30)))
                {
                    PlayerPrefs.SetInt(PLAYER_PREFS_KEY, 0);
                    log = false;
                }
            }
            else
            {
                GUILayout.Label("Message logging is off.");
                if (GUILayout.Button("TURN ON", GUILayout.Height(30)))
                {
                    PlayerPrefs.SetInt(PLAYER_PREFS_KEY, 1);
                    log = true;
                    Debug.Log("Turned on message logging.");
                }
            }

            GUILayout.Space(20);

            GUILayout.Label($"NOTE: If you're using Player Prefs, don't name your key '{PLAYER_PREFS_KEY}' as it will conflict with the plugin.");
            GUILayout.Space(20);
            if(GUILayout.Button("SHOW LESS", GUILayout.Height(30)))
            {
                showMore = false;
            }
        }
    }

    private void InitializeDiscordRPC()
    {
        if (running)
        {
            if (log)
            {
                Debug.LogError("Discord RPC is already running!");
                return;
            }
        }

        if (log)
        {
            Debug.Log("Initializing Discord RPC..."); 
        }

        string projectName = Application.productName;
        string unityVersion = Application.unityVersion;
        string sceneName = SceneManager.GetActiveScene().name;
        long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        discord = new Discord.Discord(Int64.Parse("659760066173599744"), (UInt64)Discord.CreateFlags.NoRequireDiscord);
        var activityManager = discord.GetActivityManager();

        var activity = new Discord.Activity
        {
            Name = "Unity Editor",
            State = "Developing " + projectName,
            Details = sceneName,
            Timestamps =
            {
                Start = unixTime
            },
            Assets =
            {
                LargeImage = "large",
                LargeText = "Unity Editor",
                SmallImage = "small",
                SmallText = unityVersion
            }
        };

        activityManager.UpdateActivity(activity, result =>
        {
            if (result == Result.Ok)
            {
                if (log)
                {
                    Debug.Log("Successfully initialized Discord RPC."); 
                }
            }
            else
            {
                if (log)
                {
                    Debug.LogError("Failed to initialize Discord RPC!"); 
                }
                return;
            }
        });

        running = true;
    }

    private void Update()
    {
        if (running)
        {
            discord.RunCallbacks();
        }
    }

    private void StopRPC()
    {
        if (running)
        {
            running = false;
            discord.Dispose();
            if (log)
            {
                Debug.Log("Stopped Discord RPC."); 
            }
        }
        else
        {
            if (log)
            {
                Debug.LogError("Discord RPC is already disabled."); 
            }
        }
    }

    private void OnDestroy()
    {
        if (running)
        {
            discord.Dispose(); 
        }
    }
}
