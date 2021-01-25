using Godot;
using System;

public class GameSettings : Node
{
    private static readonly string path = "user://settings.cfg";

    public event Action SettingsChangedEvent;

    public string ClientHost
    {
        get => clientHost;
        set
        {
            clientHost = value;
            config?.SetValue("network", "client_host", clientHost);
            Save();
        }
    }
    string clientHost = "127.0.0.1";

    public int ClientPort
    {
        get => clientPort;
        set
        {
            clientPort = value;
            config?.SetValue("network", "client_port", clientPort);
            Save();
        }
    }
    int clientPort = 3000;

    public int ServerPort
    {
        get => serverPort;
        set
        {
            serverPort = value;
            config?.SetValue("network", "server_port", serverPort);
            Save();
        }
    }
    int serverPort = 3000;

    ConfigFile config = new ConfigFile();

    /// <summary>
    /// GameSettings is a singleton
    /// </summary>
    private static GameSettings instance;
    public static GameSettings Instance
    {
        get
        {
            return instance;
        }
    }

    GameSettings()
    {
        instance = this;
    }

    public override void _Ready()
    {
        var err = config.Load("user://settings.cfg");
        if (err == Error.Ok)
        {
            clientPort = int.Parse(config.GetValue("network", "client_port", clientPort).ToString());
            clientHost = config.GetValue("network", "client_host", clientHost).ToString();
            serverPort = int.Parse(config.GetValue("network", "server_port", serverPort).ToString());
            Save();
        }
    }

    void Save()
    {
        config.Save(path);
        SettingsChangedEvent?.Invoke();
    }

}
