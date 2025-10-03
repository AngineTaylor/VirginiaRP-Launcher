using System;
using System.Runtime.Serialization;

namespace Launcher.WPF.Services
{
    [DataContract]
    public class ServiceFault
    {
        [DataMember]
        public string Message { get; set; }

        public ServiceFault() { }

        public ServiceFault(string message)
        {
            Message = message;
        }
    }
}
