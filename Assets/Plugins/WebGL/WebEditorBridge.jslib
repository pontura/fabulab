// Notifies the web page that embeds the editor (scene Editor, Unity WebGL in an iframe of web/client):
//   window.addEventListener('message', e => { if (e.data.source === 'fabulab-editor') ... })
// Only to a parent page served from the same origin
mergeInto(LibraryManager.library, {
  FabulabEditorNotify: function (eventPtr, payloadPtr) {
    var eventName = UTF8ToString(eventPtr);
    var payloadText = UTF8ToString(payloadPtr);
    var payload = null;
    try { payload = JSON.parse(payloadText); } catch (e) { payload = payloadText; }

    if (window.parent && window.parent !== window) {
      window.parent.postMessage({ source: 'fabulab-editor', event: eventName, payload: payload }, window.location.origin);
    }
  }
});
