#r "nuget: EasyNetQ, 7.5.3"

using EasyNetQ;

var bus = Storage.GerOrAdd<IBus>("Bus", () => RabbitHutch.CreateBus("host=localhost"));

await bus.PubSub.PublishAsync(
    new {
        Entries = Context.Entries
    });
