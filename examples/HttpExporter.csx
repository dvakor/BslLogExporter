#r "nuget: Refit, 7.0.0"

using Refit;

class Result
{
  string Status { get; set; }
}

interface IHttpClient
{
  Task<Result> SendLogs(IEnumerable<LogsEntry> entries);
}

var client = RestService.For<IHttpClient>("http://example.com");

var result = await client.SendLogs(Context.Entries);