using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace UHive
{
    internal class UniHiveNative : MonoBehaviour
    {
        enum Events
        {
            Opened,
            Authed,
            Closed,
            NewJob,
            HashFound,
            HashAccepted,
            Error,
            #if !UNITY_WEBGL
            Verified,
            HashCalculated
            #endif
        }

        #if !UNITY_WEBGL
        private static WS _ws;
        private static int _acceptedHashes = 0;

        private delegate void ErrorCallback(string error);
        private delegate void HashCalculatedCallback(string result, string nonce);
        private delegate void VerifyCallback(string result);

        #if UNITY_IOS
        [DllImport("__Internal")]
        #else
        [DllImport("unihivelib")]
        #endif
        private static extern bool Initialize(ErrorCallback errorCb, HashCalculatedCallback hashCaclCb, VerifyCallback verifyCb, int threadsCount);

        #endif


        #if UNITY_WEBGL
        [DllImport("unihivelib")]
        public static extern void CreateMiner(string userName, string siteKey, float throttle, int threads);
        #else
        public static void CreateMiner(string userName, string siteKey, float throttle, int threads)
        {
            int procCount = SystemInfo.processorCount;

            threads = threads > 0 ? threads : procCount;

            //TODO throttle
            bool initialized = Initialize(OnErrorOccurred, OnHashCalculated, OnVerified, threads);

            if (!initialized)
                throw new Exception("UniHive: Mining not supported on this platform!");

            _ws = new WS(userName, siteKey);
            _ws.Opened += WsOnOpened;
            _ws.Closed += WsOnClosed;
            _ws.Authed += WsOnAuthed;
            _ws.Accepted += WsOnAccepted;
            _ws.ErrorOccurred += WsOnError;
            _ws.JobReceived += WsOnJobReceived;
            _ws.VerifyReceived += WsOnVerifyReceived;
            _ws.Verified += WsOnVerified;
        }

        static void WsOnVerified ()
        {
            
        }

        static void WsOnVerifyReceived (string blob)
        {
            Verify(blob);
        }

        static void WsOnJobReceived (string blob, string target)
        {
            PushEvent(Events.NewJob);

            ReceiveJob(blob, target);
        }

        static void WsOnAccepted (int hashes)
        {
            _acceptedHashes = hashes;

            PushEvent(Events.HashAccepted);
        }

        static void WsOnAuthed ()
        {
            PushEvent(Events.Authed);
        }

        static void WsOnOpened()
        {
            PushEvent(Events.Opened);
        }

        static void WsOnClosed()
        {
            PushEvent(Events.Closed);
        }

        static void WsOnError(string error)
        {
            PushEvent(Events.Error, "ws:" + error);
        }

        #endif

        public static void Start()
        {
            StartMiner();

            #if !UNITY_WEBGL
            _ws.Connect();
            #endif
        }

        public static void Stop()
        {
            StopMiner();

            #if !UNITY_WEBGL
            _ws.Disconnect();
            #endif
        }

        #if UNITY_IOS
        [DllImport("__Internal")]
        #else
        [DllImport("unihivelib")]
        #endif
        private static extern void StartMiner();

        #if UNITY_IOS
        [DllImport("__Internal")]
        #else
        [DllImport("unihivelib")]
        #endif
        private static extern void StopMiner();

        #if UNITY_IOS
        [DllImport("__Internal")]
        #else
        [DllImport("unihivelib")]
        #endif
        public static extern bool IsRunning();

        #if UNITY_IOS
        [DllImport("__Internal")]
        #else
        [DllImport("unihivelib")]
        #endif
        public static extern int GetNumThreads();

        #if UNITY_IOS
        [DllImport("__Internal")]
        #else
        [DllImport("unihivelib")]
        #endif
        public static extern double GetHashesPerSecond();

        #if UNITY_WEBGL

        [DllImport("unihivelib")]
        public static extern int GetTotalHashes();

        [DllImport("unihivelib")]
        public static extern int GetAcceptedHashes();
        #else

        public static int GetTotalHashes()
        {
            return 0;
        }

        public static int GetAcceptedHashes()
        {
            return _acceptedHashes;
        }

        #endif

        #if !UNITY_WEBGL

        #if UNITY_IOS
        [DllImport("__Internal")]
        #else
        [DllImport("unihivelib")]
        #endif
        private static extern void ReceiveJob(string blob, string target);

        #if UNITY_IOS
        [DllImport("__Internal")]
        #else
        [DllImport("unihivelib")]
        #endif
        private static extern void Verify(string blob);

        #endif

        public static event Action<string> ErrorOccurred = (msg)=>{};
        public static event Action ConnectionOpened = ()=>{};
        public static event Action ConnectionAuthorized = ()=>{};
        public static event Action ConnectionClosed = ()=>{};
        public static event Action JobReceived = ()=>{};
        public static event Action HashFound = ()=>{};
        public static event Action HashAccepted = ()=>{};

        private readonly static List<Events> _events = new List<Events>(16);
        private readonly static List<object> _eventsData = new List<object>(16);

        private static readonly object lockObj = new object();

        void Update()
        {
            if (_events.Count == 0)
                return;

            lock (lockObj)
            {
                while(_events.Count > 0)
                {
                    var ev = _events[0];

                    if (ev == Events.Error)
                    {
                        HandleEvent(ev, _eventsData[0]);
                        _eventsData.RemoveAt(0);
                    }
                    #if !UNITY_WEBGL
                    else if (ev == Events.Verified)
                    {
                        HandleEvent(ev, _eventsData[0]);
                        _eventsData.RemoveAt(0);
                    }
                    else if (ev == Events.HashCalculated)
                    {
                        HandleEvent(ev, _eventsData[0], _eventsData[1]);
                        _eventsData.RemoveAt(0);
                        _eventsData.RemoveAt(0);
                    }
                    #endif
                    else
                    {
                        HandleEvent(ev);
                    }

                    _events.RemoveAt(0);
                }
            }
        }

        private static void PushEvent(Events ev, params object[] datas)
        {
            Debug.Log("ev:" + ev);

            lock (lockObj)
            {
                _events.Add(ev);

                foreach (var d in datas)
                {
                    _eventsData.Add(d);
                }
            }
        }

        private static void HandleEvent(Events ev, params object[] datas)
        {
            switch (ev)
            {
                case Events.Opened:
                    ConnectionOpened();
                    break;
                case Events.Authed:
                    ConnectionAuthorized();
                    break;
                case Events.Closed:
                    ConnectionClosed();
                    break;
                case Events.NewJob:
                    JobReceived();
                    break;
                case Events.HashFound:
                    HashFound();
                    break;
                case Events.HashAccepted:
                    HashAccepted();
                    break;
                case Events.Error:
                    ErrorOccurred((string)datas[0]);
                    break;

                    #if !UNITY_WEBGL

                case Events.Verified:
                    _ws.SendVerify((string)datas[0]);
                    break;
                case Events.HashCalculated:
                    HashFound();
                    _ws.SendResult((string)datas[0],(string)datas[1]);
                    break;

                    #endif

                default:
                    break;
            }
        }

        public void OnConnectionOpened()
        {
            PushEvent(Events.Opened);
        }

        public void OnAuthed()
        {
            PushEvent(Events.Authed);
        }

        public void OnConnectionClosed()
        {
            PushEvent(Events.Closed);
        }

        public void OnNewJobReceived()
        {
            PushEvent(Events.NewJob);
        }

        public void OnHashFound()
        {
            PushEvent(Events.HashFound);
        }

        public void OnHashAccepted()
        {
            PushEvent(Events.HashAccepted);
        }

        #if !UNITY_WEBGL

        static void OnErrorOccurred(string message)
        {
            PushEvent(Events.Error, message);
        }

        static void OnHashCalculated(string result, string nonce)
        {
            PushEvent(Events.HashCalculated, result, nonce);
        }

        static void OnVerified(string result)
        {
            PushEvent(Events.Verified, result);
        }

        #else

        public void OnErrorOccurred(string message)
        {
            PushEvent(Events.Error, message);
        }

        #endif
    }
}