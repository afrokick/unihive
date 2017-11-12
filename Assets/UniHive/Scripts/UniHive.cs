#pragma warning disable CS0162
using UnityEngine;
using System;

namespace UHive
{
    public class UniHive
    {
        /// <summary>
        /// Change to 'true' for debugging
        /// </summ ary> 
        const bool IsDebugMode = false;

        /// <summary>
        /// Gets the plugin ready
        /// </summary>
        /// <value>True if plugin ready.< /valu e> 
        public static bool Initialized { get; private set; }

        /// <summary>
        /// Gets the userName parameter.
        /// </summary>
        /// <value>userName</value>
        public static string UserName { get; private set; }

        /// <summary>
        /// Gets the siteKey parameter.
        /// </summary>
        /// <value>siteKey</value>
        public static string SiteKey { get; private set; }

        /// <summary>
        /// Gets the throttle.
        /// </summary>
        /// <value>throttle</value>
        public static float Throttle { get; private set; }

        /// <summary>
        /// Gets the threads count.
        /// </summary>
        /// <value>The threads count.</value>
        public static int Threads { get { return UniHiveNative.GetNumThreads(); } }

        /// <summary>
        /// Gets a value indicating is running.
        /// </summary>
        /// <value><c>true</c> if is running; otherwise, <c>false</c>.</value>
        public static bool IsRunning { get; private set; }

        /// <summary>
        /// Gets a value indicating hashes per second.
        /// </summary>
        /// <value>hashes per second</value>
        public static double HashesPerSecond { get { return UniHiveNative.GetHashesPerSecond(); } }

        /// <summary>
        /// Gets the total hashes.
        /// </summary>
        /// <value>The total hashes.</value>
        public static int TotalHashes { get { return UniHiveNative.GetTotalHashes(); } }

        /// <summary>
        /// Gets the accepted hashes.
        /// </summary>
        /// <value>The accepted hashes.</value>
        public static int AcceptedHashes { get { return UniHiveNative.GetAcceptedHashes(); } }

        /// <summary>
        /// Occurs when error occurred.
        /// </summary>
        public static event Action<string> ErrorOccurred;

        /// <summary>
        /// Occurs when the connection to the mining pool was opened.
        /// </summary>
        public static event Action ConnectionOpened;

        /// <summary>
        /// Occurs when the miner successfully authed with the mining pool and the siteKey was verified.
        /// </summary>
        public static event Action ConnectionAuthorized;

        /// <summary>
        /// Occurs when the connection to the pool was closed or miner stop.
        /// </summary>
        public static event Action ConnectionClosed;

        /// <summary>
        /// Occurs when job received.
        /// </summary>
        public static event Action JobReceived;

        /// <summary>
        /// Occurs when a hash meeting the pool's difficulty (currently 256) was found and will be send to the pool.
        /// </summary>
        public static event Action HashFound;

        /// <summary>
        /// Occurs when a hash that was sent to the pool was accepted.
        /// </summary>
        public static event Action HashAccepted;

        /// <summary>
        /// Initialize the specified siteKey, throttle and threads.
        /// </summary>
        /// <param name="siteKey">siteKey from https://coinhive.com/settings/sites</param>
        /// <param name="throttle">Throttle. Default: 0</param>
        /// <param name="threads">Threads count. Default: all</param>
        public static void Initialize(string siteKey, float throttle = 0, int threads = 0)
        {
            Initialize(null, siteKey, throttle, threads);
        }

        /// <summary>
        /// Initialize the specified userName, siteKey, throttle and threads.
        /// </summary>
        /// <param name="userName">User name. Any string</param>
        /// <param name="siteKey">siteKey from https://coinhive.com/settings/sites</param>
        /// <param name="throttle">Throttle. Default: 0</param>
        /// <param name="threads">Threads. Default: all</param>
        public static void Initialize(string userName, string siteKey, float throttle = 0, int threads = 0)
        {
            if (Initialized)
                return;
            
            UserName = userName;
            SiteKey = siteKey;
            Throttle = throttle;

            Prepare();

            UniHiveNative.CreateMiner(userName, siteKey, throttle, threads);

            Initialized = true;
        }

        private static void Prepare()
        {
            GameObject go = new GameObject("UniHiveManager");
            GameObject.DontDestroyOnLoad(go);
            go.AddComponent<UniHiveNative>();

            UniHiveNative.ErrorOccurred += OnErrorOccurred;
            UniHiveNative.ConnectionOpened += OnConnectionOpened;
            UniHiveNative.ConnectionAuthorized += OnAuthed;
            UniHiveNative.ConnectionClosed += OnConnectionClosed;
            UniHiveNative.JobReceived += OnNewJobReceived;
            UniHiveNative.HashFound += OnHashFound;
            UniHiveNative.HashAccepted += OnHashAccepted;
        }

        /// <summary>
        /// Start the miner
        /// </summary>
        public static void Start()
        {
            if (!Initialized)
            {
                PrintError("You should call UniHive.Initialize(...) first");
                return;
            }

            if (IsRunning)
                return;
            
            UniHiveNative.Start();

            IsRunning = true;
        }

        /// <summary>
        /// Stop the miner
        /// </summary>
        public static void Stop()
        {
            if (!Initialized)
            {
                PrintError("You should call UniHive.Initialize(...) first");
                return;
            }

            if (!IsRunning)
                return;

            UniHiveNative.Stop();

            IsRunning = false;
        }

        #region Event handlers

        private static void OnErrorOccurred(string message)
        {
            if (ErrorOccurred != null)
                ErrorOccurred(message);
        }

        private static void OnConnectionOpened()
        {
            if (ConnectionOpened != null)
                ConnectionOpened();
        }

        private static void OnAuthed()
        {
            if (ConnectionAuthorized != null)
                ConnectionAuthorized();
        }

        private static void OnConnectionClosed()
        {
            if (ConnectionClosed != null)
                ConnectionClosed();
        }

        private static void OnNewJobReceived()
        {
            if (JobReceived != null)
                JobReceived();
        }

        private static void OnHashFound()
        {
            if (HashFound != null)
                HashFound();
        }

        private static void OnHashAccepted()
        {
            if (HashAccepted != null)
                HashAccepted();
        }

        #endregion

        private static void PrintMessage(string message)
        {
            if (!IsDebugMode)
                return;

            Debug.Log("UniHive: " + message);
        }

        private static void PrintError(string message)
        {
            if (!IsDebugMode)
                return;
            
            Debug.LogError("UniHive: " + message);
        }
    }
}