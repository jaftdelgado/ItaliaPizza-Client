﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ItaliaPizzaClient.SupplyServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="SupplyServiceReference.ISupplyManager")]
    public interface ISupplyManager {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISupplyManager/Ping", ReplyAction="http://tempuri.org/ISupplyManager/PingResponse")]
        bool Ping();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISupplyManager/Ping", ReplyAction="http://tempuri.org/ISupplyManager/PingResponse")]
        System.Threading.Tasks.Task<bool> PingAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ISupplyManagerChannel : ItaliaPizzaClient.SupplyServiceReference.ISupplyManager, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class SupplyManagerClient : System.ServiceModel.ClientBase<ItaliaPizzaClient.SupplyServiceReference.ISupplyManager>, ItaliaPizzaClient.SupplyServiceReference.ISupplyManager {
        
        public SupplyManagerClient() {
        }
        
        public SupplyManagerClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public SupplyManagerClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SupplyManagerClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SupplyManagerClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool Ping() {
            return base.Channel.Ping();
        }
        
        public System.Threading.Tasks.Task<bool> PingAsync() {
            return base.Channel.PingAsync();
        }
    }
}
