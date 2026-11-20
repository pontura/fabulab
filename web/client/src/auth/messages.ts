// Texts of the login of the app (LoginManager.cs)
export const messages = {
  loginTitle: 'Si ya estás registrado completá tus datos para ingresar',
  registerTitle: 'Completá tus datos para registrarte',
  resetTitle: 'Si no recordás tu contraseña completá tu correo electrónico',

  sending: 'Enviando datos...',
  connected: 'Conectado...',
  welcome: (username: string) => `Bienvenide ${username}`,

  loginFailed: 'La dirección de correo electrónico o la contraseña son incorrectas',
  signUpDone: 'Usuario registrado correctamente',
  signUpFailed: 'Ocurrió un error al registrar el usuario, intentalo nuevamente más tarde',
  emailInUse: 'Ya existe una cuenta con ese correo electrónico',
  resetSent: (email: string) => `Hemos enviado un link a ${email} para que actualices tu contraseña`,
  resetFailed: 'La dirección de correo electrónico es incorrecta',

  usernameMissing: 'Completá tu nombre de usuario',
  emailInvalid: 'El correo electrónico parece inválido',
  passwordShort: 'La contraseña debe tener al menos 6 caracteres',
}

export const MIN_PASSWORD_LENGTH = 6

// Same check as the app: an @ and a dot
export function isEmailValid(email: string): boolean {
  return email.includes('@') && email.includes('.')
}
