using System.Runtime.Serialization;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml.Test.Models
{

    [DataContract(Namespace = "http://puchase.Interface.org/Purchase.Order")]
    public class PurchaseOrder
    {
        public PurchaseOrder()
        {
            billTo = new Address() { street = "Bill to Address" };
            shipTo = new Address() { street = "Ship to  Address" };
        }
        [DataMember]
        public Address billTo;
        [DataMember]
        public Address shipTo;
    }

    [DataContract(Namespace = "http://puchase.Interface.org/Purchase.Order.Address")]
    public class Address
    {
        [DataMember]
        public string street;
    }

}
