// Wait for the HTML to fully load before running the script
document.addEventListener("DOMContentLoaded", () => {

    // Log message to the browser console
    console.log("OnyxServer: script.js loaded successfully! No intergalactic errors here.");

    // Get the HTML elements
    const btn = document.getElementById("test-btn");
    const output = document.getElementById("js-output");
    let clickCount = 0;

    // Add a click event to the button
    btn.addEventListener("click", () => {
        clickCount++;

        // Show success message with counter
        output.innerText = `Pong! Button clicked ${clickCount} time(s). JavaScript is fully operational!`;

        // Optional: Animate the button slightly
        btn.style.transform = "scale(0.95)";
        setTimeout(() => {
            btn.style.transform = "scale(1)";
        }, 100);
    });
});