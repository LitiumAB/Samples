using Litium.FieldFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Litium.Accelerator.FieldTypes.YoutubeField
{
	public class YoutubeFieldMetaData : FieldTypeMetadataBase
	{
		public override string Id => YoutubeFieldConstants.Id;
		public override bool CanBeGridColumn => true;
		public override bool CanBeGridFilter => false;
		public override Type JsonType => typeof(string);

		public override IFieldType CreateInstance(IFieldDefinition fieldDefinition)
		{
			var item = new YoutubeFieldType();
			item.Init(fieldDefinition);

			return item;
		}
	}

	public class YoutubeFieldType : FieldTypeBase
	{
		public override object GetValue(ICollection<FieldData> fieldDatas) => fieldDatas.FirstOrDefault()?.TextValue;

		public override ICollection<FieldData> PersistFieldData(object item) => PersistFieldDataInternal(item);

		protected override ICollection<FieldData> PersistFieldDataInternal(object item) => new[] { new FieldData { TextValue = (string)item } };
	}
}
