using FluentValidation.Results;
using CMC.Kernel.Core.Enums;
using System.Threading.Tasks;

namespace CMC.Kernel.Core.Wrappers
{
    public class ValidationProcessor<T>
    {
        public Task<Response<T>> ProcessValidationResultOnFailureAsync(ValidationResult validationResult)
        {
            return Task.FromResult(ProcessValidationResultOnFailure(validationResult));
        }

        public Response<T> ProcessValidationResultOnFailure(ValidationResult validationResult)
        {
            var response = new Response<T>(true);

            if (!validationResult.IsValid)
            {
                response.Succeeded = false;
                response.Message = "Error"; //** Todo: Bring default localized error message from resources store.
                response.StatusCode = (int)HttpStatusCode.BusinessRuleViolation;

                foreach (var failure in validationResult.Errors)
                {
                    response.BrokenRules.Add(new ValidationRule
                    {
                        PropertyName = failure.PropertyName,
                        Message = failure.ErrorMessage
                    });
                }
            }

            return response;
        }
    }
}
