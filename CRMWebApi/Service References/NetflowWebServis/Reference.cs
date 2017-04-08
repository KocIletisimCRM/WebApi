﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CRMWebApi.NetflowWebServis {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="NetflowWebServis.NetflowTellcomWSSoap")]
    public interface NetflowTellcomWSSoap {
        
        // CODEGEN: Generating message contract since message GetWorkflowIdListByUserRequest has headers
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetWorkflowIdListByUser", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        CRMWebApi.NetflowWebServis.GetWorkflowIdListByUserResponse GetWorkflowIdListByUser(CRMWebApi.NetflowWebServis.GetWorkflowIdListByUserRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetWorkflowIdListByUser", ReplyAction="*")]
        System.Threading.Tasks.Task<CRMWebApi.NetflowWebServis.GetWorkflowIdListByUserResponse> GetWorkflowIdListByUserAsync(CRMWebApi.NetflowWebServis.GetWorkflowIdListByUserRequest request);
        
        // CODEGEN: Generating message contract since message GetWorkflowDetailByUserRequest has headers
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetWorkflowDetailByUser", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserResponse1 GetWorkflowDetailByUser(CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetWorkflowDetailByUser", ReplyAction="*")]
        System.Threading.Tasks.Task<CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserResponse1> GetWorkflowDetailByUserAsync(CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserRequest request);
        
        // CODEGEN: Generating message contract since message GetWorkflowDetailRequest has headers
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetWorkflowDetail", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        CRMWebApi.NetflowWebServis.GetWorkflowDetailResponse GetWorkflowDetail(CRMWebApi.NetflowWebServis.GetWorkflowDetailRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetWorkflowDetail", ReplyAction="*")]
        System.Threading.Tasks.Task<CRMWebApi.NetflowWebServis.GetWorkflowDetailResponse> GetWorkflowDetailAsync(CRMWebApi.NetflowWebServis.GetWorkflowDetailRequest request);
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1586.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class AuthHeader : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string usernameField;
        
        private string passwordField;
        
        private System.Xml.XmlAttribute[] anyAttrField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string Username {
            get {
                return this.usernameField;
            }
            set {
                this.usernameField = value;
                this.RaisePropertyChanged("Username");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string Password {
            get {
                return this.passwordField;
            }
            set {
                this.passwordField = value;
                this.RaisePropertyChanged("Password");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAnyAttributeAttribute()]
        public System.Xml.XmlAttribute[] AnyAttr {
            get {
                return this.anyAttrField;
            }
            set {
                this.anyAttrField = value;
                this.RaisePropertyChanged("AnyAttr");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1586.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class WorkflowDetailRow : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string keyField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string Key {
            get {
                return this.keyField;
            }
            set {
                this.keyField = value;
                this.RaisePropertyChanged("Key");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
                this.RaisePropertyChanged("Value");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1586.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class GetWorkflowDetailByUserResponse : object, System.ComponentModel.INotifyPropertyChanged {
        
        private int workflowIdField;
        
        private int customerIdField;
        
        private string customerNameField;
        
        private string segmentCodeField;
        
        private int ticketingTypeCodeField;
        
        private string ticketingTypeDescriptionField;
        
        private string workflowStatusCodeField;
        
        private string workflowStatusDescriptionField;
        
        private System.DateTime workflowStartTimeField;
        
        private string customerAddressCityField;
        
        private string customerAddressDistrictField;
        
        private string customerAddressField;
        
        private string customerEmailField;
        
        private string customerPhoneField;
        
        private string xdslTypeField;
        
        private string xdslServiceTypeField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public int WorkflowId {
            get {
                return this.workflowIdField;
            }
            set {
                this.workflowIdField = value;
                this.RaisePropertyChanged("WorkflowId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public int CustomerId {
            get {
                return this.customerIdField;
            }
            set {
                this.customerIdField = value;
                this.RaisePropertyChanged("CustomerId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string CustomerName {
            get {
                return this.customerNameField;
            }
            set {
                this.customerNameField = value;
                this.RaisePropertyChanged("CustomerName");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public string SegmentCode {
            get {
                return this.segmentCodeField;
            }
            set {
                this.segmentCodeField = value;
                this.RaisePropertyChanged("SegmentCode");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public int TicketingTypeCode {
            get {
                return this.ticketingTypeCodeField;
            }
            set {
                this.ticketingTypeCodeField = value;
                this.RaisePropertyChanged("TicketingTypeCode");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=5)]
        public string TicketingTypeDescription {
            get {
                return this.ticketingTypeDescriptionField;
            }
            set {
                this.ticketingTypeDescriptionField = value;
                this.RaisePropertyChanged("TicketingTypeDescription");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=6)]
        public string WorkflowStatusCode {
            get {
                return this.workflowStatusCodeField;
            }
            set {
                this.workflowStatusCodeField = value;
                this.RaisePropertyChanged("WorkflowStatusCode");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=7)]
        public string WorkflowStatusDescription {
            get {
                return this.workflowStatusDescriptionField;
            }
            set {
                this.workflowStatusDescriptionField = value;
                this.RaisePropertyChanged("WorkflowStatusDescription");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=8)]
        public System.DateTime WorkflowStartTime {
            get {
                return this.workflowStartTimeField;
            }
            set {
                this.workflowStartTimeField = value;
                this.RaisePropertyChanged("WorkflowStartTime");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=9)]
        public string CustomerAddressCity {
            get {
                return this.customerAddressCityField;
            }
            set {
                this.customerAddressCityField = value;
                this.RaisePropertyChanged("CustomerAddressCity");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=10)]
        public string CustomerAddressDistrict {
            get {
                return this.customerAddressDistrictField;
            }
            set {
                this.customerAddressDistrictField = value;
                this.RaisePropertyChanged("CustomerAddressDistrict");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=11)]
        public string CustomerAddress {
            get {
                return this.customerAddressField;
            }
            set {
                this.customerAddressField = value;
                this.RaisePropertyChanged("CustomerAddress");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=12)]
        public string CustomerEmail {
            get {
                return this.customerEmailField;
            }
            set {
                this.customerEmailField = value;
                this.RaisePropertyChanged("CustomerEmail");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=13)]
        public string CustomerPhone {
            get {
                return this.customerPhoneField;
            }
            set {
                this.customerPhoneField = value;
                this.RaisePropertyChanged("CustomerPhone");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=14)]
        public string XdslType {
            get {
                return this.xdslTypeField;
            }
            set {
                this.xdslTypeField = value;
                this.RaisePropertyChanged("XdslType");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=15)]
        public string XdslServiceType {
            get {
                return this.xdslServiceTypeField;
            }
            set {
                this.xdslServiceTypeField = value;
                this.RaisePropertyChanged("XdslServiceType");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1586.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class GetWorkflowIdByUserResponse : object, System.ComponentModel.INotifyPropertyChanged {
        
        private int workflowIdField;
        
        private System.DateTime workflowStartTimeField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public int WorkflowId {
            get {
                return this.workflowIdField;
            }
            set {
                this.workflowIdField = value;
                this.RaisePropertyChanged("WorkflowId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public System.DateTime WorkflowStartTime {
            get {
                return this.workflowStartTimeField;
            }
            set {
                this.workflowStartTimeField = value;
                this.RaisePropertyChanged("WorkflowStartTime");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1586.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class GetWorkflowListByUserRequest : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string ticketingTypeCodeField;
        
        private string statusCodeField;
        
        private string customerIdField;
        
        private string segmentCodeField;
        
        private System.DateTime searchStartDateField;
        
        private System.DateTime searchEndDateField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string TicketingTypeCode {
            get {
                return this.ticketingTypeCodeField;
            }
            set {
                this.ticketingTypeCodeField = value;
                this.RaisePropertyChanged("TicketingTypeCode");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string StatusCode {
            get {
                return this.statusCodeField;
            }
            set {
                this.statusCodeField = value;
                this.RaisePropertyChanged("StatusCode");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string CustomerId {
            get {
                return this.customerIdField;
            }
            set {
                this.customerIdField = value;
                this.RaisePropertyChanged("CustomerId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public string SegmentCode {
            get {
                return this.segmentCodeField;
            }
            set {
                this.segmentCodeField = value;
                this.RaisePropertyChanged("SegmentCode");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public System.DateTime SearchStartDate {
            get {
                return this.searchStartDateField;
            }
            set {
                this.searchStartDateField = value;
                this.RaisePropertyChanged("SearchStartDate");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=5)]
        public System.DateTime SearchEndDate {
            get {
                return this.searchEndDateField;
            }
            set {
                this.searchEndDateField = value;
                this.RaisePropertyChanged("SearchEndDate");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetWorkflowIdListByUser", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class GetWorkflowIdListByUserRequest {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public CRMWebApi.NetflowWebServis.AuthHeader AuthHeader;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public CRMWebApi.NetflowWebServis.GetWorkflowListByUserRequest request;
        
        public GetWorkflowIdListByUserRequest() {
        }
        
        public GetWorkflowIdListByUserRequest(CRMWebApi.NetflowWebServis.AuthHeader AuthHeader, CRMWebApi.NetflowWebServis.GetWorkflowListByUserRequest request) {
            this.AuthHeader = AuthHeader;
            this.request = request;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetWorkflowIdListByUserResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class GetWorkflowIdListByUserResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public CRMWebApi.NetflowWebServis.GetWorkflowIdByUserResponse[] GetWorkflowIdListByUserResult;
        
        public GetWorkflowIdListByUserResponse() {
        }
        
        public GetWorkflowIdListByUserResponse(CRMWebApi.NetflowWebServis.GetWorkflowIdByUserResponse[] GetWorkflowIdListByUserResult) {
            this.GetWorkflowIdListByUserResult = GetWorkflowIdListByUserResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetWorkflowDetailByUser", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class GetWorkflowDetailByUserRequest {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public CRMWebApi.NetflowWebServis.AuthHeader AuthHeader;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public int workflowId;
        
        public GetWorkflowDetailByUserRequest() {
        }
        
        public GetWorkflowDetailByUserRequest(CRMWebApi.NetflowWebServis.AuthHeader AuthHeader, int workflowId) {
            this.AuthHeader = AuthHeader;
            this.workflowId = workflowId;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetWorkflowDetailByUserResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class GetWorkflowDetailByUserResponse1 {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserResponse[] GetWorkflowDetailByUserResult;
        
        public GetWorkflowDetailByUserResponse1() {
        }
        
        public GetWorkflowDetailByUserResponse1(CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserResponse[] GetWorkflowDetailByUserResult) {
            this.GetWorkflowDetailByUserResult = GetWorkflowDetailByUserResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetWorkflowDetail", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class GetWorkflowDetailRequest {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public CRMWebApi.NetflowWebServis.AuthHeader AuthHeader;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public int workflowId;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public string[] keys;
        
        public GetWorkflowDetailRequest() {
        }
        
        public GetWorkflowDetailRequest(CRMWebApi.NetflowWebServis.AuthHeader AuthHeader, int workflowId, string[] keys) {
            this.AuthHeader = AuthHeader;
            this.workflowId = workflowId;
            this.keys = keys;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetWorkflowDetailResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class GetWorkflowDetailResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public CRMWebApi.NetflowWebServis.WorkflowDetailRow[] GetWorkflowDetailResult;
        
        public GetWorkflowDetailResponse() {
        }
        
        public GetWorkflowDetailResponse(CRMWebApi.NetflowWebServis.WorkflowDetailRow[] GetWorkflowDetailResult) {
            this.GetWorkflowDetailResult = GetWorkflowDetailResult;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface NetflowTellcomWSSoapChannel : CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class NetflowTellcomWSSoapClient : System.ServiceModel.ClientBase<CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap>, CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap {
        
        public NetflowTellcomWSSoapClient() {
        }
        
        public NetflowTellcomWSSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public NetflowTellcomWSSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public NetflowTellcomWSSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public NetflowTellcomWSSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        CRMWebApi.NetflowWebServis.GetWorkflowIdListByUserResponse CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap.GetWorkflowIdListByUser(CRMWebApi.NetflowWebServis.GetWorkflowIdListByUserRequest request) {
            return base.Channel.GetWorkflowIdListByUser(request);
        }
        
        public CRMWebApi.NetflowWebServis.GetWorkflowIdByUserResponse[] GetWorkflowIdListByUser(CRMWebApi.NetflowWebServis.AuthHeader AuthHeader, CRMWebApi.NetflowWebServis.GetWorkflowListByUserRequest request) {
            CRMWebApi.NetflowWebServis.GetWorkflowIdListByUserRequest inValue = new CRMWebApi.NetflowWebServis.GetWorkflowIdListByUserRequest();
            inValue.AuthHeader = AuthHeader;
            inValue.request = request;
            CRMWebApi.NetflowWebServis.GetWorkflowIdListByUserResponse retVal = ((CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap)(this)).GetWorkflowIdListByUser(inValue);
            return retVal.GetWorkflowIdListByUserResult;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<CRMWebApi.NetflowWebServis.GetWorkflowIdListByUserResponse> CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap.GetWorkflowIdListByUserAsync(CRMWebApi.NetflowWebServis.GetWorkflowIdListByUserRequest request) {
            return base.Channel.GetWorkflowIdListByUserAsync(request);
        }
        
        public System.Threading.Tasks.Task<CRMWebApi.NetflowWebServis.GetWorkflowIdListByUserResponse> GetWorkflowIdListByUserAsync(CRMWebApi.NetflowWebServis.AuthHeader AuthHeader, CRMWebApi.NetflowWebServis.GetWorkflowListByUserRequest request) {
            CRMWebApi.NetflowWebServis.GetWorkflowIdListByUserRequest inValue = new CRMWebApi.NetflowWebServis.GetWorkflowIdListByUserRequest();
            inValue.AuthHeader = AuthHeader;
            inValue.request = request;
            return ((CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap)(this)).GetWorkflowIdListByUserAsync(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserResponse1 CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap.GetWorkflowDetailByUser(CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserRequest request) {
            return base.Channel.GetWorkflowDetailByUser(request);
        }
        
        public CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserResponse[] GetWorkflowDetailByUser(CRMWebApi.NetflowWebServis.AuthHeader AuthHeader, int workflowId) {
            CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserRequest inValue = new CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserRequest();
            inValue.AuthHeader = AuthHeader;
            inValue.workflowId = workflowId;
            CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserResponse1 retVal = ((CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap)(this)).GetWorkflowDetailByUser(inValue);
            return retVal.GetWorkflowDetailByUserResult;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserResponse1> CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap.GetWorkflowDetailByUserAsync(CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserRequest request) {
            return base.Channel.GetWorkflowDetailByUserAsync(request);
        }
        
        public System.Threading.Tasks.Task<CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserResponse1> GetWorkflowDetailByUserAsync(CRMWebApi.NetflowWebServis.AuthHeader AuthHeader, int workflowId) {
            CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserRequest inValue = new CRMWebApi.NetflowWebServis.GetWorkflowDetailByUserRequest();
            inValue.AuthHeader = AuthHeader;
            inValue.workflowId = workflowId;
            return ((CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap)(this)).GetWorkflowDetailByUserAsync(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        CRMWebApi.NetflowWebServis.GetWorkflowDetailResponse CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap.GetWorkflowDetail(CRMWebApi.NetflowWebServis.GetWorkflowDetailRequest request) {
            return base.Channel.GetWorkflowDetail(request);
        }
        
        public CRMWebApi.NetflowWebServis.WorkflowDetailRow[] GetWorkflowDetail(CRMWebApi.NetflowWebServis.AuthHeader AuthHeader, int workflowId, string[] keys) {
            CRMWebApi.NetflowWebServis.GetWorkflowDetailRequest inValue = new CRMWebApi.NetflowWebServis.GetWorkflowDetailRequest();
            inValue.AuthHeader = AuthHeader;
            inValue.workflowId = workflowId;
            inValue.keys = keys;
            CRMWebApi.NetflowWebServis.GetWorkflowDetailResponse retVal = ((CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap)(this)).GetWorkflowDetail(inValue);
            return retVal.GetWorkflowDetailResult;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<CRMWebApi.NetflowWebServis.GetWorkflowDetailResponse> CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap.GetWorkflowDetailAsync(CRMWebApi.NetflowWebServis.GetWorkflowDetailRequest request) {
            return base.Channel.GetWorkflowDetailAsync(request);
        }
        
        public System.Threading.Tasks.Task<CRMWebApi.NetflowWebServis.GetWorkflowDetailResponse> GetWorkflowDetailAsync(CRMWebApi.NetflowWebServis.AuthHeader AuthHeader, int workflowId, string[] keys) {
            CRMWebApi.NetflowWebServis.GetWorkflowDetailRequest inValue = new CRMWebApi.NetflowWebServis.GetWorkflowDetailRequest();
            inValue.AuthHeader = AuthHeader;
            inValue.workflowId = workflowId;
            inValue.keys = keys;
            return ((CRMWebApi.NetflowWebServis.NetflowTellcomWSSoap)(this)).GetWorkflowDetailAsync(inValue);
        }
    }
}
