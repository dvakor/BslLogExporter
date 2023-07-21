public void Test()
{
    log.LogInformation("Test method check");
}

// Check global variables

var args = Args;
var context = Context;
var log = Log;
var storage = Storage;

// Check extension methods

log.LogInformation("Hello world");

var logs = context.Entries.Where(x => true).ToList();

// Check object access

storage.Set("key", 123);
var num = storage.Get<int>("key");

Test();