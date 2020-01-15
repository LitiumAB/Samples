using Litium.Web.Administration.Filtering;
using JetBrains.Annotations;
using Litium.Customers;
using Litium.Data.Queryable.Conditions;
using Litium.Data.Queryable.Internal;
using Litium.Runtime.DependencyInjection;
using System.Collections.Generic;
using Litium.Studio.Extenssions;

namespace Litium.Accelerator.TargetGroupconditions.LoginStatus
{
    /// <summary>
    ///  The filter service is responsible for rendering the condition in backoffice when it is added to a target group
    /// </summary>
    [Service(ServiceType = typeof(LoginStatusFilterService))]
    public class LoginStatusFilterService : IFilterService<TargetGroup>
    {
        public const string FieldId = "__target_group_login_status";
        public const string FieldTitleResx = "targetgroup.filter.loginstatus";
        public const string FieldTitleIncludeAnonymousResx = "targetgroup.filter.loginstatus.anonusers";
        public const string FieldTitleIncludeLoggedinResx = "targetgroup.filter.loginstatus.loginusers";
        public const string ValueAnonymous = "anonymous";
        public const string ValueLoggedIn = "loggedin";

        public TDescriptor ApplyFilter<TDescriptor>([NotNull] TDescriptor filterDescriptor, [NotNull] FilterArgs filterArgs) where TDescriptor : IFilterDescriptorExpression
        {
            return filterDescriptor;
        }

        public FilterCondition CreateFilterCondition([NotNull] FilterArgs filterArgs)
        {
            return new LoginStatusFilterCondition
            {
                Operator = filterArgs.Operator,
                Value = filterArgs.Value.ToString()
            };
        }

        public FilterModel GetFilterItems()
        {
            return new FilterModel
            {
                Filters = new List<FilterItem>
                {
                    new FilterItem
                    {
                        Field = FieldId,
                        Title = FieldTitleResx.AsAngularResourceString()
                    }
                }
            };
        }

        /// <summary>
        /// Returns the FilterOperator that is rendering the filter in Litium Backoffice
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FilterOperator> GetFilterOperators([NotNull] string fieldId, string culture)
        {
            yield return new FilterOperator
            {
                Field = FieldId,
                FieldTitle = FieldTitleResx.AsAngularResourceString(), // The field title shown in filter drop down listing and as first part when selected (Field title : Selected operator title and value)
                Culture = "*",
                Operator = "contains",
                OperatorTitle = FieldTitleResx.AsAngularResourceString(), // Field header shown above the selectable options
                List = new List<FilterOperator>()
                     {
                        new FilterOperator()
                        {
                            Field = FieldId,
                            FieldTitle = FieldTitleResx.AsAngularResourceString(),
                            Culture = "*",
                            Operator = "contains",
                            OperatorTitle = FieldTitleIncludeAnonymousResx.AsAngularResourceString(), // Title of selectable option in filter drop down
                            ValueTitle = $"{FieldTitleResx.AsAngularResourceString()}: {FieldTitleIncludeAnonymousResx.AsAngularResourceString()}", // Title on item when selected
                            Value = ValueAnonymous,
                            InputType = "radiowithvalue"
                        },
                        new FilterOperator()
                        {
                            Field = FieldId,
                            FieldTitle = FieldTitleResx.AsAngularResourceString(),
                            Culture = "*",
                            Operator = "contains",
                            OperatorTitle = FieldTitleIncludeLoggedinResx.AsAngularResourceString(), // Title of selectable option in filter drop down
                            ValueTitle = $"{FieldTitleResx.AsAngularResourceString()}: {FieldTitleIncludeLoggedinResx.AsAngularResourceString()}", // Title on item when selected
                            Value = ValueLoggedIn,
                            InputType = "radiowithvalue"
                        }
                     }
            };
        }

        public bool ServiceMatch([NotNull] string fieldId)
        {
            return fieldId == FieldId;
        }
    }
}

