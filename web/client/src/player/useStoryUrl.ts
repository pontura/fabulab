import { useCallback, useEffect, useState } from 'react'

// The story being watched lives in the URL (?story=<id>), so it can be shared by copying the link
const PARAM = 'story'
// Marks the history entries this site pushed, so closing can go back instead of adding another one
const STATE_KEY = 'fabulabStory'

function storyIdFromUrl(): string | null {
  return new URL(window.location.href).searchParams.get(PARAM) || null
}

function urlWithStory(id: string | null): string {
  const url = new URL(window.location.href)
  if (id) url.searchParams.set(PARAM, id)
  else url.searchParams.delete(PARAM)
  return url.pathname + url.search + url.hash
}

export function useStoryUrl() {
  const [storyId, setStoryId] = useState<string | null>(storyIdFromUrl)

  // Browser back / forward: the player follows the URL
  useEffect(() => {
    const onPopState = () => setStoryId(storyIdFromUrl())
    window.addEventListener('popstate', onPopState)
    return () => window.removeEventListener('popstate', onPopState)
  }, [])

  const openStory = useCallback((id: string) => {
    if (storyIdFromUrl() === id) return setStoryId(id)
    window.history.pushState({ [STATE_KEY]: id }, '', urlWithStory(id))
    setStoryId(id)
  }, [])

  const closeStory = useCallback(() => {
    // Opened from this site: going back removes the entry, as the browser back button would
    if (window.history.state?.[STATE_KEY]) {
      window.history.back()
      return
    }
    // Arrived with a shared link: there is nothing of ours to go back to
    window.history.replaceState(null, '', urlWithStory(null))
    setStoryId(null)
  }, [])

  return { storyId, openStory, closeStory }
}
