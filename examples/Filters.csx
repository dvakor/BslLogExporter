var filtered = Context.Entries.Where(x => x.Severity == "Ошибка").ToList();

Log.Information("Логов с ошибкой: {filtered.Count}");