﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Linq;
using System.Threading.Tasks;

namespace Piipan.Components.Forms
{
    /// <summary>
    /// The Social Security Number component
    /// </summary>
    public partial class UsaInputSSN
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        protected override void OnInitialized()
        {
            if (!string.IsNullOrEmpty(CurrentValue))
            {
                InvisibleValue = string.Join("", CurrentValue.Select(x => x != '-' ? '*' : x));
            }
            base.OnInitialized();
        }

        /// <summary>
        /// On input, we need to do add/remove hyphens, as well as potentially protect the value from prying eyes.
        /// </summary>
        private async Task Input(ChangeEventArgs e)
        {
            CurrentValue ??= "";
            string value = e.Value as string;
            var cursorPosition = await JSRuntime.InvokeAsync<int>("piipan.utilities.getCursorPosition", ElementReference);
            if (cursorPosition > value.Length)
            {
                cursorPosition = value.Length;
            }
            if (!visible)
            {
                var beginningStr = "";
                var endStr = "";
                var middleStr = "";
                if (cursorPosition >= 0)
                {
                    int endingChars = 0;
                    for (int i = value.Length - 1; i >= 0 && i >= cursorPosition; i--)
                    {
                        if (value[i] != '*' && value[i] != '-')
                        {
                            break;
                        }
                        endingChars++;
                    }
                    endStr = endingChars <= CurrentValue.Length ? CurrentValue.Substring(CurrentValue.Length - endingChars) : "";
                    int startingChars = 0;
                    for (int i = 0; i < value.Length - endingChars && i < cursorPosition; i++)
                    {
                        if (value[i] == '*' || value[i] == '-')
                        {
                            if (CurrentValue.Length > i)
                            {
                                beginningStr += CurrentValue[i];
                            }
                            else
                            {
                                beginningStr += value[i];
                            }
                            startingChars++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (startingChars + endingChars < value.Length)
                    {
                        middleStr = value.Substring(startingChars, value.Length - startingChars - endingChars);
                    }
                    if (!string.IsNullOrEmpty(beginningStr) || !string.IsNullOrEmpty(middleStr) || !string.IsNullOrEmpty(endStr))
                    {
                        value = beginningStr + middleStr + endStr;
                    }
                }

            }
            int hyphensRemovedBeforeCursor = value.Substring(0, cursorPosition).Count((c) => c == '-');
            cursorPosition -= hyphensRemovedBeforeCursor;
            bool isLonger = value.Length > CurrentValue.Length;
            var tempValue = value.Replace("-", "");
            var invisibleValue = new string('*', tempValue.Length);
            if (tempValue.Length > 3 || (tempValue.Length == 3 && isLonger))
            {
                tempValue = tempValue.Insert(3, "-");
                invisibleValue = invisibleValue.Insert(3, "-");
                if (cursorPosition >= 3)
                {
                    cursorPosition++;
                }
            }
            if (tempValue.Length > 6 || (tempValue.Length == 6 && isLonger))
            {
                tempValue = tempValue.Insert(6, "-");
                invisibleValue = invisibleValue.Insert(6, "-");
                if (cursorPosition >= 6)
                {
                    cursorPosition++;
                }
            }
            if (tempValue.Length > 11)
            {
                tempValue = tempValue.Substring(0, 11);
                invisibleValue = invisibleValue.Substring(0, 11);
            }
            if ((visible && CurrentValue == tempValue) || (!visible && InvisibleValue == invisibleValue))
            {
                // Reset the value. Blazor won't rebind, but we need to refresh it anyway
                await JSRuntime.InvokeVoidAsync("piipan.utilities.setValue", ElementReference, visible ? tempValue : invisibleValue);
            }
            CurrentValue = tempValue;
            InvisibleValue = invisibleValue;
            StateHasChanged();
            await ValueChanged.InvokeAsync(tempValue);
            await JSRuntime.InvokeVoidAsync("piipan.utilities.setCursorPosition", ElementReference, cursorPosition);
        }
    }
}