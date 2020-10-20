using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amazon;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.CognitoSync;
using Amazon.CognitoSync.SyncManager;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

public class SyncClient : MonoBehaviour
{

    [SerializeField] private Text TextInfo;

    public ImmutableCredentials credentials;
    public string identityId;
    private string login;
    public int record;

    public void Start()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);
        StartCoroutine(Identity());
    }

    private IEnumerator Identity()
    {
        _dbg("______________________________________");
        yield return CognitoGetId();
        yield return CognitoCredentialsForIdentity(identityId);
        login = SystemInfo.deviceUniqueIdentifier;
        yield return GetRecordRequest();
    }

    private IEnumerator CognitoGetId(Dictionary<string, string> logins = null)
    {
        var amzDate = DateTime.Now.ToString("yyyyMMddTHHmmssZ");
        var request = new Dictionary<string, object>() {
                {"IdentityPoolId", "us-east-1:ab7e9b03-f6b7-471b-aa95-dfb44cbe53f0"}
            };
        if (logins != null)
            request.Add("Logins", logins);
        var data = JsonConvert.SerializeObject(request);
        var post = UnityWebRequest.Put("https://cognito-identity.us-east-1.amazonaws.com/", data);
        post.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");
        post.SetRequestHeader("X-AMZ-TARGET", "com.amazonaws.cognito.identity.model.AWSCognitoIdentityService.GetId");
        post.SetRequestHeader("X-Amz-Date", amzDate);
        post.method = UnityWebRequest.kHttpVerbPOST;
        yield return post.SendWebRequest();
        if (!post.isNetworkError && !post.isHttpError)
        {
            var response = JObject.Parse(post.downloadHandler.text);
            identityId = response["IdentityId"].ToString();
            //_dbg($"handler: {post.downloadHandler.text}");
            _dbg($"identityId: {identityId}");
        }
    }
   

    public void UpdateRecord(int score)
    {
        StartCoroutine(UpdateRecordRequest(score));
    }

    private IEnumerator Request()
    {
        var graphqlJson = new Dictionary<string, object>() {
                {"query", "query { getSomething}"}
            };
        var request = JsonConvert.SerializeObject(graphqlJson);
        _dbg($"Request: {request}");
        const string service = "appsync";
        const string algorithm = "AWS4-HMAC-SHA256";
        const string contentType = "application/graphql";
        const string canonicalQuerystring = "";
        const string signedHeaders = "content-type;host;x-amz-date";
        var payloadHash = SHA256_from_utf8_string(request);
        //var url = "https://" + BackendSettings.AppsyncHost + BackendSettings.AppsyncUri;
        var url = "https://ggym4eduoza6fmscvfkmq3jmdu.appsync-api.us-east-1.amazonaws.com/graphql";
        var AppsyncHost = "ggym4eduoza6fmscvfkmq3jmdu.appsync-api.us-east-1.amazonaws.com";
        var AppsyncUri = "/graphql";
        var region = RegionEndpoint.USEast1.SystemName;
        var datetime = DateTime.Now;
        var amzDate = datetime.ToString("yyyyMMddTHHmmssZ");
        var dateStamp = datetime.ToString("yyyyMMdd");
        var canonicalHeaders = "content-type:" + contentType + "\n" + "host:" + AppsyncHost + "\n" + "x-amz-date:" + amzDate + "\n";
        var canonicalRequest = "POST" + "\n" + AppsyncUri + "\n" + canonicalQuerystring + "\n" + canonicalHeaders + "\n" + signedHeaders + "\n" + payloadHash;
        var credentialScope = dateStamp + "/" + region + "/" + service + "/" + "aws4_request";
        var stringToSign = algorithm + "\n" + amzDate + "\n" + credentialScope + "\n" + SHA256_from_utf8_string(canonicalRequest);
        var signingKey = _getSignatureKey(credentials.SecretKey, dateStamp, region, service);
        var signature = ToHex(HmacSha256(stringToSign, signingKey));
        _dbg($"Signature: {signature}");
        var authorizationHeader = algorithm + " " + "Credential=" + credentials.AccessKey + "/" + credentialScope + ", " + "SignedHeaders=" + signedHeaders +
            ", " + "Signature=" + signature;
        var post = UnityWebRequest.Put(url, request);
        post.SetRequestHeader("Content-Type", contentType);
        post.SetRequestHeader("X-Amz-Date", amzDate);
        post.SetRequestHeader("X-Amz-Security-Token", credentials.Token);
        post.SetRequestHeader("Authorization", authorizationHeader);
        post.method = UnityWebRequest.kHttpVerbPOST;
        post.timeout = 30;
        yield return post.SendWebRequest();
        if (!post.isNetworkError && !post.isHttpError)
        {
            _dbg("---> OK <---");
            _dbg($"result: {post.downloadHandler.text}");
        }
        else
        {
            _dbg(post.isNetworkError ? $"NetworkError: {post.downloadHandler.text}" : "");
            _dbg(post.isHttpError ? $"HttpError: {post.downloadHandler.text}" : "");
        }
    }

    private IEnumerator GetRecordRequest()
    {
        var graphqlJson = new Dictionary<string, object>() {
                {"query", "query { getRecord(Login: \"" + login + "\"){ Login, Record } }"}
            };
        var request = JsonConvert.SerializeObject(graphqlJson);
        _dbg($"Request: {request}");
        const string service = "appsync";
        const string algorithm = "AWS4-HMAC-SHA256";
        const string contentType = "application/graphql";
        const string canonicalQuerystring = "";
        const string signedHeaders = "content-type;host;x-amz-date";
        var payloadHash = SHA256_from_utf8_string(request);
        //var url = "https://" + BackendSettings.AppsyncHost + BackendSettings.AppsyncUri;
        var url = "https://ggym4eduoza6fmscvfkmq3jmdu.appsync-api.us-east-1.amazonaws.com/graphql";
        var AppsyncHost = "ggym4eduoza6fmscvfkmq3jmdu.appsync-api.us-east-1.amazonaws.com";
        var AppsyncUri = "/graphql";
        var region = RegionEndpoint.USEast1.SystemName;
        var datetime = DateTime.Now;
        var amzDate = datetime.ToString("yyyyMMddTHHmmssZ");
        var dateStamp = datetime.ToString("yyyyMMdd");
        var canonicalHeaders = "content-type:" + contentType + "\n" + "host:" + AppsyncHost + "\n" + "x-amz-date:" + amzDate + "\n";
        var canonicalRequest = "POST" + "\n" + AppsyncUri + "\n" + canonicalQuerystring + "\n" + canonicalHeaders + "\n" + signedHeaders + "\n" + payloadHash;
        var credentialScope = dateStamp + "/" + region + "/" + service + "/" + "aws4_request";
        var stringToSign = algorithm + "\n" + amzDate + "\n" + credentialScope + "\n" + SHA256_from_utf8_string(canonicalRequest);
        var signingKey = _getSignatureKey(credentials.SecretKey, dateStamp, region, service);
        var signature = ToHex(HmacSha256(stringToSign, signingKey));
        _dbg($"Signature: {signature}");
        var authorizationHeader = algorithm + " " + "Credential=" + credentials.AccessKey + "/" + credentialScope + ", " + "SignedHeaders=" + signedHeaders +
            ", " + "Signature=" + signature;
        var post = UnityWebRequest.Put(url, request);
        post.SetRequestHeader("Content-Type", contentType);
        post.SetRequestHeader("X-Amz-Date", amzDate);
        post.SetRequestHeader("X-Amz-Security-Token", credentials.Token);
        post.SetRequestHeader("Authorization", authorizationHeader);
        post.method = UnityWebRequest.kHttpVerbPOST;
        post.timeout = 30;
        yield return post.SendWebRequest();
        if (!post.isNetworkError && !post.isHttpError)
        {
            var response = JObject.Parse(post.downloadHandler.text);
            record = response["data"]["getRecord"]["Record"] == null ? 0 : Convert.ToInt32(response["data"]["getRecord"]["Record"]);
            _dbg("---> OK <---");
            _dbg($"result: {post.downloadHandler.text}");
        }
        else
        {
            _dbg(post.isNetworkError ? $"NetworkError: {post.downloadHandler.text}" : "");
            _dbg(post.isHttpError ? $"HttpError: {post.downloadHandler.text}" : "");
        }
    }

    private IEnumerator UpdateRecordRequest(int score)
    {
        var graphqlJson = new Dictionary<string, object>() {
                {"query", "mutation {updateRecord(input: {Login: \"" + login + "\", Score: " + score + "}){Login, Record}}"}
            };
        var request = JsonConvert.SerializeObject(graphqlJson);
        _dbg($"Request: {request}");
        const string service = "appsync";
        const string algorithm = "AWS4-HMAC-SHA256";
        const string contentType = "application/graphql";
        const string canonicalQuerystring = "";
        const string signedHeaders = "content-type;host;x-amz-date";
        var payloadHash = SHA256_from_utf8_string(request);
        //var url = "https://" + BackendSettings.AppsyncHost + BackendSettings.AppsyncUri;
        var url = "https://ggym4eduoza6fmscvfkmq3jmdu.appsync-api.us-east-1.amazonaws.com/graphql";
        var AppsyncHost = "ggym4eduoza6fmscvfkmq3jmdu.appsync-api.us-east-1.amazonaws.com";
        var AppsyncUri = "/graphql";
        var region = RegionEndpoint.USEast1.SystemName;
        var datetime = DateTime.Now;
        var amzDate = datetime.ToString("yyyyMMddTHHmmssZ");
        var dateStamp = datetime.ToString("yyyyMMdd");
        var canonicalHeaders = "content-type:" + contentType + "\n" + "host:" + AppsyncHost + "\n" + "x-amz-date:" + amzDate + "\n";
        var canonicalRequest = "POST" + "\n" + AppsyncUri + "\n" + canonicalQuerystring + "\n" + canonicalHeaders + "\n" + signedHeaders + "\n" + payloadHash;
        var credentialScope = dateStamp + "/" + region + "/" + service + "/" + "aws4_request";
        var stringToSign = algorithm + "\n" + amzDate + "\n" + credentialScope + "\n" + SHA256_from_utf8_string(canonicalRequest);
        var signingKey = _getSignatureKey(credentials.SecretKey, dateStamp, region, service);
        var signature = ToHex(HmacSha256(stringToSign, signingKey));
        _dbg($"Signature: {signature}");
        var authorizationHeader = algorithm + " " + "Credential=" + credentials.AccessKey + "/" + credentialScope + ", " + "SignedHeaders=" + signedHeaders +
            ", " + "Signature=" + signature;
        var post = UnityWebRequest.Put(url, request);
        post.SetRequestHeader("Content-Type", contentType);
        post.SetRequestHeader("X-Amz-Date", amzDate);
        post.SetRequestHeader("X-Amz-Security-Token", credentials.Token);
        post.SetRequestHeader("Authorization", authorizationHeader);
        post.method = UnityWebRequest.kHttpVerbPOST;
        post.timeout = 30;
        yield return post.SendWebRequest();
        if (!post.isNetworkError && !post.isHttpError)
        {
            var response = JObject.Parse(post.downloadHandler.text);
            record = response["data"]["updateRecord"]["Record"] == null ? 0 : Convert.ToInt32(response["data"]["updateRecord"]["Record"]);
            _dbg("---> OK <---");
            _dbg($"result: {post.downloadHandler.text}");
        }
        else
        {
            _dbg(post.isNetworkError ? $"NetworkError: {post.downloadHandler.text}" : "");
            _dbg(post.isHttpError ? $"HttpError: {post.downloadHandler.text}" : "");
        }
    }

    public static string SHA256_from_utf8_string(string rawData)
    {
        // Create a SHA256   
        using (var sha256Hash = SHA256.Create())
        {
            // ComputeHash - returns byte array  
            var hashBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return ToHex(hashBytes);
        }
    }

    public static string ToHex(byte[] data, bool lowercase = true, bool sized64 = false)
    {
        var stringBuilder = new StringBuilder();
        foreach (var el in data)
        {
            stringBuilder.Append(el.ToString(lowercase ? "x2" : "X2", CultureInfo.InvariantCulture));
        }
        return sized64 ? stringBuilder.ToString().PadLeft(64, '0') : stringBuilder.ToString();
    }

    private static byte[] _getSignatureKey(String key, String dateStamp, String regionName, String serviceName)
    {
        byte[] kSecret = Encoding.UTF8.GetBytes(("AWS4" + key).ToCharArray());
        byte[] kDate = HmacSha256(dateStamp, kSecret);
        byte[] kRegion = HmacSha256(regionName, kDate);
        byte[] kService = HmacSha256(serviceName, kRegion);
        byte[] kSigning = HmacSha256("aws4_request", kService);
        return kSigning;
    }

    private static byte[] HmacSha256(string data, byte[] key)
    {
        var kha = KeyedHashAlgorithm.Create("HmacSHA256");
        kha.Key = key;
        return kha.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private IEnumerator CognitoCredentialsForIdentity(string identityId, Dictionary<string, string> logins = null)
    {
        var amzDate = DateTime.Now.ToString("yyyyMMddTHHmmssZ");
        var request = new Dictionary<string, object> {
                { "IdentityId", identityId }
            };
        if (logins != null)
        {
            request.Add("Logins", logins);
        }
        var dataString = JsonConvert.SerializeObject(request);
        var data = System.Text.Encoding.UTF8.GetBytes(dataString);
        var post = UnityWebRequest.Put("https://cognito-identity.us-east-1.amazonaws.com/", data);
        post.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");
        post.SetRequestHeader("X-AMZ-TARGET", "com.amazonaws.cognito.identity.model.AWSCognitoIdentityService.GetCredentialsForIdentity");
        post.SetRequestHeader("X-Amz-Date", amzDate);
        post.method = UnityWebRequest.kHttpVerbPOST;
        yield return post.SendWebRequest();
        if (!post.isNetworkError && !post.isHttpError)
        {
            var response = JObject.Parse(post.downloadHandler.text);
            if (identityId != response["IdentityId"].ToString())
            {
                _dbg("Cognito user has been changed! Old: " + identityId + " New: " + response["IdentityId"].ToString());
            }
            credentials = new ImmutableCredentials(
                //response["IdentityId"].ToString(),
                response["Credentials"]["AccessKeyId"].ToString(),
                response["Credentials"]["SecretKey"].ToString(),
                response["Credentials"]["SessionToken"].ToString()
            //Convert.ToUInt64(response["Credentials"]["Expiration"].ToString()),
            //logins != null
            );
            //_dbg("credentials:");
            //_dbg($"\t* AccessKey:{credentials.AccessKey}");
            //_dbg($"\t* SecretKey:{credentials.SecretKey}");
            //_dbg($"\t* SessionToken:{credentials.Token}");
        }
        else
        {
            _dbg("Ups! Credentials not found");
        }
    }

    public void _dbg(string str)
    {
        string writePath = @"E:\MobirateDebug.txt";
        try
        {
            using (StreamWriter sw = new StreamWriter(writePath, true, System.Text.Encoding.Default))
            {
                sw.WriteLine(str);
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }

}
