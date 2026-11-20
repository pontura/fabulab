// Notifies the HTML page that embeds the story player (Unity WebGL).
// The page can listen in two ways:
//  - same page:   window.addEventListener('fabulab-story-player', e => e.detail.event / e.detail.payload)
//  - from a parent page (player inside an iframe):
//                 window.addEventListener('message', e => { if (e.data.source === 'fabulab-story-player') ... })
mergeInto(LibraryManager.library, {
  FabulabStoryPlayerNotify: function (eventPtr, payloadPtr) {
    var eventName = UTF8ToString(eventPtr);
    var payloadText = UTF8ToString(payloadPtr);
    var payload = null;
    try { payload = JSON.parse(payloadText); } catch (e) { payload = payloadText; }

    window.dispatchEvent(new CustomEvent('fabulab-story-player', { detail: { event: eventName, payload: payload } }));

    if (window.parent && window.parent !== window) {
      window.parent.postMessage({ source: 'fabulab-story-player', event: eventName, payload: payload }, '*');
    }
  }
});
