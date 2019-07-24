using Litium.FieldFramework;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Administration.FieldFramework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Litium.Accelerator.FieldTypes.YoutubeField
{
	[Service(Name = YoutubeFieldConstants.Id)]
	internal class YoutubeFieldTypeConverter : IEditFieldTypeConverter
	{
		private readonly IFieldTypeMetadata _fieldTypeMetadata;
		
		public YoutubeFieldTypeConverter(FieldTypeMetadataService fieldTypeMetadataService)
		{
			_fieldTypeMetadata = fieldTypeMetadataService.Get(YoutubeFieldConstants.Id);
		}

		public object CreateOptionsModel() => null;

		public virtual object ConvertFromEditValue(EditFieldTypeConverterArgs args, JToken item)
		{
			var fieldTypeInstance = _fieldTypeMetadata.CreateInstance(args.FieldDefinition);
			return fieldTypeInstance.ConvertFromJsonValue(item.ToObject(_fieldTypeMetadata.JsonType));
		}

		public virtual JToken ConvertToEditValue(EditFieldTypeConverterArgs args, object item)
		{
			var fieldTypeInstance = _fieldTypeMetadata.CreateInstance(args.FieldDefinition);
			var value = fieldTypeInstance.ConvertToJsonValue(item);
			if (value == null)
			{
				return JValue.CreateNull();
			}
			return JToken.FromObject(value);
		}

		/// <summary>
		/// The AngularJS controller name to edit the Youtube field.
		/// </summary>
		public string EditControllerName => null;
		/// <summary>
		/// The AngularJS template to edit the Youtube field.
		/// </summary>
		public string EditControllerTemplate => null;

		/// <summary>
		/// The Angular component to edit the Youtube field.
		/// </summary>
		/// <remark>
		/// The extension module should have the module name (Accelerator in this case) 
		/// as prefix, followed by the component name to be able to find the correct component on the client side.
		/// </remark>
		public string EditComponentName => "Accelerator#FieldEditorYoutube";

		public string SettingsControllerName => null;
		public string SettingsControllerTemplate => null;
		public string SettingsComponentName => string.Empty;
	}
}
