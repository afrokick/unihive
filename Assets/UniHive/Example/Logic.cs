using UnityEngine;
using UnityEngine.UI;
using UHive;

public class Logic : MonoBehaviour
{
    public Text _status, _threadsText;

    public string Site;
    public string User;
    public int Threads = 0;

    void Awake()
    {
        if (Threads == 0)
            Threads = SystemInfo.processorCount;
        
        UniHive.ConnectionAuthorized += UniHive_ConnectionAuthorized;
        UniHive.JobReceived += UniHive_JobReceived;
        UniHive.HashFound += UniHive_HashFound;
        UniHive.HashAccepted += UniHive_HashAccepted;
        UniHive.Initialize(User, Site, 0f, Threads);

        _threadsText.text = "Threads: " + Threads;
    }

    void OnApplicationQuit()
    {
        UniHive.Stop();
    }

    void UniHive_HashAccepted()
    {
        Debug.Log("Accepted: " + UniHive.AcceptedHashes + ", throttle:" + UniHive.Throttle);
    }

    void UniHive_HashFound()
    {
        Debug.Log("Found. total:" + UniHive.TotalHashes + ", perSec:");
        Debug.Log(UniHive.HashesPerSecond);
        Debug.Log(UniHive.Throttle);

        UpdateStatus();
    }

    void UniHive_JobReceived()
    {
        Debug.Log("New job...");
    }

    void UniHive_ConnectionAuthorized()
    {
        Debug.Log("Ok, I'm ready!");

        UpdateStatus();
    }

    public void OnStartClicked()
    {
        UniHive.Start();

        UpdateStatus();
    }

    public void OnStopClicked()
    {
        UniHive.Stop();

        UpdateStatus();
    }

    private void UpdateStatus()
    {
        string str = "";

        bool isRunning = UniHive.IsRunning;

        if (!isRunning)
        {
            str = "Stopped";
            _status.text = str;
            return;
        }
            
        str = "Mining...\n";
        str += "Hashes:" + UniHive.AcceptedHashes + "\n";
        str += "PerSec:" + UniHive.HashesPerSecond + "h/s";

        _status.text = str;
    }
}