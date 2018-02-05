# Topshelf.Leader

An extension method to the Topshelf `ServiceConfigurator<T>` class that adds Leader checking to the service startup.

## When would I use this?
Use it when your services require active / passive or any form of high availablility where the services aren't able to naturally compete. 

* Catch up suscriptions that don't allow competing consumers
* Services that can operate in an Active / Passive configuration

## When should I not use this?
Don't use this extension if you want a non-leader service to perform tasks whilst the leader service is also performing tasks. The design of the extension is that only one service is actively doing anything at any one time.

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

## Lease Manager

The responsibility for deciding if your service is the leader is delegated to any class which implements the `ILeaseManager` interface. The process is 
as follows:

1. The process will call ILeaseManager.AcquireLease() until we have obtained a lease (which means that we are the leader)
2. Do work and call ILeaseManager.ReleaseLease() until asked to stop
3. When asked to stop the service we call ILeaseManager.ReleaseLease()

You configure which manager to use during the configuration stage. If one isn't supplied then an in memory manager is used. The in memory manager is not muti-process aware so it is **not suitable for production use**. 

### Configuring the lease manager
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
            builder.WithLeaseManager(new YourManagerHere());
        });
        s.ConstructUsing(name => new TheService());
        s.WhenStopped(service => service.Stop());
    });
}
```

### Example of a simple lease manager
```c#
public class InMemoryLeaseManager : ILeaseManager
{
    private string owningNodeId;

    public InMemoryLeaseManager(string owningNodeId)
    {
        this.owningNodeId = owningNodeId;
    }

    public void AssignLeader(string newLeaderId)
    {
        this.owningNodeId = newLeaderId;
    }

    public Task<bool> AcquireLease(string nodeId, CancellationToken token)
    {
        return Task.FromResult(nodeId == owningNodeId);
    }

    public Task<bool> RenewLease(string nodeId, CancellationToken token)
    {
        return Task.FromResult(nodeId == owningNodeId);
    }

    public Task ReleaseLease(string nodeId)
    {
		owningNodeId = string.Empty;
        return Task.FromResult(true);
    }
}
```
