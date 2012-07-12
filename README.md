# ASP.NET MVC validation bridge for JavaScript frameworks

Bridge for making your view model validation attributes available to JavaScript frameworks, such as Backbone, JavaScript MVC, Ember etc.

## Usage
1. Add the controller to your project.

2. Change the `GetViewModelTypes` method to return the correct types you want client validation rules for.

3. Setup routing:
```CSharp
routes.MapRoute("ClientValidationRulesScript", "Scripts/ClientValidationRules.js", new { controller = "ClientValidationRules", action = "Script" }, new string[] { "ClientValidationBridge" });
routes.MapRoute("ClientValidationRulesJson", "ClientValidationRules.json", new { controller = "ClientValidationRules", action = "Json" }, new string[] { "ClientValidationBridge" });
```

4. Embed JavaScript:
```html
<script type="text/javascript" src="/Scripts/ClientValidationRules.js"></script>
```

5. Use the validation rules (more to come)