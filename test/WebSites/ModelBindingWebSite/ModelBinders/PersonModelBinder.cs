using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace ModelBindingWebSite
{
    public class PersonModelBinder : IModelBinder
    {
        public async Task<bool> BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                return false;
            }

            var request = bindingContext.OperationBindingContext.HttpContext.Request;
            var form = await request.ReadFormAsync();

            var person = new Person2();
            person.FirstName = form.Get("FirstName");
            person.LastName = form.Get("LastName");
            person.FullName = string.Format("{0} {1}", person.FirstName, person.LastName);

            bindingContext.Model = person;
            
            return true;
        }
    }
}