import { useCallback } from 'react'
import { RemoteImage } from '../common/RemoteImage'
import { imageUrl, type ImageFolder } from '../data/images'
import { HeartIcon } from './StoryCard'

interface Props {
  id: string
  userID: string
  name: string
  likes: number
  folder: ImageFolder
}

// Card of a character or an object: its picture, likes and name
export function WorkCard({ id, userID, name, likes, folder }: Props) {
  const loadThumb = useCallback(() => imageUrl(userID, folder, id), [userID, folder, id])
  const title = name.trim() || 'Sin nombre'

  return (
    <article className="story-card">
      <div className="story-thumb work-thumb">
        <RemoteImage load={loadThumb} alt={title} />
        <span className="story-likes" aria-label={`${likes} me gusta`}>
          <HeartIcon />
          {likes}
        </span>
      </div>
      <h3 className="story-title">{title}</h3>
    </article>
  )
}
