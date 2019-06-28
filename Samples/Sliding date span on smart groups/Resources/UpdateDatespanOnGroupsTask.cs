using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Customers;
using Litium.Data;
using Litium.Data.Queryable;
using Litium.Data.Queryable.Conditions;
using Litium.Foundation;
using Litium.Foundation.Security;
using Litium.Foundation.Tasks;

namespace Litium.Accelerator.Tasks
{
	public class UpdateDatespanOnGroupsTask : ITask
	{
		private const string FieldName = "SlidingDateSpanDays";
		private readonly DataService _dataService;

		private readonly GroupService _groupService;

		public UpdateDatespanOnGroupsTask(GroupService groupService, DataService dataService)
		{
			_groupService = groupService;
			_dataService = dataService;
		}

		public void ExecuteTask(SecurityToken token, string parameters)
		{
			var groups = GetSmartGroupsWithDateSpanField();
			foreach (var group in groups)
			{
				var daysSpan = group.Fields.GetValue<int?>(FieldName) ?? 0;
				if (daysSpan <= 0)
					continue;

				UpdateGroupDateInterval(group, daysSpan);
			}
		}

		private IEnumerable<DynamicGroup> GetSmartGroupsWithDateSpanField()
		{
			using (var query = _dataService.CreateQuery<Group>())
			{
				// Available query operators for Int: null, any, eq, gt, gte, lt, lte, neq
				var q = query.Filter(f => f
					.Bool(boolFilter => boolFilter
						.MustNot(must => must.Field(FieldName, "null", null))
						.Must(must => must.Field(FieldName, "gt", 0))
					));

				return q.ToList().OfType<DynamicGroup>();
			}
		}

		private void UpdateGroupDateInterval(DynamicGroup group, int daysSpan)
		{
			var fromDate = DateTimeOffset.Now.AddDays(-daysSpan).Date;
			var toDate = DateTimeOffset.Now.AddDays(1).Date;

			var conditionChanged = false;

			group = group.MakeWritableClone();
			foreach (var condition in group.Conditions)
			{
				if (!(condition.Data is OrderTotalFilterCondition))
					continue;

				var currentData = (OrderTotalFilterCondition) condition.Data;
				var dateChanged = currentData.FromDate != fromDate || currentData.ToDate != toDate;
				if (!dateChanged)
					continue;

				condition.Data = new OrderTotalFilterCondition
				{
					FromDate = fromDate,
					ToDate = toDate,
					Amount = currentData.Amount,
					CurrencySystemId = currentData.CurrencySystemId,
					Operator = currentData.Operator
				};

				conditionChanged = true;
			}

			if (!conditionChanged)
				return;

			using (Solution.Instance.SystemToken.Use())
			{
				_groupService.Update(group);
			}

			this.Log().Trace($"Date interval for group '{group.Name}' updated to {fromDate.Date}-{toDate.Date}");
		}
	}
}