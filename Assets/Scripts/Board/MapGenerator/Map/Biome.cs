using System.Collections.Generic;
using UnityEngine;

public static class BiomeProperties
{
    public static Dictionary<Biome, Color> Colors = new()
    {
        { Biome.Ocean, HexToColor("44447a") },
        { Biome.Lake, HexToColor("336699") },
        { Biome.Beach, HexToColor("a09077") },
        { Biome.Snow, HexToColor("ffffff") },
        { Biome.Tundra, HexToColor("bbbbaa") },
        { Biome.Scorched, HexToColor("555555") },
        { Biome.Taiga, HexToColor("99aa77") },
        { Biome.Shrubland, HexToColor("889977") },
        { Biome.TemperateDesert, HexToColor("c9d29b") },
        { Biome.PineForest, HexToColor("448855") },
        { Biome.GinkgoForest, HexToColor("679459") },
        { Biome.Grassland, HexToColor("88aa55") },
        { Biome.SubtropicalDesert, HexToColor("d2b98b") },
        { Biome.RainForest, HexToColor("337755") },
        { Biome.OakForest, HexToColor("559944") }
    };
    static Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }
}

public enum Biome
{
    Ocean,
    Lake,
    Beach,
    Snow,
    Tundra,
    Scorched,
    Taiga,
    Shrubland,
    TemperateDesert,
    PineForest,
    GinkgoForest,
    Grassland,
    RainForest,
    OakForest,
    SubtropicalDesert
}
/*
Food -> Villages
Tri-Economy -> Angels, Church
Church -> First Sphere
*/

// Snow - C
// Tundra - F
// Scorched - E

// Taiga - E
// Shrubland - F
// Temperate Desert - C

// Pine Forest - E
// Ginkgo Forest - C
// Grassland - F

// Rain Forest - C
// Oak Forest - F
// Subtropical Desert - E

// Ocean
// Lake
// Beach
