import { useEffect, useRef, useState, type ReactNode } from 'react'

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
  // The works of the user ("Mis ..."): in a box of another color, with smaller cards, one row only until
  // "Ver más" is pressed
  own?: boolean
}

// A titled grid of cards loaded from the database, with placeholders while loading and "Ver más"
export function CardSection<T>({ title, load, getKey, renderCard, emptyMessage, firstCard, own }: Props<T>) {
  const [items, setItems] = useState<T[] | null>(null)
  const [error, setError] = useState(false)
  const [visible, setVisible] = useState(PAGE_SIZE)
  // own: all the rows shown ("Ver más")
  const [expanded, setExpanded] = useState(false)
  const grid = useRef<HTMLDivElement>(null)
  // own: how many cards fit in one row (the columns of the grid, that change with the width of the window)
  const columns = useColumns(grid, own === true && items !== null)
  const cardCount = (items?.length ?? 0) + (firstCard ? 1 : 0)
  const folded = own === true && !expanded
  // Folded: only the cards of the first row (all of them until the columns are known)
  const shown = folded && columns > 0 ? Math.max(0, columns - (firstCard ? 1 : 0)) : visible

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
    <section className={own ? 'section section-own' : 'section'}>
      {/* own: "Ver más" at the right of the title, when there are more works than the ones of the first row */}
      <div className="section-header">
        <h2 className="section-title">{title}</h2>
        {own && items && columns > 0 && cardCount > columns && (
          <button className="button button-nav section-all" type="button" onClick={() => setExpanded((v) => !v)}>
            {expanded ? 'Ver menos' : 'Ver más'}
          </button>
        )}
      </div>

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
          <div ref={grid} className="card-grid">
            {firstCard && <div className="card-grid-cell">{firstCard}</div>}
            {items.slice(0, Math.min(shown, visible)).map((item) => (
              <div key={getKey(item)} className="card-grid-cell">
                {renderCard(item)}
              </div>
            ))}
          </div>
          {!folded && visible < items.length && (
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

// The columns of a grid now (0 until it is shown), kept up to date while the window changes
function useColumns(grid: React.RefObject<HTMLDivElement>, active: boolean): number {
  const [columns, setColumns] = useState(0)
  useEffect(() => {
    const element = grid.current
    if (!active || !element) return
    const measure = () => {
      const template = getComputedStyle(element).gridTemplateColumns
      setColumns(template && template !== 'none' ? template.split(' ').filter(Boolean).length : 0)
    }
    measure()
    const observer = new ResizeObserver(measure)
    observer.observe(element)
    return () => observer.disconnect()
  }, [grid, active])
  return columns
}
