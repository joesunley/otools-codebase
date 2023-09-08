﻿global using static System.Diagnostics.Debug;
using OTools.Common;

namespace OTools.Maps;

public sealed class Map : IStorable
{
    public Guid Id { get; }

    public string Title { get; set; }

    public Metadata Metadata { get; set; }
    public MapInfo MapInfo { get; set; }

    public ColourStore Colours { get; set; }
    public SpotColourStore SpotColours { get; set; }
    public SymbolStore Symbols { get; set; }
    public InstanceStore Instances { get; set; }

    public Map(string title = "")
    {
        Id = Guid.NewGuid();

        Title = title;

        Metadata = new();
        MapInfo = new();

        Colours = new();
        SpotColours = new();
        Symbols = new();
        Instances = new();
    }

    public Map(ColourStore colours, SpotColourStore spotColours, SymbolStore symbols, InstanceStore instances, string title = "")
    {
        Id = Guid.NewGuid(); // Don't think it needs to be persistent

        Title = title;

        Metadata = new();
        MapInfo = new();
        
        Colours = colours;
        SpotColours = spotColours;
        Symbols = symbols;
        Instances = instances;
    }
}