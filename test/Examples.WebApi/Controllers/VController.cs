using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Examples.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VController : ControllerBase
    {
        [HttpPost]
        public object Test([FromBody] ForValidatorDemoIn demoIn)
        {
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
