namespace Litium.Samples.OrderInspection.Litium.Sales
{
    public class OrderFixer
    {
        private readonly ValidateCancellationsFixer _validateCancellationsFixer;
        public OrderFixer(ValidateCancellationsFixer validateCancellationsFixer)
        {
            _validateCancellationsFixer = validateCancellationsFixer;
        }

        public async Task<List<string>> FixOrderAsync(OrderOverview orderOverview, OrderValidationResult validationResult)
        {
            var result = new List<string>();
            if(validationResult.ValidationChecks.TryGetValue("Cancellations", out var cancellationsCheck) && !cancellationsCheck.Status)
            {
                var fixResult = await _validateCancellationsFixer.Fix(orderOverview);
                result.AddRange(fixResult);
            }
            return result;
        }
    }
}
