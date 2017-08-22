namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class ActionResultApiDescriptionProvider : IApiDescriptionProvider
    {
        public int Order => -1000 + 10;

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {

        }

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            for (var i = 0; i < context.Results.Count; i++)
            {
                var apiDescription = context.Results[i];
                foreach (var responseType in apiDescription.SupportedResponseTypes)
                {
                    if (responseType.Type.IsGenericType && typeof(ActionResult<>).IsAssignableFrom(responseType.Type.GetGenericTypeDefinition()))
                    {
                        responseType.Type = responseType.Type.GetGenericArguments()[0];
                    }
                }
            }
        }
    }
}

