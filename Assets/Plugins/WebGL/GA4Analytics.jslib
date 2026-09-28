mergeInto(LibraryManager.library, {
  SendAnalyticsEvent: function(eventNamePtr, paramsPtr) {
    // Convertir los punteros de C# a strings
    var eventName = UTF8ToString(eventNamePtr);
    var paramsJson = UTF8ToString(paramsPtr);

    try {
      var params = JSON.parse(paramsJson);
      // Usar gtag.js para enviar el evento
      gtag('event', eventName, params);
      console.log("[GA4Analytics.jslib] Evento enviado:", eventName, params);
    } catch (e) {
      console.error("[GA4Analytics.jslib] Error parseando parámetros:", e);
    }
  }
});

