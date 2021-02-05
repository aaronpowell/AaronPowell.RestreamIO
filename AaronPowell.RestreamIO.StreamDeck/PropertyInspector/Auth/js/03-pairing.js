/**
 * Bootstrapping function
 *
 * @param {Window & globalThis} globalThis - the global object
 */
((globalThis) => {
  const { document, window } = globalThis;
  const { close, setStatusBar } = globalThis.core;

  let pairingEnabled = false;
  const load = () => {
    console.log("Pairing view loaded");
    setStatusBar("pairing");

    document.getElementById("title").innerHTML = "Link Account";

    const content = `
        <div class="leftAlign">
            <p class="leftAlign">Linking your Restream account with the plugin.</p>
            <p class="leftAlign">
                Please wait...
                <img class="imageSmall" src="images/loading.gif">
            </p>
            <hr/>
            <br/>
        </div>
        <div id="controls"></div>`;
    document.getElementById("content").innerHTML = content;

    if (pairingEnabled) {
      enablePairing();
    }

    autoPairing();
  };

  function enablePairing() {
    console.log("Pairing enabled");
    pairingEnabled = true;

    const content = `
        <div class='leftAlign'>
            <p class='leftAlign'>Linking your Restream account with the plugin.</p>
            <p class='leftAlign'>You need to generate an Access Token by <span class='linkspan' onclick='window.opener.openRestreamAuth()'>clicking here</span> then paste the token below to finish the account linking.</p>
            <hr/>
            <br/>
        </div>
      <div id='controls'></div>
    `;
    document.getElementById("content").innerHTML = content;

    autoPairing();
  }

  function autoPairing() {
    const controls = `
      <div class='inputTitle'>Approval Code:</div>
      <input type='text' class='approvalCode' placeholder='Code goes here' value='' id='approvalCode'>
      <br/>
      <div class='button' id='submit'>Submit Code</div>
      <div class='button-transparent' id='close'>Close</div>`;
    document.getElementById("controls").innerHTML = controls;

    document.getElementById("submit").addEventListener("click", submit);
    document.getElementById("close").addEventListener("click", close);
  }

  function submit() {
    const approvalCode = document.getElementById("approvalCode");
    window.opener.updateApprovalCode(approvalCode.value);
    console.log("Loading validation view");
    globalThis.result.load();
  }

  globalThis.pairing = {
    load,
    enablePairing,
  };
})(window);
