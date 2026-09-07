export interface Vec3 {
  x: number
  y: number
  z: number
}

export interface ItemInstance {
  anim: number
  color: number
  galleryID: number
  id: number
  part: number
  position: Vec3
  rotation: Vec3
  scale: Vec3
}

export interface Character {
  armsColor: number
  bg: number
  eyebrowsColor: number
  legsColor: number
  items: ItemInstance[]
}

export interface SceneObject {
  bg: number
  type: number
  items: ItemInstance[]
}

export interface StoryElementData {
  customization: string
  force_z: number
  goLeft: boolean
  id: string
  itemName: string
  pos: Vec3
  rot: number
  size: number
}

export interface StoryElement {
  type: number
  anim?: string
  emoji?: string
  fontId?: number
  direction?: number
  input?: string
  data: StoryElementData
}

export interface StoryScene {
  bgID: string
  duration: string
  lightingId: string
  lightingValue: string
  transition: string
  scenesElements: StoryElement[]
}

export type Story = StoryScene[]

export type ContentKind = 'characters' | 'so' | 'stories'

export interface MetadataEntry {
  name: string
  isPublic: boolean
  likes?: number
  tags?: string[]
  timestamp: string
  userID: string
  creators?: string[]
  speed?: number
}

export interface Preset {
  bg: number
  items: ItemInstance[]
}

export type BodypartCategory = 'BODY' | 'FACE' | 'FOOT' | 'HAIR' | 'HAND' | 'HEAD' | 'none'

export interface GameDef {
  id: string
  title: string
  description: string
  section: string
  ids: Record<string, (string | null)[]>
}

export interface Tag {
  id: string
  name: string
  itemTypes?: string[]
}

export interface UserRecord {
  uid: string
  email: string
  username?: string
  thumb_timestamp?: string
  likes?: Record<string, string>
  onboardings?: Record<string, boolean>
}
