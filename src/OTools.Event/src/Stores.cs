﻿using OTools.Common;

namespace OTools.Events;

public sealed class PersonStore : Store<Person>
{
    public Person this[Name name] => this.First(e => e.Name == name);
    
    public Person this[string name]
    {
        get
        {
            Name search;
            var s = name.Split(' ');
            if (s.Length > 2)
            {
                string last = string.Concat(s[1..]);
                search = new(s[0], last);
            }
            else if (s.Length == 2)
                search = new(s[0], s[1]);
            else search = new(s[0], "");

            return this[search];
        }
    }
}

public sealed class EntryStore : Store<Entry>
{
    public Entry this[Person person] => this.First(e => e.Person == person);
    public Entry this[Name name] => this.First(e => e.Person.Name == name);
}

public sealed class ResultStore : Store<Result>
{
}   

public sealed class PunchStore : Store<Punch>
{
    public IEnumerable<Punch> this[string cardNo] => this.Where(x => x.CardNumber ==  cardNo);
    public IEnumerable<Punch> this[ushort code] => this.Where(x => x.Code == code);
}