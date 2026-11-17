using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderSync.Api.Data;
using OrderSync.Api.Models;
using OrderSync.Api.Services;

namespace OrderSync.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(OrderDb db, WriteGate gate, SyncService sync,
                              ILogger<OrdersController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(string? status, string? search)
    {
        var query = db.Orders.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(order => order.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToUpperInvariant();
            query = query.Where(order => order.OrderNumber.Contains(search) || order.Id.ToString() == search);
        }
        return Ok(await query.OrderByDescending(order => order.CreatedAt).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var order = await Find(id);
        return order == null ? NotFound(new { message = "Order not found." }) : Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create(OrderInput input)
    {
        await gate.Lock.WaitAsync();
        try
        {
            string number = input.OrderNumber.Trim().ToUpperInvariant();
            if (await db.Orders.AnyAsync(order => order.OrderNumber == number))
                return Conflict(new { message = "This order number already exists." });
            var order = new Order();
            Copy(input, order);
            order.Events.Add(new() { Step = "Received", Message = "Order received." });
            order.Events.Add(new() { Step = "Validated", Message = "Required fields passed validation." });
            order.Events.Add(new() { Step = "Saved", Message = "Order stored in SQLite." });
            db.Orders.Add(order);
            await db.SaveChangesAsync();
            logger.LogInformation("Order {OrderNumber} received", order.OrderNumber);
            return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
        }
        finally { gate.Lock.Release(); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, OrderInput input)
    {
        await gate.Lock.WaitAsync();
        try
        {
            var order = await Find(id);
            if (order == null) return NotFound(new { message = "Order not found." });
            if (order.Status is not ("Received" or "Failed"))
                return Conflict(new { message = "Only received or failed orders can be changed." });
            string number = input.OrderNumber.Trim().ToUpperInvariant();
            if (await db.Orders.AnyAsync(item => item.Id != id && item.OrderNumber == number))
                return Conflict(new { message = "This order number already exists." });
            Copy(input, order);
            order.Events.Add(new() { Step = "Updated", Message = "Order details changed; earlier attempts retained." });
            await db.SaveChangesAsync();
            return Ok(order);
        }
        finally { gate.Lock.Release(); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await gate.Lock.WaitAsync();
        try
        {
            var order = await Find(id);
            if (order == null) return NotFound(new { message = "Order not found." });
            if (order.Status == "Sent") return Conflict(new { message = "Wait for the active send to finish." });
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
            return NoContent();
        }
        finally { gate.Lock.Release(); }
    }

    [HttpPost("{id:int}/sync")]
    public Task<IActionResult> Send(int id, SendInput input) => Process(id, input.Scenario, false);

    [HttpPost("{id:int}/retry")]
    public Task<IActionResult> Retry(int id, SendInput input) => Process(id, input.Scenario, true);

    [HttpPost("{id:int}/sync-live")]
    public Task<IActionResult> SendLive(int id) => Process(id, "success", false, true);

    private async Task<IActionResult> Process(int id, string scenario, bool retry, bool live = false)
    {
        await gate.Lock.WaitAsync();
        try
        {
            var order = await Find(id);
            if (order == null) return NotFound(new { message = "Order not found." });
            if (order.Status != (retry ? "Failed" : "Received"))
                return Conflict(new { message = retry ? "Only failed orders can be retried." : "Order already processed. Use Retry for a failed order." });
            await sync.Send(order, scenario, live);
            return Ok(order);
        }
        finally { gate.Lock.Release(); }
    }

    private Task<Order?> Find(int id) => db.Orders.Include(order => order.Attempts)
        .Include(order => order.Events).FirstOrDefaultAsync(order => order.Id == id);

    private static void Copy(OrderInput input, Order order)
    {
        order.OrderNumber = input.OrderNumber.Trim().ToUpperInvariant();
        order.CustomerName = input.CustomerName.Trim();
        order.Product = input.Product.Trim();
        order.Quantity = input.Quantity;
        order.Channel = input.Channel;
    }
}
