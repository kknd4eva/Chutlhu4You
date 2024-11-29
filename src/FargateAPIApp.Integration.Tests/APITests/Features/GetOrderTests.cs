using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FargateAPIApp.Integration.Tests.APITests.Features
{
    public class GetOrderTests
    {
        [Fact]
        public async Task GetOrder_ReturnsSuccess()
        {
            var order = new Order { Id = 1, Name = "Test Order" };

            var response = await TestHelper.GetOrder(order.Id);

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var returnedOrder = JsonConvert.DeserializeObject<Order>(responseContent);

            Assert.Equal(order.Id, returnedOrder.Id);
            Assert.Equal(order.Name, returnedOrder.Name);
        
    }
}
