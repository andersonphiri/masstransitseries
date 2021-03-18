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
        private readonly ISendEndpointProvider _SendEndpointProvider;

        public OrderController(ILogger<OrderController> logger, IRequestClient<ISubmitOrder> submitOrderRequestClient, ISendEndpointProvider sendEndpointProvider)
        {
            _Logger = logger;
            _SubmitOrderRequestClient = submitOrderRequestClient;
            _SendEndpointProvider = sendEndpointProvider;
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
                var result = await accepted;
                return Ok(result);
            }
            var reasns = (await rejected).Message.Reasons;
            return BadRequest(reasns);
        }

        [HttpPut]
        public async Task<ActionResult> Put(Guid id, string customerNumber)
        {
            var endpoint = await _SendEndpointProvider.GetSendEndpoint(new Uri("exchange:submit-order"));
            await endpoint.Send<ISubmitOrder>(new
            {
                OrderId = default(Guid),
                Timestamp = default(DateTimeOffset),
                CustomerNumber = default(string)
            });
            
            return NoContent();
        }

    }
}
