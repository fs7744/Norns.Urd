using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Examples.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VController : ControllerBase
    {
        private readonly NotificationEditInValidator validations;

        public VController(NotificationEditInValidator validations)
        {
            this.validations = validations;
        }

        [HttpPost]
        //public object Test([FromBody] ForValidatorDemoIn demoIn)
        public object Test()
        {
            var demoIn = (validations as IValidator).Validate(new ValidationContext<object>( new ForValidatorDemoIn()) as IValidationContext);
            return demoIn;
        }
    }

    public class NotificationEditInValidator : AbstractValidator<ForValidatorDemoIn>
    {
        public NotificationEditInValidator()
        {
            base.RuleFor(c => c)
                .NotNull();

            base.RuleFor(c => c.SellerID)
                .NotEmpty();

            base.RuleFor(c => c.TransactionNumber)
                .NotEmpty();
        }
    }

    /// <summary>
    /// 此实体只是用来演示验证器用的
    /// </summary>
    public class ForValidatorDemoIn
    {
        public string SellerID { get; set; }
        public int TransactionNumber { get; set; }
    }
}
