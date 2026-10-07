const failureMessage = "The program closed suddenly.";                                       // Stores our message as text in a variable.
console.log(failureMessage);                                                                 // Shows the message in the browser console.
document.getElementById("practice-output").textContent = failureMessage;                     // Finds our paragraph and shows the message on the page.
const totalFailures = 5;                                                                     // Five problems have been reported.
const fixedFailures = 2 ;                                                                    // Two problems have been fixed.
const remainingFailures = totalFailures - fixedFailures;                                     // Subtract and store how many remain.

console.log(remainingFailures);                                                              // Shows 3 in the browser console.
document.getElementById("practice-output").textContent = failureMessage + " Remaining failures: " + remainingFailures; // Shows the message and calculated number on the webpage.

const statusButton = document.getElementById("check-status");                                // Finds the button.

statusButton.addEventListener("click", function () {                                         // Runs when you click.
    let statusMessage = "";                                                                  // Stores the message we will choose.

    if (remainingFailures === 0 && totalFailures > 0) {                                      // No failures remain AND some were reported.
        statusMessage = "All reported failures are fixed!";
    } else {                                                                                 // Otherwise, show how many remain.
        statusMessage = "Remaining failures: " + remainingFailures;
    }

    document.getElementById("practice-output").textContent = statusMessage;                  // Shows it on the page.
    console.log(statusMessage);                                                              // Shows it in the console.
});

    