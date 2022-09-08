using Microsoft.AspNetCore.Components.Forms;

namespace Piipan.Components.Forms
{
    public class TemporaryRadioGroup<TValue> : InputRadioGroup<TValue>
    {
        private string? _name;

        protected override void OnParametersSet()
        {
            if (Name != _name)
            {
                _name = Name;
                base.OnParametersSet();
            }
        }
    }
}
