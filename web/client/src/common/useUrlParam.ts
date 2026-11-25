import { useCallback, useEffect, useState } from 'react'

// A value kept in a parameter of the page URL (?name=<value>), so what it opens can be shared by copying the link.
// Opening adds a history entry: the browser back button closes it
const STATE_KEY = 'fabulabParam'

function readParam(name: string): string | null {
  return new URL(window.location.href).searchParams.get(name) || null
}

function urlWithParam(name: string, value: string | null): string {
  const url = new URL(window.location.href)
  if (value) url.searchParams.set(name, value)
  else url.searchParams.delete(name)
  return url.pathname + url.search + url.hash
}

export function useUrlParam(name: string) {
  const [value, setValue] = useState<string | null>(() => readParam(name))

  // Browser back / forward: the value follows the URL
  useEffect(() => {
    const onPopState = () => setValue(readParam(name))
    window.addEventListener('popstate', onPopState)
    return () => window.removeEventListener('popstate', onPopState)
  }, [name])

  const open = useCallback(
    (newValue: string) => {
      if (readParam(name) === newValue) return setValue(newValue)
      window.history.pushState({ [STATE_KEY]: name }, '', urlWithParam(name, newValue))
      setValue(newValue)
    },
    [name],
  )

  const close = useCallback(() => {
    // Opened from this site: going back removes the entry, as the browser back button would
    if (window.history.state?.[STATE_KEY] === name) {
      window.history.back()
      return
    }
    // Arrived with a shared link: there is nothing of ours to go back to
    window.history.replaceState(null, '', urlWithParam(name, null))
    setValue(null)
  }, [name])

  return { value, open, close }
}
