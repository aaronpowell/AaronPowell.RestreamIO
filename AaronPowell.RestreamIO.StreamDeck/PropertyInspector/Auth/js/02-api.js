/**
 * Bootstrapping function
 *
 * @param {Window & globalThis} globalThis - the global object
 */
((globalThis) => {
  const { document, window } = globalThis;
  const { close, setStatusBar } = globalThis.core;

  const submitAPI = () => {
    const clientId = document.getElementById("clientId");
    const secretId = document.getElementById("clientSecret");
    window.opener.updateAPIKeys(clientId.value, secretId.value);
    window.opener.getClientId();
    globalThis.pairing.load();
  };

  const load = () => {
    setStatusBar("api");

    document.getElementById("title").innerHTML = "API Key";

    var content = `
        <div class="leftAlign">
            <p class="leftAlign">To use this plugin you need to provide a 'Client Id' and 'Client Secret'. For instructions on how to get them <span class="linkspan" onclick="window.opener.openRestreamHelp()">click here</span>.</p>
            <hr/>
            <br/>
        </div>
        <p class="small leftAlign">(If you ALREADY COMPLETED above instructions): <span class="linkspan" onclick="window.opener.openRestreamDashboard()">click here</span>.</p>
        <div>
            <div><label for="clientId">Client ID:</label><input type="text" class="approvalCode" id="clientId" value="" /></div>
            <div><label for="clientId">Client Secret:</label><input type="text" class="approvalCode" id="clientSecret" value="" /></div>
            <div><em>Don't share your Client ID or Secret with anyone!</em><div class="button" id="submitAPI">Submit</div></div>
            <div class="button-transparent" id="close">Close</div>
        </div>`;
    document.getElementById("content").innerHTML = content;

    document.getElementById("submitAPI").addEventListener("click", submitAPI);
    document.getElementById("close").addEventListener("click", close);
  };

  globalThis.api = {
    load,
  };
})(window);
