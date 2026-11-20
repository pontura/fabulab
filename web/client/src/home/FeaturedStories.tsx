import { useEffect, useState } from 'react'
import { loadFeaturedStories, type StoryMeta } from '../data/stories'
import { StoryCard } from './StoryCard'

// Cards added each time the end of the grid is reached
const PAGE_SIZE = 30

interface Props {
  onOpenStory: (story: StoryMeta) => void
}

export function FeaturedStories({ onOpenStory }: Props) {
  const [stories, setStories] = useState<StoryMeta[] | null>(null)
  const [error, setError] = useState(false)
  const [visible, setVisible] = useState(PAGE_SIZE)

  useEffect(() => {
    let current = true
    loadFeaturedStories()
      .then((result) => current && setStories(result))
      .catch((e) => {
        console.error('Could not load the stories', e)
        if (current) setError(true)
      })
    return () => {
      current = false
    }
  }, [])

  return (
    <section className="section">
      <h2 className="section-title">Historias destacadas</h2>

      {error && <p className="section-message">No pudimos cargar las historias. Probá de nuevo en un rato.</p>}

      {!error && stories === null && (
        <div className="card-grid" aria-busy="true">
          {Array.from({ length: 10 }, (_, i) => (
            <div key={i} className="story-card is-placeholder">
              <div className="story-thumb" />
              <div className="story-title-placeholder" />
            </div>
          ))}
        </div>
      )}

      {stories && stories.length === 0 && <p className="section-message">Todavía no hay historias publicadas.</p>}

      {stories && stories.length > 0 && (
        <>
          <div className="card-grid">
            {stories.slice(0, visible).map((story) => (
              <StoryCard key={story.id} story={story} onOpen={onOpenStory} />
            ))}
          </div>
          {visible < stories.length && (
            <div className="section-more">
              <button className="button button-nav" type="button" onClick={() => setVisible((v) => v + PAGE_SIZE)}>
                Ver más
              </button>
            </div>
          )}
        </>
      )}
    </section>
  )
}
