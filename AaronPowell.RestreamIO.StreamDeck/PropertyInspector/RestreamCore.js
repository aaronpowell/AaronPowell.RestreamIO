const redirectUri = "https://localhost:44376/";
const state = "33f4ec41-960d-4b71-8791-df35a6d08132";

let authWindow;
let clientId;

document.addEventListener("websocketCreate", function () {
  console.log("Websocket created!");
  checkToken(actionInfo.payload.settings);

  websocket.addEventListener("message", function (event) {
    console.log("Got message event!");

    // Received message from Stream Deck
    var jsonObj = JSON.parse(event.data);

    if (jsonObj.event === "sendToPropertyInspector") {
      var payload = jsonObj.payload;
      checkToken(payload);
    } else if (jsonObj.event === "didReceiveSettings") {
      var payload = jsonObj.payload;
      checkToken(payload.settings);
    }
  });
});

function checkToken(payload) {
  console.log("Checking Token...");

  if (payload.clientId) {
    console.log("Got new client id!");
    clientId = payload.clientId;
    if (authWindow) {
      console.log("Enabling pairing");
      authWindow.pairing.enablePairing();
    }
    return;
  }

  const tokenExists = document.getElementById("tokenExists");
  tokenExists.value = payload.tokenExists;

  if (payload.tokenExists) {
    setSettingsWrapper("");
    var event = new Event("tokenExists");
    document.dispatchEvent(event);

    if (authWindow) {
      console.log("Loading Success View");
      authWindow.results.success();
    }
  } else {
    setSettingsWrapper("none");
    if (authWindow) {
      console.log("Setup handling missing token");
      authWindow.core.handleMissingToken();
    } else {
      if (!clientId) {
        getClientId();
      }
      console.log("Loading Setup Wizard");
      authWindow = window.open("Auth/index.html");
    }
  }
}

function setSettingsWrapper(displayValue) {
  const sdWrapper = document.getElementById("sdWrapper");
  sdWrapper.style.display = displayValue;
}

function updateAPIKeys(clientId, secretId) {
  var payload = {
    property_inspector: "updateAPI",
    clientId,
    secretId,
  };
  console.log("Approving API", payload.clientId);
  sendPayloadToPlugin(payload);
}

function updateApprovalCode(approvalCode) {
  var payload = {
    property_inspector: "updateApprovalCode",
    approvalCode,
  };
  console.log("Setting approval code", payload.approvalCode);
  sendPayloadToPlugin(payload);
}

function getClientId() {
  console.log("Getting client id");

  if (clientId) {
    console.log("Client id exists");
    if (authWindow) {
      console.log("Enabling pairing");
      authWindow.pairing.enablePairing();
    }
    return;
  }

  var payload = { property_inspector: "getClientId" };
  sendPayloadToPlugin(payload);
}

function openRestreamAuth() {
  if (!clientId) {
    console.log("openRestreamAuth with no clientId");
    getClientId();
    return;
  }

  if (websocket && websocket.readyState === 1) {
    const json = {
      event: "openUrl",
      payload: {
        url: `https://api.restream.io/login?response_type=code&client_id=${clientId}&redirect_uri=${redirectUri}&state=${state}`,
      },
    };
    websocket.send(JSON.stringify(json));
  }
}

function openRestreamHelp() {
  if (websocket && websocket.readyState === 1) {
    const json = {
      event: "openUrl",
      payload: {
        url: "https://developers.restream.io/docs#getting-started",
      },
    };
    websocket.send(JSON.stringify(json));
  }
}

function openRestreamDashboard() {
  if (websocket && websocket.readyState === 1) {
    const json = {
      event: "openUrl",
      payload: {
        url: "https://developers.restream.io/apps",
      },
    };
    websocket.send(JSON.stringify(json));
  }
}
