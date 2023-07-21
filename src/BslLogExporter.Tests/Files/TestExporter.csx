var current = Context.Entries.Count();

if (Storage.TryGet<int>("Count", out var prev)) 
{
    current += prev;
}

Storage.Set("Count", current);



