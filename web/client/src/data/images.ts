import { getDownloadURL, ref } from 'firebase/storage'
import { storage } from '../firebase'

// Images uploaded by the app to Storage: images/{uid}/{folder}/{id}.jpg (FirebaseFabulabDBManager.DownloadTexture)
export type ImageFolder = 'stories' | 'characters' | 'so' | 'profile'

// One request per image, also when several components ask for it. null: the image does not exist
const urls = new Map<string, Promise<string | null>>()

export function imageUrl(uid: string, folder: ImageFolder, id: string): Promise<string | null> {
  const path = `images/${uid}/${folder}/${id}.jpg`
  let url = urls.get(path)
  if (!url) {
    url = getDownloadURL(ref(storage, path)).catch(() => null)
    urls.set(path, url)
  }
  return url
}
