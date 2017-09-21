using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class Logic : MonoBehaviour {
    [DllImport("__Internal")]
    private static extern void CreateMiner(string key);

    [DllImport("__Internal")]
    private static extern void StartMiner();

    [DllImport("__Internal")]
    private static extern void StopMiner();

    public Text Label;

    private int _hashes = 0;
    private double _rate = 0;
    private bool _started = false;

    private const string Key = "KqNONX9oxfVraMCkuewW6651VAweDTie";

	// Use this for initialization
	void Start () {
        CreateMiner(Key);
	}

    public void OnMineClick(){
        _started = !_started;

        if (_started)
            StartMiner();
        else
            StopMiner();

        UpdateLabel();
    }

    public void OnHashAccepted(double hashesPerSecond){
        _hashes++;
        _rate = hashesPerSecond;

        UpdateLabel();
    }

    private void UpdateLabel(){
        Label.text = "Hashes: " + _hashes;

        Label.text += "\nStatus: " + (_started ? "Mining" :"Stopped");
        Label.text += "\nRate: " + _rate + "h/s";
    }
}