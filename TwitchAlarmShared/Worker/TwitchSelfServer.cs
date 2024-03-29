﻿using EmbedIO;
using EmbedIO.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using TwitchAlarmShared.Container;

namespace TwitchAlarmShared.Worker
{
    //https://github.com/adrianschofield/TwitchAuthWPF/blob/master/TwitchAuthWPF/Program.cs
    public class TwitchSelfServer
    {
        private WebServer server;

        private const string twitchRedirectUri = "http://localhost:8080/twitch/callback";

        public TwitchAuthResponse ReceivedResponse = null;

        public void Start()
        {
            server = new WebServer(o => o.WithUrlPrefix("http://*:8080/").WithMode(HttpListenerMode.EmbedIO));
            server.WithModule(new ActionModule("/twitch/callback/", HttpVerbs.Any, ctx =>
            {
                TwitchAuthorizationApi(ctx.GetRequestQueryData()["code"]);
                return ctx.SendDataAsync(new { Message = "인증을 해 주셔서 감사합니다, 앱으로 돌아가서 인증상태를 확인하세요!" });
            }));

            server.Start();
            Debug.WriteLine("Web server started!!!");
        }

        public void TwitchAuthorizationApi(string code)
        {
            HttpWebRequest myWebRequest = null;
            ASCIIEncoding encoding = new ASCIIEncoding();
            Dictionary<string, string> postDataDictionary = new Dictionary<string, string>();

            //We need to prepare the POST data ahead of time, Add each entry required by the Twitch Authorization Code Flow
            //Then spin through URLEncoding the keys and values and joining them into one string using & and =

            postDataDictionary.Add("client_id", TwitchID.ID);
            postDataDictionary.Add("client_secret", TwitchID.SECRET_ID);
            postDataDictionary.Add("grant_type", "authorization_code");
            postDataDictionary.Add("redirect_uri", twitchRedirectUri);
            //postDataDictionary.Add("state", "123456");
            postDataDictionary.Add("code", code);

            string postData = "";

            foreach (KeyValuePair<string, string> kvp in postDataDictionary)
            {
                postData += HttpUtility.UrlEncode(kvp.Key) + "=" + HttpUtility.UrlEncode(kvp.Value) + "&";
            }

            //We need the POST data as a byte array, using ASCII encoding to keep things simple

            byte[] byte1 = encoding.GetBytes(postData);

            //OK set up our request for the final step in the Authorization Code Flow
            //This is the destination URI as described in https://dev.twitch.tv/docs/v5/guides/authentication/

            myWebRequest = WebRequest.CreateHttp("https://id.twitch.tv/oauth2/token");

            //This request is a POST with the required content type

            myWebRequest.Method = "POST";
            myWebRequest.ContentType = "application/x-www-form-urlencoded";

            //Set the request length based on our byte array

            myWebRequest.ContentLength = byte1.Length;

            //Things can go wrong here so let's do some sensible exception handling, this sample is
            //short lived but best practice is to manage the POST and response

            //POST

            Stream postStream = null;

            try
            {
                //Set up the request and write the data this should complete the POST

                postStream = myWebRequest.GetRequestStream();
                postStream.Write(byte1, 0, byte1.Length);
            }
            catch (Exception ex)
            {
                //We should log any exception here but I am just going to supress them for this sample
                Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                throw ex;
            }
            finally
            {
                postStream.Close();
            }

            //response to POST

            Stream responseStream = null;
            StreamReader responseStreamReader = null;
            WebResponse response = null;
            string jsonResponse = null;

            try
            {
                //Wait for the response from the POST above and get a stream with the data

                response = myWebRequest.GetResponse();
                responseStream = response.GetResponseStream();

                //Read the response, if everything worked we'll have our JSON encoded oauth token
                responseStreamReader = new StreamReader(responseStream);
                jsonResponse = responseStreamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                //We should log any exception here but I am just going to supress them for this sample
                Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                throw ex;
            }
            finally
            {
                responseStreamReader.Close();
                responseStream.Close();
                response.Close();
            }

            //We got the jsonResponse from Twitch let's Deserialize it,
            //I'm using Newtonsoft - Install-Package Newtonsoft.Json -Version 9.0.1
            //Class for deserializing is defined below

            TwitchAuthResponse myAuthResponse = null;

            try
            {
                myAuthResponse = JsonConvert.DeserializeObject<TwitchAuthResponse>(jsonResponse);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                throw ex;
            }

            ReceivedResponse = myAuthResponse;
            return;
        }
    }

    public class TwitchAuthResponse
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public List<string> scope { get; set; }
    }
}
