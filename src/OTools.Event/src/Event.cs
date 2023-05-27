﻿global using static System.Diagnostics.Debug;

namespace OTools.Event;

public interface IEvent
{
    string Name { get; set; }
    DateOnly Date { get; set; }
}

public sealed class Event : IEvent
{
    public string Name { get; set; }
    public DateOnly Date { get; set; }
}

public sealed class MultiDayEvent : IEvent
{
    public string Name { get; set; }
    public DateOnly Date { get; set; } // Start Date

    public List<Event> Events { get; set; }
}