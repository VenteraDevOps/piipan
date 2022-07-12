using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Piipan.Components.Routing;

public class PiipanNavigationManager : NavigationManager, IDisposable
{
    public PiipanNavigationManager(NavigationManager parent)
    {
        _Parent = parent;
        _Parent.LocationChanged += Parent_LocationChanged;
    }

    public void Dispose()
    {
        _Parent.LocationChanged -= Parent_LocationChanged;
    }

    private readonly NavigationManager _Parent;
    private bool _IsBlindNavigation;

    public bool BlockNavigation { get; set; } = false;

    public event EventHandler<LocationChangedEventArgs>? NavigationBlocked;

    private void Parent_LocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (_IsBlindNavigation)
        {
            _IsBlindNavigation = false;
            return;
        }

        if (e.IsNavigationIntercepted && BlockNavigation)
        {
            _IsBlindNavigation = true;
            _Parent.NavigateTo(Uri, false, true);

            NavigationBlocked?.Invoke(this, e);
            return;
        }
        else if (e.IsNavigationIntercepted)
        {
            _Parent.NavigateTo(e.Location, true);
        }
        _IsBlindNavigation = false;
        BlockNavigation = false;
        Uri = e.Location;
        NotifyLocationChanged(e.IsNavigationIntercepted);
    }

    protected override void EnsureInitialized()
    {
        Initialize(_Parent.BaseUri, _Parent.Uri);

        base.EnsureInitialized();
    }

    protected override void NavigateToCore(string uri, NavigationOptions options)
    {
        if (BlockNavigation)
        {
            // navigation is locked; notify but otherwise do nothing
            NavigationBlocked?.Invoke(this, new LocationChangedEventArgs(uri, false));
            return;
        }

        // navigation is unlocked; pass on to parent manager
        _Parent.NavigateTo(uri, options);
    }
}