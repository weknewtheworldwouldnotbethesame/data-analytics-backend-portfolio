using OrderSync.Api.Models;

namespace OrderSync.Api.Data;

public static class SeedOrders
{
    // Fixed fictional history gives the report a repeatable 14-day window.
    // These records are not actual shipments or claims about a real business.
    public static void AddIfEmpty(OrderDb db)
    {
        if (db.Orders.Any()) return;
        for (int i = 1; i <= 42; i++)
        {
            var date = new DateTime(2026, 11, 1, 9, 0, 0, DateTimeKind.Utc).AddDays((i - 1) / 3).AddMinutes(i * 2);
            bool failedBefore = i % 3 == 0;
            bool remainsFailed = i % 6 == 0;
            bool pending = i % 10 == 0;
            var order = new Order
            {
                OrderNumber = "OS-" + (1000 + i), CustomerName = "Customer " + i,
                Product = new[] { "Keyboard", "Headset", "Monitor" }[i % 3],
                Quantity = 1 + i % 4, Channel = new[] { "Web", "Store", "Partner" }[(i / 3) % 3],
                CreatedAt = date, DataOrigin = "Fictional historical fixture",
                Status = pending ? "Received" : remainsFailed ? "Failed" : "Completed"
            };
            foreach (var step in new[] { "Received", "Validated", "Saved" })
                order.Events.Add(new() { At = date, Step = step, Message = "Fictional history: " + step });
            if (!pending)
            {
                order.Events.Add(new() { At = date.AddSeconds(1), Step = "Sent", Message = "Fictional history: sent to receiver." });
                if (failedBefore)
                {
                    string type = i % 4 == 0 ? "Timeout" : "HTTP 503";
                    order.Attempts.Add(new() { AttemptedAt = date.AddSeconds(1), Outcome = "Failed",
                        ErrorType = type, ErrorMessage = "Receiver unavailable.", DurationMs = type == "Timeout" ? 3000 : 180,
                        ResponseBody = "{\"error\":\"Receiver unavailable\"}" });
                    order.Events.Add(new() { At = date.AddSeconds(4), Step = "Failed", Message = type });
                }
                if (!remainsFailed)
                {
                    var complete = failedBefore ? date.AddMinutes(10 + i) : date.AddSeconds(2);
                    order.Attempts.Add(new() { AttemptedAt = complete.AddSeconds(-1), Outcome = "Completed",
                        DurationMs = 120 + i * 3, ResponseBody = "{\"json\":{\"orderNumber\":\"" + order.OrderNumber + "\"}}" });
                    order.CompletedAt = complete;
                    order.Events.Add(new() { At = complete, Step = "Completed", Message = "Fictional history: confirmed." });
                }
                else order.LastError = "Receiver unavailable.";
            }
            db.Orders.Add(order);
        }
        db.SaveChanges();
    }
}
