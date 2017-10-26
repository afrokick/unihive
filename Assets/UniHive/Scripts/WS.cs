using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using SimpleJSON;
using UHive;
using System;

namespace UHive
{
    class WS
    {
        private const string POOL_URL = "wss://ws001.coin-hive.com/proxy";

        [System.Serializable]
        private class AuthParamsRequest
        {
            public string site_key = "qhzlbfnM4o2zwOXZJa5K7F6d9Y5KFV9W";
            public string type = "user";
            public string user = "unity3d";
            public int goal = 0;
        }

        [System.Serializable]
        private class ResultParamsRequest
        {
            public string job_id;
            public string nonce;
            public string result;
        }

        [System.Serializable]
        private class VerifyParamsRequest
        {
            public string verifiy_id;
            public bool verified;
        }

        bool Connected{ get; set; }

        WebSocket _ws;
        string _jobId;
        string _verifyId;
        string _verifyResult;

        readonly string _userName;
        readonly string _siteKey;

        public event Action Opened = ()=>{};
        public event Action Closed = ()=>{};
        public event Action Authed = ()=>{};
        public event Action<string,string> JobReceived = (blob,target)=>{};
        public event Action<string> ErrorOccurred = (error)=>{};
        public event Action<string> VerifyReceived = (blob)=>{};
        public event Action Verified = ()=>{};
        public event Action<int> Accepted = (hashes)=>{};

        public WS(string userName, string siteKey)
        {
            _userName = userName;
            _siteKey = siteKey;
        }

        public void Connect()
        {
            if (Connected)
                return;
        
            _ws = new WebSocket(POOL_URL);
            _ws.OnOpen += _ws_OnOpen;
            _ws.OnClose += _ws_OnClose;
            _ws.OnError += _ws_OnError;
            _ws.OnMessage += _ws_OnMessage;

            _ws.ConnectAsync();

            Connected = true;
        }

        public void Disconnect()
        {
            if (!Connected)
                return;
        
            _ws.CloseAsync(CloseStatusCode.Normal, "dc");

            _ws.OnOpen -= _ws_OnOpen;
            _ws.OnClose -= _ws_OnClose;
            _ws.OnError -= _ws_OnError;
            _ws.OnMessage -= _ws_OnMessage;

            _ws = null;

            Connected = false;
        }

        void _ws_OnError(object sender, ErrorEventArgs e)
        {
            ErrorOccurred(e.Message);
        }

        void _ws_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Type == Opcode.Text)
            {
                Debug.Log("ws message:" + e.Data);
                HandleMessage(e.Data);
            }
            else
                Debug.Log("ws data:" + e.Type);

        }

        void _ws_OnClose(object sender, CloseEventArgs e)
        {
            Closed();
        }

        void _ws_OnOpen(object sender, System.EventArgs e)
        {
            Opened();

            SendAuth();
        }

        void SendAuth()
        {
            JSONObject obj = (JSONObject)JSON.Parse("{}");
            obj["type"] = "auth";

            var authParams = new AuthParamsRequest();
            authParams.site_key = _siteKey;
            if (!String.IsNullOrEmpty(_userName))
            {
                authParams.type = "user";
                authParams.user = _userName;
            }
            else
            {
                authParams.type = "anonymous";
                authParams.user = null;
            }

            obj["params"] = JSON.Parse(JsonUtility.ToJson(authParams));

            string str = obj.ToString();

            _ws.Send(str);
        }

        public void SendResult(string result, string nonce)
        {
            JSONObject obj = (JSONObject)JSON.Parse("{}");
            obj["type"] = "submit";
            obj["params"] = JSON.Parse(JsonUtility.ToJson(new ResultParamsRequest()
                    { 
                        job_id = _jobId, 
                        nonce = nonce, 
                        result = result
                    }));

            string str = obj.ToString();
            Debug.Log("send res:" + str);
            _ws.Send(str);
        }

        public void SendVerify(string result)
        {
            bool verified = result.Equals(_verifyResult);

            JSONObject obj = (JSONObject)JSON.Parse("{}");
            obj["type"] = "verified";
            obj["params"] = JSON.Parse(JsonUtility.ToJson(new VerifyParamsRequest()
                    {
                        verifiy_id = _verifyId, 
                        verified = verified
                    }));

            string str = obj.ToString();

            Debug.Log("send verify:" + verified);

            _ws.Send(str);
        }

        void HandleMessage(string message)
        {
            JSONObject json = (JSONObject)JSON.Parse(message);

            string type = (string)json["type"];
            JSONObject pars = json["params"].AsObject;

            switch (type)
            {
                case "authed":
                    HandleAuthed(pars);
                    break;
                case "job":
                    HandleJob(pars);
                    break;
                case "hash_accepted":
                    HandleAccepted(pars);
                    break;
                case "verify":
                    HandleVerify(pars);
                    break;
                case "banned":

                    break;
                default:
                    Debug.Log("no handler for:" + type);
                    break;
            }
        }

        void HandleAuthed(JSONObject obj)
        {
            string token = (string)obj["token"];
            int hashes = obj["hashes"].AsInt;

            Authed();
        }

        void HandleJob(JSONObject obj)
        {
            string blob = (string)obj["blob"];
            string target = (string)obj["target"];
            string job_id = (string)obj["job_id"];

            _jobId = job_id;

            JobReceived(blob, target);
        }

        void HandleAccepted(JSONObject obj)
        {
            int hashes = obj["hashes"].AsInt;

            Accepted(hashes);
        }

        void HandleVerify(JSONObject obj)
        {
            string blob = (string)obj["blob"];
            string nonce = (string)obj["nonce"];
            _verifyResult = (string)obj["result"];
            _verifyId = (string)obj["verify_id"];

            byte[] nonceBin = StringToByteArray(nonce);
            byte[] blobBin = StringToByteArray(blob);

            for (int i = 0; i < nonceBin.Length; i++)
            {
                blobBin[39 + i] = nonceBin[i];
            }

            string newBlob = ByteArrayToString(blobBin);

            VerifyReceived(newBlob);
        }

        private static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "").ToLower();
        }
    }
}