namespace Minigram.Core.Conventions
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;

    public class ApiVersionRouteConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            foreach (SelectorModel selector in controller.Selectors)
            {
                string existingRoute = selector.AttributeRouteModel?.Template ?? "[controller]";

                selector.AttributeRouteModel = new AttributeRouteModel
                {
                    Template = $"api/v{{version:apiVersion}}/{existingRoute}"
                };
            }
        }
    }
}