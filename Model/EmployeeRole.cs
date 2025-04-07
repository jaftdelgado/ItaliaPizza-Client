using System.Collections.Generic;

namespace ItaliaPizzaClient.Model
{
    public class EmployeeRole
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ResourceKey { get; set; }

        public EmployeeRole(int id, string name)
        {
            Id = id;
            Name = name;
            ResourceKey = $"EmployeeRole_{name.Replace(" ", "")}";
        }

        public override string ToString()
        {
            return Name;
        }

        public static List<EmployeeRole> GetDefaultEmployeeRoles()
        {
            return new List<EmployeeRole>
            {
                new EmployeeRole(1, "Admin"),
                new EmployeeRole(2, "Manager"),
                new EmployeeRole(3, "Cashier"),
                new EmployeeRole(4, "Waiter"),
                new EmployeeRole(5, "Cook"),
                new EmployeeRole(6, "DeliveryDriver")
            };
        }
    }
}
