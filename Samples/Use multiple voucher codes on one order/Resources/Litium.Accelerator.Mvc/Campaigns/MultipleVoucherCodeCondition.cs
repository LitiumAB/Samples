using Litium.Application.Runtime;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns.ConditionTypes;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns;
using Litium.Foundation.Modules.ECommerce;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns.Conditions;

namespace Litium.Accelerator.Mvc.Campaigns
{
	public class MultipleVoucherCodeCondition : CartConditionType, IOrderConfirmationHandler, IConditionRequireCampaignInfo
	{
		private VoucherCodeCondition.Data m_data;
		private Action<object> m_dataPersister = null;
		private const string PATH = "~/Litium/ECommerce/WebUserControls/Conditions/VoucherCodeConditionControl.ascx";

		/// <summary>
		/// Initializes the specified data.
		/// </summary>
		/// <param name="data">The data.</param>
		protected override void Initialize(object data)
		{
			lock (this)
			{
				base.Initialize(data);
				m_data = (VoucherCodeCondition.Data)data;
			}
		}

		/// <summary>
		/// Evaluates the specified condition.
		/// </summary>
		/// <param name="conditionArgs">The condition args.</param>
		/// <returns></returns>
		protected override IEnumerable<OrderCarrier> Evaluate(ConditionArgs conditionArgs)
		{
			return conditionArgs.OrderCarriers
				.Where(x => x.CampaignInfo
				.Split('|')
				.Any(z => m_data.VoucherCodes.ContainsKey(z)));
		}

		/// <summary>
		/// Gets the panel path.
		/// </summary>
		/// <value>The panel path.</value>
		public override string PanelPath { get { return PATH; } }

		#region IOrderConfirmationHandler Members

		/// <summary>
		/// Processes the specified order on confirmation. Any changes done to the order carrier will not be saved.
		/// Use the dataPersister passed in <see cref="IOrderConfirmationHandler.Initialize"/> to save the
		/// internal data.
		/// </summary>
		/// <param name="orderCarrier">The order carrier.</param>
		public void Process(OrderCarrier orderCarrier)
		{
			lock (this)
			{
				var codes = orderCarrier.CampaignInfo.Split('|');
				foreach (var code in codes)
				{
					if (m_data.VoucherCodes.ContainsKey(code))
					{

						if (m_data.VoucherCodes[code] > 1)
							m_data.VoucherCodes[code] -= 1;
						else
						{
							m_data.VoucherCodes.Remove(code);
						}
						m_dataPersister(m_data);
					}
				}
			}
		}

		/// <summary>
		/// Initializes the specified data persister.
		/// </summary>
		/// <param name="dataPersister">The data persister.</param>
		public void Initialize(Action<object> dataPersister)
		{
			m_dataPersister = dataPersister;
		}

		#endregion

		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <param name="languageID">The language ID.</param>
		/// <returns></returns>
		public override string GetDescription(Guid languageID)
		{
			StringBuilder sb = new StringBuilder();
			if (m_data != null)
			{
				sb.Append(ModuleECommerce.Instance.Strings.GetValue("VoucherCodeConditionDescription", languageID, true));
				//makesure a new list of voucher codes is created, because at the time of getting the description, voucher codes
				//might got removed by campaign processing, which will cause a collection modified exception.
				lock (this)
				{
					foreach (KeyValuePair<string, int> item in m_data.VoucherCodes)
					{
						sb.Append(Environment.NewLine);
						sb.AppendFormat("{0} ({1})", item.Key, item.Value);
					}
				}
			}
			else
			{
				sb.Append(base.GetDescription(languageID));
			}
			return sb.ToString();
		}
	}
}