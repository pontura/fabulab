import { useEffect, useRef, useState } from 'react'

interface Props {
  // Resolves the URL of the image (null: it has none). Called once the image is about to be visible
  load: () => Promise<string | null>
  alt: string
  className?: string
}

// An image of Storage that asks for its URL only when it gets near the screen, so a long grid
// does not request all of them at once
export function RemoteImage({ load, alt, className }: Props) {
  const holder = useRef<HTMLDivElement>(null)
  const [url, setUrl] = useState<string | null>(null)
  const [state, setState] = useState<'idle' | 'loading' | 'loaded' | 'missing'>('idle')

  useEffect(() => {
    const element = holder.current
    if (!element || state !== 'idle') return
    const observer = new IntersectionObserver(
      (entries) => {
        if (!entries.some((e) => e.isIntersecting)) return
        observer.disconnect()
        setState('loading')
        load().then((result) => {
          if (result) setUrl(result)
          else setState('missing')
        })
      },
      { rootMargin: '300px' },
    )
    observer.observe(element)
    return () => observer.disconnect()
  }, [load, state])

  return (
    <div ref={holder} className={`remote-image ${className ?? ''} is-${state}`}>
      {url && <img src={url} alt={alt} onLoad={() => setState('loaded')} onError={() => setState('missing')} />}
    </div>
  )
}
