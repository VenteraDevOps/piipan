# 28. Create a Blazor Component Library for UI Use

Date: 2022-03-16

## Status

Accepted

## Context

While we have been able to have a pretty low complexity application until now, new screens and functionality are being designed that require a little more client-side interactivity.

We believe that developing a Blazor component library to be used in this application will be our best path forward, as it should decrease our development time and increase functionality.

Blazor is Microsoft's response to UI libraries and frameworks like React and Angular. In its most straight-forward usage, developers write client code in C#, compile the code to Web Assembly, deliver it to the browser, and run it as part of a Single Page Application (SPA).

While there was a decision in the past to not use Blazor, going forward at least using it in some part we feel like will give us the best path forward for developing robust UI code.

By agreeing to use a Blazor component library in the app, this may open the door to using Blazor in the app as a whole, but that hasn't been fully decided yet.

## Decision

We will develop a Blazor Component Library for use with the web applications in Piipan.

## Consequences

* A lot of error handling with forms that was previously done on the server side can be immediately done on the client side with Blazor, which makes for a better experience.
* Unclear how incorporating a component library would work with server-side rendering when navigating to different pages in the app. We may want to switch to Web Assembly as a whole in the future.

## References
* [Component Library code](https://github.com/18F/piipan/components)
