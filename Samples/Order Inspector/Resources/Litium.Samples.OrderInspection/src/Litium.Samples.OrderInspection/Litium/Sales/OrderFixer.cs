namespace Litium.Samples.OrderInspection.Litium.Sales
{
    public class OrderFixer
    {
        private readonly OrderOverviewFactory _orderOverviewFactory;
        private readonly OrderValidator _orderValidator;
        private readonly ValidateCancellationsFixer _validateCancellationsFixer;

        public OrderFixer(
            OrderOverviewFactory orderOverviewFactory,
            OrderValidator orderValidator,
            ValidateCancellationsFixer validateCancellationsFixer)
        {
            _orderOverviewFactory = orderOverviewFactory;
            _orderValidator = orderValidator;
            _validateCancellationsFixer = validateCancellationsFixer;
        }

        public async Task<List<string>> FixOrderAsync(string orderId, CancellationToken cancellationToken = default)
        {
            var orderOverview = await _orderOverviewFactory.CreateAsync(orderId, cancellationToken);
            var validationResult = _orderValidator.Validate(orderOverview);

            var result = new List<string>();
            if(validationResult.ValidationChecks.TryGetValue("Cancellations", out var cancellationsCheck) && !cancellationsCheck.Status)
            {
                var fixResult = await _validateCancellationsFixer.Fix(orderOverview);
                result.AddRange(fixResult);
            }

            var orderOverviewAfterFix = await _orderOverviewFactory.CreateAsync(orderId, cancellationToken);
            var validationAfterFix = _orderValidator.Validate(orderOverview);

            if(validationAfterFix.IsValid)
            {
                if(orderOverviewAfterFix.SalesOrder.OrderState != "Completed")
                {

                }
            }

            return result;
        }
    }
}
