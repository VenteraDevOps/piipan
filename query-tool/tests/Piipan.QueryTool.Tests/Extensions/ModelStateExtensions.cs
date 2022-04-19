using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Piipan.QueryTool.Tests.Extensions
{
    public static class ModelStateExtensions
    {
        public static void BindModel<T>(this PageModel pageModel, T model, string boundProperty)
        {
            if (model == null) return;

            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(model, context, results, true))
            {
                pageModel.ModelState.Clear();
                foreach (ValidationResult result in results)
                {
                    var key = (string.IsNullOrEmpty(boundProperty) ? "" : boundProperty + ".") +
                        result.MemberNames.FirstOrDefault() ?? "";
                    pageModel.ModelState.AddModelError(key, result.ErrorMessage);
                }
            }
        }
    }

}
