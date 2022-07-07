using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace Piipan.Components.Modals
{
    /// <summary>
    /// The manager service that takes care of showing/hiding modals for the application.
    /// </summary>
    public class ModalManager : IModalManager
    {
        public Action ModalsUpdated { get; set; }
        public List<ModalInfo> OpenModals { get; set; } = new List<ModalInfo>();

        // Creates the ModalInfo and adds it to our open modals, and then calls ModalsUpdated.
        // This will get picked up by the ModalContainer to render all open modals.
        public void Show<T>(T modal, ModalInfo modalInfo = null) where T : IComponent
        {
            Type modalType = typeof(T);
            var properties = modalType.GetProperties()
                .Where(prop => prop.IsDefined(typeof(ParameterAttribute), false));

            modalInfo ??= new ModalInfo();
            modalInfo.RenderFragment = new RenderFragment(n =>
            {
                n.OpenComponent<T>(1);

                int sequenceNum = 2;
                foreach (var prop in properties)
                {
                    object value = prop.GetValue(modal);
                    n.AddAttribute(sequenceNum++, prop.Name, value);
                }

                n.CloseComponent();
            });
            OpenModals.Add(modalInfo);
            ModalsUpdated?.Invoke();
        }

        // Removes the given modalInfo from our open modals, and then calls ModalsUpdated.
        // This will get picked up by the ModalContainer to render all remaining open modals.
        public void Close(ModalInfo modalInfo)
        {
            OpenModals.Remove(modalInfo);
            ModalsUpdated?.Invoke();
        }
    }
}
