import { UserAvatar, useUsername } from '../common/UserAvatar'
import { timeValue } from '../data/timestamps'

interface Props {
  // The users that made the work, in the order they last modified it (the last one modified it last)
  creators: string[]
  // When it was modified last (by the last creator). The others have no date saved
  timestamp?: string
}

// Everyone that took part in a work, the last one first: picture, name and what it did (CreatorsDialog, the player)
export function CreatorsList({ creators, timestamp }: Props) {
  const date = formatDate(timestamp)
  const last = creators.length - 1
  return (
    <ol className="creators-list">
      {creators
        .map((uid, index) => ({ uid, index }))
        .reverse()
        .map(({ uid, index }) => (
          <CreatorRow key={uid} uid={uid} detail={detailOf(index, last, date)} />
        ))}
    </ol>
  )
}

function CreatorRow({ uid, detail }: { uid: string; detail: string }) {
  const name = useUsername(uid)
  return (
    <li className="creators-row">
      <UserAvatar uid={uid} name={name} className="creators-avatar" />
      <span className="creators-text">
        <span className="creators-name">{name || 'Usuario'}</span>
        <span className="creators-role">{detail}</span>
      </span>
    </li>
  )
}

// What each one did. The date saved is the one of the last modification, so only the last one shows it
function detailOf(index: number, last: number, date: string): string {
  if (index === last) {
    if (last === 0) return date ? `Autor · modificada el ${date}` : 'Autor'
    return date ? `Última modificación · ${date}` : 'Última modificación'
  }
  return index === 0 ? 'Creó la obra' : 'Participó'
}

function formatDate(timestamp?: string): string {
  const value = timestamp ? timeValue(timestamp) : 0
  if (!value) return ''
  return new Date(value).toLocaleDateString('es-AR', { day: 'numeric', month: 'long', year: 'numeric' })
}
