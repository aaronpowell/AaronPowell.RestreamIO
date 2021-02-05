/**
 * Bootstrapping function
 *
 * @param {Window & globalThis} globalThis - the global object
 */
((globalThis) => {
  const { document } = globalThis;
  const { close, setStatusBar } = globalThis.core;

  const startPairing = () => {
    globalThis.api.load();
  };

  const load = () => {
    setStatusBar("intro");

    document.getElementById("title").innerHTML = "RestreamIO Setup";

    const template = `
        <p>You will need a <a href="https://restream.io" target="_blank">Restream</a> account to use this plugin.</p>
        <div class="button" id="start">Authorise Plugin</div>
        <div class="button-transparent" id="close">Abort</div>
    `;

    document.getElementById("content").innerHTML = template;
    document.getElementById("start").addEventListener("click", startPairing);
    document.getElementById("close").addEventListener("click", close);
  };

  globalThis.intro = {
    load,
  };
})(window);
