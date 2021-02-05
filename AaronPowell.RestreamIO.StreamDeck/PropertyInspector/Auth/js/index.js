/**
 * Bootstrapping function
 *
 * @param {Window & globalThis} globalThis - the global object
 */
((globalThis) => {
  let currentSetupPhase;
  function setStatusBar(view) {
    currentSetupPhase = view;

    let statusCells = document.getElementsByClassName("status-cell");
    Array.from(statusCells).forEach(function (cell) {
      cell.classList.remove("active");
    });

    document.getElementById("status-" + view).classList.add("active");
  }

  const close = () => globalThis.close();

  const handleMissingToken = () => {
    if (currentSetupPhase !== "result") {
      console.log("handleMissingToken called in phase:", currentSetupPhase);
      return;
    }

    globalThis.result.failed();
  };

  globalThis.window.addEventListener("load", () => {
    globalThis.intro.load();
  });

  globalThis.core = { setStatusBar, close, handleMissingToken };
})(window);
