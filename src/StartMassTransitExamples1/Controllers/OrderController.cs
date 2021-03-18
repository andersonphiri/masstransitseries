using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.Contracts;

namespace StartMassTransitExamples1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        readonly ILogger<OrderController> _Logger;
        private readonly IRequestClient<ISubmitOrder> _SubmitOrderRequestClient;

        public OrderController(ILogger<OrderController> logger, IRequestClient<ISubmitOrder> submitOrderRequestClient)
        {
            _Logger = logger;
            _SubmitOrderRequestClient = submitOrderRequestClient;
        }

        [HttpPost]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "MCA0001:Anonymous type does not map to message contract", Justification = "<Pending>")]
        public async Task<ActionResult> Post(Guid id, string customerNumber)
        {
            var (accepted, rejected) = await _SubmitOrderRequestClient.GetResponse<IOrderSubmissionAccepted, IOrderSubmissionRejected>(new
            {

                OrderId = id,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });
            if (accepted.IsCompletedSuccessfully)
            {
                return Ok(await accepted);
            }
            return BadRequest((await rejected).Message.Reasons);
        }
    }
}
