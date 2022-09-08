using System;
using System.Linq.Expressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Piipan.Components.Modals;
using Piipan.Components.Routing;
using Piipan.Dashboard.Client.Helpers;
using Piipan.Dashboard.Client.Models;
using Piipan.States.Api.Models;

namespace Piipan.Dashboard.Tests
{
    /// <summary>
    /// The Base Test for all Blazor components, which allows you to set initial values for the component, as well as
    /// perform operations such as creating and updating the component
    /// </summary>
    /// <typeparam name="T">The type of the component we are testing</typeparam>
    public abstract class BaseComponentTest<T> : TestContext where T : IComponent, new()
    {
        protected T InitialValues { get; set; } = new T();
        protected AppData AppData { get; set; } = new AppData();

        protected IRenderedComponent<T> Component { get; set; }

        protected void UpdateParameter<P>(Expression<Func<T, P>> parameter, P value)
        {
            Component?.SetParametersAndRender(parameters =>
                parameters.Add(parameter, value));
        }

        public BaseComponentTest()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddModalManager();
            Services.AddPiipanNavigationManager();
            AppData.StateInfo = new StatesInfoResponse
            {
                Results = new System.Collections.Generic.List<StateInfoResponseData>
                {
                    new StateInfoResponseData
                    {
                        Email = "ea-test@usda.example",
                        Phone = "123-123-1234",
                        State = "Echo Alpha",
                        StateAbbreviation = "EA"
                    }
                }
            };
        }

        protected virtual void CreateTestComponent()
        {
            Services.TryAddSingleton<AppData>();
            var appData = Services.GetService<AppData>();

            PropertyCopier.CopyPropertiesTo(AppData, appData);
        }
    }
}
