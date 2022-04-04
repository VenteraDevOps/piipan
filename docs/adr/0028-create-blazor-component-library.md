# 28. Create a Blazor Component Library for UI Use

Date: 2022-03-16

## Status

Proposed

Supercedes [11. Use Razor pages for web-apps](0011-use-razor-pages-for-web-apps.md)

## Context

While we have been able to have a pretty low complexity application until now, new screens and functionality are being designed that require a little more client-side interactivity.

We believe that developing a Blazor component library to be used in this application will be our best path forward, as it should decrease our development time and increase functionality.

Blazor is Microsoft's response to UI libraries and frameworks like React and Angular. In its most straight-forward usage, developers write client code in C#, compile the code to Web Assembly, deliver it to the browser, and run it as part of a Single Page Application (SPA).

While there was a decision in the past to not use Blazor, going forward at least using it in some part we feel like will give us the best path forward for developing robust UI code.

By agreeing to use a Blazor component library in the app, this may open the door to using Blazor in the app as a whole, but that hasn't been fully decided yet.

## Decision

Continuing to use server-side rendered pages from the Query Tool and Dashboard are still possible using Blazor components. After testing both Blazor Server and Blazor Web Assembly, Web Assembly is better for our use case for a few reasons:
1. We will not have to have an open SignalR connection to the server, which will alleviate some strain on our server and eliminate the need for considering architectural changes, such as Azure SignalR.
2. User interactions will be immediate and not have to go over the wire and back. An example of this was the SSN field, where if we used Blazor Server the checking that occurred after every character input was slow and not responsive when going over the wire. With Blazor Web Assembly it was instantaneous.
3. There were some errors even getting the web sockets working with Blazor Server. This problem could probably have been fixed, but due to the first two items it was decided that it wasn't worth looking into it.

Thus, the proposal is to use server-side rendered pages that incorporate Blazor Web Assembly.

## Consequences

* Projects that incorporate the Blazor Component Library must either be full Web Assembly projects using .Net 3.1, or server-side rendered using .Net 6. Since our current projects are server-side rendered, they will need to upgrade to .Net 6.
* We will need to make a concious effort not to push any secrets to the client side. For the Query Tool web assembly project, that means only incorporating the component library and possibly a shared client library, but none of the server side libraries.
* Users would need a browser that supports web assembly, but only Internet Explorer could be an issue. After discussion with the team, it was determined that Internet Explorer would not be an acceptable browser and the user should upgrade.

## References
* [Component Library code](https://github.com/18F/piipan/components)
* [Web Assembly from Server Side pages](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/built-in/component-tag-helper?view=aspnetcore-6.0)
