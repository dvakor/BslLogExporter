var filtered = Context.Entries.Where(x => x.Severity == "Ошибка").ToList();

Log.LogInformation("Логов с ошибкой: {filtered.Count}");