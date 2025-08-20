using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace api.Binders
{
	public class JsonFormDataModelBinder : IModelBinder
	{
		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
			if (valueProviderResult == ValueProviderResult.None)
			{
				bindingContext.Result = ModelBindingResult.Success(null);
				return Task.CompletedTask;
			}

			var value = valueProviderResult.FirstValue;
			if (string.IsNullOrEmpty(value))
			{
				bindingContext.Result = ModelBindingResult.Success(null);
				return Task.CompletedTask;
			}

			try
			{
				var result = JsonSerializer.Deserialize(value, bindingContext.ModelType);
				bindingContext.Result = ModelBindingResult.Success(result);
			}
			catch
			{
				bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Invalid JSON");
			}

			return Task.CompletedTask;
		}

	}
		
}