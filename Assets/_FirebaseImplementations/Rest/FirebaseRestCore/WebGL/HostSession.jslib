// Session of the web page that embeds this player (see FirebaseRestHostSession.cs).
// Only a parent page of the same origin can give it.
mergeInto(LibraryManager.library, {
  FabulabGetHostSession: function () {
    var text = '';
    try {
      if (window.parent && window.parent !== window && window.parent.fabulabHostSession)
        text = JSON.stringify(window.parent.fabulabHostSession);
    } catch (e) {
      // Another origin: no access to the parent page
      text = '';
    }
    var size = lengthBytesUTF8(text) + 1;
    var buffer = _malloc(size);
    stringToUTF8(text, buffer, size);
    return buffer;
  }
});
