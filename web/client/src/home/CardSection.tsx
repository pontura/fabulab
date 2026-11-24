import { useEffect, useState, type ReactNode } from 'react'

// Cards added each time "Ver más" is pressed
const PAGE_SIZE = 30

interface Props<T> {
  title: string
  // Called once when the section is shown
  load: () => Promise<T[]>
  getKey: (item: T) => string
  renderCard: (item: T) => ReactNode
  emptyMessage: string
  // A first card before the items (ex: "Crear"). With it the empty message is not shown
  firstCard?: ReactNode
}

// A titled grid of cards loaded from the database, with placeholders while loading and "Ver más"
export function CardSection<T>({ title, load, getKey, renderCard, emptyMessage, firstCard }: Props<T>) {
  const [items, setItems] = useState<T[] | null>(null)
  const [error, setError] = useState(false)
  const [visible, setVisible] = useState(PAGE_SIZE)

  useEffect(() => {
    let current = true
    load()
      .then((result) => current && setItems(result))
      .catch((e) => {
        console.error(`Could not load "${title}"`, e)
        if (current) setError(true)
      })
    return () => {
      current = false
    }
  }, [load, title])

  return (
    <section className="section">
      <h2 className="section-title">{title}</h2>

      {error && <p className="section-message">No pudimos cargar esta sección. Probá de nuevo en un rato.</p>}

      {!error && items === null && (
        <div className="card-grid" aria-busy="true">
          {Array.from({ length: 10 }, (_, i) => (
            <div key={i} className="story-card is-placeholder">
              <div className="story-thumb" />
              <div className="story-title-placeholder" />
            </div>
          ))}
        </div>
      )}

      {items && items.length === 0 && !firstCard && <p className="section-message">{emptyMessage}</p>}

      {items && (items.length > 0 || firstCard) && (
        <>
          <div className="card-grid">
            {firstCard && <div className="card-grid-cell">{firstCard}</div>}
            {items.slice(0, visible).map((item) => (
              <div key={getKey(item)} className="card-grid-cell">
                {renderCard(item)}
              </div>
            ))}
          </div>
          {visible < items.length && (
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
