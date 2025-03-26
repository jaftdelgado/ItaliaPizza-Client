using System.Collections.Generic;

public class UnitMeasure
{
    public int Id { get; set; }
    public string Name { get; set; }

    public UnitMeasure(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<UnitMeasure> GetDefaultUnitMeasures()
    {
        return new List<UnitMeasure>
        {
            new UnitMeasure(1, "Kilogramos (kg)"),
            new UnitMeasure(2, "Litros (L)"),
            new UnitMeasure(3, "Piezas (pzas)"),
        };
    }
}
