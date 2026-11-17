using System.ComponentModel.DataAnnotations;

namespace OrderSync.Api.Models;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Product { get; set; } = "";
    public int Quantity { get; set; }
    public string Channel { get; set; } = "";
    public string Status { get; set; } = "Received";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string LastError { get; set; } = "";
    public string DataOrigin { get; set; } = "Fictional interactive order";
    public List<SyncAttempt> Attempts { get; set; } = new();
    public List<OrderEvent> Events { get; set; } = new();
}

public class OrderInput
{
    [Required, StringLength(40)] public string OrderNumber { get; set; } = "";
    [Required, StringLength(80)] public string CustomerName { get; set; } = "";
    [Required, StringLength(80)] public string Product { get; set; } = "";
    [Range(1, 10000)] public int Quantity { get; set; }
    [Required, RegularExpression("Web|Store|Partner")] public string Channel { get; set; } = "Web";
}

public class SendInput
{
    [Required, RegularExpression("success|unavailable|timeout|invalid-response")]
    public string Scenario { get; set; } = "success";
}

public class SyncAttempt
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DateTime AttemptedAt { get; set; }
    public string Outcome { get; set; } = "";
    public string ErrorType { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public string ResponseBody { get; set; } = "";
    public long DurationMs { get; set; }
}

public class OrderEvent
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DateTime At { get; set; } = DateTime.UtcNow;
    public string Step { get; set; } = "";
    public string Message { get; set; } = "";
}
