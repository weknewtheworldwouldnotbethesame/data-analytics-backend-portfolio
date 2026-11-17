using System.Diagnostics;
using System.Text.Json;
using OrderSync.Api.Data;
using OrderSync.Api.Models;

namespace OrderSync.Api.Services;

public class SyncService(OrderDb db, IHttpClientFactory clients, IConfiguration config,
                         ILogger<SyncService> logger)
{
    // The controller holds the write gate before calling this method.
    public async Task Send(Order order, string scenario, bool live = false)
    {
        var attempt = new SyncAttempt { OrderId = order.Id, AttemptedAt = DateTime.UtcNow };
        order.Status = "Sent";
        order.Events.Add(new() { Step = "Sent", Message = live ? "Sent to Postman Echo." : "Sent to the local receiver." });
        db.Attempts.Add(attempt);
        await db.SaveChangesAsync();
        logger.LogInformation("Order {OrderNumber} sent", order.OrderNumber);
        var timer = Stopwatch.StartNew();
        try
        {
            var url = live ? "https://postman-echo.com/post" : config["ReceiverUrl"]!;
            var payload = new { orderNumber = order.OrderNumber, product = order.Product,
                                quantity = order.Quantity, channel = order.Channel, scenario };
            using var response = await clients.CreateClient("receiver").PostAsJsonAsync(url, payload);
            attempt.ResponseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Fail(order, attempt, "HTTP " + (int)response.StatusCode,
                     "Receiver returned HTTP " + (int)response.StatusCode + ".");
            }
            else
            {
                using var json = JsonDocument.Parse(attempt.ResponseBody);
                if (!json.RootElement.TryGetProperty("json", out var echoed) ||
                    !echoed.TryGetProperty("orderNumber", out var number) ||
                    number.GetString() != order.OrderNumber)
                    throw new JsonException("Receiver did not confirm the matching order number.");
                order.Status = "Completed";
                order.CompletedAt = DateTime.UtcNow;
                order.LastError = "";
                attempt.Outcome = "Completed";
                order.Events.Add(new() { Step = "Completed", Message = "Matching order received successfully." });
            }
        }
        catch (TaskCanceledException)
        {
            Fail(order, attempt, "Timeout", "No response within 3 seconds. Check the receiver before retrying.");
        }
        catch (HttpRequestException)
        {
            Fail(order, attempt, "Connection", "Cannot reach the receiver. Check that it is running.");
        }
        catch (JsonException)
        {
            Fail(order, attempt, "Invalid response", "The receiver response did not confirm this order.");
        }
        finally
        {
            attempt.DurationMs = timer.ElapsedMilliseconds;
            await db.SaveChangesAsync();
            logger.LogInformation("Order {OrderNumber}: {Status}", order.OrderNumber, order.Status);
        }
    }

    private static void Fail(Order order, SyncAttempt attempt, string type, string message)
    {
        order.Status = "Failed";
        order.LastError = message;
        attempt.Outcome = "Failed";
        attempt.ErrorType = type;
        attempt.ErrorMessage = message;
        order.Events.Add(new() { Step = "Failed", Message = message });
    }
}
