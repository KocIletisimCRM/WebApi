using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class DTOResponseError
    {
        public int errorCode { get; set; }
        public string errorMessage { get; set; }
        public static DTOResponseError NoError()
        {
            return new DTOResponseError { errorCode = 0, errorMessage = string.Empty };
        }
        public static DTOResponseError create(int code, string message)
        {
            return new DTOResponseError { errorCode = code, errorMessage = message };
        }
    }
    public class DTOResponsePagingInfo
    {
        public int pageNo { get; set; }
        public int pageCount { get; set; }
        public int rowsPerPage { get; set; }
        public int totalRowCount { get; set; }
    }
    public class DTOResponseData
    {
        private List<object> _data = new List<object>();
        public List<object> rows { get { return _data; } }
        public DTOResponseData(){}
        public DTOResponseData(List<object> datarows)
        {
            _data.AddRange(datarows);
        }
    }
    public class DTOResponsePagedData : DTOResponseData
    {
        public DTOResponsePagingInfo pagingInfo { get; set; }
        public DTOResponsePagedData(){}
        public DTOResponsePagedData(List<object> datarows, DTOResponsePagingInfo paginginfo)
            : base(datarows)
        {
            pagingInfo = paginginfo;
        }
    }
    public class DTOResponse
    {
        public DTOResponseError error { get; set; }
        public DTOResponseData data { get; set; }
        public DTOResponse(){}
        public DTOResponse(DTOResponseError error, DTOResponseData data)
        {
            this.data = data;
            this.error = error;
        }
        public DTOResponse(DTOResponseError error, List<object> datarows)
        {
            this.error = error;
            data = new DTOResponseData(datarows);
        }
    }
    public class DTOPagedResponse : DTOResponse
    {
        public string SQL { get; set; }
        public new DTOResponsePagedData data { get; set; }
        public DTOPagedResponse(){}
        public DTOPagedResponse(DTOResponseError error, DTOResponsePagedData pageddata)
        {
            this.error = error;
            this.data = pageddata;
        }
        public DTOPagedResponse(DTOResponseError error, List<object> datarows, DTOResponsePagingInfo paginginfo){
            this.error = error;
            this.data = new DTOResponsePagedData(datarows, paginginfo);
        }
        public DTOPagedResponse(DTOResponseError error, List<object> datarows, DTOResponsePagingInfo paginginfo, string sql)
            : this(error, datarows, paginginfo)
        {
            this.SQL = sql;
        }
    }


}