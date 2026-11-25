import { useUrlParam } from '../common/useUrlParam'

// The story being watched lives in the URL (?story=<id>), so it can be shared by copying the link
export function useStoryUrl() {
  const { value, open, close } = useUrlParam('story')
  return { storyId: value, openStory: open, closeStory: close }
}
