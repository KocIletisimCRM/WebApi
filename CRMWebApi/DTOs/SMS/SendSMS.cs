using CRMWebApi.Models.Adsl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace CRMWebApi.DTOs.SMS
{
    public class ComSMSApi
    {
        private static string ApiBase = "http://gw.barabut.com/v1/json/syncreply/";
        private static string UserHeader = "Username";
        private static string UserName = "kocsmsapi";
        private static string PasswordHeader = "Password";
        private static string Password = "1q2w3e";
        private static string DataCodingHeader = "DataCoding";
        private static string Default = "Default";
        private static string FromName = "KOCILETISIM";
        private static string message = "Turkcell Süperonline'a Hoş Gelniniz.";

        public string getMessage()
        {
            return message;
        }

        private JProperty Credential()
        {
            return new JProperty("Credential", new JObject(new JProperty(UserHeader, UserName), new JProperty(PasswordHeader, Password)));
        }

        private JProperty Header(int valid)
        {
            return new JProperty("Header", new JObject(new JProperty("From", FromName), new JProperty("ValidityPeriod", valid)));
        }

        /// <summary>
        /// message: Message content; phones: Phone numbers to send messages (905xxxxxxxxx)
        /// </summary>
        public Submit sendSMS(string message, List<string> phones)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var json = new JObject(Credential(), new JProperty(DataCodingHeader, Default), Header(0), new JProperty("Message", message), new JProperty("To", phones));
                byte[] data = new UTF8Encoding().GetBytes(json.ToString());
                HttpWebRequest request = WebRequest.Create($"{ApiBase}Submit") as HttpWebRequest;
                request.Method = "POST";
                request.ContentLength = data.Length;
                request.ContentType = "application/json";
                request.GetRequestStream().Write(data, 0, data.Length);

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    return JsonConvert.DeserializeObject<Submit>(streamReader.ReadToEnd());
            }
        }

        /// <summary>
        /// mid: MessageId; list: Numbers to be queried || null (905xxxxxxxxx)
        /// </summary>
        public Query getQuery(long mid, List<string> list)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var json = new JObject(Credential(), new JProperty("MessageId", mid), new JProperty("MSISDN", list));
                byte[] data = new UTF8Encoding().GetBytes(json.ToString());
                HttpWebRequest request = WebRequest.Create($"{ApiBase}Query") as HttpWebRequest;
                request.Method = "POST";
                request.ContentLength = data.Length;
                request.ContentType = "application/json";
                request.GetRequestStream().Write(data, 0, data.Length);

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    return JsonConvert.DeserializeObject<Query>(streamReader.ReadToEnd());
            }
        }

        public Balance getBalance()
        {
            var json = new JObject(Credential());
            byte[] data = new UTF8Encoding().GetBytes(json.ToString());
            HttpWebRequest request = WebRequest.Create($"{ApiBase}GetBalance") as HttpWebRequest;
            request.Method = "POST";
            request.ContentLength = data.Length;
            request.ContentType = "application/json";
            request.GetRequestStream().Write(data, 0, data.Length);

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            using (var streamReader = new StreamReader(response.GetResponseStream()))
                return JsonConvert.DeserializeObject<Balance>(streamReader.ReadToEnd());
        }
    }

    public class Status
    {
        public int Code { get; set; }
        public string Description { get; set; }
    }

    /* Submit Response */
    public class Submit
    {
        public SubmitResponse Response { get; set; }
    }

    public class SubmitResponse
    {
        public long MessageId { get; set; }
        public Status Status { get; set; }
    }

    /* Balance Response */
    public class BalanceResponse
    {
        public double Limit { get; set; }
        public double Main { get; set; }
    }

    public class BalanceResp
    {
        public BalanceResponse Balance { get; set; }
        public Status Status { get; set; }
    }

    public class Balance
    {
        public BalanceResp Response { get; set; }
    }

    /* Query Response */
    public class QueryList
    {
        public long Id { get; set; }
        public int Network { get; set; }
        public string MSISDN { get; set; }
        public double Cost { get; set; }
        public DateTime Submitted { get; set; }
        public DateTime LastUpdated { get; set; }
        public string State { get; set; }
        public int Sequence { get; set; }
        public int ErrorCode { get; set; }
        public string Payload { get; set; }
        public string Xser { get; set; }
    }

    public class QueryRD
    {
        public List<QueryList> List { get; set; }
    }

    public class QueryResponse
    {
        public QueryRD ReportDetail { get; set; }
        public Status Status { get; set; }
    }

    public class Query
    {
        public QueryResponse Response { get; set; }
    }
}