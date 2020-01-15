using Litium.Data.Queryable.Conditions;

namespace Litium.Accelerator.TargetGroupconditions.LoginStatus
{
    public class LoginStatusFilterCondition : FilterCondition
    {
        private string _operator;
        private string _value;

        public virtual string Operator
        {
            get
            {
                return _operator;
            }
            set
            {
                ThrowIfReadOnly();
                _operator = value;
            }
        }

        public virtual string Value
        {
            get
            {
                return _value;
            }
            set
            {
                ThrowIfReadOnly();
                _value = value;
            }
        }
    }
}

