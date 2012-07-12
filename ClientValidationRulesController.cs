using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace ClientValidationBridge
{
	/// <summary>
	/// Controller that creates client validation rules for each model as JSON.
	/// </summary>
	public class ClientValidationRulesController : Controller
	{
		/// <summary>
		/// Returns JavaScript that initialized a dictionary with client validation rules, when embedded on a page.
		/// </summary>
		/// <returns>Returns JavaScript code with all client validation rules in a dictionary.</returns>
		[OutputCache(Duration = 604800)]
		public ActionResult Script()
		{
			StringBuilder sb = new StringBuilder();
			Dictionary<string, object> clientValidationRules = this.GetClientValidationRules();

			sb.AppendLine("(function () {");
			sb.AppendLine(String.Format("\tMilkshake.clientValidationRules = {0};", JsonConvert.SerializeObject(clientValidationRules)));
			sb.AppendLine("})();");

			return Content(sb.ToString(), "text/javascript");
		}

		/// <summary>
		/// Returns a dictionary of client validation rules as JSON.
		/// </summary>
		/// <returns>Returns a JSON dictionary with client validation rules.</returns>
		[OutputCache(Duration = 604800)]
		public ActionResult Json()
		{
			Dictionary<string, object> clientValidationRules = this.GetClientValidationRules();

			return this.Json(clientValidationRules, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Gets the client validation rules.
		/// </summary>
		/// <returns>Returns the client validation rules for all view models.</returns>
		private Dictionary<string, object> GetClientValidationRules()
		{
			Dictionary<string, object> clientValidationRules = new Dictionary<string, object>();
			IEnumerable<Type> viewModelTypes = GetViewModelTypes();

			foreach (Type type in viewModelTypes)
			{
				Dictionary<string, IDictionary<string, object>> validationRules = new Dictionary<string, IDictionary<string, object>>();
				ViewData.Model = Activator.CreateInstance(type);
				HtmlHelper html = this.GetHtmlHelper();
				html.ViewContext.FormContext = new FormContext();

				foreach (ModelMetadata item in ViewData.ModelMetadata.Properties)
				{
					var metadata = ModelMetadata.FromStringExpression(item.PropertyName, ViewData);
					IDictionary<string, object> data = html.GetUnobtrusiveValidationAttributes(String.Join(".", metadata.ContainerType.FullName, item.PropertyName), metadata);

					if (data.Count > 0)
					{
						validationRules.Add(item.PropertyName, data);
					}
				}

				if (validationRules.Count > 0)
				{
					clientValidationRules.Add(type.ToString(), new { type = type.ToString(), rules = validationRules });
				}
			}
			return clientValidationRules;
		}

		/// <summary>
		/// Returns all types in the current AppDomain implementing the IViewModelBase interface.
		/// </summary>
		/// <returns>Returns all types that implement the IViewModelBase interface</returns>
		private static IEnumerable<Type> GetViewModelTypes()
		{
			return AppDomain.CurrentDomain.GetAssemblies().ToList().SelectMany(s => s.GetTypes()).Where(p => typeof(IViewModel).IsAssignableFrom(p));
		}

		/// <summary>
		/// Gets the HTML helper.
		/// </summary>
		/// <returns>Returns a fake HTML Helper</returns>
		private HtmlHelper GetHtmlHelper()
		{
			ViewContext viewContext = new ViewContext(ControllerContext, new FakeView(), ViewData, TempData, TextWriter.Null);

			return new HtmlHelper(viewContext, new ViewPage());
		}

		/// <summary>
		/// Fake view for creating a fake HTML Helper.
		/// </summary>
		public class FakeView : IView
		{
			/// <summary>
			/// Renders the specified view context by using the specified the writer object.
			/// </summary>
			/// <param name="viewContext">The view context.</param>
			/// <param name="writer">The writer object.</param>
			public void Render(ViewContext viewContext, TextWriter writer)
			{
				throw new InvalidOperationException();
			}
		}
	}

	/// <summary>
	/// Interface that all view models implement.
	/// </summary>
	public interface IViewModel
	{
	}
}