using System.Collections.Generic;
using System.Windows;

namespace ItaliaPizzaClient.Model
{
    public class MeasureUnit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string ResourceKey { get; set; }
        public string AbbreviationResourceKey { get; set; }

        public MeasureUnit(int id, string keyBase)
        {
            Id = id;
            ResourceKey = $"MeasureU_{keyBase}";
            AbbreviationResourceKey = $"MeasureUA_{keyBase}";
            Name = Application.Current.Resources[ResourceKey]?.ToString() ?? keyBase;
            Abbreviation = Application.Current.Resources[AbbreviationResourceKey]?.ToString() ?? keyBase;
        }

        public override string ToString()
        {
            return FullDisplayName;
        }

        public string FullDisplayName => $"{Name} ({Abbreviation})";

        public static List<MeasureUnit> GetDefaultMeasureUnits()
        {
            return new List<MeasureUnit>
            {
                new MeasureUnit(1, "Kilograms"),
                new MeasureUnit(2, "Liters"),
                new MeasureUnit(3, "Pieces")
            };
        }
    }
}
