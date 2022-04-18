namespace Piipan.QueryTool.Client.Models
{
    /// <summary>
    /// A placeholder for passing data into Blazor components from CSHTML views.
    /// As of .net 6, Blazor Components rendered from CSHTML views cannot accept a parameter whose type is
    /// defined outside of the Blazor WebAssembly app.
    /// https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/built-in/component-tag-helper?view=aspnetcore-6.0#:~:text=Parameters%20whose%20type%20is%20defined%20outside%20of%20the%20Blazor%20WebAssembly%20app%20or%20within%20a%20lazily%2Dloaded%20assembly
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ParameterBase<T>
    {
        public T Data { get; set; }

        public static ParameterBase<T> FromObject(T obj)
        {
            return new ParameterBase<T> { Data = obj };
        }
    }
}
