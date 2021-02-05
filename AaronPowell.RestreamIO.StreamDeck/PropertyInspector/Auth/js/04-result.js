/**
 * Bootstrapping function
 *
 * @param {Window & globalThis} globalThis - the global object
 */
((globalThis) => {
  const { document, window } = globalThis;
  const { close, setStatusBar } = globalThis.core;

  const load = () => {
    console.log("loading result view");
    setStatusBar("result");

    document.getElementById("title").innerHTML = "Validating access";

    var content = `
      <p>Validating access, please stand by...</p>
      <div id='loader'></div>
      <div class='button' id='close'>Close</div>`;
    document.getElementById("content").innerHTML = content;

    document.getElementById("close").addEventListener("click", close);
  };

  const success = () => {
    console.log("loadSuccessView called!");
    // Set the status bar
    setStatusBar("result");

    // Fill the title
    document.getElementById("title").innerHTML =
      "Validating access - success 😁";

    // Fill the content area
    var content = `
      <p>Restream connect validated, you're ready to go!</p>
      <img class='image' src='images/paired.png'>
      <div class='button' id='close'>Close</div>`;
    document.getElementById("content").innerHTML = content;

    // Add event listener
    document.getElementById("close").addEventListener("click", close);
  };

  const failed = () => {
    console.log("loading failed view");
    setStatusBar("result");

    // Fill the title
    document.getElementById("title").innerHTML =
      "Validating access - Failed 😣";

    // Fill the content area
    var content = `
      <p>Failed to validate access. Please start again and ensure you paste in your Client ID, Client Secret and Access Token completely.</p>
      <img class='image' src='images/fail.png'>
      <div class='button' id='failRetry'>Retry</div>
      <div class='button-transparent' id='close'>Close</div>
    `;
    document.getElementById("content").innerHTML = content;

    document.getElementById("close").addEventListener("click", close);
    document.getElementById("failRetry").addEventListener("click", failRetry);

    function failRetry() {
      console.log("failRetry called!");
      document.removeEventListener("close", close);
      document.removeEventListener("failRetry", failRetry);

      globalThis.intro.load();
    }
  };

  globalThis.results = {
    load,
    success,
    failed,
  };
})(window);
