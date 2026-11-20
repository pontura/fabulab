import { useCallback } from 'react'
import { RemoteImage } from '../common/RemoteImage'
import { imageUrl } from '../data/images'
import type { StoryMeta } from '../data/stories'

interface Props {
  story: StoryMeta
  onOpen: (story: StoryMeta) => void
}

export function StoryCard({ story, onOpen }: Props) {
  const loadThumb = useCallback(() => imageUrl(story.userID, 'stories', story.id), [story.userID, story.id])
  const title = story.name.trim() || 'Sin título'

  return (
    <article className="story-card">
      <button type="button" className="story-thumb" onClick={() => onOpen(story)} aria-label={`Ver ${title}`}>
        <RemoteImage load={loadThumb} alt={title} />
        <span className="story-likes" aria-label={`${story.likes} me gusta`}>
          <HeartIcon />
          {story.likes}
        </span>
        <span className="story-play" aria-hidden="true">
          <svg viewBox="0 0 24 24" width="30" height="30">
            <path fill="currentColor" d="M8 5.5v13l11-6.5z" />
          </svg>
        </span>
      </button>
      <h3 className="story-title">{title}</h3>
    </article>
  )
}

function HeartIcon() {
  return (
    <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true">
      <path
        fill="currentColor"
        d="M12 21s-7.5-4.6-9.6-9.2C.9 8.4 3 4.5 6.7 4.5c2.1 0 3.6 1.1 5.3 3 1.7-1.9 3.2-3 5.3-3 3.7 0 5.8 3.9 4.3 7.3C19.5 16.4 12 21 12 21z"
      />
    </svg>
  )
}
