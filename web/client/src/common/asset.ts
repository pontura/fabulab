// URL of a file of public/ (images, the Unity builds). The site can be served from a subfolder
// (vite.config base './'), so paths must not start at the root of the domain
export function asset(path: string): string {
  return import.meta.env.BASE_URL + path.replace(/^\//, '')
}
