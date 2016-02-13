# crocodile
Contract-based RPC library.

## Motivation
I wanted a super easy to use RPC library that wasn't as horrifically bloated and riddled with XML config nonsense as WCF.

This project allows you to *quickly and easily* add the ability to *invoke methods in another process* just like you would invoke any local method and with *minimal effort*.

The project uses the  [url:XDMessaging library|http://xdmessaging.codeplex.com], this library allows for quick and easy communication to other processes via a few transport methods, my project makes use of the IOStream method and therefore doesn't work across the network (this may change down the line). *UPDATE* The library *from the source code tab* now *supports propagation over the network*.

# NETWORK INFORMATION
*while the library has network propagation enabled, it doesn't appear to be functional as of the moment*

*COPYRIGHT NOTICE*
This project contains code from *XDMessaging* which can be found at the URL above, this is copyright of the respective creator, while *not necessary* whenever using this code either in part or in full please *do upload any change* and please please please do *credit the project / myself*

Time for some code :)

# Example

Let's say you're writing an application that can provide the user with notifications (like [url:Growl|http://growl.info]) so you want your application to work as a service that other applications can call into, to do this you'd normally need to:

* Define an endpoint through which applications can communicate with yours
* Define a protocol to use for the communication
* Parse and deal with the ins and outs of the messages

Now wouldn't it be great if you could simply define a *contract* and with a small amount of work have applications be able to talk to yours *as if they were calling any other code*?

So here goes =]

## Contract

First off you need to come up with a contract, let's say for simplicity we just want the ability to show a simple string message.

```csharp
public interface INotificationProvider
{
   void showNotification(string Application, string Message);
}
```

And presumably you'd use a *shared library* to provide the third-parties with the interface (this however *is not necessary* so long as the *definitions match*).

On the service side you would create an instance to be called:

## Service-side notification provider

```csharp
public class NotificationProvider : INotificationProvider
{
   public void showNotification(string Application, string Message)
   {
      MessageBox.Show(String.Format("{0} says - '{1}'", Application, Message);
   }
}
```

Now assuming you're using a *shared library* and you don't want the third-party developers to have to *worry about the implementation of a client*, you can *create your own*.

## Client-side proxy

```csharp
public class NotificationClient : INotificationProvider
{
   private xInvokeClient<INotificationProvider> _client = null;

   public NotificationClient()
   {
      _client = new xInvokeClient<INotificationProvider>("NotificationService");
      _client.Begin();
   }

   public void showNotification(string Application, string Message)
   {
      _client.DoCall(new object[] { Application, Message });
   }
}
```

If however the onus were on the third-party developer to create the client all they would need would be
* The contract (either in shared type or definition)
* An instance of the xInvokeClient<Contract> class
* A locally defined class that makes use of that xInvokeClient

A few new things there namely:
* *xInvokeClient<INotificationProvider>*
* *new xInvokeClient<INotificationProvider>("NotificationService")*

They'll be covered now =]

## The actual pieces of the framework

There are *two pieces* to the framework and they are the *xInvokeService<T>* and *xInvokeClient<T>* classes respectively, I'm now going to cover what they are how to use them.

### xInvokeService<T>
This class acts as the service that client's will be able to call, it *needs three things*
* The contract (Passed as the generic type)
* The instance (Passed in the constructor)
* The service name (Passed in the constructor)

So starting the service for our notification application would look akin to this:

```csharp
NotificationProvider ProviderInstance = new NotificationProvider();
xInvokeService<INotificationProvider> ProviderService = new xInvokeService<INotificationProvider>(ProviderInstance, "NotificationService");
ProviderService.Begin();
```

And that as they say *is it*, those few lines have now set it up ready for applications to call across.

### xInvokeClient<T>
This class acts as the client, it needs *two(ish) things*
* The contract (Passed as the generic type) - (unless of course it is wrapped in a custom class, making the 3rd party developer free from knowing this class even exists)
* The service name (Passed in the constructor)

*As per the design* the following code shows how to set up the client

```csharp
NotificationClient NotificationClientInstance = new NotificationClient();
```

And yes *that really is it*, because the client instance has taken care of creating and starting the *xInvokeClient*

## The result

Now in order to send a notification to the service the third-party application simply works like this:

```csharp
NotificationClient NotificationClientInstance = new NotificationClient();
NotificationClientInstance.showNotification("This Application", "This is a message sent to another process");
```

# Extras & Limitations
* Return types *ARE* supported
** Return types are done using the *xInvokeClient<T>.DoCall<K>(new object[] {PARAMS})*
*** K being the return type
* ref and out *ARE NOT* supported
* Properties work as they are just methods
* You *CAN NOT* pass *null* values for a parameter value
** This is due to null being non serialisable
*** If you wish to be able to pass null you should use *your own* implementation for dealing with this

# License

The code probably says it's GPL as it's old code, I'm relicensing under MIT as part of the migration to GH.
