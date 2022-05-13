using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Piipan.Components.Forms
{
    /// <summary>
    /// The Social Security Number component
    /// </summary>
    public partial class UsaInputSSN
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        /// <summary>
        /// This timer is used to hide the last SSN character typed after 1 second. This protects it as expected, but allows
        /// the user to see what they're typing to validate it's correct.
        /// </summary>
        Timer ssnProtectionTimer = new Timer();

        protected override void OnInitialized()
        {
            if (!string.IsNullOrEmpty(CurrentValue))
            {
                InvisibleValue = string.Join("", CurrentValue.Select(x => x != '-' ? '*' : x));
            }
            base.OnInitialized();
            ssnProtectionTimer.Interval = 1000;
            ssnProtectionTimer.Elapsed += async (object sender, ElapsedEventArgs e) =>
            {
                ssnProtectionTimer.Stop();
                if (!visible)
                {
                    InvisibleValue ??= "";
                    InvisibleValue = string.Join("", InvisibleValue.Select(n => n != '-' ? '*' : n));
                    int cursorPosition = await JSRuntime.InvokeAsync<int>("piipan.utilities.getCursorPosition", ElementReference);
                    await InvokeAsync(StateHasChanged);
                    await JSRuntime.InvokeVoidAsync("piipan.utilities.setCursorPosition", ElementReference, cursorPosition);
                }
            };
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
                ssnProtectionTimer.Stop();
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
                ssnProtectionTimer.Start();
            }
            int hyphensRemovedBeforeCursor = value.Substring(0, cursorPosition).Count((c) => c == '-');
            char? lastChar = null;
            var inputString = e.Value as string;
            if (inputString.Length > cursorPosition - 1 && cursorPosition > 0 && inputString.Length == InvisibleValue?.Length + 1)
            {
                lastChar = (e.Value as string)[cursorPosition - 1];
            }
            cursorPosition -= hyphensRemovedBeforeCursor;
            bool isLonger = value.Length > CurrentValue.Length;
            var tempValue = value.Replace("-", "");

            var invisibleValue = new string('*', tempValue.Length);
            if (lastChar != null)
            {
                char[] array = invisibleValue.ToCharArray();
                array[cursorPosition - 1] = lastChar.Value;
                invisibleValue = new string(array);
            }
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
                // This happens when you try deleting a hyphen that's in the middle of the SSN and the above logic puts it back in.
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
