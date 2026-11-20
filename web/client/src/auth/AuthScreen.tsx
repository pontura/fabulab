import { useState, type FormEvent } from 'react'
import { errorCode, resetPassword, signIn, signUp, type UserProfile } from './authService'
import { isEmailValid, messages, MIN_PASSWORD_LENGTH } from './messages'

type Mode = 'login' | 'register' | 'reset'

const titles: Record<Mode, string> = {
  login: messages.loginTitle,
  register: messages.registerTitle,
  reset: messages.resetTitle,
}

interface Props {
  onSignedUp: (profile: UserProfile) => void
}

// Login, sign up and password reset by email, like the email panel of the app (LoginManager)
export function AuthScreen({ onSignedUp }: Props) {
  const [mode, setMode] = useState<Mode>('login')
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [feedback, setFeedback] = useState('')
  const [submitting, setSubmitting] = useState(false)

  function goTo(next: Mode) {
    setMode(next)
    setUsername('')
    setEmail('')
    setPassword('')
    setFeedback('')
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (submitting) return

    if (mode === 'register') {
      if (username.trim() === '') return setFeedback(messages.usernameMissing)
      if (!isEmailValid(email)) return setFeedback(messages.emailInvalid)
      if (password.length < MIN_PASSWORD_LENGTH) return setFeedback(messages.passwordShort)
    }

    setSubmitting(true)
    setFeedback(messages.sending)
    try {
      if (mode === 'login') {
        await signIn(email, password)
        setFeedback(messages.connected)
      } else if (mode === 'register') {
        const profile = await signUp(username, email, password)
        setFeedback(messages.signUpDone)
        onSignedUp(profile)
      } else {
        await resetPassword(email)
        setFeedback(messages.resetSent(email.trim()))
      }
    } catch (error) {
      setFeedback(errorMessage(mode, errorCode(error)))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="screen auth-screen">
      <img className="logo" src="/images/logo.png" alt="Fabulab" />

      <form className="auth-form" onSubmit={handleSubmit} noValidate>
        <p className="auth-title">{titles[mode]}</p>

        {mode === 'register' && (
          <input
            className="field"
            type="text"
            placeholder="nombre de usuario"
            autoComplete="nickname"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
          />
        )}

        <input
          className="field"
          type="email"
          placeholder="correo electrónico"
          autoComplete="email"
          inputMode="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />

        {mode !== 'reset' && (
          <div className="password-field">
            <input
              className="field"
              type={showPassword ? 'text' : 'password'}
              placeholder="contraseña"
              autoComplete={mode === 'register' ? 'new-password' : 'current-password'}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
            <button
              type="button"
              className={`password-toggle ${showPassword ? 'is-on' : ''}`}
              aria-label={showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'}
              aria-pressed={showPassword}
              onClick={() => setShowPassword((v) => !v)}
            >
              <img src="/images/eye-open.png" alt="" />
            </button>
          </div>
        )}

        <button className="button button-primary" type="submit" disabled={submitting}>
          {mode === 'login' ? 'Ingresar' : mode === 'register' ? 'Registrarme' : 'Enviar'}
        </button>

        <p className="feedback" role="status" aria-live="polite">{feedback}</p>
      </form>

      <nav className="auth-nav">
        {mode !== 'login' && (
          <button className="button button-nav" type="button" onClick={() => goTo('login')}>
            Ingresar
          </button>
        )}
        {mode !== 'register' && (
          <button className="button button-nav" type="button" onClick={() => goTo('register')}>
            Registro
          </button>
        )}
        {mode !== 'reset' && (
          <button className="button button-nav" type="button" onClick={() => goTo('reset')}>
            Olvidé mi contraseña
          </button>
        )}
      </nav>
    </div>
  )
}

function errorMessage(mode: Mode, code: string): string {
  if (mode === 'login') return messages.loginFailed
  if (mode === 'reset') return messages.resetFailed
  if (code === 'auth/email-already-in-use') return messages.emailInUse
  if (code === 'auth/invalid-email') return messages.emailInvalid
  if (code === 'auth/weak-password') return messages.passwordShort
  return messages.signUpFailed
}
