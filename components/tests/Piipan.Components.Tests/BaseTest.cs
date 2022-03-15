using Bunit;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Components.Tests
{
    public abstract class BaseTest<T> : TestContext where T : IComponent, new()
    {
        protected T InitialValues { get; set; } = new T();

        protected IRenderedComponent<T>? Component { get; set; }

        protected void UpdateParameter<P>(Expression<Func<T, P>> parameter, P value)
        {
            Component?.SetParametersAndRender(parameters =>
                parameters.Add(parameter, value));
        }

        protected abstract void CreateTestComponent();
    }
}
