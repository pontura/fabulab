import { Component, type ErrorInfo, type ReactNode } from 'react'

interface Props {
  children: ReactNode
}

interface State {
  error: Error | null
}

export class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null }

  static getDerivedStateFromError(error: Error): State {
    return { error }
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Admin panel crashed:', error, info.componentStack)
  }

  render() {
    if (this.state.error) {
      return (
        <div style={{ padding: 32 }}>
          <h1 className="page-title">Ocurrió un error</h1>
          <p>Esta pantalla tiene datos que el panel no pudo mostrar.</p>
          <pre className="json-viewer">{this.state.error.message}</pre>
          <button className="btn primary" onClick={() => this.setState({ error: null })}>
            Volver a intentar
          </button>
        </div>
      )
    }
    return this.props.children
  }
}
