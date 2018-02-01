# Topshelf.Leader

An extension method to the Topshelf `ServiceConfigurator<T>` class that adds Leader checking to the service startup.

## When would I use this?
Use it when your services require active / passive or any form of high availablility.

## Getting started
```
Install-Package Topshelf.Leader
```

Once the package is installed, create a Console application and wireup the Topshelf service as you normally would except for the `WhenStarted()` method - this should no longer be used. 

You should use the `WhenStartedAsLeader()` method that Topshelf.Leader provides instead which constains its own version of the `WhenStarted()` method, one with cancellation token support.

### Example
```c#
using Topshelf.Leader;

public class Program
{
    static void Main(string[] args)
    {
      var rc = HostFactory.Run(x =>
      {
          x.Service<TheService>(s =>
          {
              s.WhenStartedAsLeader(builder =>
              {
                  builder.WhenStarted(async (service, token) =>
                  {
                      await service.Start(token);
                  });
              });
              s.ConstructUsing(name => new TheService());
              s.WhenStopped(service => service.Stop());
          });
      }
  }
}
```

## How does it work?

The `WhenStarted()` method will be executed when the service discovers that it is the current leader. If that situation changes the cancellation token will be set to cancelled. You decide how to handle this situation, throw an exception, exit gracefully or even carry on whatever you were doing - that's entirely up to you.

### Example of a service which supports leadership change
```c#
    public class TestService
    {
        public async Task Start(CancellationToken stopToken)
        {
            while (!stopToken.IsCancellationRequested)
            {
                // do your work here, if it's async pass the stopToken to it
            }
        }
    }
```

## Leadership Manager

The responsibility for deciding who is the leader, and maintaining that status is delegated to any class which implements the  `ILeaderManager` interface. You configure which manager to use during the configuration stage. If one isn't supplied then an in memory manager is used - this is not muti-process aware so is **not suitable for production use - only for testing**. 

### Configuring the leadership manager
```c#
var rc = HostFactory.Run(x =>
{
    x.Service<TheService>(s =>
    {
        s.WhenStartedAsLeader(builder =>
        {
            builder.WhenStarted(async (service, token) =>
            {
                await service.Start(token);
            });
            builder.WithLeadershipManager(new YourManagerHere());
        });
        s.ConstructUsing(name => new TheService());
        s.WhenStopped(service => service.Stop());
    });
}
```

### Example of a simple leadership manager
```c#
public class InMemoryLeadershipManager : ILeadershipManager
{
    private string owningNodeId;

    public InMemoryLeadershipManager(string owningNodeId)
    {
        this.owningNodeId = owningNodeId;
    }

    public void AssignLeader(string newLeaderId)
    {
        this.owningNodeId = newLeaderId;
    }

    public Task<bool> AcquireLock(string nodeId, CancellationToken token)
    {
        return Task.FromResult(nodeId == owningNodeId);
    }

    public Task<bool> RenewLock(string nodeId, CancellationToken token)
    {
        return Task.FromResult(nodeId == owningNodeId);
    }
}
```
