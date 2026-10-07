using System.Collections.Generic;                                               // Read: Make List available; later we use it to group failures.
using System.IO;                                                                // Read: Make file tools available; later File.WriteAllText saves the JSON.
using System.Text.Json;                                                         // Read: Make JSON tools available; later they turn the list into JSON text.


Console.WriteLine("Order Sync Failure Tracking Dashboard");                     // Read: Print the dashboard title; next print its divider.
Console.WriteLine("-------------------------------------");                     // Read: Print a divider; next create the failure records.


// Create failure 1
OrderFailure failure1 = new OrderFailure();                                     // Read: Create a new failure record named failure1; next fill in its properties.

failure1.OrderId = 1001;                                                        // Read: Set failure1's order ID to 1001; next set its error message.
failure1.ErrorMessage = "The warehouse did not answer";                         // Read: Store this error message in failure1; next set its resolved status.
failure1.IsResolved = false;                                                    // Read: Mark failure1 as unresolved; this record is ready to add to the list.


// Create failure 2
OrderFailure failure2 = new OrderFailure();                                     // Read: Create a new failure record named failure2; next fill in its properties.

failure2.OrderId = 1002;                                                        // Read: Set failure2's order ID to 1002; next set its error message.
failure2.ErrorMessage = "The product was not found";                            // Read: Store this error message in failure2; next set its resolved status.
failure2.IsResolved = true;                                                     // Read: Mark failure2 as resolved; this record is ready to add to the list.


// Create failure 3
OrderFailure failure3 = new OrderFailure();                                     // Read: Create a new failure record named failure3; next fill in its properties.

failure3.OrderId = 1003;                                                        // Read: Set failure3's order ID to 1003; next set its error message.
failure3.ErrorMessage = "The address is missing";                               // Read: Store this error message in failure3; next set its resolved status.
failure3.IsResolved = true;                                                     // Read: Mark failure3 as resolved; this record is ready to add to the list.


// Create a list to hold all failures
List<OrderFailure> failures = new List<OrderFailure>();                         // Read: Create an empty list of OrderFailure objects; next add records. failures.Count would give its total.

failures.Add(failure1);                                                         // Read: Add failure1 to the list; next add the following record.
failures.Add(failure2);                                                         // Read: Add failure2 to the list; next add the following record.
failures.Add(failure3);                                                         // Read: Add failure3 to the list; the list now holds all three records, ready to display.


// Show all failures
ShowAllFailures(failures);                                                      // Read: Call ShowAllFailures with this list; after it finishes, count unresolved records.


// Count unresolved failures
int unresolvedCount = CountUnresolved(failures);                                // Read: Count unresolved records and store the returned number; next print it.

Console.WriteLine();                                                            // Read: Print a blank line; the next output starts below it.
Console.WriteLine("Unresolved failures: " + unresolvedCount);                   // Read: Join this label with the count and print it; then continue to the next step.


// Resolve order 1001
ResolveFailure(failures, 1001);                                                 // Read: Call ResolveFailure to mark order 1001 resolved; next show the updated list.


// Show failures again
Console.WriteLine();                                                            // Read: Print a blank line; the next output starts below it.
Console.WriteLine("After resolving Order 1001:");                               // Read: Print a heading for the update; next show the failures again.
ShowAllFailures(failures);                                                      // Read: Call ShowAllFailures with this list; after it finishes, count unresolved records.


// Count unresolved failures again
unresolvedCount = CountUnresolved(failures);                                    // Read: Count again after resolving the order; replace the old number, then print it.

Console.WriteLine();                                                            // Read: Print a blank line; the next output starts below it.
Console.WriteLine("Unresolved failures: " + unresolvedCount);                   // Read: Join this label with the count and print it; then continue to the next step.


// Save failures to JSON
SaveFailures(failures, "failures.json");                                        // Read: Pass the list and file name to SaveFailures; it writes JSON before we print confirmation.

Console.WriteLine();                                                            // Read: Print a blank line; the next output starts below it.
Console.WriteLine("Failures saved to failures.json");                           // Read: Print the save confirmation; the main program has finished its steps.


// ---------------- METHODS ----------------


void ShowAllFailures(List<OrderFailure> failureList)                            // Read: Define a method that receives a failure list and returns no value; its body prints each record.
{
    Console.WriteLine();                                                        // Read: Print a blank line; the next output starts below it.
    Console.WriteLine("All Order Failures:");                                   // Read: Print the list heading; next loop through its records.

    foreach (OrderFailure failure in failureList)                               // Read: Take each record from the list as failure; run the following block once per record.
    {
        Console.WriteLine("-------------------");                               // Read: Print a record divider; next print this record's details.
        Console.WriteLine("Order ID: " + failure.OrderId);                      // Read: Print this record's order ID; next print its error message.
        Console.WriteLine("Error: " + failure.ErrorMessage);                    // Read: Print this record's error; next print whether it is resolved.
        Console.WriteLine("Resolved: " + failure.IsResolved);                   // Read: Print its true/false resolved status; the loop then moves to the next record.
    }
}


int CountUnresolved(List<OrderFailure> failureList)                             // Read: Define a method that receives a list and returns an integer; its body calculates the count.
{
    int count = 0;                                                              // Read: Start the counter at zero; next check each failure.

    foreach (OrderFailure failure in failureList)                               // Read: Take each record from the list as failure; run the following block once per record.
    {
        if (failure.IsResolved == false)                                        // Read: If this failure is unresolved, run the block; otherwise skip it (there is no else block).
        {
            count++;                                                            // Read: Add one to count for this unresolved failure; next continue the loop.
        }
    }

    return count;                                                               // Read: Send the final count back and leave this method; the caller stores the result.
}


void ResolveFailure(List<OrderFailure> failureList, int orderId)                // Read: Define a method receiving a list and target order ID; its body searches for matching records.
{
    foreach (OrderFailure failure in failureList)                               // Read: Take each record from the list as failure; run the following block once per record.
    {
        if (failure.OrderId == orderId)                                         // Read: If this record's ID equals the target ID, run the block; otherwise check the next record.
        {
            failure.IsResolved = true;                                          // Read: Mark the matching record resolved; next print a confirmation.

            Console.WriteLine();                                                // Read: Print a blank line; the next output starts below it.
            Console.WriteLine("Order " + orderId + " has been resolved.");      // Read: Print confirmation for this order; the loop then checks any remaining records.
        }
    }
}


void SaveFailures(List<OrderFailure> failureList, string fileName)              // Read: Define a method receiving a list and file name; its body converts and saves the data.
{
    JsonSerializerOptions options = new JsonSerializerOptions();                // Read: Create JSON formatting settings named options; next enable indentation.

    options.WriteIndented = true;                                               // Read: Turn on readable spacing and line breaks; next use these settings to make JSON.

    string json = JsonSerializer.Serialize(failureList, options);               // Read: Convert the list into JSON text using options and store it in json; next write that text to a file.

    File.WriteAllText(fileName, json);                                          // Read: Write the JSON text to this file, replacing existing contents; then return to the caller.
}


// ---------------- CLASS ----------------


class OrderFailure                                                              // Read: Define the blueprint for each failure record; the properties below hold its data.
{
    public int OrderId { get; set; }                                            // Read: Declare a whole-number order ID that can be read or changed; next define the error text.

    public string ErrorMessage { get; set; } = "";                              // Read: Declare readable/changeable error text, initially empty; next define the resolved status.

    public bool IsResolved { get; set; }                                        // Read: Declare a readable/changeable true/false status, initially false; next define the time.

    public DateTime FailedAt { get; set; } = DateTime.Now;                      // Read: Store the current local date and time when this object is created; it is also included in the saved JSON.
}