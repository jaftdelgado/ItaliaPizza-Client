using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ItaliaPizzaClient.Model
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime RegDate { get; set; }
        public bool IsActive { get; set; }
        public int AddressID { get; set; }
        public Address Address { get; set; }
        public byte[] ProfilePic { get; set; }

        public string FullName => $"{LastName}, {FirstName}";

        public string FullAddress
        {
            get
            {
                if (Address == null) return "Sin dirección";
                return $"{Address.AddressName}, {Address.ZipCode}, {Address.City}";
            }
        }

        public string DisplayFullName => string.IsNullOrWhiteSpace(FullName) ? "N/D" : FullName;
    }
}
