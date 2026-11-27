import { useCallback, useEffect, useState } from 'react'
import { RemoteImage } from '../common/RemoteImage'
import { useUsername } from '../common/UserAvatar'
import type { Credit } from '../data/credits'
import { imageUrl } from '../data/images'
import { findWork } from '../data/works'

// The works of other users that a story or minigame uses: the picture of each one, its name and its author
export function CreditsList({ credits }: { credits: Credit[] }) {
  return (
    <ul className="credits-list">
      {credits.map((credit) => (
        <CreditRow key={`${credit.kind}/${credit.id}`} credit={credit} />
      ))}
    </ul>
  )
}

function CreditRow({ credit }: { credit: Credit }) {
  const author = useUsername(credit.authorId)
  const [name, setName] = useState('')
  const [isBackground, setIsBackground] = useState(false)
  // The picture of the original (it may have been deleted by its author: then, none)
  const loadThumb = useCallback(
    () => imageUrl(credit.authorId, credit.kind, credit.id),
    [credit.authorId, credit.kind, credit.id],
  )

  useEffect(() => {
    let current = true
    findWork(credit.kind, credit.id)
      .then((work) => {
        if (!current) return
        setName(work?.name.trim() ?? '')
        setIsBackground(work?.type === 1)
      })
      .catch(() => {})
    return () => {
      current = false
    }
  }, [credit.kind, credit.id])

  return (
    <li className="credits-row">
      <span className="credits-thumb">
        <RemoteImage load={loadThumb} alt={name} />
      </span>
      <span className="creators-text">
        <span className="creators-name">{name || (credit.kind === 'characters' ? 'Personaje' : isBackground ? 'Fondo' : 'Objeto')}</span>
        <span className="creators-role">por {author || 'otro usuario'}</span>
      </span>
    </li>
  )
}
