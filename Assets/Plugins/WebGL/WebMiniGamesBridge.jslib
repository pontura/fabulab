// Notifies the web page that embeds the minigames (scene MiniGames, Unity WebGL in an iframe of web/client):
//   window.addEventListener('message', e => { if (e.data.source === 'fabulab-minigames') ... })
// Only to a parent page served from the same origin
mergeInto(LibraryManager.library, {
  FabulabMiniGamesNotify: function (eventPtr, payloadPtr) {
    var eventName = UTF8ToString(eventPtr);
    var payloadText = UTF8ToString(payloadPtr);
    var payload = null;
    try { payload = JSON.parse(payloadText); } catch (e) { payload = payloadText; }

    if (window.parent && window.parent !== window) {
      window.parent.postMessage({ source: 'fabulab-minigames', event: eventName, payload: payload }, window.location.origin);
    }
  }
});
