# Fabulab Admin

Panel interno para moderar contenido de usuarios (personajes, objetos de escena,
historias) y gestionar tags, presets, games y usuarios en la Realtime Database de
Firebase (`fabu-lab`).

## Setup

1. `npm install`
2. Copiar `.env.example` a `.env` y completar con la config del Web App de Firebase
   (Firebase Console → Project settings → General → Your apps → Fabulab Admin).
3. `npm run dev` y abrir la URL que muestra Vite (por defecto http://localhost:5173).

Login con la cuenta admin configurada en Firebase Authentication.

## Producción

El panel está deployado en **https://fabulab-admin.web.app** (Firebase Hosting, site
`fabulab-admin`, separado del site default del proyecto).

Para publicar una nueva versión:

```
npm run build
cd ../..            # raíz del repo, donde está firebase.json
firebase deploy --only hosting --project fabu-lab
```

`npm run build` lee `.env` en build-time y lo embebe en el bundle — necesitás tener
ese archivo local antes de buildear (no está en git).

## Limitaciones v1

- No hay renderizado visual de personajes/escenas: `galleryID`/`part`/`color` son
  índices a assets de Unity que no se pueden resolver desde el navegador. La
  moderación es sobre datos (nombre, tags, visibilidad, likes, texto de diálogos),
  no visual.
- Eliminar un usuario borra sus datos de Realtime Database pero no su cuenta de
  Firebase Authentication (requiere Admin SDK / Cloud Function, fuera de alcance).
- La página de login es pública (cualquiera puede llegar a la URL), pero sin las
  credenciales de la cuenta admin no se puede hacer nada: las reglas de RTDB sólo
  dan acceso de lectura/escritura amplio a ese UID específico.
