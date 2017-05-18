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
        public List<OperatorInfo> sendSMS(string message, List<string> phones)
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
                {
                    var res = JsonConvert.DeserializeObject<Submit>(streamReader.ReadToEnd());
                    if (res.Response.Status.Code == 200)
                    {
                        List<SMSInfo> infos = new List<SMSInfo>();
                        foreach (var item in phones)
                            infos.Add(new SMSInfo
                            {
                                Gsm = item,
                                MessageId = res.Response.MessageId
                            });
                        db.SMSInfo.AddRange(infos);
                        db.SaveChanges();
                        return getQuery(res.Response.MessageId, null);
                    }
                }
                return new List<OperatorInfo>();
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

        /// <summary>
        /// mid: MessageId; list: Numbers to be queried || null (905xxxxxxxxx)
        /// </summary>
        public List<OperatorInfo> getQuery(int mid, List<string> list)
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
                {
                    var res = JsonConvert.DeserializeObject<Query>(streamReader.ReadToEnd());
                    if (res.Response.Status.Code == 200 && res.Response.ReportDetail.List.Count > 0)
                    {
                        List<SMSContent> cns = new List<SMSContent>();
                        List<OperatorInfo> opList = new List<OperatorInfo>();
                        foreach (var item in res.Response.ReportDetail.List)
                        {
                            cns.Add(new SMSContent()
                            {
                                Cost = item.Cost,
                                ErrorCode = item.ErrorCode,
                                Id = item.Id,
                                LastUpdated = item.LastUpdated < DateTime.Now.AddYears(-1) ? DateTime.Now : item.LastUpdated,
                                MSISDN = item.MSISDN,
                                Network = item.Network,
                                Payload = item.Payload,
                                Sequence = item.Sequence,
                                State = item.State,
                                Submitted = item.Submitted < DateTime.Now.AddYears(-1) ? DateTime.Now : item.Submitted,
                                Xser = item.Xser
                            });
                            opList.Add(new OperatorInfo { gsm = item.MSISDN, op = item.Network });
                        }
                        db.SMSContent.AddRange(cns);
                        db.SaveChanges();
                        List<SMSInfo> ins = db.SMSInfo.Where(r => r.MessageId == mid).ToList();
                        ins.ForEach(r =>
                        {
                            r.Cid = cns.First(t => t.MSISDN == r.Gsm).Cid;
                        });
                        //db.SMSInfo.AddRange(ins);
                        db.SaveChanges();
                        return opList;
                    }
                }
                return new List<OperatorInfo>();
            }
        }
    }

    public class OperatorInfo
    {
        public string gsm { get; set; }
        public int op { get; set; }
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
        public int MessageId { get; set; }
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