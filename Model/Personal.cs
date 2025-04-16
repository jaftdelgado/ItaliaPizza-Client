using System;
using System.Windows;

namespace ItaliaPizzaClient.Model
{
    public class Personal
    {
        public int PersonalID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RFC { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public byte[] ProfilePic { get; set; }
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; }
        public int RoleID { get; set; }
        public int AddressID { get; set; }
        public Address Address { get; set; }
        public bool IsOnline { get; set; }

        public string FullName => $"{LastName}, {FirstName}";

        public string TranslatedRole
        {
            get
            {
                var role = EmployeeRole.GetDefaultEmployeeRoles().Find(r => r.Id == RoleID);
                if (role == null) return "Desconocido";

                var translated = Application.Current.TryFindResource(role.ResourceKey) as string;
                return translated ?? role.Name;
            }
        }

        public string FullAddress
        {
            get
            {
                if (Address == null) return "Sin dirección";
                return $"{Address.AddressName}, {Address.ZipCode}, {Address.City}";
            }
        }
    }
}
