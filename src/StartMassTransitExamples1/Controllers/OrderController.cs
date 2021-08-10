using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Http;
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
        private readonly IRequestClient<CheckOrder> _CheckOrderClient;
        private readonly IPublishEndpoint _PublishEndpoint;

        public OrderController(ILogger<OrderController> logger, 
            IRequestClient<ISubmitOrder> submitOrderRequestClient, 
            ISendEndpointProvider sendEndpointProvider,
            IRequestClient<CheckOrder> checkOrderClient, 
            IPublishEndpoint publishEndpoint)
        {
            _Logger = logger;
            _SubmitOrderRequestClient = submitOrderRequestClient;
            _SendEndpointProvider = sendEndpointProvider;
            _CheckOrderClient = checkOrderClient;
            _PublishEndpoint = publishEndpoint;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Get(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Id specified was empty");
            }
            var (found, notFound) = await _CheckOrderClient.GetResponse<OrderStatus, OrderNotFound>(new { OrderId = id });

            if (found.IsCompletedSuccessfully)
            {
                var response = await found;
                if (response != null)
                    {
                        return Ok(response.Message);
                    }
                return NotFound($"Order with ID: {id} was not found");

            }
            var notFoundResponse = await notFound;
            if (notFoundResponse != null)
            {
                return NotFound(notFoundResponse.Message);
            }
            return NotFound($"Order with ID: {id} was not found");
        }
        [HttpGet]
        [Route("orders/approve")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetPublishedOrder(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Id specified was empty");
            }
            await _PublishEndpoint.Publish<OrderAccepted>(new
            {
              OrderId = id,InVar.Timestamp });
            return Accepted();
            
        }

        [HttpPost]
        public async Task<ActionResult> Post(Guid id, string customerNumber)
        {
            var (accepted, rejected) = await _SubmitOrderRequestClient.GetResponse<OrderSubmissionAccepted, OrderSubmissionRejected>(new
            {

                OrderId = id,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });
            if (accepted.IsCompletedSuccessfully)
            {
                var result = await accepted;
                return Ok(result.Message);
            }
            var reasns = (await rejected).Message.Reasons;
            return BadRequest(reasns);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Put(Guid id, string customerNumber)
        {
            if (id != Guid.Empty && !string.IsNullOrEmpty(customerNumber) )
            {
                var endpoint = await _SendEndpointProvider.GetSendEndpoint(new Uri("queue:submit-order"));
                await endpoint.Send<ISubmitOrder>(new
                {
                    OrderId = id,
                    InVar.Timestamp,
                    CustomerNumber = customerNumber
                });
                return Ok("success!!");
            }
            return BadRequest("Either id or customer number is empty");
           

        }

    }
}
