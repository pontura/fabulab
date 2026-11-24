import { getDownloadURL, ref } from 'firebase/storage'
import { storage } from '../firebase'

// Images uploaded by the app to Storage: images/{uid}/{folder}/{id}.jpg (FirebaseFabulabDBManager.DownloadTexture)
export type ImageFolder = 'stories' | 'characters' | 'so' | 'profile'

// One request per image, also when several components ask for it. null: the image does not exist
const urls = new Map<string, Promise<string | null>>()
// Changes when the editor may have replaced images: the browser must not show its cached copies
let version = 0

export function imageUrl(uid: string, folder: ImageFolder, id: string): Promise<string | null> {
  const path = `images/${uid}/${folder}/${id}.jpg`
  let url = urls.get(path)
  if (!url) {
    const v = version
    url = getDownloadURL(ref(storage, path))
      .then((u) => (v > 0 ? `${u}&v=${v}` : u))
      .catch(() => null)
    urls.set(path, url)
  }
  return url
}

// After the editor saved: images are asked again, skipping the browser cache
export function invalidateImages() {
  urls.clear()
  version = Date.now()
}
