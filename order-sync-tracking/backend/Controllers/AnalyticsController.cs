using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderSync.Api.Data;

namespace OrderSync.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController(OrderDb db) : ControllerBase
{
    [HttpGet("orders")]
    public async Task<IActionResult> Orders() => Ok(await Rows());

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var csv = new StringBuilder("OrderId,OrderNumber,OrderDate,Channel,Product,Quantity,Status,AttemptCount,FailureCount,RetryCount,Recovered,LastFailureType,ResolutionMinutes,DataOrigin\r\n");
        foreach (var row in await Rows())
        {
            object?[] values = { row.OrderId, row.OrderNumber, row.OrderDate, row.Channel, row.Product,
                row.Quantity, row.Status, row.AttemptCount, row.FailureCount, row.RetryCount,
                row.Recovered, row.LastFailureType, row.ResolutionMinutes, row.DataOrigin };
            csv.AppendLine(string.Join(",", values.Select(value => "\"" +
                Convert.ToString(value, CultureInfo.InvariantCulture)?.Replace("\"", "\"\"") + "\"")));
        }
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "orders.csv");
    }

    private async Task<List<AnalyticsRow>> Rows()
    {
        var orders = await db.Orders.AsNoTracking().Include(order => order.Attempts).OrderBy(order => order.Id).ToListAsync();
        var result = new List<AnalyticsRow>();
        foreach (var order in orders)
        {
            var failures = order.Attempts.Where(attempt => attempt.Outcome == "Failed").OrderBy(attempt => attempt.AttemptedAt).ToList();
            bool recovered = failures.Count > 0 && order.Status == "Completed";
            result.Add(new AnalyticsRow
            {
                OrderId = order.Id, OrderNumber = order.OrderNumber,
                OrderDate = order.CreatedAt.ToString("yyyy-MM-dd"), Channel = order.Channel,
                Product = order.Product, Quantity = order.Quantity, Status = order.Status,
                AttemptCount = order.Attempts.Count, FailureCount = failures.Count,
                RetryCount = Math.Max(0, order.Attempts.Count - 1), Recovered = recovered ? 1 : 0,
                LastFailureType = failures.LastOrDefault()?.ErrorType ?? "None",
                ResolutionMinutes = recovered ? Math.Round((order.CompletedAt!.Value - failures[0].AttemptedAt).TotalMinutes, 2) : null,
                DataOrigin = order.DataOrigin
            });
        }
        return result;
    }
}

public class AnalyticsRow
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = "";
    public string OrderDate { get; set; } = "";
    public string Channel { get; set; } = "";
    public string Product { get; set; } = "";
    public int Quantity { get; set; }
    public string Status { get; set; } = "";
    public int AttemptCount { get; set; }
    public int FailureCount { get; set; }
    public int RetryCount { get; set; }
    public int Recovered { get; set; }
    public string LastFailureType { get; set; } = "";
    public double? ResolutionMinutes { get; set; }
    public string DataOrigin { get; set; } = "";
}
