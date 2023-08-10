using FluentValidation;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Core.Wrappers;
using System;
using System.Threading.Tasks;

namespace CMC.Kernel.Core.Services
{
    public abstract class BaseServiceHandler
    {
        private readonly IValidatorFactory _validatorFactory;
        protected IUnitOfWork _unitOfWork;

        public BaseServiceHandler(IValidatorFactory validatorFactory, IUnitOfWork unitOfWork)
        {
            _validatorFactory = validatorFactory;
            _unitOfWork = unitOfWork;
        }

        public BaseServiceHandler()
        {
        }



        protected async Task<Response<bool>> ValidateAsync<T>(T dto)
        {
            if (_validatorFactory == null)
                throw new NullReferenceException("ValidatorFactory is null, please override the parameterized constructor to inject it.");

            var validator = _validatorFactory.GetValidator(dto.GetType());

            if (validator != null)
            {
                var context = new ValidationContext<T>(dto);
                var validationResult = validator.Validate(context);

                if (!validationResult.IsValid)
                    return await new ValidationProcessor<bool>().ProcessValidationResultOnFailureAsync(validationResult);

                return new Response<bool>(true);
            }

            return new Response<bool>(true);
        }
    }
}
