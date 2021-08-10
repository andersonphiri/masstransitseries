using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StartMassTransitExamples1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        readonly IPublishEndpoint _PublishedEndpoint;
        public CustomerController(IPublishEndpoint publishedEndpoint)
        {
            _PublishedEndpoint = publishedEndpoint;
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(Guid id, string customerNumber)
        {
            try
            {
                await _PublishedEndpoint.Publish<CustomerAccountClosed>(new
                {
                    CustomerNumber = customerNumber,
                    CustomerId = id
                });
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
